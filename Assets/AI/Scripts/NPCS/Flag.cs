using UnityEngine;

public class Flag : MonoBehaviour // hecho para gestionar el ciclo de vida de una bandera
{
    [Header("Configuracion")]
    public int teamID = 0; // 0=bandera roja  1=bandera azul
    public Transform basePosition;

    private bool isCarried = false;
    private Transform carrier = null; // quien lleva la bandera
    private float cooldown = 0f; // t espera tras volver a la base
    private Vector3 homePosition; // pos guardada en start
    private Quaternion homeRotation; // rotación guardada

    void Start()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
        if (isCarried && carrier != null)
            transform.position = carrier.position + Vector3.up * 1.8f; // flota sobre quien la lleva
    }

    public bool TryPickUpByPlayer(Transform p)
    {
        if (isCarried) { Debug.Log(name + ": ya llevada"); return false; } // ya tiene dueño
        if (cooldown > 0f) { Debug.Log(name + ": cooldown " + cooldown.ToString("F1") + "s"); return false; } // en espera
        isCarried = true;
        carrier = p;
        Debug.Log(name + " recogida por JUGADOR");
        return true;
    }

    public bool TryPickUpByNPC(Transform n)
    {
        if (isCarried || cooldown > 0f) return false;
        isCarried = true;
        carrier = n;
        return true;
    }

    public void Capture()
    {
        if (!isCarried) return;
        bool playerTeamScores = IsCarrierOnPlayerTeam(); // comprueba de qué equipo es el portador

        Debug.Log(name + " capturada — portador: " + carrier?.name +
                  " — equipo jugador: " + TeamSelector.GetTeam() +
                  " — cuenta para jugador: " + playerTeamScores);

        ReturnToBase(); // devuelve la bandera a su sitio

        if (playerTeamScores)
            GameManager.Instance?.PlayerCaptures(); // da punto al jugador
        else
            GameManager.Instance?.EnemyCaptures(); // da punto al enemigo
    }

    bool IsCarrierOnPlayerTeam()
    {
        if (carrier == null) return false;

        if (carrier.CompareTag("Player")) return true;

        int playerTeam = TeamSelector.GetTeam();

        BuscadorController b = carrier.GetComponent<BuscadorController>();
        if (b != null) return b.teamID == playerTeam;

        DefensorController d = carrier.GetComponent<DefensorController>();
        if (d != null) return d.teamID == playerTeam;

        return false;
    }

    public void ReturnToBase()
    {
        isCarried = false; 
        carrier = null;
        cooldown = 1f; //reset estado y pone cooldown
        transform.position = homePosition;
        transform.rotation = homeRotation; // devuelve bandera a su sitio
        GameManager.Instance?.FlagReturnedToBase();
    }

    public bool IsCarried() => isCarried; // devuelve si está siendo cargada
    public Transform GetCarrier() => carrier; // devuelve quién la lleva
}