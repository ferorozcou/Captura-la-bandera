using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuirState : StateDefensor
{
    public HuirState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        // Si pierde al jugador ? buscar
        if (npc.LifeRemaining() > 70)
        {
            npc.ChangeState(new PatrullarState(npc));
        }
        else if (npc.FlagStolen() == true)
        {
                npc.ChangeState(new RecuperarState(npc));
        }
        else if (npc.LifeRemaining() == 0)
        {
            npc.ChangeState(new MorirState(npc));
        }
    }

    public override void Exit() { }
}
