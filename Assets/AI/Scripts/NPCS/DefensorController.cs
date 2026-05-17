using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DefensorController : MonoBehaviour
{
    [Header("Identidad")]
    public string npcName = "npc_defensor";

    [Header("Equipo: 0=Rojo  1=Azul")]
    public int teamID = 0;

    [Header("Referencias")]
    public Transform player;
    public Transform flag;       // bandera propia que protege
    public Flag flagToSteal;     // bandera enemiga que puede robar
    public Transform ownBase;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform respawnPoint;

    [Header("Waypoints de patrulla")]
    public Transform[] waypoints;

    [Header("Vision")]
    public float visionRange = 15f;
    public float visionAngle = 90f;

    [Header("Vida")]
    public float maxLife = 10f;
    public float currentLife;

    [Header("Ataque")]
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;

    [Header("Patrulla")]
    public float patrolRadius = 15f;
    public float listenRadius = 3f;

    [Header("Radio para coger bandera")]
    public float flagPickupRadius = 2.5f;

    [Header("Regeneración de vida")]
    public float regenAmount = 1f;
    public float regenInterval = 5f;

    [HideInInspector] public Vector3 lastKnownPlayerPosition = Vector3.zero;

    private StateDefensor currentState;
    private bool isDead = false;
    private bool isTakingDamage = false;
    private float lastAttackTime = 0f;
    private bool foundFlag = false;
    private Coroutine returnCoroutine;

    private float lowVelocityTimer = 0f;
    private const float MAX_LOW_VELOCITY_TIME = 6f;

    private float regenTimer = 0f;

    public bool IsEnemyOfPlayer => teamID != TeamSelector.GetTeam();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentLife = maxLife;
        agent.updateRotation = false;
        ChangeState(new PatrullarState(this)); // estado inicial
        GetComponentInChildren<NPCHealthDisplay>()?.UpdateHealth(currentLife, maxLife);
        GetComponentInChildren<NPCNombre>()?.UpdateNameColor(teamID, false);
    }

    void Update()
    {
        if (isDead) return;

        // anima velocidad y rota hacia la dirección de movimiento
        animator.SetFloat("Speed", agent.velocity.magnitude);
        if (agent.velocity.magnitude > 0.2f)
        {
            Vector3 dir = agent.velocity; dir.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 12f);
        }

        CheckFlagPickup();
        RegenerateLife();

        //ejecuta el estado si no está corriendo a la base con la bandera
        if (!foundFlag)
            currentState?.Update();

        // intenta atacar al portador de la bandera propia primero, luego al enemigo más cercano
        if (!TryAttackFlagCarrier())
            TryAttackNearestEnemy();

        WatchdogAntiStop();
    }

    void RegenerateLife()
    {
        if (currentLife <= 0 || currentLife >= maxLife) { regenTimer = 0f; return; }
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
        if (agent.velocity.magnitude < 0.1f)
        {
            lowVelocityTimer += Time.deltaTime;
            if (lowVelocityTimer >= MAX_LOW_VELOCITY_TIME)
            {
                lowVelocityTimer = 0f;
                agent.isStopped = false;
                agent.ResetPath();
                ChangeState(new PatrullarState(this));
                Debug.Log(npcName + " watchdog: velocidad 0 → reinicio patrulla");
            }
        }
        else lowVelocityTimer = 0f;
    }

    void OnDrawGizmos()
    {
        // dibuja los waypoints en el editor 
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = teamID == 0 ? new Color(1f, 0.2f, 0.2f, 0.8f) : new Color(0.2f, 0.5f, 1f, 0.8f);
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position + Vector3.up * 0.5f, 0.4f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position + Vector3.up * 0.5f, waypoints[next].position + Vector3.up * 0.5f);
        }
    }

    void CheckFlagPickup()
    {
        if (foundFlag || flagToSteal == null || flagToSteal.IsCarried()) return;
        bool near = false;
        Collider[] cols = Physics.OverlapSphere(transform.position, flagPickupRadius);
        foreach (Collider c in cols)
            if (c.GetComponent<Flag>() == flagToSteal) { near = true; break; }
        if (!near) near = Vector3.Distance(transform.position, flagToSteal.transform.position) < flagPickupRadius;
        if (near && flagToSteal.TryPickUpByNPC(transform))
        {
            foundFlag = true;
            GetComponentInChildren<NPCNombre>()?.UpdateNameColor(teamID, true);
            //notifica al gamemanaer según si es enemigo o aliado del jugador
            if (IsEnemyOfPlayer) GameManager.Instance?.NotifyNPCPickedFlag();
            else GameManager.Instance?.NotifyAllyPickedFlag();
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnToBase());
        }
    }

    IEnumerator ReturnToBase()
    {
        if (ownBase == null) { foundFlag = false; yield break; }
        while (foundFlag)
        {
            // llegó a la base con la bandera, dapunto
            if (Vector3.Distance(transform.position, ownBase.position) < 3.5f)
            { foundFlag = false; GetComponentInChildren<NPCNombre>()?.UpdateNameColor(teamID, false); flagToSteal.Capture(); yield return null; yield break; }
            // actualiza destino cada 0.5s
            agent.SetDestination(SampleNavMesh(ownBase.position));
            agent.isStopped = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    bool TryAttackFlagCarrier()
    {
        if (Time.time - lastAttackTime < attackCooldown) return false; // en cooldown
        if (flag == null) return false;
        Flag f = flag.GetComponent<Flag>();
        if (f == null || !f.IsCarried()) return false; // nadie lleva la bandera propia
        Transform carrier = f.GetCarrier();
        if (carrier == null) return false;
        // comprueba si el portador es enemigo
        bool isEnemy = false;
        if (carrier.CompareTag("Player") && IsEnemyOfPlayer) isEnemy = true;
        BuscadorController b = carrier.GetComponent<BuscadorController>();
        if (b != null && b.teamID != teamID) isEnemy = true;
        DefensorController d = carrier.GetComponent<DefensorController>();
        if (d != null && d != this && d.teamID != teamID) isEnemy = true;
        if (!isEnemy) return false;
        if (Vector3.Distance(transform.position, carrier.position) > attackRange) return false; // fuera de rango
        FaceTarget(carrier.position);
        lastAttackTime = Time.time;
        animator.SetBool("IsAttacking", true);
        Invoke(nameof(StopAttack), 1f);
        // aplica daño al portador sea jugador, buscador o defensor
        carrier.GetComponent<PlayerController>()?.TakeDamage(1);
        carrier.GetComponent<BuscadorController>()?.TakeDamage(attackDamage);
        carrier.GetComponent<DefensorController>()?.TakeDamage(attackDamage);
        return true;
    }

    bool TryAttackNearestEnemy()
    {
        if (Time.time - lastAttackTime < attackCooldown) return false; // en cooldown
        Transform nearest = null; float nearestDist = attackRange;
        //busca enemigos dentro del rango de ataque
        Collider[] cols = Physics.OverlapSphere(transform.position, attackRange);
        foreach (Collider col in cols)
        {
            Transform t = col.transform; float dt = Vector3.Distance(transform.position, t.position);
            if (t.CompareTag("Player") && IsEnemyOfPlayer && dt < nearestDist) { nearest = t; nearestDist = dt; }
            BuscadorController bus = col.GetComponent<BuscadorController>();
            if (bus != null && bus.teamID != teamID && dt < nearestDist) { nearest = t; nearestDist = dt; }
            DefensorController def = col.GetComponent<DefensorController>();
            if (def != null && def != this && def.teamID != teamID && dt < nearestDist) { nearest = t; nearestDist = dt; }
        }
        if (nearest == null) return false;
        FaceTarget(nearest.position);
        lastAttackTime = Time.time;
        animator.SetBool("IsAttacking", true);
        Invoke(nameof(StopAttack), 1f);
        // aplica daño al enemigo más cercano
        nearest.GetComponent<PlayerController>()?.TakeDamage(1);
        nearest.GetComponent<BuscadorController>()?.TakeDamage(attackDamage);
        nearest.GetComponent<DefensorController>()?.TakeDamage(attackDamage);
        return true;
    }

    void FaceTarget(Vector3 t) { Vector3 d = t - transform.position; d.y = 0f; if (d != Vector3.zero) transform.rotation = Quaternion.LookRotation(d); }
    void StopAttack() => animator.SetBool("IsAttacking", false);

    public void ChangeState(StateDefensor s) { currentState?.Exit(); currentState = s; currentState.Enter(); }

    public float LifeRemaining() => currentLife;
    public float DistanceToOpponent() => player == null ? Mathf.Infinity : Vector3.Distance(transform.position, player.position);
    // distancia entre el jugador y la bandera que protege el defensor
    public float DistanceOpponentFlag() => (player == null || flag == null) ? Mathf.Infinity : Vector3.Distance(player.position, flag.position);

    public bool LineOfSigthToPlayer()
    {
        if (player == null) return false;
        Vector3 dir = player.position - transform.position;
        if (dir.magnitude > visionRange) return false;                          // fuera del rango de visión
        if (Vector3.Angle(transform.forward, dir) > visionAngle / 2f) return false; // fuera del ángulo de visión
        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, visionRange))
            return hit.transform == player;
        return false;
    }

    public bool FlagStolen()
    {
        if (GameManager.Instance == null) return false;
        // evuelve true si la bandera del equipo del defensor fue robada
        return IsEnemyOfPlayer ? GameManager.Instance.flagStolenByPlayer
                               : GameManager.Instance.flagStolenByEnemy;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentLife -= amount; currentLife = Mathf.Max(currentLife, 0);
        regenTimer = 0f; // resetea regeneración al recibir daño
        GetComponentInChildren<NPCHealthDisplay>()?.UpdateHealth(currentLife, maxLife);
        if (!isTakingDamage && HasParam("TakeDamage")) StartCoroutine(TakeDamageAnim());
        if (currentLife <= 0) StartCoroutine(DieAndRespawn());
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
        // si llevaba la bandera enemiga al morir, la devuelve a su base
        if (flagToSteal != null && flagToSteal.IsCarried() && flagToSteal.GetCarrier() == transform)
            flagToSteal.ReturnToBase();
        yield return new WaitForSeconds(3f); // espera animación de muerte
        agent.Warp(respawnPoint.position); transform.position = respawnPoint.position;
        NPCAnimatorHelper.ResetToIdle(animator);
        yield return null;
        // restaura estado completo y vuelve al estado inicial
        isDead = false; currentLife = maxLife;
        agent.isStopped = false; agent.updateRotation = false; agent.ResetPath();
        lastKnownPlayerPosition = Vector3.zero;
        lowVelocityTimer = 0f;
        regenTimer = 0f;
        GetComponentInChildren<NPCHealthDisplay>()?.UpdateHealth(currentLife, maxLife);
        ChangeState(new PatrullarState(this));
    }

    // busca el punto del ¡navMesh más cercano al destininoi
    Vector3 SampleNavMesh(Vector3 t)
    {
        foreach (float r in new float[] { 0.5f, 1f, 2f, 5f, 10f })
            if (NavMesh.SamplePosition(t, out NavMeshHit h, r, NavMesh.AllAreas)) return h.position;
        return t;
    }

    bool HasParam(string p) { foreach (var x in animator.parameters) if (x.name == p) return true; return false; }
    public void TriggerAttack() { animator.SetBool("IsAttacking", true); Invoke(nameof(StopAttack), 1f); }
}