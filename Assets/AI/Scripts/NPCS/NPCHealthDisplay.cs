using UnityEngine;
using UnityEngine.UI;

public class NPCHealthDisplay : MonoBehaviour // muestra ek numero de vida de los npcs
{
    public Text healthText;// texto ui
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        // forzar que el texto sea visible al iniciar
        if (healthText != null)
            healthText.color = Color.white;
    }

    void LateUpdate()
    {
        // el canvas siempre mira a la cámara
        if (mainCamera != null)
            transform.rotation = mainCamera.transform.rotation;
    }

    public void UpdateHealth(float current, float max)
    {
        if (healthText == null)
        {
            Debug.LogWarning("NPCHealthDisplay: healthText es null en " + gameObject.name);
            return;
        }
        // muestra numero
        healthText.text = Mathf.CeilToInt(current).ToString();
    }
}