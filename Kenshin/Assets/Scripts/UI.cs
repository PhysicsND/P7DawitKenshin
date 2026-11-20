using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject uiObject;
    [SerializeField] private GameObject HealthBar;
    public GameObject InGameUI;
    public GameObject player;

    [Header("Restart Settings")]
    [SerializeField] private string restartText = "Restart";
    // Drag an empty GameObject here to mark where the player respawns
    [SerializeField] private Transform respawnPoint;
    // Used to refill the player's health on restart (optional)
    [SerializeField] private HealthScript healthScript;

    private TMP_Text text;
    private bool isRestartMode = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (uiObject == null)
            uiObject = GameObject.Find("UI");

        // Text on the same object this script is attached to (or a child of it)
        text = GetComponentInChildren<TMP_Text>();

        HealthBar.SetActive(false);
        InGameUI.SetActive(false);
        player.GetComponent<PlayerMovement>().enabled = false;
    }

    // Called by HealthScript when the player dies.
    // (Update can't detect the death: when uiObject is hidden, this script stops updating.)
    public void ShowRestart()
    {
        if (isRestartMode) return;
        isRestartMode = true;

        if (text == null)
            text = GetComponentInChildren<TMP_Text>(true);

        if (uiObject != null)
            uiObject.SetActive(true);

        HealthBar.SetActive(false);
        InGameUI.SetActive(false);

        if (text != null)
            text.text = restartText;
    }

    private void RespawnPlayer()
    {
        isRestartMode = false;

        // Move the player to the respawn point
        if (respawnPoint != null)
        {
            player.transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

            // Clear any leftover velocity so the player doesn't keep falling/sliding
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
            Rigidbody2D rb2D = player.GetComponent<Rigidbody2D>();
            if (rb2D != null) rb2D.linearVelocity = Vector2.zero;
        }

        if (healthScript != null)
            healthScript.SetHealth(healthScript.MaxHealth);

        player.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("UI Clicked");

        if (isRestartMode)
        {
            RespawnPlayer();
        }

        if (uiObject != null)
        {
            uiObject.SetActive(false);
            HealthBar.SetActive(true);
            InGameUI.SetActive(true);
        }
        // Movement is only allowed while both the in-game UI and the health bar are visible
        bool gameplayUIVisible = InGameUI.activeSelf && HealthBar.activeSelf;
        player.GetComponent<PlayerMovement>().enabled = gameplayUIVisible;
    }
}
