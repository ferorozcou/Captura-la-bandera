using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtegerState : StateDefensor
{
    public ProtegerState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        // Si pierde al jugador → buscar
        if (npc.DistanceOpponentFlag() > 3)
        {
            npc.ChangeState(new PatrullarState(npc));
        }
        if (npc.FlagStolen() == true)
        {
            npc.ChangeState(new RecuperarState(npc));
        }
        if (npc.LifeRemaining() == 0)
        {
            npc.ChangeState(new MorirState(npc));
        }
    }

    public override void Exit() { }
}
