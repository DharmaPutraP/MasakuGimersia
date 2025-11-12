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
    public SpriteRenderer customerSprite; // Keep for backward compatibility
    public Animator customerAnimator; // NEW: Animator for customer animations
    public Transform patienceBarTransform; // The foreground bar that scales
    public Transform patienceBarBackground; // Optional: Background bar
    public SpriteRenderer patienceBarSprite; // To change color based on patience
    public TextMeshProUGUI orderDisplayText; // NEW: Text to show order
    
    [Header("Sound Effects")]
    public AudioSource audioSource; // AudioSource for customer sounds
    public AudioClip barbarianAngrySound; // Sound when Barbarian gets angry
    public AudioClip wizardHappySound; // Sound when Wizard is satisfied (magical sound)
    
    private bool isServed = false;
    
    public void Initialize(Customer customer, int seat)
    {
        customerData = customer;
        currentPatience = customer.maxPatience;
        seatIndex = seat;
        isServed = false;
        
        // Use sprite renderer if no animator
        if (customerSprite != null && customer.customerSprite != null)
        {
            customerSprite.sprite = customer.customerSprite;
        }
        
        // Play sitting animation
        if (customerAnimator != null)
        {
            customerAnimator.SetTrigger("Sit");
            // Or use: customerAnimator.Play("Sitting");
        }
        
        UpdatePatienceBar();
        UpdateOrderDisplay(); // NEW: Update order display
        Debug.Log($"{customer.customerName} duduk di kursi {seat} dengan kesabaran {currentPatience}");
    }
    
    // NEW: Method to display required order
    void UpdateOrderDisplay()
    {
        // if (orderDisplayText == null) return;
        
        // string orderText = "";
        // foreach (CardType cardType in customerData.requiredCards)
        // {
        //     orderText += cardType.ToString() + " ";
        // }
        
        // orderDisplayText.text = orderText;
    }
    
    public void DecreasePatience()
    {
        if (isServed) return;
        
        int oldPatience = currentPatience;
        currentPatience -= customerData.patienceDecayPerTurn;
        currentPatience = Mathf.Max(0, currentPatience);
        
        Debug.Log($"{customerData.customerName} patience: {oldPatience} → {currentPatience} (max: {customerData.maxPatience})");
        
        UpdatePatienceBar();
        
        if (currentPatience <= 0)
        {
            Debug.Log($"{customerData.customerName} patience reached 0! Leaving angry...");
            OnCustomerAngry();
        }
    }
    
    void OnCustomerAngry()
    {
        Debug.Log($"{customerData.customerName} pergi dengan marah!");
        
        // Jika Barbarian, tambahkan curse dan kurangi reputasi ekstra
        if (customerData.isBarbarian)
        {
            Debug.Log("Barbarian menambahkan CURSE ke deck Anda!");
            
            // Play Barbarian angry sound
            PlaySound(barbarianAngrySound);
            
            GameManager.Instance.AddCurseCard();
        }
        
        // Kurangi reputasi (normal customer leaving)
        GameManager.Instance.LoseReputation(1);
        
        // Hapus customer dari kursi
        GameManager.Instance.RemoveCustomer(seatIndex);
        Destroy(gameObject);
    }
    
    public bool TryServeOrder(List<CardType> comboCards)
    {
        // Cek apakah kombo sesuai dengan pesanan
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
        // Harus memiliki jumlah kartu yang sama
        if (comboCards.Count != customerData.requiredCards.Count)
            return false;
        
        // Buat list sementara untuk pengecekan
        List<CardType> requiredCopy = new List<CardType>(customerData.requiredCards);
        List<CardType> comboCopy = new List<CardType>(comboCards);
        
        // Cek setiap kartu dalam combo
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
        
        // Jika semua kartu cocok, requiredCopy harus kosong
        return requiredCopy.Count == 0;
    }
    
    void OnOrderCorrect()
    {
        isServed = true;
        Debug.Log($"{customerData.customerName} puas! Memberikan +{customerData.focusReward} Fokus");
        
        // Play cheering animation
        if (customerAnimator != null)
        {
            customerAnimator.SetTrigger("Cheer");
            // Or use: customerAnimator.Play("Cheering");
        }
        
        // Berikan reward fokus
        GameManager.Instance.AddFocus(customerData.focusReward);
        
        // Jika Wizard dan dilayani cepat (>5 patience), beri boon
        if (customerData.isWizard && currentPatience > 5)
        {
            Debug.Log("Wizard memberikan BOON!");
            
            // Play Wizard happy/magical sound (limited to 3-4 seconds)
            PlaySoundLimited(wizardHappySound, Random.Range(3f, 4f));
            
            GameManager.Instance.GiveWizardBoon();
        }
        
        // Hapus customer dari kursi
        GameManager.Instance.RemoveCustomer(seatIndex);
        
        // Animasi customer senang dan pergi
        StartCoroutine(CustomerLeaveHappy());
    }
    
    void OnOrderWrong()
    {
        Debug.Log($"{customerData.customerName} pesanan salah! Kehilangan 1 Kesabaran");
        currentPatience -= 1;
        UpdatePatienceBar();
        
        if (currentPatience <= 0)
        {
            OnCustomerAngry();
        }
    }
    
    IEnumerator CustomerLeaveHappy()
    {
        // Wait for cheer animation to play
        yield return new WaitForSeconds(1.5f);
        
        // Optional: Play leaving animation
        if (customerAnimator != null)
        {
            customerAnimator.SetTrigger("Leave");
            // Or use: customerAnimator.Play("Leaving");
            yield return new WaitForSeconds(0.5f);
        }
        
        Destroy(gameObject);
    }
    
    void UpdatePatienceBar()
    {
        if (patienceBarTransform == null) return;
        
        // Calculate patience percentage
        float patiencePercent = (float)currentPatience / customerData.maxPatience;
        
        // Update bar scale
        patienceBarTransform.localScale = new Vector3(patiencePercent, 1f, 1f);
        
        // Change bar color based on patience level
        if (patienceBarSprite != null)
        {
            if (patiencePercent > 0.6f)
            {
                // High patience - Green
                patienceBarSprite.color = new Color(0.2f, 0.8f, 0.2f); // Green
            }
            else if (patiencePercent > 0.3f)
            {
                // Medium patience - Yellow
                patienceBarSprite.color = new Color(1f, 0.9f, 0.2f); // Yellow
            }
            else
            {
                // Low patience - Red
                patienceBarSprite.color = new Color(0.9f, 0.2f, 0.2f); // Red
            }
        }
        
        Debug.Log($"{customerData.customerName} patience bar: {patiencePercent * 100:F0}%");
    }
    
    public bool IsServed()
    {
        return isServed;
    }
    
    public Customer GetCustomer()
    {
        return customerData;
    }
    
    // Helper method to play customer sounds
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"Playing customer sound: {clip.name}");
        }
        else if (clip == null)
        {
            Debug.LogWarning("Customer sound clip is not assigned!");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is not assigned in CustomerInstance!");
        }
    }
    
    // Helper method to play sound with time limit
    void PlaySoundLimited(AudioClip clip, float duration)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            Debug.Log($"Playing customer sound (limited to {duration}s): {clip.name}");
            
            // Stop the sound after duration
            StartCoroutine(StopSoundAfterDelay(duration));
        }
        else if (clip == null)
        {
            Debug.LogWarning("Customer sound clip is not assigned!");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is not assigned in CustomerInstance!");
        }
    }
    
    // Coroutine to stop sound after delay
    IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Wizard sound stopped after time limit");
        }
    }
}
