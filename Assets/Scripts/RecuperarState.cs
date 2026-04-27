using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecuperarState : StateDefensor
{
    public RecuperarState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        // Si pierde al jugador → buscar
        if (npc.LifeRemaining() == 0)
        {
            npc.ChangeState(new MorirState(npc));
        }
    }

    public override void Exit() { }
}
