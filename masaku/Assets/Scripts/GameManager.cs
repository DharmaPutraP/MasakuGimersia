using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game State")]
    public int currentDay = 1;
    public int reputation = 5;
    public int currentFocus = 3;
    public const int BASE_FOCUS = 3;
    public int bonusFocusNextTurn = 0;
    
    [Header("Customer Management")]
    public List<Customer> allCustomers; // Semua tipe customer
    public Transform[] customerSeats; // 4 kursi
    public GameObject customerPrefab;
    private CustomerInstance[] activeCustomers = new CustomerInstance[4];
    private Queue<Customer> currentDayDeck = new Queue<Customer>();
    
    [Header("Day Configuration")]
    public List<DayConfiguration> dayConfigurations;
    
    [Header("Preparation Station")]
    public List<CardType> preparationStation = new List<CardType>(); // Kombo kartu yang sedang disiapkan
    
    [Header("References")]
    public MasakuCardManager cardManager;
    
    private bool isPlayerTurn = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        StartDay(currentDay);
    }
    
    public void StartDay(int day)
    {
        currentDay = day;
        Debug.Log($"=== HARI {currentDay} DIMULAI ===");
        
        // Load deck untuk hari ini
        LoadDayDeck(day);
        
        // Isi 4 kursi dengan customer pertama
        FillEmptySeats();
        
        // Mulai giliran player
        StartPlayerTurn();
    }
    
    void LoadDayDeck(int day)
    {
        currentDayDeck.Clear();
        
        if (day <= 0 || day > dayConfigurations.Count)
        {
            Debug.LogError($"Konfigurasi untuk Hari {day} tidak ditemukan!");
            return;
        }
        
        DayConfiguration config = dayConfigurations[day - 1];
        
        foreach (CustomerType type in config.customerQueue)
        {
            Customer customer = GetCustomerByType(type);
            if (customer != null)
            {
                currentDayDeck.Enqueue(customer);
            }
        }
        
        Debug.Log($"Deck Hari {day} dimuat: {currentDayDeck.Count} pelanggan");
    }
    
    Customer GetCustomerByType(CustomerType type)
    {
        foreach (Customer customer in allCustomers)
        {
            if (customer.customerType == type)
                return customer;
        }
        return null;
    }
    
    public void FillEmptySeats()
    {
        for (int i = 0; i < customerSeats.Length; i++)
        {
            if (activeCustomers[i] == null && currentDayDeck.Count > 0)
            {
                Customer nextCustomer = currentDayDeck.Dequeue();
                SpawnCustomer(nextCustomer, i);
            }
        }
    }
    
    void SpawnCustomer(Customer customer, int seatIndex)
    {
        if (customerSeats[seatIndex] == null)
        {
            Debug.LogError($"Seat {seatIndex} tidak ditemukan!");
            return;
        }
        
        GameObject customerObj = Instantiate(customerPrefab, customerSeats[seatIndex].position, Quaternion.identity, customerSeats[seatIndex]);
        CustomerInstance instance = customerObj.GetComponent<CustomerInstance>();
        
        if (instance != null)
        {
            instance.Initialize(customer, seatIndex);
            activeCustomers[seatIndex] = instance;
        }
    }
    
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        
        // Reset fokus + bonus dari wizard
        currentFocus = BASE_FOCUS + bonusFocusNextTurn;
        bonusFocusNextTurn = 0;
        
        // Tarik kartu hingga 5
        if (cardManager != null)
        {
            cardManager.DrawToHandSize();
        }
        
        // Kosongkan preparation station
        preparationStation.Clear();
        
        Debug.Log($"--- Giliran Player Dimulai (Fokus: {currentFocus}) ---");
    }
    
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;
        
        isPlayerTurn = false;
        Debug.Log("--- Giliran Player Berakhir ---");
        
        // Buang semua kartu di tangan
        if (cardManager != null)
        {
            cardManager.DiscardHand();
        }
        
        // Buang kartu di preparation station
        preparationStation.Clear();
        
        // Fase Kesabaran: Semua customer kehilangan patience
        StartCoroutine(PatiencePhase());
    }
    
    IEnumerator PatiencePhase()
    {
        Debug.Log("--- Fase Kesabaran ---");
        
        foreach (CustomerInstance customer in activeCustomers)
        {
            if (customer != null)
            {
                customer.DecreasePatience();
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        // Cek apakah hari sudah selesai
        if (IsDayComplete())
        {
            EndDay();
        }
        else
        {
            // Isi kursi kosong
            FillEmptySeats();
            
            // Mulai giliran baru
            yield return new WaitForSeconds(1f);
            StartPlayerTurn();
        }
    }
    
    bool IsDayComplete()
    {
        // Hari selesai jika deck kosong dan semua kursi kosong
        if (currentDayDeck.Count > 0)
            return false;
        
        foreach (CustomerInstance customer in activeCustomers)
        {
            if (customer != null)
                return false;
        }
        
        return true;
    }
    
    void EndDay()
    {
        Debug.Log($"=== HARI {currentDay} SELESAI ===");
        Debug.Log($"Sisa Reputasi: {reputation}");
        
        if (reputation <= 0)
        {
            GameOver();
        }
        else if (currentDay >= 7)
        {
            Victory();
        }
        else
        {
            // Lanjut ke hari berikutnya
            StartCoroutine(PrepareNextDay());
        }
    }
    
    IEnumerator PrepareNextDay()
    {
        yield return new WaitForSeconds(2f);
        
        // Reset deck kartu aksi
        if (cardManager != null)
        {
            cardManager.ResetDeck();
        }
        
        StartDay(currentDay + 1);
    }
    
    public void AddCardToPreparation(CardType cardType)
    {
        preparationStation.Add(cardType);
        Debug.Log($"Kartu {cardType} ditambahkan ke Stasiun Persiapan. Total: {preparationStation.Count}");
    }
    
    public void SubmitOrder(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= activeCustomers.Length)
        {
            Debug.LogWarning("Indeks kursi tidak valid!");
            return;
        }
        
        if (activeCustomers[seatIndex] == null)
        {
            Debug.LogWarning("Tidak ada customer di kursi ini!");
            return;
        }
        
        if (preparationStation.Count == 0)
        {
            Debug.LogWarning("Stasiun Persiapan kosong!");
            return;
        }
        
        Debug.Log($"Mengirim pesanan ke {activeCustomers[seatIndex].customerData.customerName}...");
        
        // Coba layani customer
        bool success = activeCustomers[seatIndex].TryServeOrder(new List<CardType>(preparationStation));
        
        // Kosongkan stasiun persiapan
        preparationStation.Clear();
        
        if (success)
        {
            Debug.Log("Pesanan BERHASIL!");
        }
        else
        {
            Debug.Log("Pesanan GAGAL!");
        }
    }
    
    public void RemoveCustomer(int seatIndex)
    {
        if (seatIndex >= 0 && seatIndex < activeCustomers.Length)
        {
            activeCustomers[seatIndex] = null;
        }
    }
    
    public void AddFocus(int amount)
    {
        currentFocus += amount;
        Debug.Log($"Fokus +{amount}. Total: {currentFocus}");
    }
    
    public bool SpendFocus(int amount)
    {
        if (currentFocus >= amount)
        {
            currentFocus -= amount;
            return true;
        }
        return false;
    }
    
    public void LoseReputation(int amount)
    {
        reputation -= amount;
        reputation = Mathf.Max(0, reputation);
        Debug.Log($"Reputasi -{amount}. Sisa: {reputation}");
        
        if (reputation <= 0)
        {
            GameOver();
        }
    }
    
    public void AddCurseCard()
    {
        if (cardManager != null)
        {
            cardManager.AddCurseToDiscard();
        }
    }
    
    public void GiveWizardBoon()
    {
        // Random: Tarik 2 kartu ATAU +1 fokus next turn
        int random = Random.Range(0, 2);
        
        if (random == 0)
        {
            Debug.Log("Wizard Boon: Tarik 2 kartu sekarang!");
            if (cardManager != null)
            {
                cardManager.DrawCards(2);
            }
        }
        else
        {
            Debug.Log("Wizard Boon: +1 Fokus di awal giliran berikutnya!");
            bonusFocusNextTurn += 1;
        }
    }
    
    void GameOver()
    {
        Debug.Log("=== GAME OVER ===");
        Debug.Log("Reputasi Anda habis!");
        // TODO: Load Game Over scene
    }
    
    void Victory()
    {
        Debug.Log("=== VICTORY ===");
        Debug.Log("Selamat! Anda memenangkan Golden Bean Award!");
        // TODO: Load Victory scene
    }
}

[System.Serializable]
public class DayConfiguration
{
    public int day;
    public string dayName;
    public List<CustomerType> customerQueue;
}
