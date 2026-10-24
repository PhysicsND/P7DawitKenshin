using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Drop your reference object here")]
    public Transform referenceObject;

    void Start()
    {
        if (referenceObject == null)
        {
            Debug.LogError("Please assign a Reference Object in the Inspector.");
            return;
        }

        // 1. Find the specific wall GameObjects by name
        GameObject roomWall = GameObject.Find("Wall for Room");
        GameObject hallwayWall = GameObject.Find("Wall for Hallway");

        // 2. Extract the target Y scale from the reference object
        float targetYScale = referenceObject.localScale.y;

        // 3. Apply the scale strictly to the Y axis of the Room Wall
        if (roomWall != null)
        {
            Vector3 roomScale = roomWall.transform.localScale;
            roomScale.y = targetYScale;
            roomWall.transform.localScale = roomScale;
        }
        else
        {
            Debug.LogWarning("Could not find 'Wall for Room' in the scene.");
        }

        // 4. Apply the scale strictly to the Y axis of the Hallway Wall
        if (hallwayWall != null)
        {
            Vector3 hallwayScale = hallwayWall.transform.localScale;
            hallwayScale.y = targetYScale;
            hallwayWall.transform.localScale = hallwayScale;
        }
        else
        {
            Debug.LogWarning("Could not find 'Wall for Hallway' in the scene.");
        }

    }

// Update is called once per frame
void Update()
    {
        
    }
    private void OnMouseDown()
    {
        Debug.Log("Mouse Clicked");
    }
}
