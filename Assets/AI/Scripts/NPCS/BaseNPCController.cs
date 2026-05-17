using UnityEngine;
using UnityEngine.AI;

public abstract class BaseNPCController : MonoBehaviour // clase base abstracta
{
    public NavMeshAgent agent;

    [Header("Vision")]
    public Transform player;
    public float viewDistance = 15f;
    public float viewAngle = 100f;
    public LayerMask obstacleLayer; // capas que bloquean visión

    [Header("Memoria")]
    public Vector3 lastKnownPosition;
    public bool hasLastKnownPosition = false; // si hay pos conocida guardada

    [Header("Stats")]
    public int health = 100;

    [Header("Configuración de Juego")]
    public int teamID;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // obtiene el componente de navegación

        if (agent == null)
        {
            Debug.LogError("Falta NavMeshAgent en " + gameObject.name);
        }
    }

    protected virtual void Update()
    {
        Perceive(); // actualiza lo que percibe
        Think(); // decide qué hacer
        Act(); // ejecuta
    }

    protected virtual void Perceive()
    {
        if (CheckVision()) // si ve al jugador, guarda su pos
        {
            lastKnownPosition = player.position;
            hasLastKnownPosition = true;
        }
    }

    public bool CheckVision()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position); // calcula dirección y distancia del jugador

        if (distance > viewDistance) return false; // si supera vie distance

        float angle = Vector3.Angle(transform.forward, dirToPlayer); 

        if (angle < viewAngle / 2f) // si angulo mayor a viewangle/2, no ve, así que false
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distance, obstacleLayer))
                return true;
        }

        return false;
    }

    public float DistanceToPlayer() 
    {
        if (player == null) return 999f;
        return Vector3.Distance(transform.position, player.position);
    }

    protected abstract void Think();
    protected abstract void Act();
}