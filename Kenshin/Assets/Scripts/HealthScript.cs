using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
     public float Health, MaxHealth, Width, Height;
     public GameObject player;
    [SerializeField] private RectTransform healthBar;
    // Drag the object with the UI script here
    [SerializeField] private UI ui;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void SetMaxHealth(float maxHealth)
   {
   MaxHealth = maxHealth;
   }
   public void SetHealth(float health)
   {
   Health = health;
   float newWidth = Health /  MaxHealth * Width;
    healthBar.sizeDelta = new Vector2(newWidth, Height);
    if (Health <= 0)
    {
      // Hide instead of Destroy so the UI script can respawn the player
      player.SetActive(false);

      // Tell the UI to show the Restart screen
      if (ui != null)
        ui.ShowRestart();
    }
   }
}
