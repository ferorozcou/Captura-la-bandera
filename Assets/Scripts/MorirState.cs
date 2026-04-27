using UnityEngine;

public class MorirState : StateDefensor
{
    public MorirState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        // Cambiar ubi a la base cuando muere, subir vida al 100 
       
        npc.ChangeState(new PatrullarState(npc));
        

    }

    public override void Exit() { }
}
