using UnityEngine;


public class EnemyBase : MonoBehaviour // trigger base enemigao
{
    void Start()
    {
        Collider col = GetComponent<Collider>(); // comprueba collider y trigger
        if (col == null)
            Debug.LogError("EnemyBase no tiene Collider!");
        else if (!col.isTrigger)
            Debug.LogWarning("EnemyBase Collider no es Trigger!");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("EnemyBase tocado por: " + other.gameObject.name);

        BuscadorController buscador = other.GetComponent<BuscadorController>();
        if (buscador == null) return;
        if (!buscador.foundFlag) return;

        // punto enemigo
        Debug.Log("Buscador entrego la bandera en su base! PUNTO ENEMIGO!");

        // devolver la bandera del jugador a su base
        Flag[] flags = FindObjectsByType<Flag>(FindObjectsSortMode.None);
        foreach (Flag flag in flags)
        {
            if (flag.teamID == 0 && flag.IsCarried() && flag.GetCarrier() == buscador.transform)
            {
                flag.Capture();
                buscador.foundFlag = false;
                return;
            }
        }
    }
}