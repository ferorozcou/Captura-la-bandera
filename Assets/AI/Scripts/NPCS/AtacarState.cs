using UnityEngine;

public class AtacarState : StateDefensor // defensor persigue y ataca
{
    public AtacarState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        Debug.Log(npc.npcName + " ? AtacarState");
    }

    public override void Update()
    {
        // acción de perseguir al jugador
        if (npc.player != null)
            npc.agent.SetDestination(npc.player.position);

        // TRANSICIONES

        // si vida baja, huir
        if (npc.LifeRemaining() < 4f)
        {
            npc.ChangeState(new HuirState(npc));
            return;
        }

        // si pierde al jugador y está lejos, volver a patrullar
        if (!npc.LineOfSigthToPlayer() && npc.DistanceToOpponent() > 3f)
        {
            npc.ChangeState(new PatrullarState(npc));
            return;
        }

        // si el enemigo se alejó del área, volver a patrullar
        if (DistanceFromPatrolCenter() > npc.patrolRadius * 1.5f)
        {
            npc.ChangeState(new PatrullarState(npc));
            return;
        }
    }

    float DistanceFromPatrolCenter()
    {
        // calcula distancia desde la posición actual hasta el centro de patrulla
        Vector3 center;
        if (npc.waypoints != null && npc.waypoints.Length > 0)
            center = npc.waypoints[0].position;
        else if (npc.flag != null)
            center = npc.flag.position;
        else return 0f;

        return Vector3.Distance(npc.transform.position, center);
    }

    public override void Exit() { }
}