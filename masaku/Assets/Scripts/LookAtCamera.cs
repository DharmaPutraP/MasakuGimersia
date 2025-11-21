using UnityEngine;

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
        }
    }
    
    void LateUpdate()
    {
        if (mainCamera == null) return;
        
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        
        if (lockXZRotation)
        {
            directionToCamera.y = 0;
        }
        
        if (flipDirection)
        {
            directionToCamera = -directionToCamera;
        }
        
        if (directionToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
}
