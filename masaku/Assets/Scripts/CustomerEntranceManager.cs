using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerEntranceManager : MonoBehaviour
{
    public static CustomerEntranceManager Instance;
    
    [Header("Door Settings")]
    public Transform doorTransform; // The door object to rotate
    public Transform doorPosition; // Where the door is located
    public Transform entranceSpawnPoint; // Where customers spawn (outside door)
    public float doorOpenRotation = 0f; // Y rotation when open
    public float doorClosedRotation = 90f; // Y rotation when closed
    public float doorRotationSpeed = 2f; // How fast door rotates
    
    [Header("Movement Settings")]
    public float customerWalkSpeed = 2f;
    public float customerRotationOffset = 0f; // Adjust if character faces wrong direction while walking
    
    private Queue<CustomerEntranceData> entranceQueue = new Queue<CustomerEntranceData>();
    private bool isProcessingEntrance = false;
    
    private class CustomerEntranceData
    {
        public GameObject customerPrefab;
        public Customer customerData;
        public int seatIndex;
        public Transform seatTransform;
    }
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void QueueCustomerEntrance(GameObject customerPrefab, Customer customerData, int seatIndex, Transform seatTransform)
    {
        CustomerEntranceData data = new CustomerEntranceData
        {
            customerPrefab = customerPrefab,
            customerData = customerData,
            seatIndex = seatIndex,
            seatTransform = seatTransform
        };
        
        entranceQueue.Enqueue(data);
        
        // Start processing if not already doing so
        if (!isProcessingEntrance)
        {
            StartCoroutine(ProcessEntranceQueue());
        }
    }
    
    IEnumerator ProcessEntranceQueue()
    {
        isProcessingEntrance = true;
        
        while (entranceQueue.Count > 0)
        {
            CustomerEntranceData data = entranceQueue.Dequeue();
            yield return StartCoroutine(PlayEntranceSequence(data));
            
            // Wait a bit before next customer enters
            yield return new WaitForSeconds(0.5f);
        }
        
        isProcessingEntrance = false;
    }
    
    IEnumerator PlayEntranceSequence(CustomerEntranceData data)
    {
        Debug.Log($"=== {data.customerData.customerName} entrance sequence start ===");
        
        // Check if door is assigned
        if (doorTransform == null)
        {
            Debug.LogWarning("⚠️ Door Transform is not assigned in CustomerEntranceManager!");
        }
        if (entranceSpawnPoint == null)
        {
            Debug.LogError("❌ Entrance Spawn Point is not assigned!");
            yield break;
        }
        
        // Step 1: Spawn customer at entrance (outside door)
        GameObject customerObj = Instantiate(data.customerPrefab, entranceSpawnPoint.position, Quaternion.identity);
        CustomerInstance customerInstance = customerObj.GetComponent<CustomerInstance>();
        Animator customerAnimator = customerObj.GetComponent<Animator>();
        
        if (customerInstance == null)
        {
            Debug.LogError("CustomerInstance component not found!");
            Destroy(customerObj);
            yield break;
        }
        
        Debug.Log($"1. Customer spawned at entrance: {entranceSpawnPoint.position}");
        
        // Step 2: Open door (rotate to 0 degrees)
        if (doorTransform != null)
        {
            Debug.Log($"2. Door opening from {doorTransform.eulerAngles.y}° to {doorOpenRotation}°");
            yield return StartCoroutine(RotateDoor(doorOpenRotation));
        }
        else
        {
            Debug.LogWarning("⚠️ Door Transform not assigned - skipping door animation");
        }
        
        // Step 3: Customer walks to door
        if (customerAnimator != null)
        {
            customerAnimator.SetBool("IsWalking", true);
        }
        
        Debug.Log("3. Customer walking to door...");
        yield return StartCoroutine(MoveCustomerToPosition(customerObj.transform, doorPosition.position));
        
        // Step 4: Customer walks through door to seat
        Debug.Log("4. Customer walking to seat...");
        yield return StartCoroutine(MoveCustomerToPosition(customerObj.transform, data.seatTransform.position));
        
        // Step 5: Stop walking animation
        if (customerAnimator != null)
        {
            customerAnimator.SetBool("IsWalking", false);
        }
        
        Debug.Log("5. Customer arrived at seat");
        
        // Step 6: Play sit down animation
        if (customerAnimator != null)
        {
            customerAnimator.SetTrigger("SitDown"); // Transition: Walking -> Sit Chair Down
            Debug.Log("6. Playing sit down animation");
            yield return new WaitForSeconds(0.5f); // Wait for sit down animation
            
            // Step 7: Transition to sit idle
            customerAnimator.SetTrigger("SitIdle"); // Transition: Sit Chair Down -> Sit Chair Idle
            Debug.Log("7. Customer now sitting idle");
        }
        
        // Step 8: Close door (rotate back to 90 degrees)
        if (doorTransform != null)
        {
            Debug.Log("8. Door closing...");
            yield return StartCoroutine(RotateDoor(doorClosedRotation));
        }
        
        // Step 9: Initialize customer (set data, patience, etc.)
        customerInstance.Initialize(data.customerData, data.seatIndex);
        
        // Register customer with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCustomerAtSeat(data.seatIndex, customerInstance);
        }
        
        Debug.Log($"9. {data.customerData.customerName} fully initialized at seat {data.seatIndex}");
        
        // Update UI menus after customer is fully seated
        if (MasakuUI.Instance != null)
        {
            MasakuUI.Instance.UpdateCustomerMenus();
        }
        
        Debug.Log("=== Entrance sequence complete ===\n");
    }
    
    IEnumerator MoveCustomerToPosition(Transform customer, Vector3 targetPosition)
    {
        float stoppingDistance = 0.1f;
        
        while (Vector3.Distance(customer.position, targetPosition) > stoppingDistance)
        {
            // Move customer
            customer.position = Vector3.MoveTowards(customer.position, targetPosition, customerWalkSpeed * Time.deltaTime);
            
            // Rotate to face movement direction
            Vector3 direction = (targetPosition - customer.position).normalized;
            if (direction != Vector3.zero)
            {
                // Calculate angle with adjustable offset
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + customerRotationOffset;
                customer.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            }
            
            yield return null;
        }
        
        // Snap to final position
        customer.position = targetPosition;
    }
    
    IEnumerator RotateDoor(float targetYRotation)
    {
        if (doorTransform == null) yield break;
        
        Quaternion startRotation = doorTransform.rotation;
        Quaternion targetRotation = Quaternion.Euler(doorTransform.eulerAngles.x, targetYRotation, doorTransform.eulerAngles.z);
        
        float elapsedTime = 0f;
        float duration = 1f / doorRotationSpeed; // Time to complete rotation
        
        while (elapsedTime < duration)
        {
            doorTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Snap to final rotation
        doorTransform.rotation = targetRotation;
        Debug.Log($"Door rotated to Y: {targetYRotation}°");
    }
    
    // Public method to check if there are customers waiting to enter
    public bool HasCustomersWaiting()
    {
        return entranceQueue.Count > 0 || isProcessingEntrance;
    }
}
