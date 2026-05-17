using UnityEngine;

public class PlayerBase : MonoBehaviour // trigger en la base del jugador, detecta bandera
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("PlayerBase tocado por: " + other.gameObject.name + " tag=" + other.tag);

        if (!other.CompareTag("Player")) return;

        // busca si el jugador lleva bandera enemiga team id = 1
        Flag[] flags = FindObjectsByType<Flag>(FindObjectsSortMode.None);
        foreach (Flag flag in flags)
        {
            if (flag.teamID == 1 && flag.IsCarried() && flag.GetCarrier() == other.transform)
            {
                Debug.Log("Jugador entrego la bandera en su base! PUNTO!");
                flag.Capture(); // da el punto 
                return;
            }
        }
    }
}