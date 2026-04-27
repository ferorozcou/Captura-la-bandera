using UnityEngine;

public class Huir2State : StateBuscador
{
    public Huir2State(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        if(npc.LineOfSigthToPlayer() == false)
        {
            npc.ChangeState(new BuscarState(npc));
        }
        else if (npc.TeamFoundFlag() == true)
        {
            npc.ChangeState(new ProtegerAbanderadoState(npc));
        }
        else if(npc.LifeRemaining() == 0)
        {
            npc.ChangeState(new Morir2State(npc));
        }

    }

    public override void Exit() { }
}
