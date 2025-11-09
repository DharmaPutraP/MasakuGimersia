using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("Project Masaku - Setup Instructions")]
    [TextArea(15, 30)]
    public string setupInstructions = @"
=== PROJECT MASAKU - SETUP GUIDE ===
Slay The Spire + Overcooked/Dinner Dash

1. BUAT CUSTOMER SCRIPTABLEOBJECTS:
   - Klik kanan > Create > Masaku > Customer
   - Buat 4 customer:
   
   KNIGHT:
   - Order: 1x Potong Daging + 1x Panaskan Air
   - Max Patience: 10
   - Patience Decay: 2 (Tidak Sabaran)
   - Focus Reward: 3
   
   ELF:
   - Order: 2x Potong Sayuran + 1x Panaskan Air
   - Max Patience: 20
   - Patience Decay: 1 (Sabar)
   - Focus Reward: 5
   
   BARBARIAN:
   - Order: 1x Potong Daging + 1x Panaskan Daging
   - Max Patience: 5
   - Patience Decay: 2
   - Focus Reward: 3
   - Is Barbarian: TRUE
   
   WIZARD:
   - Order: 1x Potong Sayuran + 1x Potong Daging + 1x Panaskan Air
   - Max Patience: 10
   - Patience Decay: 2
   - Focus Reward: 2
   - Is Wizard: TRUE

2. BUAT ACTION CARDS:
   - Klik kanan > Create > Cards > Action Card
   - Buat 5 kartu dasar:
   * Potong Sayuran (Fokus: 1)
   * Potong Daging (Fokus: 1)
   * Panaskan Air (Fokus: 1)
   * Panaskan Daging (Fokus: 1)
   * Tarik Nafas (Special: true, Discard: 1, Draw: 1)
   * Barbarian Curse (untuk curse mechanic)

3. SETUP GAME MANAGER:
   - Buat GameObject 'GameManager'
   - Tambahkan script GameManager
   - Assign 4 Customer ScriptableObjects
   - Setup 4 Customer Seats (Transform)
   - Assign Customer Prefab
   - Setup Day Configurations (7 hari)

4. SETUP CARD MANAGER:
   - Buat GameObject 'MasakuCardManager'
   - Assign 5 Base Action Cards
   - Assign Curse Card

5. SETUP UI:
   - Buat Canvas dengan MasakuUI script
   - Setup Focus Display
   - Setup Reputation Display (5 hearts)
   - Setup Hand Container
   - Setup Preparation Station Display
   - Setup Submit & End Turn Buttons
   - Setup 4 Seat Selection Buttons

6. DAY CONFIGURATIONS (di GameManager):
   Hari 1: [Knight, Knight, Knight]
   Hari 2: [Knight, Elf, Knight]
   Hari 3: [Knight, Barbarian, Elf, Knight]
   Hari 4: [Knight, Wizard, Barbarian, Elf]
   Hari 5: [Knight, Barbarian, Elf, Knight, Barbarian, Wizard]
   Hari 6: [Knight, Elf, Wizard, Knight, Barbarian, Elf, Knight]
   Hari 7: [Barbarian, Wizard, Elf, Barbarian, Wizard, Elf, Barbarian]
";
    
    [Header("Quick Test Controls")]
    public KeyCode testPlayCard1 = KeyCode.Alpha1;
    public KeyCode testPlayCard2 = KeyCode.Alpha2;
    public KeyCode testPlayCard3 = KeyCode.Alpha3;
    public KeyCode testPlayCard4 = KeyCode.Alpha4;
    public KeyCode testPlayCard5 = KeyCode.Alpha5;
    public KeyCode testSubmitToSeat1 = KeyCode.Q;
    public KeyCode testSubmitToSeat2 = KeyCode.W;
    public KeyCode testSubmitToSeat3 = KeyCode.E;
    public KeyCode testSubmitToSeat4 = KeyCode.R;
    public KeyCode testEndTurn = KeyCode.Space;
    
    void Update()
    {
        if (GameManager.Instance == null || MasakuCardManager.Instance == null) return;
        
        // Test play cards
        if (Input.GetKeyDown(testPlayCard1))
        {
            MasakuCardManager.Instance.PlayCardByIndex(0);
        }
        if (Input.GetKeyDown(testPlayCard2))
        {
            MasakuCardManager.Instance.PlayCardByIndex(1);
        }
        if (Input.GetKeyDown(testPlayCard3))
        {
            MasakuCardManager.Instance.PlayCardByIndex(2);
        }
        if (Input.GetKeyDown(testPlayCard4))
        {
            MasakuCardManager.Instance.PlayCardByIndex(3);
        }
        if (Input.GetKeyDown(testPlayCard5))
        {
            MasakuCardManager.Instance.PlayCardByIndex(4);
        }
        
        // Test submit order to seats
        if (Input.GetKeyDown(testSubmitToSeat1))
        {
            GameManager.Instance.SubmitOrder(0);
        }
        if (Input.GetKeyDown(testSubmitToSeat2))
        {
            GameManager.Instance.SubmitOrder(1);
        }
        if (Input.GetKeyDown(testSubmitToSeat3))
        {
            GameManager.Instance.SubmitOrder(2);
        }
        if (Input.GetKeyDown(testSubmitToSeat4))
        {
            GameManager.Instance.SubmitOrder(3);
        }
        
        // Test end turn
        if (Input.GetKeyDown(testEndTurn))
        {
            GameManager.Instance.EndPlayerTurn();
        }
    }
}
