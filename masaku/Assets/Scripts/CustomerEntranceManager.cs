using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerEntranceManager : MonoBehaviour
{
    public static CustomerEntranceManager Instance;
    
    [Header("Door Settings")]
    public Transform doorTransform; 
    public Transform doorPosition; 
    public Transform entranceSpawnPoint; 
    public float doorOpenRotation = -82f; 
    public float doorClosedRotation = 90f; 
    public float doorRotationSpeed = 2f; 
    
    [Header("Movement Settings")]
    public float customerWalkSpeed = 2f;
    public float customerRotationOffset = 0f; 
    
    [Header("Sound Effects")]
    public AudioSource audioSource; 
    public AudioClip doorOpenSound; 
    
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
            
            yield return new WaitForSeconds(0.5f);
        }
        
        isProcessingEntrance = false;
    }
    
    IEnumerator PlayEntranceSequence(CustomerEntranceData data)
    {
        if (doorTransform == null)
        {
        }
        if (entranceSpawnPoint == null)
        {
            yield break;
        }
        
        GameObject customerObj = Instantiate(data.customerPrefab, entranceSpawnPoint.position, Quaternion.identity);
        CustomerInstance customerInstance = customerObj.GetComponent<CustomerInstance>();
        Animator customerAnimator = customerObj.GetComponent<Animator>();
        
        if (customerInstance == null)
        {
            Destroy(customerObj);
            yield break;
        }
        if (doorTransform != null)
        {
            PlaySound(doorOpenSound);
            
            yield return StartCoroutine(RotateDoor(doorOpenRotation));
        }
        else
        {
        }
        
        if (customerAnimator != null)
        {
            customerAnimator.SetBool("IsWalking", true);
        }
        yield return StartCoroutine(MoveCustomerToPosition(customerObj.transform, doorPosition.position));
        
        yield return StartCoroutine(MoveCustomerToPosition(customerObj.transform, data.seatTransform.position));
        
        if (customerAnimator != null)
        {
            customerAnimator.SetBool("IsWalking", false);
        }
        if (customerAnimator != null)
        {
            customerAnimator.SetTrigger("SitDown"); 
            yield return new WaitForSeconds(0.5f); 
            
            customerAnimator.SetTrigger("SitIdle");
        }
        
        if (doorTransform != null)
        {
            PlaySound(doorOpenSound);
            
            yield return StartCoroutine(RotateDoor(doorClosedRotation));
        }
        
        customerInstance.Initialize(data.customerData, data.seatIndex);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCustomerAtSeat(data.seatIndex, customerInstance);
        }
        if (MasakuUI.Instance != null)
        {
            MasakuUI.Instance.UpdateCustomerMenus();
        }
    }
    
    IEnumerator MoveCustomerToPosition(Transform customer, Vector3 targetPosition)
    {
        float stoppingDistance = 0.1f;
        
        while (Vector3.Distance(customer.position, targetPosition) > stoppingDistance)
        {
            customer.position = Vector3.MoveTowards(customer.position, targetPosition, customerWalkSpeed * Time.deltaTime);
            
            Vector3 direction = (targetPosition - customer.position).normalized;
            if (direction != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + customerRotationOffset;
                customer.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            }
            
            yield return null;
        }
        
        customer.position = targetPosition;
    }
    
    IEnumerator RotateDoor(float targetYRotation)
    {
        if (doorTransform == null) yield break;
        
        Quaternion startRotation = doorTransform.rotation;
        Quaternion targetRotation = Quaternion.Euler(doorTransform.eulerAngles.x, targetYRotation, doorTransform.eulerAngles.z);
        
        float elapsedTime = 0f;
        float duration = 1f / doorRotationSpeed; 
        
        while (elapsedTime < duration)
        {
            doorTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        doorTransform.rotation = targetRotation;
    }
    
    public bool HasCustomersWaiting()
    {
        return entranceQueue.Count > 0 || isProcessingEntrance;
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else if (clip == null)
        {
        }
        else if (audioSource == null)
        {
        }
    }
}
