using UnityEngine;
using UnityEngine.AI;

public class ProtegerAbanderadoState : StateBuscador // ayuda a quien lleva la bandera del equipo enemigo
{
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float destinationTimer = 0f;
    private const float STUCK_TIME = 2f;
    private const float DEST_REFRESH = 0.5f;

    public ProtegerAbanderadoState(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        npc.agent.ResetPath();
        stuckTimer = 0f; destinationTimer = 0f;
        lastPosition = npc.transform.position;
        Debug.Log(npc.npcName + " → ProtegerAbanderadoState");
    }

    public override void Update()
    {
        // TRANSICIONES

        // salir del estado si ya nadie aliado lleva la bandera enemiga
        if (!npc.IsEnemyFlagCarriedByAlly())
        { npc.ChangeState(new BuscarState(npc)); return; }

        // obtención de quien lleva la bandera que hay que robar
        Transform carrier = null;
        if (npc.flagToSteal != null && npc.flagToSteal.IsCarried())
            carrier = npc.flagToSteal.GetCarrier();

        // si no hay portador, buscar la bandera
        if (carrier == null || carrier == npc.transform) 
        { npc.ChangeState(new BuscarState(npc)); return; }

        // antiatasco
        float moved = Vector3.Distance(npc.transform.position, lastPosition);
        if (moved < 0.08f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= STUCK_TIME)
            {
                stuckTimer = 0f;
                npc.agent.ResetPath();
            }
        }
        else stuckTimer = 0f;
        lastPosition = npc.transform.position;

        destinationTimer += Time.deltaTime;
        if (destinationTimer >= DEST_REFRESH || !npc.agent.hasPath ||
            npc.agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            destinationTimer = 0f;
            Vector3 dir = (npc.transform.position - carrier.position).normalized;
            if (dir == Vector3.zero) dir = npc.transform.forward;
            Vector3 followPos = carrier.position + dir * 2f; // se posiciona 2 unidades detrás del portador aliado
            if (NavMesh.SamplePosition(followPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                npc.agent.SetDestination(hit.position);
            else
                npc.agent.SetDestination(carrier.position);
        }

        if (npc.agent.isStopped) npc.agent.isStopped = false;
    }

    public override void Exit() { npc.agent.isStopped = false; }
}