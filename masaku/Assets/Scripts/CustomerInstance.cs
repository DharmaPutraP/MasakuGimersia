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
    public Transform patienceBarTransform;
    public TextMeshProUGUI orderDisplayText; // NEW: Text to show order
    
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
        
        // Kurangi reputasi
        GameManager.Instance.LoseReputation(1);
        
        // Jika Barbarian, tambahkan curse
        if (customerData.isBarbarian)
        {
            Debug.Log("Barbarian menambahkan CURSE ke deck Anda!");
            GameManager.Instance.AddCurseCard();
        }
        
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
        // Update visual patience bar
        if (patienceBarTransform != null)
        {
            float patiencePercent = (float)currentPatience / customerData.maxPatience;
            patienceBarTransform.localScale = new Vector3(patiencePercent, 1f, 1f);
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
}
