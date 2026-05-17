using UnityEngine;


public class NPCTeamActivator : MonoBehaviour
{
    [Header("NPC extra para cada equipo")]
    [Tooltip("Buscador ROJO extra — se activa si el jugador elige Azul")]
    public GameObject extraBuscadorRojo;

    [Tooltip("Buscador AZUL extra — se activa si el jugador elige Rojo")]
    public GameObject extraBuscadorAzul;

    void Awake()
    {
        int playerTeam = TeamSelector.GetTeam(); 

        if (playerTeam == 0) // jugador es equipo rojo
        {
            // activar buscador azul extra, desactivar rojo extra
            if (extraBuscadorAzul != null) extraBuscadorAzul.SetActive(true);
            if (extraBuscadorRojo != null) extraBuscadorRojo.SetActive(false);
            Debug.Log("NPCTeamActivator: activado Buscador AZUL extra.");
        }
        else // jugador es equipo azul
        {
            //  activar buscador rojo extra, desactivar azul extra
            if (extraBuscadorRojo != null) extraBuscadorRojo.SetActive(true);
            if (extraBuscadorAzul != null) extraBuscadorAzul.SetActive(false);
            Debug.Log("NPCTeamActivator: activado Buscador ROJO extra.");
        }
    }
}