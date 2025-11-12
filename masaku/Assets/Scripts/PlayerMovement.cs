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
    
    [Header("Location Animations")]
    public Animator knifeAnimator; // Animator for the knife at cutting board
    public Animator stoveAnimator; // Animator for the stove
    
    [Header("Stove Particle Effects")]
    public ParticleSystem stoveFireParticle; // Fire particle system at stove
    public ParticleSystem stoveSmokeParticle; // Smoke particle system at stove
    
    [Header("Sound Effects")]
    public AudioSource audioSource; // AudioSource for playing sounds
    public AudioClip walkingSound; // Walking/footsteps sound
    public AudioClip pickupSound; // Sound when picking up ingredients
    public AudioClip cuttingSound; // Sound when cutting at cutting board
    public AudioClip cookingSound; // Sound when cooking at stove
    public AudioClip servingSound; // Sound when at serving counter
    
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
            
            // Play walking sound
            PlaySound(walkingSound, true); // Loop walking sound
            
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
        
        // Stop walking sound
        StopSound();
        
        Debug.Log("Sampai di tujuan, berhenti sebentar...");
        
        // Check if there are more locations in queue
        if (movementQueue.Count > 0)
        {
            // Mark that we picked up ingredient at first location
            if (!hasPickedUpIngredient && !string.IsNullOrEmpty(currentActionCard.pickupTag))
            {
                hasPickedUpIngredient = true;
                Debug.Log($"Mengambil bahan dari: {currentActionCard.pickupTag}");
                
                // Play pickup sound
                PlaySound(pickupSound, false);
                
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
            if (currentActionCard != null)
            {
                Debug.Log($"Sampai di lokasi akhir: {currentActionCard.targetTag}");
            }
            else
            {
                Debug.Log("Sampai di lokasi akhir");
            }
            
            // Wait before executing action
            StartCoroutine(WaitAndExecuteAction());
        }
    }
    
    IEnumerator WaitAndMoveToNextLocation()
    {
        float waitTime = Random.Range(0.5f, 1f);
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
            
            // Resume walking sound
            PlaySound(walkingSound, true);
            
            if (currentActionCard != null)
            {
                Debug.Log($"Melanjutkan ke lokasi: {currentActionCard.targetTag}");
            }
            else
            {
                Debug.Log("Melanjutkan ke lokasi berikutnya");
            }
        }
        else
        {
            Debug.LogWarning("No more locations in queue!");
        }
    }
    
    IEnumerator WaitAndExecuteAction()
    {
        // Execute action immediately when arriving at final location
        isWaiting = true; // Keep waiting flag during entire action
        
        // Eksekusi aksi di lokasi and wait for it to complete
        yield return StartCoroutine(ExecuteActionAtLocation());
        
        isWaiting = false; // Clear waiting flag after action completes
        
        // Don't clear currentActionCard here - let MoveToLocation() handle it
        // This prevents issues when multiple cards are executed in sequence
        hasPickedUpIngredient = false;
    }
    
    IEnumerator ExecuteActionAtLocation()
    {
        if (currentActionCard == null) yield break;
        
        // Check if at serving counter (special case - no cardType, just targetTag)
        if (currentActionCard.targetTag == "ServingCounter")
        {
            Debug.Log("Di serving counter, menyajikan pesanan...");
            // Play serving sound
            PlaySound(servingSound, false);
            yield return new WaitForSeconds(1f); // Wait 1 second for serving action
            yield break;
        }
        
        // Di sini bisa ditambahkan logika spesifik untuk setiap aksi
        switch (currentActionCard.cardType)
        {
            case CardType.PotongSayuran:
                Debug.Log("Memotong sayuran...");
                // Trigger knife animation ONLY at cutting board
                if (currentActionCard.targetTag.Contains("CuttingBoard") && knifeAnimator != null)
                {
                    knifeAnimator.SetBool("Cut", true);
                }
                // Play cutting sound
                PlaySound(cuttingSound, true); // Loop cutting sound
                yield return StartCoroutine(PerformAction("Potong Sayuran", 2f));
                break;
                
            case CardType.PotongDaging:
                Debug.Log("Memotong daging...");
                // Trigger knife animation ONLY at cutting board
                if (currentActionCard.targetTag.Contains("CuttingBoard") && knifeAnimator != null)
                {
                    knifeAnimator.SetBool("Cut", true);
                }
                // Play cutting sound
                PlaySound(cuttingSound, true); // Loop cutting sound
                yield return StartCoroutine(PerformAction("Potong Daging", 2f));
                break;
                
            case CardType.PanaskanAir:
                Debug.Log("Memanaskan air...");
                // Trigger stove animation ONLY at stove
                if (currentActionCard.targetTag.Contains("Stove") && stoveAnimator != null)
                {
                    stoveAnimator.SetBool("Cook", true);
                }
                // Play cooking sound
                PlaySound(cookingSound, true); // Loop cooking sound
                // Start particle effects (ensure GameObject is active, stop and clear first, then play)
                if (currentActionCard.targetTag.Contains("Stove") && stoveFireParticle != null)
                {
                    Debug.Log("Starting fire particle...");
                    stoveFireParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveFireParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveFireParticle.Clear(); // Extra clear
                    stoveFireParticle.Play(true); // Play with children
                    Debug.Log($"Fire particle playing: {stoveFireParticle.isPlaying}, isEmitting: {stoveFireParticle.isEmitting}");
                }
                else
                {
                    Debug.LogWarning("Fire particle is NULL! Assign it in Inspector.");
                }
                if (stoveSmokeParticle != null)
                {
                    Debug.Log("Starting smoke particle...");
                    stoveSmokeParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveSmokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveSmokeParticle.Clear(); // Extra clear
                    stoveSmokeParticle.Play(true); // Play with children
                    Debug.Log($"Smoke particle playing: {stoveSmokeParticle.isPlaying}, isEmitting: {stoveSmokeParticle.isEmitting}");
                }
                else
                {
                    Debug.LogWarning("Smoke particle is NULL! Assign it in Inspector.");
                }
                yield return StartCoroutine(PerformAction("Panaskan Air", 3f));
                break;
                
            case CardType.PanaskanDaging:
                Debug.Log("Memanaskan daging...");
                // Trigger stove animation
                if (stoveAnimator != null)
                {
                    stoveAnimator.SetBool("Cook", true);
                }
                // Play cooking sound
                PlaySound(cookingSound, true); // Loop cooking sound
                // Start particle effects (ensure GameObject is active, stop and clear first, then play)
                if (currentActionCard.targetTag.Contains("Stove") && stoveFireParticle != null)
                {
                    Debug.Log("Starting fire particle...");
                    stoveFireParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveFireParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveFireParticle.Clear(); // Extra clear
                    stoveFireParticle.Play(true); // Play with children
                    Debug.Log($"Fire particle playing: {stoveFireParticle.isPlaying}, isEmitting: {stoveFireParticle.isEmitting}");
                }
                else if (stoveFireParticle == null)
                {
                    Debug.LogWarning("Fire particle is NULL! Assign it in Inspector.");
                }
                if (currentActionCard.targetTag.Contains("Stove") && stoveSmokeParticle != null)
                {
                    Debug.Log("Starting smoke particle...");
                    stoveSmokeParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveSmokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveSmokeParticle.Clear(); // Extra clear
                    stoveSmokeParticle.Play(true); // Play with children
                    Debug.Log($"Smoke particle playing: {stoveSmokeParticle.isPlaying}, isEmitting: {stoveSmokeParticle.isEmitting}");
                }
                else if (stoveSmokeParticle == null)
                {
                    Debug.LogWarning("Smoke particle is NULL! Assign it in Inspector.");
                }
                yield return StartCoroutine(PerformAction("Panaskan Daging", 3f));
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
        
        // Stop any looping sounds
        StopSound();
        
        // Stop animations based on action type
        if (actionName.Contains("Potong"))
        {
            // Stop knife cutting animation
            if (knifeAnimator != null)
            {
                knifeAnimator.SetBool("Cut", false);
            }
        }
        
        // Stop stove particle effects and animation after cooking actions
        if (actionName.Contains("Panaskan"))
        {
            // Stop stove animation
            if (stoveAnimator != null)
            {
                stoveAnimator.SetBool("Cook", false);
            }
            
            // Stop particles
            if (stoveFireParticle != null)
            {
                stoveFireParticle.Stop();
            }
            if (stoveSmokeParticle != null)
            {
                stoveSmokeParticle.Stop();
            }
        }
        
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
        StopSound();
    }
    
    // Helper method to play sound effects
    void PlaySound(AudioClip clip, bool loop = false)
    {
        if (audioSource != null && clip != null)
        {
            // Stop current sound if playing
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
            Debug.Log($"Playing sound: {clip.name}, Loop: {loop}");
        }
    }
    
    // Helper method to stop sound effects
    void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Stopping sound");
        }
    }
}
