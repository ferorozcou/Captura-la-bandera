
using UnityEngine;

public class ProtegerState : StateDefensor //se interpone entre el jugador y la bandera
{
    public ProtegerState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        // bloquea al jugador
        if (npc.player != null)
            npc.agent.destination = npc.player.position;

        // TRANSICIONES 

        // si el enemigo ya está lejos de la bandera, patrullar
        if (npc.DistanceOpponentFlag() > 3f)
        {
            npc.ChangeState(new PatrullarState(npc));
            return;
        }

        // si bandera robada, recuperar
        if (npc.FlagStolen())
        {
            npc.ChangeState(new RecuperarState(npc));
            return;
        }
    }

    public override void Exit() { }
}