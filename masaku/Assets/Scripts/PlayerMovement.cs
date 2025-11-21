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
        if (!string.IsNullOrEmpty(card.pickupTag))
        {
            Vector3 pickupPos = KitchenLocationManager.Instance.GetLocationPosition(card.pickupTag);
            if (pickupPos != Vector3.zero)
            {
                movementQueue.Enqueue(pickupPos);
                locationTagQueue.Enqueue(card.pickupTag);
            }
            else
            {
            }
        }
        
        movementQueue.Enqueue(destination);
        locationTagQueue.Enqueue(card.targetTag);
        if (movementQueue.Count > 0)
        {
            targetPosition = movementQueue.Dequeue();
            string firstLocation = locationTagQueue.Peek(); // Don't dequeue yet, just peek
            isMoving = true;
            
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
            
            PlaySound(walkingSound, true); // Loop walking sound
        }
    }
    
    void MoveTowardsTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget <= stoppingDistance)
        {
            ArrivedAtDestination();
            return;
        }
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        string nextLocation = "";
        if (locationTagQueue.Count > 0)
        {
            nextLocation = locationTagQueue.Peek(); // Look at next location without removing it
        }
        
        if (direction != Vector3.zero)
        {
            float targetAngle = 0f;
            
            if (nextLocation.Contains("Storage") || nextLocation.Contains("CuttingBoard"))
            {
                targetAngle = 90f;
            }
            else if (nextLocation.Contains("Stove"))
            {
                targetAngle = 180f;
            }
            else
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 120f;
            }
            
            float currentAngle = transform.eulerAngles.y;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0f, newAngle, 0f);
        }
    }
    
    void ArrivedAtDestination()
    {
        if (locationTagQueue.Count > 0)
        {
            locationTagQueue.Dequeue();
        }
        
        isMoving = false;
        isWaiting = true; // Set waiting flag
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        StopSound();
        if (movementQueue.Count > 0)
        {
            if (!hasPickedUpIngredient && !string.IsNullOrEmpty(currentActionCard.pickupTag))
            {
                hasPickedUpIngredient = true;
                PlaySound(pickupSound, false);
                
                if (animator != null)
                {
                    animator.SetTrigger("PickUp");
                }
            }
            
            StartCoroutine(WaitAndMoveToNextLocation());
        }
        else
        {
            if (currentActionCard != null)
            {
            }
            else
            {
            }
            
            StartCoroutine(WaitAndExecuteAction());
        }
    }
    
    IEnumerator WaitAndMoveToNextLocation()
    {
        float waitTime = Random.Range(0.5f, 1f);
        yield return new WaitForSeconds(waitTime);
        
        isWaiting = false; // Clear waiting flag
        
        if (movementQueue.Count > 0)
        {
            targetPosition = movementQueue.Dequeue();
            isMoving = true;
            
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
            
            PlaySound(walkingSound, true);
            
            if (currentActionCard != null)
            {
            }
            else
            {
            }
        }
        else
        {
        }
    }
    
    IEnumerator WaitAndExecuteAction()
    {
        isWaiting = true; // Keep waiting flag during entire action
        
        yield return StartCoroutine(ExecuteActionAtLocation());
        
        isWaiting = false; // Clear waiting flag after action completes
        
        hasPickedUpIngredient = false;
    }
    
    IEnumerator ExecuteActionAtLocation()
    {
        if (currentActionCard == null) yield break;
        
        if (currentActionCard.targetTag == "ServingCounter")
        {
            PlaySound(servingSound, false);
            yield return new WaitForSeconds(1f); // Wait 1 second for serving action
            yield break;
        }
        
        switch (currentActionCard.cardType)
        {
            case CardType.PotongSayuran:
                if (currentActionCard.targetTag.Contains("CuttingBoard") && knifeAnimator != null)
                {
                    knifeAnimator.SetBool("Cut", true);
                }
                PlaySound(cuttingSound, true); // Loop cutting sound
                yield return StartCoroutine(PerformAction("Potong Sayuran", 2f));
                break;
                
            case CardType.PotongDaging:
                if (currentActionCard.targetTag.Contains("CuttingBoard") && knifeAnimator != null)
                {
                    knifeAnimator.SetBool("Cut", true);
                }
                PlaySound(cuttingSound, true); // Loop cutting sound
                yield return StartCoroutine(PerformAction("Potong Daging", 2f));
                break;
                
            case CardType.PanaskanAir:
                if (currentActionCard.targetTag.Contains("Stove") && stoveAnimator != null)
                {
                    stoveAnimator.SetBool("Cook", true);
                }
                PlaySound(cookingSound, true); // Loop cooking sound
                if (currentActionCard.targetTag.Contains("Stove") && stoveFireParticle != null)
                {
                    stoveFireParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveFireParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveFireParticle.Clear(); // Extra clear
                    stoveFireParticle.Play(true); // Play with children
                }
                else
                {
                }
                if (stoveSmokeParticle != null)
                {
                    stoveSmokeParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveSmokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveSmokeParticle.Clear(); // Extra clear
                    stoveSmokeParticle.Play(true); // Play with children
                }
                else
                {
                }
                yield return StartCoroutine(PerformAction("Panaskan Air", 3f));
                break;
                
            case CardType.PanaskanDaging:
                if (stoveAnimator != null)
                {
                    stoveAnimator.SetBool("Cook", true);
                }
                PlaySound(cookingSound, true); // Loop cooking sound
                if (currentActionCard.targetTag.Contains("Stove") && stoveFireParticle != null)
                {
                    stoveFireParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveFireParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveFireParticle.Clear(); // Extra clear
                    stoveFireParticle.Play(true); // Play with children
                }
                else if (stoveFireParticle == null)
                {
                }
                if (currentActionCard.targetTag.Contains("Stove") && stoveSmokeParticle != null)
                {
                    stoveSmokeParticle.gameObject.SetActive(true); // Make sure GameObject is active
                    stoveSmokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    stoveSmokeParticle.Clear(); // Extra clear
                    stoveSmokeParticle.Play(true); // Play with children
                }
                else if (stoveSmokeParticle == null)
                {
                }
                yield return StartCoroutine(PerformAction("Panaskan Daging", 3f));
                break;
                
            case CardType.TarikNafas:
                break;
        }
    }
    
    IEnumerator PerformAction(string actionName, float duration)
    {
        if (animator != null)
        {
            animator.SetTrigger(actionName.Replace(" ", ""));
        }
        yield return new WaitForSeconds(duration);
        StopSound();
        
        if (actionName.Contains("Potong"))
        {
            if (knifeAnimator != null)
            {
                knifeAnimator.SetBool("Cut", false);
            }
        }
        
        if (actionName.Contains("Panaskan"))
        {
            if (stoveAnimator != null)
            {
                stoveAnimator.SetBool("Cook", false);
            }
            
            if (stoveFireParticle != null)
            {
                stoveFireParticle.Stop();
            }
            if (stoveSmokeParticle != null)
            {
                stoveSmokeParticle.Stop();
            }
        }
        
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
    
    void PlaySound(AudioClip clip, bool loop = false)
    {
        if (audioSource != null && clip != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }
    
    void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
