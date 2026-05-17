using UnityEngine;
using UnityEngine.UI;

// singleton que gestiona el estado global del juego
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [Header("Puntuacion")]
    public int scoreJugador = 0;
    public int scoreEnemigo = 0;
    public int maxScore = 3;

    [Header("UI-Marcador")]
    public Text scoreText;

    [Header("UI-Alerta: Enemigo con bandera")]
    public GameObject myFlagAlertPanel;
    public Text myFlagAlertText;

    [Header("UI-Alerta: Tenemos la bandera")]
    public GameObject hasFlagAlertPanel;
    public Text hasFlagAlertText;

    [Header("UI-Fin de partida")]
    public GameObject winScreen;
    public GameObject loseScreen;

    // bools que leen los npcs cada frame
    [HideInInspector] public bool flagStolenByEnemy = false;
    [HideInInspector] public bool flagStolenByPlayer = false;
    [HideInInspector] public bool flagRecovered = false;

    private bool gameOver = false;

    void Awake() { Instance = this; } // singleton

    void Start()
    {
        // reset de inicio
        scoreJugador = 0; scoreEnemigo = 0; gameOver = false;
        Time.timeScale = 1f;
        UpdateUI();
        // ocultar panels:
        if (myFlagAlertPanel != null) myFlagAlertPanel.SetActive(false);
        if (hasFlagAlertPanel != null) hasFlagAlertPanel.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }

    // si el jugador recogió la bandera enemiga, activa ui
    public void NotifyPlayerPickedFlag()
    {
        flagStolenByPlayer = true;
        ShowHasFlagAlert("Llevas la bandera! Corre a tu base!");
    }

    // si un aliado recogió la bandera enemiga, activa ui
    public void NotifyAllyPickedFlag()
    {
        flagStolenByPlayer = true; 
        ShowHasFlagAlert("Tu equipo tiene la bandera! Protegelo!");
    }

    // si un enemigo recogió la bandera del jugador, activa ui
    public void NotifyNPCPickedFlag()
    {
        flagStolenByEnemy = true;
        if (myFlagAlertPanel != null) myFlagAlertPanel.SetActive(true);
        if (myFlagAlertText != null)
        { myFlagAlertText.text = "El enemigo tiene tu bandera!"; myFlagAlertText.color = Color.red; }
    }

    void ShowHasFlagAlert(string msg) // muestra ui
    {
        if (hasFlagAlertPanel != null) hasFlagAlertPanel.SetActive(true);
        if (hasFlagAlertText != null)
        { hasFlagAlertText.text = msg; hasFlagAlertText.color = Color.yellow; }
    }

    // compatibilidad con llamadas antiguas
    public void ShowAlert(string type)
    {
        if (type == "none")
        {
            if (hasFlagAlertPanel != null) hasFlagAlertPanel.SetActive(false);
            if (myFlagAlertPanel != null) myFlagAlertPanel.SetActive(false);
        }
        else if (type == "playerHasFlag") NotifyPlayerPickedFlag();
        else if (type == "enemyHasFlag") NotifyNPCPickedFlag();
    }

    public void PlayerCaptures()
    {
        if (gameOver) return;
        scoreJugador++; // suma punto
        Debug.Log("PUNTO EQUIPO JUGADOR: " + scoreJugador + "/" + maxScore);
        UpdateUI(); // actualizar marcador
        flagStolenByPlayer = false; // reset señal
        if (hasFlagAlertPanel != null) hasFlagAlertPanel.SetActive(false);
        StartCoroutine(ClearRecovered());
        CheckWin(); // comprobar si alguien ganó
    }

    // mismo que el método de arriba pero con los enemigos
    public void EnemyCaptures()
    {
        if (gameOver) return;
        scoreEnemigo++;
        Debug.Log("PUNTO EQUIPO ENEMIGO: " + scoreEnemigo + "/" + maxScore);
        UpdateUI();
        flagStolenByEnemy = false;
        if (myFlagAlertPanel != null) myFlagAlertPanel.SetActive(false);
        StartCoroutine(ClearRecovered());
        CheckWin();
    }

    System.Collections.IEnumerator ClearRecovered()
    {
        flagRecovered = true; 
        yield return null;
        flagRecovered = false;
    }

    void UpdateUI() // actualiza texto del marcador
    {
        if (scoreText != null)
            scoreText.text = "TU: " + scoreJugador + "   ENEMIGO: " + scoreEnemigo;
    }

    void CheckWin() // verificamos si alguien ya ganó
    {
        if (scoreJugador >= maxScore) // si equipo player ganó, muestra win screen
        {
            gameOver = true;
            if (winScreen != null) winScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else if (scoreEnemigo >= maxScore) // si no, muestra loose screen
        {
            gameOver = true;
            if (loseScreen != null) loseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    // Llamado cuando una bandera vuelve a su base SIN anotar punto (portador murió)
    // Resetea todas las señales para que los NPCs no queden en bucle de estados
    public void FlagReturnedToBase()
    {
        flagStolenByPlayer = false;
        flagStolenByEnemy = false;
        if (hasFlagAlertPanel != null) hasFlagAlertPanel.SetActive(false);
        if (myFlagAlertPanel != null) myFlagAlertPanel.SetActive(false);
        StartCoroutine(ClearRecovered());
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Saliendo del juego");
    }
}