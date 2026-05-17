using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BuscadorController : MonoBehaviour // cerebro del buscador, para gestionar estados, vida..
{
    // campos configurables en unity
    [Header("Identidad")]
    public string npcName = "npc_buscador";

    [Header("Equipo: 0=Rojo  1=Azul")]
    public int teamID = 0;

    [Header("Referencias")]
    public Transform player; // referencia al jugador
    public NavMeshAgent agent;
    public Animator animator;
    public Transform respawnPoint;
    public Flag flagToSteal;
    public Transform ownBase; // base donde se entrega la bandera
    public Flag ownFlag;

    [Header("Vision")]
    public float visionRange = 15f;
    public float visionAngle = 90f;
    public float listenRadius = 5f;

    [Header("Vida")]
    public float maxLife = 10f;
    public float currentLife;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;

    [Header("Regeneraci?n de vida")]
    public float regenAmount = 1f;
    public float regenInterval = 5f;

    [Header("Radios")]
    public float flagPickupRadius = 2.5f;
    public float baseDeliverRadius = 3.5f;

    public Flag enemyFlag => flagToSteal; // para acceder a la bandera enemiga

    // se?ales para el estado activo
    [HideInInspector] public bool foundFlag = false;
    [HideInInspector] public bool ownTeamFlagSignal = false;
    [HideInInspector] public bool enemyFlagSignal = false;
    [HideInInspector] public Vector3 lastKnownPosition;
    [HideInInspector] public bool hasLastKnownPosition = false;

    private StateBuscador currentState; //estado actual
    private bool isDead = false; // muerte
    private bool isTakingDamage = false;
    private float lastAttackTime = 0f;
    private Coroutine returnCoroutine; //sorrutina de vuelta a la base


    private float lowVelocityTimer = 0f; // watchdog de inactividad
    private const float MAX_LOW_VELOCITY_TIME = 5f;

    private float regenTimer = 0f;

    public bool IsEnemyOfPlayer => teamID != TeamSelector.GetTeam(); //detectar si es enemigo
    public bool IsAllyOfPlayer => teamID == TeamSelector.GetTeam(); // detectar si es aliado

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentLife = maxLife;
        agent.updateRotation = false;
        if (flagToSteal == null) Debug.LogError(npcName + ": flagToSteal es NULL!");
        if (ownBase == null) Debug.LogError(npcName + ": ownBase es NULL!");
        ChangeState(new BuscarState(this)); // ESTADO INICIal
        GetComponentInChildren<NPCHealthDisplay>()?.UpdateHealth(currentLife, maxLife);
        GetComponentInChildren<NPCNombre>()?.UpdateNameColor(teamID, false);
    }

    void Update()
    {
        if (isDead) return; //si está muerto, no hacer nada

        // para leer se?ales del game manager
        if (GameManager.Instance != null)
        {
            ownTeamFlagSignal = GameManager.Instance.flagStolenByEnemy;
            enemyFlagSignal = GameManager.Instance.flagStolenByPlayer;
        }

        CheckFlagPickup(); // intenta recoger la bandera si est? cerca
        RegenerateLife();

        if (!foundFlag) // si no lleva bandera, ejecuta estado activo
            currentState?.Update();

        // animar velocidad y rota hacia donde se mueve
        animator.SetFloat("Speed", agent.velocity.magnitude);
        if (agent.velocity.magnitude > 0.2f)
        {
            Vector3 dir = agent.velocity; dir.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 12f);
        }

        if (!TryAttackFlagCarrier()) // ataca si puede
            TryAttackNearestEnemy();

        WatchdogAntiStop(); //si lleva 5s sin moverse, reinicia buscar state
    }

    void RegenerateLife() //regenera la vida
    {
        if (currentLife <= 0 || currentLife >= maxLife) { regenTimer = 0f; return; } // no regenera si esta muerto o lleno
        regenTimer += Time.deltaTime;
        if (regenTimer >= regenInterval)
        {
            regenTimer = 0f;
            currentLife = Mathf.Min(currentLife + regenAmount, maxLife);
            GetComponentInChildren<NPCHealthDisplay>()?.UpdateHealth(currentLife, maxLife);
        }
    }

    void WatchdogAntiStop()
    {
        if (foundFlag) { lowVelocityTimer = 0f; return; } // si lleva la bandera no se actviva

        if (agent.velocity.magnitude < 0.1f)
        {
            lowVelocityTimer += Time.deltaTime;
            if (lowVelocityTimer >= MAX_LOW_VELOCITY_TIME)
            {
                lowVelocityTimer = 0f;
                agent.isStopped = false;
                agent.ResetPath();
                ChangeState(new BuscarState(this));
                Debug.Log(npcName + " watchdog: velocidad 0 ? reinicio");
            }
        }
        else lowVelocityTimer = 0f;
    }

    void CheckFlagPickup()
    {
        if (foundFlag || flagToSteal == null || flagToSteal.IsCarried()) return; // ya la tiene o no existe
        bool near = false;
        Collider[] cols = Physics.OverlapSphere(transform.position, flagPickupRadius);
        foreach (Collider c in cols)
            if (c.GetComponent<Flag>() == flagToSteal) { near = true; break; }
        if (!near) near = Vector3.Distance(transform.position, flagToSteal.transform.position) < flagPickupRadius;
        if (near && flagToSteal.TryPickUpByNPC(transform))
        {
            foundFlag = true;
            GetComponentInChildren<NPCNombre>()?.UpdateNameColor(teamID, true);
            if (IsAllyOfPlayer) GameManager.Instance?.NotifyAllyPickedFlag();
            else GameManager.Instance?.NotifyNPCPickedFlag();
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnToBaseCoroutine());
        }
    }

    IEnumerator ReturnToBaseCoroutine()
    {
        if (ownBase == null) { foundFlag = false; yield break; }
        Vector3 lastPos = transform.position; float stuckSec = 0f;
        while (foundFlag)
        {
            if (Vector3.Distance(transform.position, ownBase.position) < baseDeliverRadius)
            {
                foundFlag = false;
                GetComponentInChildren<NPCNombre>()?.UpdateNameColor(teamID, false);
                flagToSteal.Capture(); // llego, captura
                yield return new WaitForSeconds(0.1f);
                ChangeState(new BuscarState(this));
                yield break;
            }
            agent.SetDestination(SampleNavMesh(ownBase.position)); // va hacia la base
            agent.isStopped = false;
            yield return new WaitForSeconds(0.5f);
            if (Vector3.Distance(transform.position, lastPos) < 0.1f)
            {
                stuckSec += 0.5f;
                if (stuckSec >= 3f) { agent.ResetPath(); stuckSec = 0f; }
            }
            else stuckSec = 0f;
            lastPos = transform.position;
        }
    }

    bool TryAttackFlagCarrier() //ataca al portador de la bandera propia si es enemigo y est? dentro del rango
    {
        if (Time.time - lastAttackTime < attackCooldown) return false;
        if (ownFlag == null || !ownFlag.IsCarried()) return false;
        Transform carrier = ownFlag.GetCarrier();
        if (carrier == null) return false;
        bool isEnemy = false;
        if (carrier.CompareTag("Player") && IsEnemyOfPlayer) isEnemy = true;
        BuscadorController b = carrier.GetComponent<BuscadorController>();
        if (b != null && b.teamID != teamID) isEnemy = true;
        DefensorController d = carrier.GetComponent<DefensorController>();
        if (d != null && d.teamID != teamID) isEnemy = true;
        if (!isEnemy) return false;
        if (Vector3.Distance(transform.position, carrier.position) > attackRange) return false;
        FaceTarget(carrier.position);
        lastAttackTime = Time.time;
        animator.SetBool("IsAttacking", true);
        Invoke(nameof(StopAttack), 1f);
        carrier.GetComponent<PlayerController>()?.TakeDamage(1);
        carrier.GetComponent<BuscadorController>()?.TakeDamage(attackDamage);
        carrier.GetComponent<DefensorController>()?.TakeDamage(attackDamage);
        return true;
    }

    bool TryAttackNearestEnemy() // busca el enemigo m?s cercano dentro del rango y lo atacaa
    {
        if (Time.time - lastAttackTime < attackCooldown) return false;
        Transform nearest = null; float nearestDist = attackRange;
        Collider[] cols = Physics.OverlapSphere(transform.position, attackRange);
        foreach (Collider col in cols)
        {
            Transform t = col.transform; float dt = Vector3.Distance(transform.position, t.position);
            if (t.CompareTag("Player") && IsEnemyOfPlayer && dt < nearestDist) { nearest = t; nearestDist = dt; }
            BuscadorController bus = col.GetComponent<BuscadorController>();
            if (bus != null && bus != this && bus.teamID != teamID && dt < nearestDist) { nearest = t; nearestDist = dt; }
            DefensorController def = col.GetComponent<DefensorController>();
            if (def != null && def.teamID != teamID && dt < nearestDist) { nearest = t; nearestDist = dt; }
        }
        if (nearest == null) return false;
        FaceTarget(nearest.position);
        lastAttackTime = Time.time;
        animator.SetBool("IsAttacking", true);
        Invoke(nameof(StopAttack), 1f);
        nearest.GetComponent<PlayerController>()?.TakeDamage(1);
        nearest.GetComponent<BuscadorController>()?.TakeDamage(attackDamage);
        nearest.GetComponent<DefensorController>()?.TakeDamage(attackDamage);
        return true;
    }

    void FaceTarget(Vector3 t) { Vector3 d = t - transform.position; d.y = 0f; if (d != Vector3.zero) transform.rotation = Quaternion.LookRotation(d); }
    void StopAttack() => animator.SetBool("IsAttacking", false);

    public void ChangeState(StateBuscador s)
    {
        currentState?.Exit(); // sale del estado actual
        currentState = s;
        currentState.Enter(); //entra al nuevo
    }

    public bool CheckVision()
    {
        if (player == null) return false;
        Vector3 dir = player.position - transform.position;
        if (dir.magnitude > visionRange) return false;
        if (Vector3.Angle(transform.forward, dir) > visionAngle / 2f) return false;
        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, visionRange))
            if (hit.transform == player) { lastKnownPosition = player.position; hasLastKnownPosition = true; return true; }
        return false;
    }

    public bool LineOfSigthToPlayer()
    {
        if (player == null) return false;
        Vector3 dir = player.position - transform.position;
        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, 20f))
            return hit.transform == player;
        return false;
    }

    public float DistanceToPlayer() => player == null ? 999f : Vector3.Distance(transform.position, player.position);
    public float LifeRemaining() => currentLife;
    public bool FlagStolen() => GameManager.Instance != null && GameManager.Instance.flagStolenByEnemy;
    public bool FlagRecovered() => GameManager.Instance != null && GameManager.Instance.flagRecovered;
    public bool TeamFoundFlag() => foundFlag;
    public bool FlagStolenByPlayer() => GameManager.Instance != null && GameManager.Instance.flagStolenByPlayer;

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentLife -= amount; currentLife = Mathf.Max(currentLife, 0);
        regenTimer = 0f; // resetea regeneraci?n al recibir da?o
        GetComponentInChildren<NPCHealthDisplay>()?.UpdateHealth(currentLife, maxLife);
        if (!isTakingDamage && HasParam("TakeDamage")) StartCoroutine(TakeDamageAnim()); // animacion take damage
        if (currentLife <= 0) StartCoroutine(DieAndRespawn());
    }

    public bool IsEnemyFlagCarriedByAlly() //verifica que la bandera la tenga un aliado
    {
        if (flagToSteal == null || !flagToSteal.IsCarried()) return false;
        Transform carrier = flagToSteal.GetCarrier();
        if (carrier == null) return false;
        if (carrier.CompareTag("Player") && IsAllyOfPlayer) return true;
        BuscadorController b = carrier.GetComponent<BuscadorController>();
        if (b != null && b.teamID == teamID) return true;
        DefensorController d = carrier.GetComponent<DefensorController>();
        if (d != null && d.teamID == teamID) return true;
        return false;
    }
    IEnumerator TakeDamageAnim()
    {
        isTakingDamage = true; animator.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(0.5f); isTakingDamage = false;
    }

    IEnumerator DieAndRespawn()
    {
        isDead = true; foundFlag = false;
        GetComponentInChildren<NPCNombre>()?.UpdateNameColor(teamID, false);
        if (returnCoroutine != null) { StopCoroutine(returnCoroutine); returnCoroutine = null; }
        agent.isStopped = true; agent.ResetPath();
        NPCAnimatorHelper.ResetToIdle(animator);
        animator.SetBool("IsDead", true);
        if (flagToSteal != null && flagToSteal.IsCarried() && flagToSteal.GetCarrier() == transform)
            flagToSteal.ReturnToBase(); // si llevaba bandera, le devuelve
        yield return new WaitForSeconds(3f); // espera 3s (animaci?n muerte)
        Vector3 spawnPos = GetSpawnOutsideBase();
        agent.Warp(spawnPos); transform.position = spawnPos;
        NPCAnimatorHelper.ResetToIdle(animator);
        yield return null;
        isDead = false; currentLife = maxLife;
        agent.isStopped = false; agent.updateRotation = false; agent.ResetPath();
        lowVelocityTimer = 0f;
        regenTimer = 0f;
        GetComponentInChildren<NPCHealthDisplay>()?.UpdateHealth(currentLife, maxLife);
        ChangeState(new BuscarState(this)); //vuelve al estado inicial
    }

    Vector3 GetSpawnOutsideBase()
    {
        Vector3 basePos = ownBase != null ? ownBase.position : respawnPoint.position;
        if (respawnPoint != null && Vector3.Distance(respawnPoint.position, basePos) > baseDeliverRadius + 1f)
            if (NavMesh.SamplePosition(respawnPoint.position, out NavMeshHit rh, 3f, NavMesh.AllAreas)) return rh.position;
        if (ownBase != null)
            for (int i = 0; i < 8; i++)
            {
                Vector3 d = new Vector3(Mathf.Cos(i * 45f * Mathf.Deg2Rad), 0f, Mathf.Sin(i * 45f * Mathf.Deg2Rad));
                if (NavMesh.SamplePosition(ownBase.position + d * (baseDeliverRadius + 3f), out NavMeshHit ch, 3f, NavMesh.AllAreas)) return ch.position;
            }
        if (respawnPoint != null && NavMesh.SamplePosition(respawnPoint.position, out NavMeshHit fh, 5f, NavMesh.AllAreas)) return fh.position;
        return transform.position;
    }

    Vector3 SampleNavMesh(Vector3 t)
    {
        foreach (float r in new float[] { 0.5f, 1f, 2f, 5f, 10f })
            if (NavMesh.SamplePosition(t, out NavMeshHit h, r, NavMesh.AllAreas)) return h.position;
        return t;
    }

    bool HasParam(string p) { foreach (var x in animator.parameters) if (x.name == p) return true; return false; }
}