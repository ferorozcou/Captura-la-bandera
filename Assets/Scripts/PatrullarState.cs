using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrullarState : StateDefensor
{
    public PatrullarState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        
        if (npc.DistanceToOpponent() < 3 ||npc.LineOfSigthToPlayer()==true)//y dentro del area de patrullaje
        {
            if (npc.LifeRemaining() < 50)
            {
                npc.ChangeState(new AtacarState(npc));
            }
        }
        else if(npc.DistanceOpponentFlag() < 3 && npc.LineOfSigthToPlayer()==true)
        {
            npc.ChangeState(new ProtegerState(npc));
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
