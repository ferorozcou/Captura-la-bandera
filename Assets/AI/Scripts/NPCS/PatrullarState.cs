using UnityEngine;
using UnityEngine.AI;

public class PatrullarState : StateDefensor // defensor recorre waypoints alrededor de su bandera
{
    private int waypointIndex = 0; // indice waypoint actual
    private Vector3 lastPosition; // por si hay atasco
    private float stuckTimer = 0f;
    private const float STUCK_TIME = 2f;
    private const float ARRIVE_DIST = 1.5f; // distancia para considerar que llegó al waypoint

    public PatrullarState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        stuckTimer = 0f;
        lastPosition = npc.transform.position;

        if (npc.waypoints == null || npc.waypoints.Length == 0)
        {
            Debug.LogWarning(npc.npcName + ": Sin waypoints.");
            return;
        }

        waypointIndex = Random.Range(0, npc.waypoints.Length); // empieza en waypoint aleatorio
        SetDestination();
    }

    public override void Update()
    {
        // TRANSICIONES

        // si bandera robada, recuperar state
        if (npc.FlagStolen())
        { npc.ChangeState(new RecuperarState(npc)); return; }

        // si el enemigo está muy cerca de la bandera y tenemos línea de visión, proteger
        if (npc.DistanceOpponentFlag() < 3f && npc.LineOfSigthToPlayer())
        {
            npc.ChangeState(new ProtegerState(npc));
            return;
        }
            // si detecta a enemigo en la zona y tiene vida suficiente, atacar
            if (npc.IsEnemyOfPlayer)
        {
            bool detect = npc.LineOfSigthToPlayer() || npc.DistanceToOpponent() < npc.listenRadius;
            bool inZone = IsPlayerInZone(); // si enemigo dentro del area de patrulla
            bool healthy = npc.LifeRemaining() > 5f; // si tiene suficiente vida
            if (detect && inZone && healthy)
            {
                if (npc.player != null) npc.lastKnownPlayerPosition = npc.player.position;
                npc.ChangeState(new AtacarState(npc));
                return;
            }
        }


        // si no hay waypoints, va hacia la bandera
        if (npc.waypoints == null || npc.waypoints.Length == 0)
        {
            if (npc.flag != null) npc.agent.SetDestination(npc.flag.position);
            return;
        }

        // ir al sig. waypoint
        float dist = Vector3.Distance(npc.transform.position, npc.waypoints[waypointIndex].position);
        if (dist < ARRIVE_DIST)
        {
            NextWaypoint();
            return;
        }

        // path invalido, sig. waypoint
        float moved = Vector3.Distance(npc.transform.position, lastPosition);
        if (moved < 0.08f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= STUCK_TIME) { NextWaypoint(); return; }
        }
        else stuckTimer = 0f;

        lastPosition = npc.transform.position;

        // path que no funciona
        if (!npc.agent.pathPending && npc.agent.pathStatus == NavMeshPathStatus.PathInvalid)
            NextWaypoint();
    }

    void NextWaypoint()
    {
        if (npc.waypoints.Length == 1) { waypointIndex = 0; SetDestination(); return; }
        int next;
        do { next = Random.Range(0, npc.waypoints.Length); } while (next == waypointIndex);
        waypointIndex = next;
        SetDestination();
    }

    void SetDestination()
    {
        if (npc.waypoints == null || waypointIndex >= npc.waypoints.Length) return;
        Vector3 target = npc.waypoints[waypointIndex].position;
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            target = hit.position;
        npc.agent.isStopped = false;
        npc.agent.SetDestination(target);
        stuckTimer = 0f;
        lastPosition = npc.transform.position;
    }

    bool IsPlayerInZone()
    {
        if (npc.player == null) return false;
        Vector3 center = npc.waypoints != null && npc.waypoints.Length > 0
            ? npc.waypoints[0].position
            : (npc.flag != null ? npc.flag.position : npc.transform.position);
        return Vector3.Distance(npc.player.position, center) < npc.patrolRadius;
    }

    public override void Exit() { npc.agent.isStopped = false; }
}