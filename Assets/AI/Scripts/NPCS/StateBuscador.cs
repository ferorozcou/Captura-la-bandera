using UnityEngine;

public abstract class StateBuscador
{
    protected BuscadorController npc; // lo usamos para guardar referencia al NPC dueó del estado

    public StateBuscador(BuscadorController npc) // constructor
    {
        this.npc = npc;
    }

    public abstract void Enter(); // se ejecuta al entrar al estado
    public abstract void Update(); // se ejecuta cada frame 
    public abstract void Exit(); // se ejecuta al salir del estado


}

