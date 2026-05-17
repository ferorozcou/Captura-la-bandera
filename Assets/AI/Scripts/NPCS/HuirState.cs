using UnityEngine;
using UnityEngine.AI;

public class HuirState : StateDefensor // si la vida está baja, huye
{
    public HuirState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        npc.agent.ResetPath();
        Debug.Log(npc.npcName + " ? HuirState");
    }

    public override void Update()
    {
        // huir en dirección contraria al jugador 
        if (npc.player != null)
        {
            Vector3 awayDir = (npc.transform.position - npc.player.position).normalized;
            Vector3 fleeTarget = npc.transform.position + awayDir * 8f;
            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                npc.agent.SetDestination(hit.position);
        }

        // TRANSCICIONES 

        // si se han robado su bandera, recuperar
        if (npc.FlagStolen())
        {
            npc.ChangeState(new RecuperarState(npc));
            return;
        }


        // si vida se ha recuperado, volver a patrullar
        if (npc.LifeRemaining() > 7f)
        {
            npc.ChangeState(new PatrullarState(npc));
            return;
        }
    }

    public override void Exit() { }
}