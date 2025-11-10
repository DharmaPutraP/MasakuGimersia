using UnityEngine;

/// <summary>
/// Makes the GameObject always face the camera.
/// Attach this to customer prefabs to make them always look at the camera.
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("If true, only rotates on Y axis (billboard effect)")]
    public bool lockXZRotation = true;
    
    [Tooltip("Flip 180 degrees if character is facing the wrong way")]
    public bool flipDirection = false;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogWarning("No main camera found! LookAtCamera will not work.");
        }
    }
    
    void LateUpdate()
    {
        if (mainCamera == null) return;
        
        // Get direction to camera
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        
        if (lockXZRotation)
        {
            // Only rotate on Y axis (billboard effect)
            directionToCamera.y = 0;
        }
        
        // Flip if needed
        if (flipDirection)
        {
            directionToCamera = -directionToCamera;
        }
        
        // Look at camera
        if (directionToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
}
