using UnityEngine;

public class RecuperarState : StateDefensor // persigue a quien lleva su bandera
{
    public RecuperarState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        npc.agent.ResetPath();
        Debug.Log(npc.npcName + " → RecuperarState (persiguiendo al que tiene nuestra bandera)");
    }

    public override void Update()
    {
        //TRANSICIONES

        // si bandera recuperada, volver a patrullar
        if (!npc.FlagStolen())
        {
            npc.ChangeState(new PatrullarState(npc));
            return;
        }

        // acción de perseguir al portador de nuestra bandera
        if (npc.flag != null)
        {
            Flag ownFlagComp = npc.flag.GetComponent<Flag>();
            if (ownFlagComp != null && ownFlagComp.IsCarried())
            {
                Transform carrier = ownFlagComp.GetCarrier();
                if (carrier != null)
                {
                    npc.agent.SetDestination(carrier.position);
                    return;
                }
            }
        }

        // si no encontramos al portador, ir hacia donde está la bandera en el suelo
        if (npc.flag != null)
            npc.agent.SetDestination(npc.flag.transform.position);
    }

    public override void Exit() { }
}