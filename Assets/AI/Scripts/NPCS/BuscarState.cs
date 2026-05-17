using UnityEngine;
using UnityEngine.AI;


public class BuscarState : StateBuscador
{
    // variables utilizadas para evitar q el NPC se quede trabado
    private float exitBaseTimer = 0f; // t que lleva intentando salir de la base
    private Vector3 lastPosition; // pos del frame anterior
    private float stuckTimer = 0f; // t que lleva sin moverse
    private float destinationTimer = 0f; // temporizador para reenviar destino (cada 0.5s)
    private const float STUCK_TIME = 2f;
    private const float DEST_REFRESH = 0.5f;

    public BuscarState(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false; // asegura q el navmesh agent esté activo
        npc.foundFlag = false; // resetea al booleano de bandera encontrada
        npc.agent.ResetPath(); // borra las rutas anteriores
        // reset de timers:
        exitBaseTimer = 0f;
        stuckTimer = 0f;
        destinationTimer = 0f;
        lastPosition = npc.transform.position;
        Debug.Log(npc.npcName + " → BuscarState");
    }

    public override void Update()
    {
        if (npc.enemyFlag == null) return; // si no hay bandera enemira, no hace nada

        // TRANSICIONES:

        // si jugador está a menos de dos metros, huye
        if (npc.DistanceToPlayer() < 2f) 
        { npc.ChangeState(new Huir2State(npc)); return; }

        // si mi equipo tiene la bandera, protegerlo
        if (npc.enemyFlagSignal && !npc.FlagRecovered())
        {
            
            if (npc.IsEnemyFlagCarriedByAlly())
                npc.ChangeState(new ProtegerAbanderadoState(npc));
            else
                npc.ChangeState(new DefenderState(npc));
            return;
        }

        // si enemigo tieme nuestra bandera, defenderla
        if (npc.enemyFlagSignal && !npc.FlagRecovered())
        { npc.ChangeState(new DefenderState(npc)); return; }

        // si recogió la bandera, correr a la base
        if (npc.foundFlag)
        { npc.ChangeState(new CorrerBaseState(npc)); return; }

        // salir de la base si está adentro
        if (npc.ownBase != null)
        {
            float distToBase = Vector3.Distance(npc.transform.position, npc.ownBase.position);
            if (distToBase < npc.baseDeliverRadius + 1f)
            {
                exitBaseTimer += Time.deltaTime;
                Vector3 awayDir = (npc.transform.position - npc.ownBase.position).normalized;
                if (awayDir == Vector3.zero) awayDir = npc.transform.forward;
                if (NavMesh.SamplePosition(npc.transform.position + awayDir * 5f, out NavMeshHit eh, 6f, NavMesh.AllAreas))
                    npc.agent.SetDestination(eh.position);
                return;
            }
        }
        exitBaseTimer = 0f;

        // anti-atasco con recálculo de path 
        float moved = Vector3.Distance(npc.transform.position, lastPosition);
        if (moved < 0.08f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= STUCK_TIME)
            {
                stuckTimer = 0f;
                // recalcular el path 
                npc.agent.ResetPath();
                Vector3 flagDir = (npc.enemyFlag.transform.position - npc.transform.position).normalized;
                Vector3 sideOffset = new Vector3(-flagDir.z, 0f, flagDir.x) * Random.Range(-3f, 3f);
                Vector3 intermediate = npc.transform.position + flagDir * 4f + sideOffset;
                if (NavMesh.SamplePosition(intermediate, out NavMeshHit ih, 5f, NavMesh.AllAreas))
                    npc.agent.SetDestination(ih.position);
            }
        }
        else stuckTimer = 0f;
        lastPosition = npc.transform.position;

        // reenviar destino cada 0.5s
        destinationTimer += Time.deltaTime;
        if (destinationTimer >= DEST_REFRESH || !npc.agent.hasPath ||
            npc.agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            destinationTimer = 0f;
            Vector3 flagPos = npc.enemyFlag.transform.position;
            Vector3 toFlag = flagPos - npc.transform.position; toFlag.y = 0f;
            Vector3 dest = toFlag.magnitude > 1.5f ? flagPos - toFlag.normalized * 1f : flagPos;
            if (NavMesh.SamplePosition(dest, out NavMeshHit fHit, 5f, NavMesh.AllAreas))
                npc.agent.SetDestination(fHit.position);
            else
                npc.agent.SetDestination(flagPos);
        }

        if (npc.agent.isStopped) npc.agent.isStopped = false;
    }

    public override void Exit() { }
}