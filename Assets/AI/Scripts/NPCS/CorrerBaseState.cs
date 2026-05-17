using UnityEngine;
using UnityEngine.AI;

public class CorrerBaseState : StateBuscador // cuando tienes la bandera, regresar inmediatamente a la base
{
    public CorrerBaseState(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false; // aseguramos que se pueda mover. el movimiento real se gestiona a través de returnto basecoroutine() en buscadorcontroller.
        npc.agent.ResetPath();

        if (NavMesh.SamplePosition(npc.transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            if (Vector3.Distance(npc.transform.position, hit.position) > 0.3f)
                npc.agent.Warp(hit.position);

        if (npc.ownBase == null)
            Debug.LogError(npc.npcName + ": ownBase es NULL en CorrerBaseState!");
        else
            Debug.Log(npc.npcName + " ? CorrerBaseState");
    }

    public override void Update()
    {
        // si perdió la bandera(murió), volver a buscar
        if (!npc.foundFlag)
        {
            npc.ChangeState(new BuscarState(npc));
        }
    }

    public override void Exit() { }
}