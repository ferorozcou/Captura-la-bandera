using UnityEngine;

public class BuscarState : StateBuscador
{
    public BuscarState(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        // Si pierde al jugador → buscar
        if (npc.DistanceToOpponent() < 2)
        {
            npc.ChangeState(new Huir2State(npc));
        }
        else if (npc.FoundFlag() == true)
        {
            npc.ChangeState(new CorrerBaseState(npc));
        }
        else if (npc.TeamFoundFlag()==true)
        {
            npc.ChangeState(new ProtegerAbanderadoState(npc));
        }
        else if(npc.FlagStolen()==true)
        {
            npc.ChangeState(new DefenderState(npc));
        }
    }

    public override void Exit() { }

}
