using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
     public float Health, MaxHealth, Width, Height;
    [SerializeField] private RectTransform healthBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void SetMaxHealth(float maxHealth)
   {
   MaxHealth = maxHealth;
   }
   public void SetHealth(float health)
   {
   Health = health;
   float newWidth = (Health /  MaxHealth) * Width;
    healthBar.sizeDelta = new Vector2(newWidth, Height);
   }
}
