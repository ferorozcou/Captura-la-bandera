using UnityEngine;

public class BuscarState : StateBuscador
{
    public BuscarState(BuscadorController npc) : base(npc) { }

    public override void Enter() { npc.agent.isStopped = false; }

    public override void Update()
    {
        if (npc.CheckVision())
        {
            // Si lo ve, lo persigue
            npc.agent.destination = npc.player.position;
        }
        else if (npc.hasLastKnownPosition)
        {
            // Si NO lo ve pero recuerda dónde estaba, va hacia allá
            npc.agent.destination = npc.lastKnownPosition;

            if (Vector3.Distance(npc.transform.position, npc.lastKnownPosition) < 1.5f)
            {
                npc.hasLastKnownPosition = false; // Llegó y no hay nadie
            }
        }
        else
        {
            // Si no sabe nada, patrulla o vuelve a base
            // npc.agent.destination = basePos;
        }
    }

    public override void Exit() { }
}

