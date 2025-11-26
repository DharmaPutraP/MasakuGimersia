using UnityEngine;

public class MainSceneLightingFixer : MonoBehaviour
{
    [Header("Scene Lighting Settings")]
    [Tooltip("The main directional light that should be active in this scene")]
    public Light mainDirectionalLight;
    
    [Tooltip("Should destroy all lights not belonging to this scene?")]
    public bool destroyExternalLights = true;
    
    void Awake()
    {
        CleanupLighting();
    }
    
    void CleanupLighting()
    {
        // Find all lights in the scene
        Light[] allLights = FindObjectsOfType<Light>(true);
        
        string currentSceneName = gameObject.scene.name;
        
        foreach (Light light in allLights)
        {
            if (light == null || light.gameObject == null) continue;
            
            // Check if this light belongs to the current scene
            bool isFromThisScene = light.gameObject.scene.name == currentSceneName;
            
            if (!isFromThisScene && destroyExternalLights)
            {
                // This light is from a previous scene (MainMenu, etc.)
                Debug.Log($"Destroying external light: {light.gameObject.name} from scene: {light.gameObject.scene.name}");
                Destroy(light.gameObject);
            }
            else if (isFromThisScene)
            {
                // This is a light from the current scene
                if (light != mainDirectionalLight)
                {
                    // Keep it but make sure it's configured properly
                    light.enabled = true;
                }
            }
        }
        
        // Ensure the main directional light is active
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.enabled = true;
            mainDirectionalLight.gameObject.SetActive(true);
            Debug.Log($"Main scene directional light activated: {mainDirectionalLight.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("Main directional light not assigned in MainSceneLightingFixer!");
        }
    }
}
