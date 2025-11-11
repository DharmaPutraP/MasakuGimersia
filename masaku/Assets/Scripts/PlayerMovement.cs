using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 0.5f;
    
    [Header("Animation")]
    public Animator animator;
    
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isWaiting = false; // NEW: Flag to prevent movement during wait
    private ActionCard currentActionCard;
    private Queue<Vector3> movementQueue = new Queue<Vector3>();
    private Queue<string> locationTagQueue = new Queue<string>(); // Track which location we're going to
    private bool hasPickedUpIngredient = false;
    
    void Update()
    {
        if (isMoving && !isWaiting)
        {
            MoveTowardsTarget();
        }
    }
    
    public void MoveToLocation(Vector3 destination, ActionCard card)
    {
        currentActionCard = card;
        movementQueue.Clear();
        locationTagQueue.Clear();
        hasPickedUpIngredient = false;
        
        Debug.Log($"=== Starting movement for card: {card.cardName} ===");
        Debug.Log($"Pickup Tag: '{card.pickupTag}' | Target Tag: '{card.targetTag}'");
        
        // Check if card requires picking up ingredient first
        if (!string.IsNullOrEmpty(card.pickupTag))
        {
            Vector3 pickupPos = KitchenLocationManager.Instance.GetLocationPosition(card.pickupTag);
            Debug.Log($"Pickup position for '{card.pickupTag}': {pickupPos}");
            
            if (pickupPos != Vector3.zero)
            {
                movementQueue.Enqueue(pickupPos);
                locationTagQueue.Enqueue(card.pickupTag);
                Debug.Log($"✓ Enqueued pickup location: {card.pickupTag} at {pickupPos}");
            }
            else
            {
                Debug.LogError($"✗ Pickup location '{card.pickupTag}' tidak ditemukan atau posisi invalid!");
            }
        }
        
        // Then add the main target location
        Debug.Log($"Target position: {destination}");
        movementQueue.Enqueue(destination);
        locationTagQueue.Enqueue(card.targetTag);
        Debug.Log($"✓ Enqueued target location: {card.targetTag} at {destination}");
        
        Debug.Log($"Total locations in queue: {movementQueue.Count}");
        
        // Start moving to first location
        if (movementQueue.Count > 0)
        {
            targetPosition = movementQueue.Dequeue();
            string firstLocation = locationTagQueue.Peek(); // Don't dequeue yet, just peek
            isMoving = true;
            
            // Set animasi berjalan jika ada
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
            
            Debug.Log($"→ Starting movement to: {firstLocation} at position {targetPosition}");
        }
        
        Debug.Log("=== Movement setup complete ===\n");
    }
    
    void MoveTowardsTarget()
    {
        // Hitung jarak ke target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget <= stoppingDistance)
        {
            // Sampai di tujuan
            ArrivedAtDestination();
            return;
        }
        
        // Gerakkan karakter
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Get the target location tag to determine rotation
        string nextLocation = "";
        if (locationTagQueue.Count > 0)
        {
            nextLocation = locationTagQueue.Peek(); // Look at next location without removing it
        }
        
        // Rotasi karakter menghadap sesuai tujuan
        if (direction != Vector3.zero)
        {
            float targetAngle = 0f;
            
            // Determine target angle based on destination
            if (nextLocation.Contains("Storage") || nextLocation.Contains("CuttingBoard"))
            {
                // Rotate to face left (90 degrees) while walking
                targetAngle = 90f;
            }
            else if (nextLocation.Contains("Stove"))
            {
                // Rotate to face backward (180 degrees) while walking
                targetAngle = 180f;
            }
            else
            {
                // Default: calculate angle based on movement direction
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 120f;
            }
            
            float currentAngle = transform.eulerAngles.y;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0f, newAngle, 0f);
        }
    }
    
    void ArrivedAtDestination()
    {
        // Remove current location from queue
        if (locationTagQueue.Count > 0)
        {
            locationTagQueue.Dequeue();
        }
        
        // Stop walking animation immediately
        isMoving = false;
        isWaiting = true; // Set waiting flag
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        Debug.Log("Sampai di tujuan, berhenti sebentar...");
        
        // Check if there are more locations in queue
        if (movementQueue.Count > 0)
        {
            // Mark that we picked up ingredient at first location
            if (!hasPickedUpIngredient && !string.IsNullOrEmpty(currentActionCard.pickupTag))
            {
                hasPickedUpIngredient = true;
                Debug.Log($"Mengambil bahan dari: {currentActionCard.pickupTag}");
                
                // Optional: Add pickup animation here
                if (animator != null)
                {
                    animator.SetTrigger("PickUp");
                }
            }
            
            // Wait 1-2 seconds before moving to next location
            StartCoroutine(WaitAndMoveToNextLocation());
        }
        else
        {
            // No more locations, finish movement
            Debug.Log($"Sampai di lokasi akhir: {currentActionCard.targetTag}");
            
            // Wait before executing action
            StartCoroutine(WaitAndExecuteAction());
        }
    }
    
    IEnumerator WaitAndMoveToNextLocation()
    {
        float waitTime = Random.Range(1.5f, 2f);
        Debug.Log($"Menunggu {waitTime:F1} detik...");
        yield return new WaitForSeconds(waitTime);
        
        isWaiting = false; // Clear waiting flag
        
        // Check if there's a next location before dequeuing
        if (movementQueue.Count > 0)
        {
            // Move to next location
            targetPosition = movementQueue.Dequeue();
            isMoving = true;
            
            // Resume walking animation
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
            
            Debug.Log($"Melanjutkan ke lokasi: {currentActionCard.targetTag}");
        }
        else
        {
            Debug.LogWarning("No more locations in queue!");
        }
    }
    
    IEnumerator WaitAndExecuteAction()
    {
        float waitTime = Random.Range(1.5f, 2f);
        Debug.Log($"Menunggu {waitTime:F1} detik sebelum bekerja...");
        yield return new WaitForSeconds(waitTime);
        
        isWaiting = false; // Clear waiting flag
        
        // Eksekusi aksi di lokasi
        ExecuteActionAtLocation();
        
        currentActionCard = null;
        hasPickedUpIngredient = false;
    }
    
    void ExecuteActionAtLocation()
    {
        if (currentActionCard == null) return;
        
        // Di sini bisa ditambahkan logika spesifik untuk setiap aksi
        switch (currentActionCard.cardType)
        {
            case CardType.PotongSayuran:
                Debug.Log("Memotong sayuran...");
                // Tambahkan animasi atau efek memotong sayuran
                StartCoroutine(PerformAction("Potong Sayuran", 2f));
                break;
                
            case CardType.PotongDaging:
                Debug.Log("Memotong daging...");
                StartCoroutine(PerformAction("Potong Daging", 2f));
                break;
                
            case CardType.PanaskanAir:
                Debug.Log("Memanaskan air...");
                StartCoroutine(PerformAction("Panaskan Air", 3f));
                break;
                
            case CardType.PanaskanDaging:
                Debug.Log("Memanaskan daging...");
                StartCoroutine(PerformAction("Panaskan Daging", 3f));
                break;
                
            case CardType.TarikNafas:
                Debug.Log("Menarik nafas...");
                break;
        }
    }
    
    IEnumerator PerformAction(string actionName, float duration)
    {
        // Set animasi aksi jika ada
        if (animator != null)
        {
            animator.SetTrigger(actionName.Replace(" ", ""));
        }
        
        Debug.Log($"Melakukan: {actionName}");
        yield return new WaitForSeconds(duration);
        Debug.Log($"Selesai: {actionName}");
        
        // Bisa tambahkan reward atau efek setelah selesai
    }
    
    public bool IsMoving()
    {
        return isMoving || isWaiting; // Return true if moving OR waiting
    }
    
    public void StopMovement()
    {
        isMoving = false;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
    }
}
