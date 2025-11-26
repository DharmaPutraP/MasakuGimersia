using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Add this for TextMeshPro

public class CustomerInstance : MonoBehaviour
{
    public Customer customerData;
    public int currentPatience;
    public int seatIndex; // Kursi mana (0-3)
    
    [Header("UI References")]
    public SpriteRenderer customerSprite; 
    public Animator customerAnimator; 
    public Canvas patienceCanvas; 
    public Transform patienceBarTransform;  
    public Transform patienceBarBackground; 
    public SpriteRenderer patienceBarSprite;  
    public TextMeshProUGUI orderDisplayText; 
    public TextMeshProUGUI patienceText; 
    
    [Header("Patience Bar Settings")]
    public int patienceSegments = 10; 
    public GameObject dividerPrefab; 
    public Transform dividersParent; 
    private List<GameObject> dividers = new List<GameObject>();
    private Camera mainCamera;

    [Header("Sound Effects")]
    public AudioSource audioSource; 
    public AudioClip barbarianAngrySound; 
    public AudioClip wizardHappySound;
    public AudioClip happyEmoteSound;
    public AudioClip angryEmoteSound; 
    
    [Header("Highlight Effect")]
    public GameObject highlightIndicator;
    
    [Header("Emote System")]
    public UnityEngine.UI.Image happyEmoteImage;
    public UnityEngine.UI.Image angryEmoteImage;
    public float emoteDuration = 2f;
    public float emotePopDuration = 0.3f;
    public float emoteFadeDuration = 0.3f;
    
    private bool isServed = false;
    
    void Start()
    {
        if(happyEmoteImage != null && angryEmoteImage != null)
        {
            happyEmoteImage.gameObject.SetActive(false);
            angryEmoteImage.gameObject.SetActive(false);
        }
        mainCamera = Camera.main;
        
        if (patienceBarBackground != null && dividersParent != null)
        {
            CreatePatienceBarDividers();
        }
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        
        // If highlightIndicator is assigned in prefab, use it
        // Otherwise, create one programmatically
        if (highlightIndicator == null)
        {
            CreateHighlightIndicator();
        }
        else
        {
            // Start pulse animation for prefab indicator
            StartCoroutine(PulseHighlight());
        }
        
        if (highlightIndicator != null)
        {
            SpriteRenderer sr = highlightIndicator.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(1f, 1f, 0f, 0.8f);
            sr.sortingOrder = -1;
            highlightIndicator.transform.localScale = Vector3.one * 1f;
            highlightIndicator.SetActive(false);
        }
        
    }
    
    void CreateHighlightIndicator()
    {
        highlightIndicator = new GameObject("HighlightIndicator");
        highlightIndicator.transform.SetParent(transform);
        highlightIndicator.transform.localPosition = new Vector3(0f, 2.76f, 0f);
        
        SpriteRenderer sr = highlightIndicator.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 1f, 0f, 0.8f);
        sr.sortingOrder = -1;
        
        highlightIndicator.transform.localScale = Vector3.one * 0.5f;
        
