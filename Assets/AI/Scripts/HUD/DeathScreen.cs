using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScreen : MonoBehaviour // pantalla negra que sale cuando se muere el player
{
    public static DeathScreen Instance;

    [Header("Arrastra desde la Hierarchy")]
    public Image blackOverlay;    // imagen negra que cubre toda la pantalla
    public Text countdownText;  

    void Awake()
    {
        Instance = this;
        SetAlpha(0f); // transparente al inicio
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false); 
    }

    public IEnumerator ShowDeathAndRespawn(System.Action onRespawn)
    {
        if (blackOverlay == null)
        {
            Debug.LogError("DeathScreen: blackOverlay no asignado!");
            yield return new WaitForSeconds(3f);
            onRespawn?.Invoke();
            yield break;
        }

        // fade a negro 1.5 seg
        blackOverlay.gameObject.SetActive(true);
        float timer = 0f;
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            SetAlpha(Mathf.Clamp01(timer / 1.5f));
            yield return null;
        }
        SetAlpha(1f);

        // cuenta atrás 3, 2...
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            for (int i = 3; i >= 1; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DeathScreen: countdownText no asignado, esperando 3s");
            yield return new WaitForSeconds(3f);
        }

        //  respawn mientras pantalla negra 
        onRespawn?.Invoke();
        yield return null;

        // fade de vuelta a transparente 
        timer = 0f;
        while (timer < 0.8f)
        {
            timer += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(timer / 0.8f));
            yield return null;
        }
        SetAlpha(0f);
        blackOverlay.gameObject.SetActive(false);
    }

    void SetAlpha(float a) // cambia transparencia
    {
        if (blackOverlay == null) return;
        Color c = blackOverlay.color;
        c.a = a;
        blackOverlay.color = c;
    }
}