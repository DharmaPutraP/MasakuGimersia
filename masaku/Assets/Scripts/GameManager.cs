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
    private bool[] seatReserved = new bool[4]; // Track if seat is reserved during entrance animation
    
    public CustomerInstance[] GetActiveCustomers()
    {
        return activeCustomers;
    }
    
    public void SetCustomerAtSeat(int seatIndex, CustomerInstance customer)
    {
        if (seatIndex >= 0 && seatIndex < activeCustomers.Length)
        {
            activeCustomers[seatIndex] = customer;
            seatReserved[seatIndex] = false; // Seat is now filled, not just reserved
        }
    }
    
    public bool IsSeatAvailable(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= activeCustomers.Length)
            return false;
        
        // Seat is available if no customer AND not reserved
        return activeCustomers[seatIndex] == null && !seatReserved[seatIndex];
    }
    
    private Queue<Customer> currentDayDeck = new Queue<Customer>();
    
    [Header("Day Configuration")]
    public List<DayConfiguration> dayConfigurations;
    
    [Header("Preparation Station")]
    public List<CardType> preparationStation = new List<CardType>(); // Kombo kartu yang sedang disiapkan
    
    [Header("References")]
    public MasakuCardManager cardManager;
    
    [Header("Special Cards")]
    public ActionCard wizardBoonDraw; 
    public ActionCard wizardBoonFocus; 
    
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
        bool customersSpawned = false;
        
        for (int i = 0; i < customerSeats.Length; i++)
        {
            // Only spawn if seat is truly available (not occupied and not reserved)
            if (IsSeatAvailable(i) && currentDayDeck.Count > 0)
            {
                Customer nextCustomer = currentDayDeck.Dequeue();
                SpawnCustomer(nextCustomer, i);
                customersSpawned = true;
                
                Debug.Log($"Spawning {nextCustomer.customerName} at seat {i}. Remaining in deck: {currentDayDeck.Count}");
            }
        }
        
        // Update UI menus after spawning new customers
        if (customersSpawned && MasakuUI.Instance != null)
        {
            MasakuUI.Instance.UpdateCustomerMenus();
        }
    }
    
    void SpawnCustomer(Customer customer, int seatIndex)
    {
        if (customerSeats[seatIndex] == null)
        {
            Debug.LogError($"Seat {seatIndex} tidak ditemukan!");
            return;
        }
        
        // Use customer-specific prefab if available, otherwise use default
        GameObject prefabToSpawn = customer.customerPrefab != null ? customer.customerPrefab : customerPrefab;
        
        if (prefabToSpawn == null)
        {
            Debug.LogError($"No prefab available for {customer.customerName}!");
            return;
        }

        // Check if entrance manager exists for animated entrance
        if (CustomerEntranceManager.Instance != null)
        {
            // Reserve the seat immediately to prevent double-spawning
            seatReserved[seatIndex] = true;
            
            // Use entrance animation system
            CustomerEntranceManager.Instance.QueueCustomerEntrance(
                prefabToSpawn, 
                customer, 
                seatIndex, 
                customerSeats[seatIndex]
            );
            
            Debug.Log($"Queued {customer.customerName} for entrance animation to seat {seatIndex} (seat now reserved)");
        }
        else
        {
            // Fallback: Instant spawn (old behavior)
            Quaternion faceCamera = Quaternion.Euler(0, 180, 0);
            GameObject customerObj = Instantiate(prefabToSpawn, customerSeats[seatIndex].position, faceCamera, customerSeats[seatIndex]);
            CustomerInstance instance = customerObj.GetComponent<CustomerInstance>();
            
            if (instance != null)
            {
                instance.Initialize(customer, seatIndex);
                activeCustomers[seatIndex] = instance;
            }
            else
            {
                Debug.LogError($"Customer prefab for {customer.customerName} is missing CustomerInstance component!");
            }
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
        
        // Update UI to show new hand
        if (MasakuUI.Instance != null)
        {
            MasakuUI.Instance.UpdateHandDisplay();
        }
        
        Debug.Log($"--- Giliran Player Dimulai (Fokus: {currentFocus}) ---");
    }
    
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;
        
        isPlayerTurn = false;
        Debug.Log("--- Giliran Player Berakhir ---");
        
        // Clear selected cards before discarding hand
        if (cardManager != null)
        {
            cardManager.ClearSelection();
        }
        
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
        
        int customerCount = 0;
        foreach (CustomerInstance customer in activeCustomers)
        {
            if (customer != null)
            {
                customerCount++;
                Debug.Log($"Customer {customer.GetCustomer().customerName} losing patience...");
                customer.DecreasePatience();
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        Debug.Log($"Patience phase complete. Customers remaining: {customerCount}");
        
        // Cek apakah hari sudah selesai
        if (IsDayComplete())
        {
            Debug.Log("Day is complete! Moving to next day...");
            EndDay();
        }
        else
        {
            Debug.Log("Day continues. Filling empty seats and starting new turn...");
            
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
        Debug.Log($"Checking if day complete. Deck count: {currentDayDeck.Count}");
        
        if (currentDayDeck.Count > 0)
            return false;
        
        // Check if there are customers waiting to enter
        if (CustomerEntranceManager.Instance != null && CustomerEntranceManager.Instance.HasCustomersWaiting())
        {
            Debug.Log("Customers still waiting in entrance queue");
            return false;
        }
        
        int activeCount = 0;
        foreach (CustomerInstance customer in activeCustomers)
        {
            if (customer != null)
            {
                activeCount++;
                Debug.Log($"Active customer found: {customer.GetCustomer().customerName}");
            }
        }
        
        Debug.Log($"Active customers: {activeCount}");
        return activeCount == 0;
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
            // Automatic transition to next day with fade
            StartCoroutine(TransitionToNextDay());
        }
    }
    
    IEnumerator TransitionToNextDay()
    {
        Debug.Log("Starting day transition...");
        
        // Fade out
        if (ScreenFade.Instance != null)
        {
            yield return ScreenFade.Instance.FadeOut();
        }
        else
        {
            // Fallback if no fade system
            yield return new WaitForSeconds(0.5f);
        }
        
        // Prepare next day while screen is dark
        currentDay++;
        
        // Reset deck kartu aksi
        if (cardManager != null)
        {
            cardManager.ResetDeck();
        }
        
        // Load new day deck
        LoadDayDeck(currentDay);
        
        // Small delay while black
        yield return new WaitForSeconds(0.5f);
        
        // Fade in
        if (ScreenFade.Instance != null)
        {
            yield return ScreenFade.Instance.FadeIn();
        }
        
        Debug.Log($"=== HARI {currentDay} DIMULAI ===");
        
        // Fill seats and start new day
        FillEmptySeats();
        StartPlayerTurn();
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
            seatReserved[seatIndex] = false; // Clear reservation
            Debug.Log($"Seat {seatIndex} is now empty and available");
            
            // Update UI to remove customer menu
            if (MasakuUI.Instance != null)
            {
                MasakuUI.Instance.UpdateCustomerMenus();
            }
            
            // Check if day is complete after removing customer
            CheckAndEndDayIfComplete();
        }
    }
    
    void CheckAndEndDayIfComplete()
    {
        // Check if day is complete and trigger transition
        if (IsDayComplete())
        {
            Debug.Log("All customers served! Day complete - automatically transitioning...");
            EndDay();
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
        int randomBoon = Random.Range(0, 2);
        
        if (randomBoon == 0 && wizardBoonDraw != null)
        {
            cardManager.AddCardToDeck(wizardBoonDraw);
            Debug.Log("Wizard memberikan BOON: Draw 2 Cards!");
        }
        else if (randomBoon == 1 && wizardBoonFocus != null)
        {
            cardManager.AddCardToDeck(wizardBoonFocus);
            Debug.Log("Wizard memberikan BOON: +1 Focus Next Turn!");
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
