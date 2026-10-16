using UnityEngine;

public class CameraFollow : MonoBehaviour
{
  
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 5f, -10f);
    
   
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // Calculate the base target position including our offset
        float targetX = followX ? playerTransform.position.x + cameraOffset.x : transform.position.x;
        float targetY = followY ? playerTransform.position.y + cameraOffset.y : transform.position.y;
        float targetZ = playerTransform.position.z + cameraOffset.z; // Always follow Z

        Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);

        // Smoothly interpolate from current camera position to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
