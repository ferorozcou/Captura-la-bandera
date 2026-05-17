using UnityEngine;

public class Morir2State : StateBuscador
{
    public Morir2State(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = true;
    }
    public override void Update() { }
    public override void Exit() { }
}