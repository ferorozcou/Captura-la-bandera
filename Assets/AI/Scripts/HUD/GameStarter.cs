using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStarter : MonoBehaviour // cuenta atrás, desactiva todo antes de iniciar
{
    [Header("Countdown pre-partida")]
    public GameObject countdownPanel;   // panel 
    public Text countdownText;    // texto countdown 3,2...

    [Header("Referencia al HUD")]
    public GameObject heartsParent;     // el objeto que descargamos

    private bool gameStarted = false;

    void Start()
    {
        // oculta corazones 
        if (heartsParent != null) heartsParent.SetActive(false);

        // desactiva controles del jugador durante la cuenta atrás
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.enabled = false;

        // Detener todos los NPCs
        foreach (var b in FindObjectsByType<BuscadorController>(FindObjectsSortMode.None))
            b.enabled = false;
        foreach (var d in FindObjectsByType<DefensorController>(FindObjectsSortMode.None))
            d.enabled = false;

        StartCoroutine(PreGameCountdown());
    }

    IEnumerator PreGameCountdown()
    {
        if (countdownPanel != null) countdownPanel.SetActive(true);

        string[] steps = { "3", "2", "1", "YA!" };
        foreach (string s in steps)
        {
            if (countdownText != null) countdownText.text = s;
            yield return new WaitForSeconds(1f); // muestra cada número 1 seg
        }

        if (countdownPanel != null) countdownPanel.SetActive(false);

        // muestra corazones
        if (heartsParent != null) heartsParent.SetActive(true);

        // activar jugador y NPCs
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.enabled = true;

        foreach (var b in FindObjectsByType<BuscadorController>(FindObjectsSortMode.None))
            b.enabled = true;
        foreach (var d in FindObjectsByType<DefensorController>(FindObjectsSortMode.None))
            d.enabled = true;

        gameStarted = true;
        Debug.Log("Partida iniciada!");
    }
}