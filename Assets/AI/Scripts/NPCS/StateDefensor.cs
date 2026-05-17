using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateDefensor // Tiene a misma funcionalidad que state buscador: Ahí ya está explicado :)
{
    protected DefensorController npc; 

    public StateDefensor(DefensorController npc)
    {
        this.npc = npc;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();


}
