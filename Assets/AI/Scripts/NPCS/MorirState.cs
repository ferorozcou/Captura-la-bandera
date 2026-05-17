using UnityEngine;

public class MorirState : StateDefensor // morir
{
    public MorirState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = true; // para NPC
    }
    public override void Update() { } // no hace nada
    public override void Exit() { }
}