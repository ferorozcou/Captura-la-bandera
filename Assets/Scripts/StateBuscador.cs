using UnityEngine;

public abstract class StateBuscador
{
    protected BuscadorController npc;

    public StateBuscador(BuscadorController npc)
    {
        this.npc = npc;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();


}

