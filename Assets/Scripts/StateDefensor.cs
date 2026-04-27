using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateDefensor
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
