using UnityEngine;
using UnityEngine.AI;

public class BuscadorController : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Patrullar")]
    public Transform[] patrolPoints;
    public int currentPatrolIndex;

    [Header("Detection")]
    public Transform player;
    public float detectionRange = 10f;
    public float loseRange = 15f;

    private StateBuscador currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ChangeState(new BuscarState(this));
    }

    void Update()
    {
        currentState.Update();
    }

    public void ChangeState(StateBuscador newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public float DistanceToOpponent()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    public bool LineOfSigthToPlayer()
    {
        //RaycastHit hit;
        //Vector3 direction = (player.position - transform.position).normalized;
        //if (Physics.Raycast(transform.position, direction, out hit, detectionRange))
        //{
        //    return hit.transform == player;
        //}
        return false;
    }

    public int LifeRemaining()
    {
        // Aquí deberías implementar la lógica para obtener la vida restante del NPC
        return 100; // Placeholder
    }
     public bool FoundFlag()
    {
        return false;
    }

    public bool TeamFoundFlag()
    {
        return false;
    }

    public bool FlagStolen()
    {
        // Aquí deberías implementar la lógica para determinar si el flag del oponente ha sido robado
        return false; // Placeholder
    }


}
