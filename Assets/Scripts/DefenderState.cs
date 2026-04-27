using UnityEngine;

public class DefenderState : StateBuscador
{
    public DefenderState(BuscadorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
    }

    public override void Update()
    {
        npc.agent.destination = npc.player.position;

        // Si pierde al jugador ? buscar
        if (npc.FlagStolen() ==true)
        {
            npc.ChangeState(new BuscarState(npc));
        }
       
    }

    public override void Exit() { }

}
