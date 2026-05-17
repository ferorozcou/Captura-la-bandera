using UnityEngine;
using UnityEngine.UI;

public class NPCNombre : MonoBehaviour // hace que el texto del nombre del NPC siempre mire a la camara
{
    public Text nombreText;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (nombreText != null)
            nombreText.color = Color.white;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
            transform.rotation = mainCamera.transform.rotation;
    }

    public void UpdateNameColor(int teamID, bool hasFlag)
    {
        if (nombreText == null) return;
        if (hasFlag)
            nombreText.color = new Color(0.90f, 0.82f, 0.30f);
        else if (teamID == 0)
            nombreText.color = new Color(0.85f, 0.35f, 0.35f);
        else
            nombreText.color = new Color(0.35f, 0.55f, 0.85f);
    }
}