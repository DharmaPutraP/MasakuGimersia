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
    
    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }
    
    public void MoveToLocation(Vector3 destination, ActionCard card)
    {
        targetPosition = destination;
        currentActionCard = card;
        isMoving = true;
        
        // Set animasi berjalan jika ada
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
        
        Debug.Log($"Bergerak ke lokasi: {card.targetTag}");
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
