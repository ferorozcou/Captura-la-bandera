using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour // para gestionar los botones del menu principal
{
    public const string TEAM_KEY = "SelectedTeam";

    public static int selectedTeam => TeamSelector.GetTeam();

    [Header("Pantallas")]
    public GameObject mainScreen;
    public GameObject teamSelectScreen;

    [Header("Nombre de la escena del juego")]
    public string gameSceneName = "AI";

    void Start()
    {
        if (TeamSelector.Instance == null) // crea un team selector si no hay uno ya
        {
            GameObject ts = new GameObject("TeamSelector");
            ts.AddComponent<TeamSelector>();
        }

        if (mainScreen != null) mainScreen.SetActive(true); // muestra pantalla principal
        if (teamSelectScreen != null) teamSelectScreen.SetActive(false); // oculta pantalla seleccion equipo
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    public void OnPlayButton() // boton jugar
    {
        if (mainScreen != null) mainScreen.SetActive(false);
        if (teamSelectScreen != null) teamSelectScreen.SetActive(true);
    }

    public void SelectRed() // boton selección equipo rojjo
    {
        EnsureTeamSelector();
        TeamSelector.Instance.selectedTeam = 0;
        PlayerPrefs.SetInt(TEAM_KEY, 0);
        PlayerPrefs.Save();
        Debug.Log("Equipo elegido: ROJO (0)");
        SceneManager.LoadScene(gameSceneName);
    }

    public void SelectBlue() // selecciñon quipo azul
    {
        EnsureTeamSelector();
        TeamSelector.Instance.selectedTeam = 1;
        PlayerPrefs.SetInt(TEAM_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("Equipo elegido: AZUL (1)");
        SceneManager.LoadScene(gameSceneName);
    }

    void EnsureTeamSelector() 
    {
        if (TeamSelector.Instance == null)
        {
            GameObject ts = new GameObject("TeamSelector");
            ts.AddComponent<TeamSelector>();
        }
    }

    public void OnQuitButton() => Application.Quit(); //salir
}