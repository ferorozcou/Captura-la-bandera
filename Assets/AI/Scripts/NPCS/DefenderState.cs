using UnityEngine;
using UnityEngine.AI;

public class DefenderState : StateBuscador // buscador defiende la bandera propia cuando el enemigo la tiene
{
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float destinationTimer = 0f;
    private const float STUCK_TIME = 2f;
    private const float DEST_REFRESH = 0.5f;

    public DefenderState(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        npc.agent.ResetPath();
        stuckTimer = 0f; destinationTimer = 0f;
        lastPosition = npc.transform.position;
        Debug.Log(npc.npcName + " ? DefenderState");
    }

    public override void Update()
    {
        // TRANSICIONES

        // si la bandera fue recuperada o no se tiene la bandera, volvemos a buscar
        if (npc.FlagRecovered() || !npc.enemyFlagSignal)
        { npc.ChangeState(new BuscarState(npc)); return; }

        // decide a quién perseguir
        Transform target = null;
        if (npc.ownFlag != null && npc.ownFlag.IsCarried()) // persigue a quien tiene la bandera
            target = npc.ownFlag.GetCarrier();
        if (target == null && npc.ownFlag != null) // va hacia donde está la bandera
            target = npc.ownFlag.transform;

        if (target == null)
        {

            if (npc.ownBase != null)
            {
                destinationTimer += Time.deltaTime;
                if (destinationTimer >= DEST_REFRESH)
                {
                    destinationTimer = 0f;
                    if (NavMesh.SamplePosition(npc.ownBase.position, out NavMeshHit bHit, 5f, NavMesh.AllAreas))
                        npc.agent.SetDestination(bHit.position);
                }
            }
            return;
        }

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

        // reenvía destino 
        destinationTimer += Time.deltaTime;
        if (destinationTimer >= DEST_REFRESH || !npc.agent.hasPath ||
            npc.agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            destinationTimer = 0f;
            if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                npc.agent.SetDestination(hit.position);
            else
                npc.agent.SetDestination(target.position);
        }

        if (npc.agent.isStopped) npc.agent.isStopped = false;
    }

    public override void Exit() { npc.agent.isStopped = false; }
}