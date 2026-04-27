using UnityEngine;
using UnityEngine.AI;

public class BuscadorController : BaseNPCController
{
    [Header("Patrullar")]
    public Transform[] patrolPoints;
    public int currentPatrolIndex;

    private StateBuscador currentState;

    protected override void Start()
    {
        base.Start();
        ChangeState(new BuscarState(this));
    }

    protected override void Think()
    {
        // La lógica de transición se mantiene en los estados por ahora
    }

    protected override void Act()
    {
        if (currentState != null)
            currentState.Update();
    }

    public void ChangeState(StateBuscador newState)
    {
        if (currentState != null)
            currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // --- FUNCIONES PARA CORREGIR LOS ERRORES DE LA CONSOLA ---


    // Este corrige el error en ProtegerAbanderadoState y Huir2State
    public bool TeamFoundFlag() => false;

    // Este corrige el error en DefenderState
    public bool FoundFlag()
    {
        // Si la bandera enemiga está cerca y no ha sido capturada
        if (enemyFlag != null && Vector3.Distance(transform.position, enemyFlag.transform.position) < 2f)
        {
            return true;
        }
        return false;
    }

    public bool FlagStolen()
    {
        // Si nuestra bandera aliada ha sido capturada por alguien
        return alliedFlag != null && alliedFlag.isCaptured;
    }
    public int LifeRemaining() => health;

    // Estos ayudan a la compatibilidad con nombres antiguos
    public float DistanceToOpponent() => base.DistanceToPlayer();
    public bool LineOfSigthToPlayer() => base.CheckVision();
}

