using UnityEngine;

public class CorrerBaseState : StateBuscador
{
    public CorrerBaseState(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        if (npc.LifeRemaining() == 0)
        {
            npc.ChangeState(new Morir2State(npc));
        }
        else if (npc.LifeRemaining() == 0)
        {
            npc.ChangeState(new Morir2State(npc));
        }


    }

    public override void Exit() { }
}
