using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour // actualiza los corazones 
{
    [Header("Referencias")]
    public Image[] heartImages;// array corazones 
    public Sprite heartFull;            
    public Sprite heartEmpty;  

    public void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHearts)
                heartImages[i].sprite = heartFull;
            else
                heartImages[i].sprite = heartEmpty;
        }
    }

    void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            UpdateHearts(player.currentHearts);
    }
}