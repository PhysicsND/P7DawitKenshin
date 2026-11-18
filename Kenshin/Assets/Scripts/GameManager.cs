using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ---------- Enemy spawning ----------
    // Spawns one enemy at the start. When that enemy is destroyed, a timer starts,
    // and when it runs out a new enemy spawns. Only one enemy is alive at a time.

        public GameObject[] enemyPrefabs;

   
    public GameObject[] spawnPoints;

   
    public float spawnDelay = 60f; // seconds after an enemy dies before the next one spawns


    [SerializeField] private KeyCode destroyEnemyKey = KeyCode.K;
    [SerializeField] private KeyCode skipTimerKey = KeyCode.T;
    [SerializeField] private TMP_Text timerText; //  shows the countdown on screen

    private GameObject currentEnemy;
    private float timer;

    void Start()
    {
        SpawnEnemy();
        timer = spawnDelay;
    }

    // Update is called once per frame
    void Update()
    {
        // Testing shortcuts
        if (Input.GetKeyDown(destroyEnemyKey)) DestroyEnemy();
        if (Input.GetKeyDown(skipTimerKey)) SkipTimer();

        if (currentEnemy == null)
        {
            // No enemy alive, so count down to the next one
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnEnemy();
                timer = spawnDelay;
            }
        }

        UpdateTimerText();
    }

    public void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("GameManager: no enemy prefabs assigned.");
            return;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("GameManager: no spawn points assigned.");
            return;
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (prefab == null || point == null)
        {
            Debug.LogWarning("GameManager: an empty slot is in the prefab or spawn point list.");
            return;
        }

        currentEnemy = Instantiate(prefab, point.transform.position, point.transform.rotation);
        Debug.Log("Spawned " + prefab.name + " at " + point.name);
    }

    // TEST: hook this to a UI Button's OnClick, or press K
    public void DestroyEnemy()
    {
        if (currentEnemy != null)
        {
            Debug.Log("Destroyed " + currentEnemy.name);
            Destroy(currentEnemy);
            currentEnemy = null;
        }
        else
        {
            Debug.Log("No enemy to destroy.");
        }
    }

    // TEST: hook this to a UI Button's OnClick, or press T
    public void SkipTimer()
    {
        if (currentEnemy == null)
        {
            Debug.Log("Timer skipped.");
            timer = 0f;
        }
        else
        {
            Debug.Log("Enemy is still alive, so the timer isn't running.");
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        if (currentEnemy != null)
            timerText.text = "Enemy alive";
        else
            timerText.text = "Next enemy: " + Mathf.CeilToInt(timer) + "s";
    }

    
}
