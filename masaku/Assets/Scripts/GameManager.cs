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
    private bool isDayEnding = false; 
    
    [Header("Customer Management")]
    public List<Customer> allCustomers; 
    public Transform[] customerSeats; 
    public GameObject customerPrefab;
    private CustomerInstance[] activeCustomers = new CustomerInstance[4];
    private bool[] seatReserved = new bool[4]; 
    
    public CustomerInstance[] GetActiveCustomers()
    {
        return activeCustomers;
    }
    
    public void SetCustomerAtSeat(int seatIndex, CustomerInstance customer)
    {
        if (seatIndex >= 0 && seatIndex < activeCustomers.Length)
        {
            activeCustomers[seatIndex] = customer;
            seatReserved[seatIndex] = false; 
        }
    }
    
    public bool IsSeatAvailable(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= activeCustomers.Length)
            return false;
        
        return activeCustomers[seatIndex] == null && !seatReserved[seatIndex];
    }
    
    private Queue<Customer> currentDayDeck = new Queue<Customer>();
    
    [Header("Day Configuration")]
    public List<DayConfiguration> dayConfigurations;
    
    [Header("Preparation Station")]
    public List<CardType> preparationStation = new List<CardType>(); 
    
    [Header("References")]
    public MasakuCardManager cardManager;
    
    [Header("Special Cards")]
    public ActionCard wizardBoonDraw; 
    public ActionCard wizardBoonFocus; 
    
    public bool isPlayerTurn = true;
    
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
    
    void Update()
    {
    }
    
    public void StartDay(int day)
    {
        currentDay = day;
        isDayEnding = false; 
        LoadDayDeck(day);
        
        FillEmptySeats();
        
        StartPlayerTurn();
    }
    
    void LoadDayDeck(int day)
    {
        currentDayDeck.Clear();
        
        if (day <= 0 || day > dayConfigurations.Count)
        {
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
            if (IsSeatAvailable(i) && currentDayDeck.Count > 0)
            {
                Customer nextCustomer = currentDayDeck.Dequeue();
                SpawnCustomer(nextCustomer, i);
                customersSpawned = true;
            }
        }
        
        if (customersSpawned && MasakuUI.Instance != null)
        {
            MasakuUI.Instance.UpdateCustomerMenus();
        }
    }
    
    void SpawnCustomer(Customer customer, int seatIndex)
    {
        if (customerSeats[seatIndex] == null)
        {
            return;
        }
        
        GameObject prefabToSpawn = customer.customerPrefab != null ? customer.customerPrefab : customerPrefab;
        
        if (prefabToSpawn == null)
        {
            return;
        }

        if (CustomerEntranceManager.Instance != null)
        {
            seatReserved[seatIndex] = true;
            
            CustomerEntranceManager.Instance.QueueCustomerEntrance(
                prefabToSpawn, 
                customer, 
                seatIndex, 
                customerSeats[seatIndex]
            );
        }
        else
        {
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
            }
        }
    }
    
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        
        currentFocus = BASE_FOCUS + bonusFocusNextTurn;
        bonusFocusNextTurn = 0;
        
        if (cardManager != null)
        {
            cardManager.DrawToHandSize();
        }
        
        preparationStation.Clear();
        
        if (MasakuUI.Instance != null)
        {
            MasakuUI.Instance.UpdateHandDisplay();
            StartCoroutine(DelayedUpdateCustomerMenus());
        }
    }
    
    IEnumerator DelayedUpdateCustomerMenus()
    {
        yield return new WaitForSeconds(0.1f);
        if (MasakuUI.Instance != null)
        {
            MasakuUI.Instance.UpdateCustomerMenus();
        }
    }
    
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;
        
        isPlayerTurn = false;
        if (cardManager != null)
        {
            cardManager.ClearSelection();
        }
        
        if (cardManager != null)
        {
            cardManager.DiscardHand();
        }
        
        preparationStation.Clear();
        
        StartCoroutine(PatiencePhase());
    }
    
    IEnumerator PatiencePhase()
    {
        int customerCount = 0;
        foreach (CustomerInstance customer in activeCustomers)
        {
            if (customer != null)
            {
                customerCount++;
                customer.DecreasePatience();
                yield return new WaitForSeconds(0.2f);
            }
        }
        if (isDayEnding)
        {
            yield break;
        }
        
        if (IsDayComplete())
        {
            EndDay();
        }
        else
        {
            FillEmptySeats();
            
            yield return new WaitForSeconds(0.3f);
            StartPlayerTurn();
        }
    }
    
    bool IsDayComplete()
    {
        if (currentDayDeck.Count > 0)
            return false;
        
        if (CustomerEntranceManager.Instance != null && CustomerEntranceManager.Instance.HasCustomersWaiting())
        {
            return false;
        }
        
        int activeCount = 0;
        foreach (CustomerInstance customer in activeCustomers)
        {
            if (customer != null)
            {
                activeCount++;
            }
        }
        return activeCount == 0;
    }
    
    void EndDay()
    {
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
            StartCoroutine(TransitionToNextDay());
        }
    }
    
    IEnumerator TransitionToNextDay()
    {
        if (ScreenFade.Instance != null)
        {
            yield return ScreenFade.Instance.FadeOut();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        currentDay++;
        isDayEnding = false; 
        
        if (cardManager != null)
        {
            cardManager.ResetDeck();
        }
        
        LoadDayDeck(currentDay);
        
        yield return new WaitForSeconds(0.5f);
        
        if (ScreenFade.Instance != null)
        {
            yield return ScreenFade.Instance.FadeIn();
        }
        FillEmptySeats();
        StartPlayerTurn();
    }
    
    IEnumerator PrepareNextDay()
    {
        yield return new WaitForSeconds(2f);
        
        if (cardManager != null)
        {
            cardManager.ResetDeck();
        }
        
        StartDay(currentDay + 1);
    }
    
    public void AddCardToPreparation(CardType cardType)
    {
        preparationStation.Add(cardType);
    }
    
    public void SubmitOrder(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= activeCustomers.Length)
        {
            return;
        }
        
        if (activeCustomers[seatIndex] == null)
        {
            return;
        }
        
        if (preparationStation.Count == 0)
        {
            return;
        }
        bool success = activeCustomers[seatIndex].TryServeOrder(new List<CardType>(preparationStation));
        
        preparationStation.Clear();
        
        if (success)
        {
        }
        else
        {
        }
    }
    
    public void RemoveCustomer(int seatIndex)
    {
        if (seatIndex >= 0 && seatIndex < activeCustomers.Length)
        {
            activeCustomers[seatIndex] = null;
            seatReserved[seatIndex] = false; 
            if (MasakuUI.Instance != null)
            {
                MasakuUI.Instance.UpdateCustomerMenus();
            }
            
            CheckAndEndDayIfComplete();
        }
    }
    
    void CheckAndEndDayIfComplete()
    {
        if (IsDayComplete() && !isDayEnding)
        {
            isDayEnding = true; 
            EndDay();
        }
    }
    
    public void AddFocus(int amount)
    {
        currentFocus += amount;
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
        }
        else if (randomBoon == 1 && wizardBoonFocus != null)
        {
            cardManager.AddCardToDeck(wizardBoonFocus);
        }
    }
    
    void GameOver()
    {
        PlayerPrefs.SetInt("ShowLosePanel", 1);
        PlayerPrefs.Save();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    void Victory()
    {
        PlayerPrefs.SetInt("ShowEndingCutscene", 1);
        PlayerPrefs.Save();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

[System.Serializable]
public class DayConfiguration
{
    public int day;
    public string dayName;
    public List<CustomerType> customerQueue;
}
