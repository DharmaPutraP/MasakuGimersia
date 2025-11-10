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
    private ActionCard currentActionCard;
    private Queue<Vector3> movementQueue = new Queue<Vector3>();
    private bool hasPickedUpIngredient = false;
    
    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }
    
    public void MoveToLocation(Vector3 destination, ActionCard card)
    {
        currentActionCard = card;
        movementQueue.Clear();
        hasPickedUpIngredient = false;
        
        // Check if card requires picking up ingredient first
        if (!string.IsNullOrEmpty(card.pickupTag))
        {
            Vector3 pickupPos = KitchenLocationManager.Instance.GetLocationPosition(card.pickupTag);
            if (pickupPos != Vector3.zero)
            {
                movementQueue.Enqueue(pickupPos);
                Debug.Log($"Akan mengambil bahan dari: {card.pickupTag}");
            }
        }
        
        // Then add the main target location
        movementQueue.Enqueue(destination);
        
        // Start moving to first location
        if (movementQueue.Count > 0)
        {
            targetPosition = movementQueue.Dequeue();
            isMoving = true;
            
            // Set animasi berjalan jika ada
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
            
            Debug.Log($"Bergerak ke lokasi: {(!string.IsNullOrEmpty(card.pickupTag) && !hasPickedUpIngredient ? card.pickupTag : card.targetTag)}");
        }
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
        
        // Rotasi karakter menghadap arah tujuan
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void ArrivedAtDestination()
    {
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
            
            // Move to next location
            targetPosition = movementQueue.Dequeue();
            isMoving = true;
            
            Debug.Log($"Melanjutkan ke lokasi: {currentActionCard.targetTag}");
            return;
        }
        
        // No more locations, finish movement
        isMoving = false;
        
        // Set animasi idle jika ada
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        Debug.Log($"Sampai di lokasi: {currentActionCard.targetTag}");
        
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
        return isMoving;
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
