using UnityEngine;
using UnityEngine.AI;

public class BuscarJugadorState : StateDefensor // va al último punto donde vió al jugador
{
    private float searchTimer = 0f; // tiempo buscando
    private const float MAX_TIME = 5f; // máximo 5s buscando antes de volver a patrullar
    private bool reachedPoint = false;
    private float circleTimer = 0f;

    public BuscarJugadorState(DefensorController npc) : base(npc) { }

    public override void Enter()
    {
        npc.agent.isStopped = false;
        searchTimer = 0f;
        reachedPoint = false;
        circleTimer = 0f;

        // va a la última posición conocida
        if (npc.lastKnownPlayerPosition != Vector3.zero)
            npc.agent.SetDestination(npc.lastKnownPlayerPosition);
        else
            npc.ChangeState(new PatrullarState(npc)); // si no hay punto, patrulla directamente
    }

    public override void Update()
    {
        searchTimer += Time.deltaTime;

        // si ve al jugador, ataca
        if (npc.LineOfSigthToPlayer() && npc.DistanceToOpponent() < npc.visionRange)
        { npc.ChangeState(new AtacarState(npc)); return; }

        // si pasan 5 seg sin encontrar al jugador, patrulla
        if (searchTimer >= MAX_TIME)
        { npc.ChangeState(new PatrullarState(npc)); return; }

        float dist = Vector3.Distance(npc.transform.position, npc.lastKnownPlayerPosition);
        if (dist < 1.5f)
        {
            reachedPoint = true;
            // en lugar de pararse, caminar en pequeños círculos bbuscando al jugador
            circleTimer += Time.deltaTime;
            float angle = circleTimer * 90f * Mathf.Deg2Rad; 
            Vector3 circleOffset = new Vector3(Mathf.Cos(angle) * 2f, 0f, Mathf.Sin(angle) * 2f);
            Vector3 circleTarget = npc.lastKnownPlayerPosition + circleOffset;
            if (NavMesh.SamplePosition(circleTarget, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                npc.agent.SetDestination(hit.position);
            else
                npc.agent.SetDestination(npc.lastKnownPlayerPosition);
        }
    }

    public override void Exit()
    {
        npc.agent.isStopped = false;
    }
}