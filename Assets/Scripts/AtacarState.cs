using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AtacarState : StateDefensor
{
    public AtacarState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        // Si pierde al jugador ? buscar
        if (npc.DistanceToOpponent() > 3 && npc.LineOfSigthToPlayer() == false)
        {
            npc.ChangeState(new PatrullarState(npc));
        }
        else if (npc.LifeRemaining() < 40){
            npc.ChangeState(new HuirState(npc));
        }
        else if (npc.LifeRemaining() == 0)
        {
            npc.ChangeState(new MorirState(npc));
        }

    }

    public override void Exit() { }
}
