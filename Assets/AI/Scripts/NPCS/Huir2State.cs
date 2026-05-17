using UnityEngine;
using UnityEngine.AI;

public class Huir2State : StateBuscador
{
    private float refreshTimer = 0f; // temporizador para recalcular dirección de huida
    private const float REFRESH = 0.3f; // cada 0.3s recalcula hacia dónde huir

    public Huir2State(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        npc.agent.ResetPath();
        refreshTimer = 0f;
        Debug.Log(npc.npcName + " ? Huir2State");
    }

    public override void Update()
    {
        // reenviar destino de huida cada 0.3s para que siempre huya del jugador actual
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= REFRESH || !npc.agent.hasPath)
        {
            refreshTimer = 0f;
            if (npc.player != null)
            { // calculo de dirección opuesta del jugador
                Vector3 awayDir = (npc.transform.position - npc.player.position).normalized;
                // punto de huida: 8 unidades en esa dirección
                Vector3 fleeTarget = npc.transform.position + awayDir * 8f;
                if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 6f, NavMesh.AllAreas))
                    npc.agent.SetDestination(hit.position);
            }
        }

        if (npc.agent.isStopped) npc.agent.isStopped = false;

        // TRANSICIONES 
        // si jugador ya está lejos, volver a buscar state
        if (npc.DistanceToPlayer() > 3.5f)
            npc.ChangeState(new BuscarState(npc));
    }

    public override void Exit() { }
}