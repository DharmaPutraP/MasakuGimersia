using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenLocationManager : MonoBehaviour
{
    public static KitchenLocationManager Instance;
    
    [System.Serializable]
    public class KitchenLocation
    {
        public string locationTag;
        public Transform locationTransform;
        public string locationName;
    }
    
    [Header("Kitchen Locations")]
    public List<KitchenLocation> locations = new List<KitchenLocation>();
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public Transform GetLocationByTag(string tag)
    {
        foreach (KitchenLocation location in locations)
        {
            if (location.locationTag == tag)
            {
                return location.locationTransform;
            }
        }
        return null;
    }
    
    public Vector3 GetLocationPosition(string tag)
    {
        Transform location = GetLocationByTag(tag);
        if (location != null)
        {
            return location.position;
        }
        
        return Vector3.zero;
    }
    
    [ContextMenu("Auto-Find Locations")]
    public void AutoFindLocations()
    {
        locations.Clear();
        
        string[] kitchenTags = { "CuttingBoard", "Stove", "ServingCounter", "StorageVegetable", "StorageMeat" };
        
        foreach (string tag in kitchenTags)
        {
            GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in foundObjects)
            {
                KitchenLocation newLocation = new KitchenLocation
                {
                    locationTag = tag,
                    locationTransform = obj.transform,
                    locationName = obj.name
                };
                locations.Add(newLocation);
            }
        }
    }
}
