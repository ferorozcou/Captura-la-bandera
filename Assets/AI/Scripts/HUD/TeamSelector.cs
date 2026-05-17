using UnityEngine;


public class TeamSelector : MonoBehaviour // singleton que se encarga de guardar el equipo elegido
{
    public static TeamSelector Instance;

    [HideInInspector] public int selectedTeam = 0; // 0=crojo, 1=azul

    void Awake()
    {
        if (Instance != null) // solo existe 1
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public static int GetTeam()
    {
        if (Instance != null) return Instance.selectedTeam; 
        return PlayerPrefs.GetInt("SelectedTeam", 0);
    }
}