using UnityEngine;
using UnityEngine.AI;

public abstract class BaseNPCController : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Percepcion y Vision")]
    public Transform player;
    public float viewDistance = 15f;
    public float viewAngle = 100f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Memoria")]
    public Vector3 lastKnownPosition;
    public bool hasLastKnownPosition = false;

    [Header("Stats")]
    public int health = 100;
   
    [Header("Configuración de Juego")]
    public int teamID; // 1 o 2
    public Flag enemyFlag;
    public Flag alliedFlag;


    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Si olvidaste poner el componente, esto te avisará con un mensaje claro
        if (agent == null)
        {
            Debug.LogError("¡Falta el NavMeshAgent en " + gameObject.name + "! Por favor, añádelo en el Inspector.");
        }
    }


    protected virtual void Update()
    {
        Perceive();
        Think();
        Act();
    }

    protected virtual void Perceive()
    {
        if (CheckVision())
        {
            lastKnownPosition = player.position;
            hasLastKnownPosition = true;
        }
    }

    protected abstract void Think();
    protected abstract void Act();

    public bool CheckVision()
    {
        if (player == null) return false;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > viewDistance) return false;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer < viewAngle / 2f)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distanceToPlayer, obstacleLayer))
                return true;
        }
        return false;
    }

    // ESTA ES LA FUNCIÓN QUE TE FALTA O NO RECONOCE:
    public float DistanceToPlayer()
    {
        if (player == null) return 1000f;
        return Vector3.Distance(transform.position, player.position);
    }
}

