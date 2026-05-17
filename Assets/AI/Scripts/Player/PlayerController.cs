using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 7f;
    public float rotationSpeed = 12f;
    public float jumpForce = 6f;
    public float gravity = -25f;

    [Header("Vidas")]
    public int maxHearts = 10;
    public int currentHearts;
    public Transform respawnRojo; // punto de reaparición si el jugador es equipo rojo
    public Transform respawnAzul; //azul

    [Header("Ataque")]
    public float attackDamage = 2f;
    public float attackRange = 2f;
    public float attackDuration = 0.25f;

    [Header("Regeneración de vida")]
    public int regenAmount = 1;
    public float regenInterval = 5f;

    [Header("EQUIPO ROJO: roba bandera AZUL, entrega en BaseRoja")]
    public Flag flagParaRojo;
    public Transform baseParaRojo;

    [Header("EQUIPO AZUL: roba bandera ROJA, entrega en BaseAzul")]
    public Flag flagParaAzul;
    public Transform baseParaAzul;

    [Header("Radios")]
    public float flagPickupRadius = 3f;
    public float baseDeliverRadius = 3f;

    [Header("Referencias")]
    public Transform cameraTransform; //para orientar 

    [HideInInspector] public int myTeamID = 0;
    [HideInInspector] public bool isDead = false;

    private Animator animator;
    private CharacterController cc;
    private float verticalVelocity = 0f; //velocidad vertical acumulada
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool hasFlag = false;        //está llevando la bandera enemiga
    private bool pickupBlocked = false;

    private Flag myFlag;         // bandera que debe robar según su equipo
    private Transform myBase;    //base donde debe entregarla
    private Transform myRespawn; // punto donde reaparece al morir

    private float regenTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        currentHearts = maxHearts;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        myTeamID = TeamSelector.GetTeam(); // lee el equipo elegido en el menú principal

        // asigna bandera, base y spawn según el equipo
        if (myTeamID == 0)
        { myFlag = flagParaRojo; myBase = baseParaRojo; myRespawn = respawnRojo; }
        else
        { myFlag = flagParaAzul; myBase = baseParaAzul; myRespawn = respawnAzul; }

        // teletransporta al jugador a su punto de spawn al inicio
        if (myRespawn != null)
        {
            transform.position = myRespawn.position;
        }

        FindFirstObjectByType<HeartsUI>()?.UpdateHearts(currentHearts);
    }

    void Update()
    {
        if (isDead) return;
        HandleMovementAndJump();
        HandleAttack();
        RegenerateLife();
        if (!pickupBlocked) CheckFlagPickup();
        CheckFlagDelivery();
        CheckFallDeath();
    }

    void RegenerateLife()
    {
        if (PlayerStats.Instance == null) return;
        if (PlayerStats.Instance.Health <= 0) { regenTimer = 0f; return; }
        if (PlayerStats.Instance.Health >= PlayerStats.Instance.MaxHealth) { regenTimer = 0f; return; }

        regenTimer += Time.deltaTime;
        if (regenTimer >= regenInterval)
        {
            regenTimer = 0f;
            PlayerStats.Instance.Heal(regenAmount);
            currentHearts = (int)PlayerStats.Instance.Health;
            FindFirstObjectByType<HeartsUI>()?.UpdateHearts(currentHearts);
            Debug.Log("Jugador regenera vida ? " + currentHearts + "/" + maxHearts);
        }
    }

    void CheckFlagPickup()
    {
        if (myFlag == null || hasFlag) return;
        if (myFlag.IsCarried()) return; //ya la lleva alguien
        bool nearFlag = false;
        Collider[] cols = Physics.OverlapSphere(transform.position, flagPickupRadius);
        foreach (Collider c in cols)
            if (c.GetComponent<Flag>() == myFlag) { nearFlag = true; break; }
        if (Vector3.Distance(transform.position, myFlag.transform.position) < flagPickupRadius) nearFlag = true;
        if (nearFlag && myFlag.TryPickUpByPlayer(transform))
        {
            hasFlag = true;
            GameManager.Instance?.NotifyPlayerPickedFlag(); // activa flagstolenbyplayer
        }
    }

    void CheckFlagDelivery()
    {
        if (!hasFlag || myFlag == null || myBase == null) return;
        if (!myFlag.IsCarried() || myFlag.GetCarrier() != transform) { hasFlag = false; return; } //perdió la bandera
        // llegó a su base con la bandera,da punto
        if (Vector3.Distance(transform.position, myBase.position) < baseDeliverRadius)
        {
            hasFlag = false;
            myFlag.Capture();
            StartCoroutine(BlockPickup(1.5f)); // espera 1.5s antes de poder recoger otra vez
        }
    }

    IEnumerator BlockPickup(float t) { pickupBlocked = true; yield return new WaitForSeconds(t); pickupBlocked = false; }

    void HandleMovementAndJump()
    {
        if (!cc.enabled) return;

        bool grounded = cc.isGrounded;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (grounded)
        {
            verticalVelocity = -1f; // pequeña fuerza hacia abajo para mantenerlo pegado al suelo
            if (isJumping) { isJumping = false; if (HasParam("IsJumping")) animator.SetBool("IsJumping", false); }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = jumpForce; isJumping = true;
                animator.SetFloat("Speed", 0f);
                if (HasParam("IsJumping")) animator.SetBool("IsJumping", true);
            }
        }
        else verticalVelocity += gravity * Time.deltaTime; // aplica gravedad

        Vector3 moveDir = Vector3.zero;
        if (inputDir.magnitude > 0.1f && !isAttacking)
        {
            // movimiento relativo a la cámara
            moveDir = cameraTransform.forward * v + cameraTransform.right * h;
            moveDir.y = 0f; moveDir.Normalize();
            // rota suavemente hacia la dirección de movimiento
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(moveDir), rotationSpeed * Time.deltaTime);
        }

        moveDir.y = verticalVelocity;
        cc.Move(moveDir * speed * Time.deltaTime);
        if (!isAttacking && !isJumping) animator.SetFloat("Speed", inputDir.magnitude);
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isJumping)
            StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsAttacking", true);
        //daña a todos los NPCs enemigos dentro del rango de ataque
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (Collider hit in hits)
        {
            var def = hit.GetComponent<DefensorController>();
            if (def != null && def.teamID != myTeamID) def.TakeDamage(attackDamage);
            var bus = hit.GetComponent<BuscadorController>();
            if (bus != null && bus.teamID != myTeamID) bus.TakeDamage(attackDamage);
        }
        yield return new WaitForSeconds(attackDuration); // duración de la animación de ataque
        animator.SetBool("IsAttacking", false);
        isAttacking = false;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        PlayerStats.Instance.TakeDamage(amount);
        regenTimer = 0f; // resetea regeneración al recibir daño
        currentHearts = (int)PlayerStats.Instance.Health;
        FindFirstObjectByType<HeartsUI>()?.UpdateHearts(currentHearts);
        if (HasParam("TakeDamage")) animator.SetTrigger("TakeDamage");
        if (PlayerStats.Instance.Health <= 0) StartCoroutine(DieAndRespawn());
    }

    IEnumerator DieAndRespawn()
    {
        isDead = true; isAttacking = false; isJumping = false;
        animator.SetBool("IsAttacking", false);
        animator.SetFloat("Speed", 0f);
        if (HasParam("IsJumping")) animator.SetBool("IsJumping", false);
        animator.SetBool("IsDead", true);

        //si llevaba la bandera al morir, la devuelve a su base
        if (hasFlag && myFlag != null && myFlag.IsCarried() && myFlag.GetCarrier() == transform)
        { hasFlag = false; myFlag.ReturnToBase(); }

        // usadeathscreen
        yield return StartCoroutine(DeathScreen.Instance.ShowDeathAndRespawn(() =>
        {
            NPCAnimatorHelper.ResetToIdle(animator);
            cc.enabled = false;
            transform.position = myRespawn != null ? myRespawn.position : Vector3.up; // reaparece en su spawn
            cc.enabled = true;
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.Heal(PlayerStats.Instance.MaxHealth); // curacion
                currentHearts = maxHearts;
            }
            FindFirstObjectByType<HeartsUI>()?.UpdateHearts(currentHearts);
            regenTimer = 0f;
            StartCoroutine(BlockPickup(2f)); //2s de cooldown para recoger bandera tras respawn
        }));

        // isDead = false DESPUÉS del fade-out completo
        isDead = false;
    }

    //muere si cae por debajo del nivel del suelo
    void CheckFallDeath()
    {
        if (transform.position.y < -10f && !isDead) StartCoroutine(DieAndRespawn());
    }

    bool HasParam(string p) { foreach (var x in animator.parameters) if (x.name == p) return true; return false; }
}