        StartCoroutine(PulseHighlight());
    }
    
    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    IEnumerator PulseHighlight()
    {
        while (true)
        {
            if (highlightIndicator != null && highlightIndicator.activeSelf)
            {
                float scale = 0.75f + Mathf.PingPong(Time.time * 2f, 0.2f);
                highlightIndicator.transform.localScale = Vector3.one * scale;
            }
            yield return null;
        }
    }
    
    public void SetHighlight(bool active)
    {
        if (highlightIndicator != null)
        {
            highlightIndicator.SetActive(active);
        }
    }
    
    void OnMouseDown()
    {
        if (MasakuUI.Instance != null)
        {
            MasakuUI.Instance.OnCustomerClicked(seatIndex);
        }
    }
    
    void LateUpdate()
    {
        if (mainCamera != null && patienceCanvas != null)
        {
            patienceCanvas.transform.LookAt(patienceCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
            
            patienceCanvas.transform.Rotate(0, 180, 0);
        }
        
        if (mainCamera != null && patienceBarBackground != null)
        {
            patienceBarBackground.LookAt(patienceBarBackground.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
        
        if (mainCamera != null && highlightIndicator != null && highlightIndicator.activeSelf)
        {
            highlightIndicator.transform.LookAt(highlightIndicator.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
        
    }
    
    void CreatePatienceBarDividers()
    {
        foreach (GameObject divider in dividers)
        {
            if (divider != null)
                Destroy(divider);
        }
        dividers.Clear();
        
        SpriteRenderer bgSprite = patienceBarBackground.GetComponent<SpriteRenderer>();
        if (bgSprite == null) return;
        
        float barWidth = bgSprite.bounds.size.x;
        float segmentWidth = barWidth / patienceSegments;
        
        for (int i = 1; i < patienceSegments; i++)
        {
            GameObject divider;
            
            if (dividerPrefab != null)
            {
                divider = Instantiate(dividerPrefab, dividersParent);
            }
            else
            {
                divider = new GameObject($"Divider_{i}");
                divider.transform.SetParent(dividersParent);
                
                SpriteRenderer sr = divider.AddComponent<SpriteRenderer>();
                sr.sprite = CreateLineSprite();
                sr.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); 
                sr.sortingOrder = bgSprite.sortingOrder + 2; 
            }
            
            float xPosition = -barWidth / 2 + (segmentWidth * i);
            divider.transform.localPosition = new Vector3(xPosition, 0, -0.01f);
            divider.transform.localScale = new Vector3(0.02f, bgSprite.bounds.size.y * 1.2f, 1f);
            
            dividers.Add(divider);
        }
    }
    
    Sprite CreateLineSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
    
    public void Initialize(Customer customer, int seat)
    {
        customerData = customer;
        currentPatience = customer.maxPatience;
        seatIndex = seat;
        isServed = false;
        
        if (patienceBarBackground != null)
        {
            patienceBarBackground.gameObject.SetActive(false);
        }
        if (patienceCanvas != null)
        {
            patienceCanvas.gameObject.SetActive(false);
        }
        
        if (customerSprite != null && customer.customerSprite != null)
        {
            customerSprite.sprite = customer.customerSprite;
        }
        
        if (customerAnimator != null)
        {
            customerAnimator.SetTrigger("SitDown");
        }
        
        UpdatePatienceBar();
        UpdateOrderDisplay(); // NEW: Update order display
    }
    
    void UpdateOrderDisplay()
    {
        
        
    }
    
    public void DecreasePatience()
    {
        if (isServed) return;
        
        int oldPatience = currentPatience;
        currentPatience -= customerData.patienceDecayPerTurn;
        currentPatience = Mathf.Max(0, currentPatience);
        UpdatePatienceBar();
        
        if (currentPatience <= 0)
        {
            OnCustomerAngry();
        }
    }
    
    void OnCustomerAngry()
    {
        ShowAngryEmote();
        PlaySound(angryEmoteSound);
        
        if (customerData.isBarbarian)
        {
            PlaySound(barbarianAngrySound);
            
            GameManager.Instance.AddCurseCard();
        }
        
        GameManager.Instance.LoseReputation(1);
        
        StartCoroutine(CustomerLeaveAngry());
    }
    
    IEnumerator CustomerLeaveAngry()
    {
        yield return new WaitForSeconds(1.5f);
        
        if (CustomerEntranceManager.Instance == null)
        {
            GameManager.Instance.RemoveCustomer(seatIndex);
            Destroy(gameObject);
            yield break;
        }
        
        if (customerAnimator != null)
        {
            customerAnimator.SetBool("IsWalking", true);
        }
        
        Transform doorPos = CustomerEntranceManager.Instance.doorPosition;
        if (doorPos != null)
        {
            yield return StartCoroutine(WalkToPosition(doorPos.position));
        }
        
        if (CustomerEntranceManager.Instance.doorTransform != null)
        {
            CustomerEntranceManager.Instance.StartCoroutine(
                OpenDoorAndPlaySound(CustomerEntranceManager.Instance.doorOpenRotation)
            );
            yield return new WaitForSeconds(0.5f);
        }
        
        Transform exitPos = CustomerEntranceManager.Instance.exitPosition;
        if (exitPos != null)
        {
            yield return StartCoroutine(WalkToPosition(exitPos.position));
        }
        else
        {
            Transform spawnPoint = CustomerEntranceManager.Instance.entranceSpawnPoint;
            if (spawnPoint != null)
            {
                yield return StartCoroutine(WalkToPosition(spawnPoint.position));
            }
        }
        
        if (CustomerEntranceManager.Instance.doorTransform != null)
        {
            CustomerEntranceManager.Instance.StartCoroutine(
                CloseDoorAndPlaySound(CustomerEntranceManager.Instance.doorClosedRotation)
            );
            yield return new WaitForSeconds(0.3f);
        }
        
        GameManager.Instance.RemoveCustomer(seatIndex);
        Destroy(gameObject);
    }
    
    public bool TryServeOrder(List<CardType> comboCards)
    {
        if (IsOrderCorrect(comboCards))
        {
            OnOrderCorrect();
            return true;
        }
        else
        {
            OnOrderWrong();
            return false;
        }
    }
    
    bool IsOrderCorrect(List<CardType> comboCards)
    {
        if (comboCards.Count != customerData.requiredCards.Count)
            return false;
        
        List<CardType> requiredCopy = new List<CardType>(customerData.requiredCards);
        List<CardType> comboCopy = new List<CardType>(comboCards);
        
        foreach (CardType card in comboCopy)
        {
            if (requiredCopy.Contains(card))
            {
                requiredCopy.Remove(card);
            }
            else
            {
                return false;
            }
        }
        
        return requiredCopy.Count == 0;
    }
    
    void OnOrderCorrect()
    {
        isServed = true;
        
        ShowHappyEmote();
        PlaySound(happyEmoteSound);
        
        if (customerAnimator != null)
        {
            customerAnimator.Play("Cheering");
        }
        
        GameManager.Instance.AddFocus(customerData.focusReward);
        
        if (customerData.isWizard && currentPatience > 5)
        {
            PlaySoundLimited(wizardHappySound, Random.Range(3f, 4f));
            
            GameManager.Instance.GiveWizardBoon();
        }
        
        GameManager.Instance.RemoveCustomer(seatIndex);
        
        StartCoroutine(CustomerLeaveHappy());
    }
    
    void OnOrderWrong()
    {
        ShowAngryEmote();
        PlaySound(angryEmoteSound);
        
        currentPatience -= 1;
        UpdatePatienceBar();
        
        if (currentPatience <= 0)
        {
            OnCustomerAngry();
        }
    }
    
    IEnumerator CustomerLeaveHappy()
    {
        yield return new WaitForSeconds(1.5f);
        
        if (CustomerEntranceManager.Instance == null)
        {
            Destroy(gameObject);
            yield break;
        }
        
        if (customerAnimator != null)
        {
            customerAnimator.SetBool("IsWalking", true);
        }
        
        Transform doorPos = CustomerEntranceManager.Instance.doorPosition;
        if (doorPos != null)
        {
            yield return StartCoroutine(WalkToPosition(doorPos.position));
        }
        
        if (CustomerEntranceManager.Instance.doorTransform != null)
        {
            CustomerEntranceManager.Instance.StartCoroutine(
                OpenDoorAndPlaySound(CustomerEntranceManager.Instance.doorOpenRotation)
            );
            yield return new WaitForSeconds(0.5f); 
        }
        
        Transform exitPos = CustomerEntranceManager.Instance.exitPosition;
        if (exitPos != null)
        {
            yield return StartCoroutine(WalkToPosition(exitPos.position));
        }
        else
        {
            Transform spawnPoint = CustomerEntranceManager.Instance.entranceSpawnPoint;
            if (spawnPoint != null)
            {
                yield return StartCoroutine(WalkToPosition(spawnPoint.position));
            }
        }
        
        if (CustomerEntranceManager.Instance.doorTransform != null)
        {
            CustomerEntranceManager.Instance.StartCoroutine(
                CloseDoorAndPlaySound(CustomerEntranceManager.Instance.doorClosedRotation)
            );
            yield return new WaitForSeconds(0.3f);
        }
        Destroy(gameObject);
    }
    
    IEnumerator WalkToPosition(Vector3 targetPosition)
    {
        float walkSpeed = CustomerEntranceManager.Instance.customerWalkSpeed;
        float rotationOffset = CustomerEntranceManager.Instance.customerRotationOffset;
        float stoppingDistance = 0.1f;
        
        while (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + rotationOffset;
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            }
            
            yield return null;
        }
        
        transform.position = targetPosition;
    }
    
    IEnumerator OpenDoorAndPlaySound(float targetRotation)
    {
        if (CustomerEntranceManager.Instance.audioSource != null && CustomerEntranceManager.Instance.doorOpenSound != null)
        {
            CustomerEntranceManager.Instance.audioSource.PlayOneShot(CustomerEntranceManager.Instance.doorOpenSound);
        }
        
        Transform door = CustomerEntranceManager.Instance.doorTransform;
        Quaternion startRotation = door.rotation;
        Quaternion targetRot = Quaternion.Euler(door.eulerAngles.x, targetRotation, door.eulerAngles.z);
        
        float elapsedTime = 0f;
        float duration = 1f / CustomerEntranceManager.Instance.doorRotationSpeed;
        
        while (elapsedTime < duration)
        {
            door.rotation = Quaternion.Slerp(startRotation, targetRot, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        door.rotation = targetRot;
    }
    
    IEnumerator CloseDoorAndPlaySound(float targetRotation)
    {
        if (CustomerEntranceManager.Instance.audioSource != null && CustomerEntranceManager.Instance.doorOpenSound != null)
        {
            CustomerEntranceManager.Instance.audioSource.PlayOneShot(CustomerEntranceManager.Instance.doorOpenSound);
        }
        
        Transform door = CustomerEntranceManager.Instance.doorTransform;
        Quaternion startRotation = door.rotation;
        Quaternion targetRot = Quaternion.Euler(door.eulerAngles.x, targetRotation, door.eulerAngles.z);
        
        float elapsedTime = 0f;
        float duration = 1f / CustomerEntranceManager.Instance.doorRotationSpeed;
        
        while (elapsedTime < duration)
        {
            door.rotation = Quaternion.Slerp(startRotation, targetRot, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        door.rotation = targetRot;
    }
    
    
    void UpdatePatienceBar()
    {
        if (patienceBarBackground != null && !patienceBarBackground.gameObject.activeSelf)
        {
            patienceBarBackground.gameObject.SetActive(true);
        }
        if (patienceCanvas != null && !patienceCanvas.gameObject.activeSelf)
        {
            patienceCanvas.gameObject.SetActive(true);
        }
        
        if (patienceText != null)
        {
            patienceText.text = $"{currentPatience}/{customerData.maxPatience}";
            
            float patiencePercent = (float)currentPatience / customerData.maxPatience;
            
            if (patiencePercent > 0.6f)
            {
                patienceText.color = new Color(0.2f, 0.8f, 0.2f); // Green
            }
            else if (patiencePercent > 0.3f)
            {
                patienceText.color = new Color(1f, 0.9f, 0.2f); // Yellow
            }
            else
            {
                patienceText.color = new Color(0.9f, 0.2f, 0.2f); // Red
            }
        }
        
            
            
    }
    
    public bool IsServed()
    {
        return isServed;
    }
    
    public Customer GetCustomer()
    {
        return customerData;
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
    
    void PlaySoundLimited(AudioClip clip, float duration)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            StartCoroutine(StopSoundAfterDelay(duration));
        }
        else if (clip == null)
        {
        }
        else if (audioSource == null)
        {
        }
    }
    
    public void ShowHappyEmote()
    {
        if (happyEmoteImage == null)
        {
            Debug.LogWarning("Happy emote image not assigned on " + gameObject.name);
            return;
        }
        
        StopCoroutine("HappyEmoteCoroutine");
        StopCoroutine("AngryEmoteCoroutine");
        StartCoroutine(HappyEmoteCoroutine());
    }
    
    public void ShowAngryEmote()
    {
        if (angryEmoteImage == null)
        {
            Debug.LogWarning("Angry emote image not assigned on " + gameObject.name);
            return;
        }
        
        StopCoroutine("HappyEmoteCoroutine");
        StopCoroutine("AngryEmoteCoroutine");
        StartCoroutine(AngryEmoteCoroutine());
    }
    
    IEnumerator HappyEmoteCoroutine()
    {
        yield return StartCoroutine(PlayEmoteAnimation(happyEmoteImage));
    }
    
    IEnumerator AngryEmoteCoroutine()
    {
        yield return StartCoroutine(PlayEmoteAnimation(angryEmoteImage));
    }
    
    IEnumerator PlayEmoteAnimation(UnityEngine.UI.Image emoteImage)
    {
        if (emoteImage == null) yield break;
        
        emoteImage.gameObject.SetActive(true);
        emoteImage.color = Color.white;
        
        RectTransform emoteRect = emoteImage.GetComponent<RectTransform>();
        if (emoteRect == null)
        {
            Debug.LogError("No RectTransform on emote image!");
            emoteImage.gameObject.SetActive(false);
            yield break;
        }
        
        Vector2 startPos = new Vector2(0.5f, 0.89f);
        Vector2 midPos = startPos + new Vector2(0f, 0.05f);
        Vector2 endPos = midPos + new Vector2(0f, 0.03f);
        
        emoteRect.anchoredPosition = startPos;
        emoteRect.localScale = Vector3.zero;
        
        CanvasGroup canvasGroup = emoteImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = emoteImage.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;
        
        float elapsedTime = 0f;
        while (elapsedTime < emotePopDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Sin((elapsedTime / emotePopDuration) * Mathf.PI * 0.5f);
            emoteRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, t);
            emoteRect.anchoredPosition = Vector2.Lerp(startPos, midPos, t * 0.5f);
            yield return null;
        }
        emoteRect.localScale = Vector3.one * 1.2f;
        emoteRect.anchoredPosition = midPos;
        
        yield return new WaitForSeconds(emoteDuration - emotePopDuration - emoteFadeDuration);
        
        elapsedTime = 0f;
        while (elapsedTime < emoteFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / emoteFadeDuration;
            canvasGroup.alpha = 1f - t;
            emoteRect.anchoredPosition = Vector2.Lerp(midPos, endPos, t);
            yield return null;
        }
        
        emoteImage.gameObject.SetActive(false);
        emoteRect.anchoredPosition = startPos;
        emoteRect.localScale = Vector3.zero;
        canvasGroup.alpha = 1f;
    }
    
    IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
