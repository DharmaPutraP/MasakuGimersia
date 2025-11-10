# Complete Unity Setup Guide - Masaku Gimersia

## From First Open to Playable Game

---

## 📋 What You Have (Assets Checklist)

Before starting, make sure you have:

- ✅ Action card images (Potong Sayuran, Potong Daging, Panaskan Air, Panaskan Daging, Tarik Nafas)
- ✅ Special card images (Barbarian Curse, Wizard Boon Draw, Wizard Boon Focus)
- ✅ Customer sprites (Knight, Elf, Barbarian, Wizard)
- ✅ Main character sprite + animations
- ✅ Customer animations
- ✅ Customer menu images (what each customer orders)
- ✅ Environment/kitchen location sprites
- ✅ All scripts already in `Assets/Scripts/` folder

---

## 🎯 Setup Flow Overview

```
1. Project Setup & Import Assets
2. Create ScriptableObjects (Cards & Customers)
3. Setup Scene Hierarchy
4. Configure Manager GameObjects
5. Setup UI System
6. Setup Player Character
7. Configure Animations
8. Test & Play
```

---

## PART 1: PROJECT SETUP & IMPORT ASSETS

### Step 1.1: Open Unity Project

1. Open Unity Hub
2. Open project: `MasakuGimersia`
3. Wait for project to load

### Step 1.2: Organize Asset Folders

Create this folder structure in Project window:

```
Assets/
├── Scripts/                    (already exists)
├── Card/                       (for card ScriptableObjects)
├── Customer/                   (for customer ScriptableObjects)
├── Images/
│   ├── Cards/                  (import card images here)
│   ├── Customers/              (import customer sprites)
│   ├── Menus/                  (import menu images)
│   ├── Character/              (import main character)
│   └── Environment/            (import kitchen sprites)
├── Animations/
│   ├── Character/              (character animation clips)
│   └── Customers/              (customer animation clips)
├── Prefabs/
│   ├── CardUI.prefab
│   ├── CustomerPrefab.prefab
│   └── MenuIcon.prefab
└── Scenes/
    └── MainGame.unity
```

### Step 1.3: Import All Images

1. Drag your images into respective folders
2. For each image, select in Project window
3. In Inspector:
   - **Texture Type**: Sprite (2D and UI)
   - **Pixels Per Unit**: 100 (adjust based on your art)
   - Click **Apply**

---

## PART 2: CREATE SCRIPTABLEOBJECTS

### Step 2.1: Create Action Cards (10 cards total)

#### Create Base Action Cards:

1. Right-click in `Assets/Card/` folder
2. **Create → Cards → Action Card**
3. Name it: `Potong Sayuran 1`

#### Configure "Potong Sayuran 1":

```
Card Name: Potong Sayuran
Card Type: PotongSayuran (dropdown)
Focus Cost: 1
Pickup Tag: StorageVegetable
Target Tag: CuttingBoard
Card Image: [Drag sprite from Images/Cards/]
Description: "Potong sayuran untuk memasak"
Is Special Card: FALSE
Discard Count: 0
Draw Count: 0
```

#### Duplicate for second copy:

1. Select `Potong Sayuran 1`
2. **Ctrl+D** (duplicate)
3. Rename to: `Potong Sayuran 2`

#### Repeat for all base cards:

Create 2 copies each of:

- ✅ **Potong Sayuran** (PotongSayuran, StorageVegetable → CuttingBoard)
- ✅ **Potong Daging** (PotongDaging, StorageMeat → CuttingBoard)
- ✅ **Panaskan Air** (PanaskanAir, (empty) → Stove)
- ✅ **Panaskan Daging** (PanaskanDaging, StorageMeat → Stove)
- ✅ **Tarik Nafas** (TarikNafas, Special Card = TRUE, Discard: 1, Draw: 1)

### Step 2.2: Create Special Cards

#### Barbarian Curse:

```
Card Name: Barbarian Curse
Card Type: BarbarianCurse
Focus Cost: 0
Pickup Tag: (empty)
Target Tag: (empty)
Card Image: [Drag curse image]
Description: "Kartu kutukan dari Barbarian yang marah"
Is Special Card: TRUE
```

#### Wizard Boon - Draw:

```
Card Name: Wizard Boon - Draw
Card Type: WizardBoonDraw
Focus Cost: 0
Pickup Tag: (empty)
Target Tag: (empty)
Card Image: [Drag boon image]
Description: "Tarik 2 kartu tambahan"
Is Special Card: TRUE
Draw Count: 2
```

#### Wizard Boon - Focus:

```
Card Name: Wizard Boon - Focus
Card Type: WizardBoonFocus
Focus Cost: 0
Pickup Tag: (empty)
Target Tag: (empty)
Card Image: [Drag boon image]
Description: "+1 Fokus untuk turn berikutnya"
Is Special Card: TRUE
```

### Step 2.3: Create Customer ScriptableObjects

1. Right-click in `Assets/Customer/` folder
2. **Create → Masaku → Customer**
3. Name it: `Knight`

#### Configure Knight:

```
Customer Name: Knight
Customer Type: Knight (dropdown)
Customer Sprite: [Drag knight sprite]
Max Patience: 10
Patience Decay Per Turn: 2
Required Cards:
  - PotongDaging
  - PanaskanAir
Focus Reward: 2
Is Barbarian: FALSE
Is Wizard: FALSE
Order Description: "Membutuhkan daging potong dan air panas"
```

#### Create all customers:

**Elf:**

```
Customer Name: Elf
Customer Type: Elf
Max Patience: 8
Patience Decay Per Turn: 1
Required Cards:
  - PotongSayuran
  - PanaskanAir
Focus Reward: 3
Order Description: "Membutuhkan sayuran potong dan air panas"
```

**Barbarian:**

```
Customer Name: Barbarian
Customer Type: Barbarian
Max Patience: 6
Patience Decay Per Turn: 3
Required Cards:
  - PotongDaging
  - PanaskanDaging
Focus Reward: 1
Is Barbarian: TRUE ← Important!
Order Description: "Membutuhkan daging potong dan daging panas"
```

**Wizard:**

```
Customer Name: Wizard
Customer Type: Wizard
Max Patience: 12
Patience Decay Per Turn: 2
Required Cards:
  - PotongSayuran
  - PanaskanAir
  - PanaskanDaging
Focus Reward: 4
Is Wizard: TRUE ← Important!
Order Description: "Membutuhkan sayuran potong, air panas, dan daging panas"
```

---

## PART 3: SETUP SCENE HIERARCHY

### Step 3.1: Create Main Scene

1. **File → New Scene**
2. **File → Save As**: `MainGame.unity`
3. Save in `Assets/Scenes/`

### Step 3.2: Create Tag System

1. **Edit → Project Settings → Tags and Layers**
2. Click **+** to add new tags:
   - `CuttingBoard`
   - `Stove`
   - `StorageVegetable`
   - `StorageMeat`
   - `ServingCounter`

### Step 3.3: Setup Environment/Kitchen

#### Create Kitchen Container:

```
Hierarchy:
Kitchen (Empty GameObject)
├── Background (Sprite)
├── Floor (Sprite)
└── Walls (Sprite)
```

#### Create Storage Locations:

```
Kitchen/Storages (Empty GameObject)
├── VegetableStorage
│   ├── Add SpriteRenderer (your vegetable storage sprite)
│   └── Tag: "StorageVegetable"
└── MeatStorage
    ├── Add SpriteRenderer (your meat storage sprite)
    └── Tag: "StorageMeat"
```

#### Create Workstations:

```
Kitchen/Workstations (Empty GameObject)
├── CuttingBoard_01
│   ├── Add SpriteRenderer (cutting board sprite)
│   └── Tag: "CuttingBoard"
├── CuttingBoard_02
│   ├── Add SpriteRenderer
│   └── Tag: "CuttingBoard"
├── Stove_01
│   ├── Add SpriteRenderer (stove sprite)
│   └── Tag: "Stove"
└── Stove_02
    ├── Add SpriteRenderer
    └── Tag: "Stove"
```

#### Create Customer Seats:

```
Kitchen/CustomerSeats (Empty GameObject)
├── Seat_0 (Empty GameObject) - Position: (-5, 0, 0)
├── Seat_1 (Empty GameObject) - Position: (-3, 0, 0)
├── Seat_2 (Empty GameObject) - Position: (3, 0, 0)
└── Seat_3 (Empty GameObject) - Position: (5, 0, 0)
```

### Step 3.4: Create Main Camera

```
Main Camera
├── Position: (0, 0, -10)
├── Projection: Orthographic
├── Size: 5 (adjust to fit your scene)
└── Background: Your preferred color
```

---

## PART 4: CONFIGURE MANAGER GAMEOBJECTS

### Step 4.1: Create GameManager

1. **GameObject → Create Empty**
2. Name: `GameManager`
3. **Add Component → Game Manager (script)**

#### Configure GameManager:

```
Game Manager (Script)
├── Current Day: 1
├── Reputation: 5
├── Current Focus: 3
├── Bonus Focus Next Turn: 0
│
├── Customer Management:
│   ├── All Customers (Size: 4)
│   │   ├── Element 0: [Drag Knight]
│   │   ├── Element 1: [Drag Elf]
│   │   ├── Element 2: [Drag Barbarian]
│   │   └── Element 3: [Drag Wizard]
│   │
│   ├── Customer Seats (Size: 4)
│   │   ├── Element 0: [Drag Seat_0 from Hierarchy]
│   │   ├── Element 1: [Drag Seat_1]
│   │   ├── Element 2: [Drag Seat_2]
│   │   └── Element 3: [Drag Seat_3]
│   │
│   └── Customer Prefab: [Will create in Step 4.4]
│
├── Day Configuration (Size: 7)
│   ├── Day 1:
│   │   ├── Day: 1
│   │   ├── Day Name: "Hari Pertama"
│   │   └── Customer Queue (Size: 5): Knight, Elf, Knight, Elf, Knight
│   │
│   ├── Day 2:
│   │   ├── Day: 2
│   │   ├── Day Name: "Hari Kedua"
│   │   └── Customer Queue: Elf, Knight, Barbarian, Elf, Knight
│   │
│   └── ... (configure all 7 days)
│
├── References:
│   └── Card Manager: [Drag MasakuCardManager after creation]
│
└── Special Cards:
    ├── Wizard Boon Draw: [Drag Wizard Boon - Draw]
    └── Wizard Boon Focus: [Drag Wizard Boon - Focus]
```

### Step 4.2: Create MasakuCardManager

1. **GameObject → Create Empty**
2. Name: `MasakuCardManager`
3. **Add Component → Masaku Card Manager (script)**

#### Configure MasakuCardManager:

```
Masaku Card Manager (Script)
├── Card Setup:
│   ├── Base Action Cards (Size: 5) ⚠️ CRITICAL - Must have all 5!
│   │   ├── Element 0: [Drag Potong Sayuran 1] ← Cannot be None!
│   │   ├── Element 1: [Drag Potong Daging 1] ← Cannot be None!
│   │   ├── Element 2: [Drag Panaskan Air 1] ← Cannot be None!
│   │   ├── Element 3: [Drag Panaskan Daging 1] ← Cannot be None!
│   │   └── Element 4: [Drag Tarik Nafas 1] ← Cannot be None!
│   │
│   └── Curse Card: [Drag Barbarian Curse]
│
├── Hand Settings:
│   └── Max Hand Size: 5
│
└── Player Reference:
    └── Player Movement: [Drag Player after creation]
```

**⚠️ IMPORTANT:** All 5 Base Action Cards MUST be assigned! If any are "None", the deck will be empty and no cards will appear in your hand!

### Step 4.3: Create KitchenLocationManager

1. **GameObject → Create Empty**
2. Name: `KitchenLocationManager`
3. **Add Component → Kitchen Location Manager (script)**
4. Click **⋮ (menu)** on the component
5. Select **"Auto-Find Locations"**
6. Verify all tagged locations appear in the list

### Step 4.4: Create Customer Prefab

#### Create CustomerPrefab GameObject:

```
Hierarchy:
CustomerPrefab (Empty GameObject)
├── CustomerSprite
│   ├── Add Component: Sprite Renderer
│   └── Sprite: (will be set at runtime)
│
├── Canvas (Canvas component)
│   ├── Render Mode: World Space
│   ├── Width: 200, Height: 100
│   │
│   ├── PatienceBar (UI → Image)
│   │   ├── Anchor: Top-Center
│   │   ├── Width: 180, Height: 20
│   │   ├── Color: Green
│   │   └── Fill (child Image for actual bar)
│   │       └── Image Type: Filled (Horizontal)
│   │
│   └── OrderDisplay (TextMeshProUGUI)
│       ├── Anchor: Bottom-Center
│       ├── Text: "Order: ..."
│       ├── Font Size: 12
│       └── Alignment: Center
```

#### Add CustomerInstance Script:

1. Select `CustomerPrefab` (root)
2. **Add Component → Customer Instance (script)**
3. Configure:
   ```
   Customer Instance (Script)
   ├── Customer Data: (leave empty - set at runtime)
   ├── Current Patience: 0 (set at runtime)
   ├── Seat Index: 0 (set at runtime)
   │
   └── UI References:
       ├── Customer Sprite: [Drag CustomerSprite]
       ├── Patience Bar Transform: [Drag Fill (child of PatienceBar)]
       └── Order Display Text: [Drag OrderDisplay]
   ```

#### Save as Prefab:

1. Drag `CustomerPrefab` from Hierarchy to `Assets/Prefabs/` folder
2. Delete from Hierarchy (GameManager will spawn it)

#### Assign to GameManager:

1. Select `GameManager` in Hierarchy
2. Find **Customer Prefab** field
3. Drag `CustomerPrefab` from Project window

---

## PART 5: SETUP UI SYSTEM

### Step 5.1: Create Canvas

1. **GameObject → UI → Canvas**
2. Name: `UI_Canvas`
3. Configure Canvas:
   ```
   Canvas:
   ├── Render Mode: Screen Space - Overlay
   └── Canvas Scaler:
       ├── UI Scale Mode: Scale With Screen Size
       └── Reference Resolution: 1920x1080
   ```

### Step 5.2: Create HUD Panel

```
UI_Canvas/
├── HUD_Panel (UI → Panel)
    ├── Anchor: Stretch-Top
    ├── Height: 100
    │
    ├── FocusText (TextMeshProUGUI)
    │   ├── Position: Top-Left
    │   ├── Text: "Fokus: 3"
    │   └── Font Size: 24
    │
    ├── DayText (TextMeshProUGUI)
    │   ├── Position: Top-Center
    │   ├── Text: "Hari 1/7"
    │   └── Font Size: 32
    │
    └── ReputationPanel (Empty GameObject)
        ├── Position: Top-Right
        │
        ├── ReputationText (TextMeshProUGUI)
        │   └── Text: "Reputasi:"
        │
        └── Hearts (Horizontal Layout Group)
            ├── Heart_1 (UI → Image) - Sprite: Heart icon
            ├── Heart_2 (UI → Image)
            ├── Heart_3 (UI → Image)
            ├── Heart_4 (UI → Image)
            └── Heart_5 (UI → Image)
```

### Step 5.3: Create Customer Menu Display

```
UI_Canvas/
├── CustomerMenuPanel (UI → Panel)
    ├── Anchor: Top-Left
    ├── Position: (150, -150)
    ├── Width: 300, Height: 600
    │
    ├── Title (TextMeshProUGUI): "Customer Orders"
    │
    └── MenuContainer (Vertical Layout Group)
        ├── CustomerMenu_Seat0 (UI → Image)
        │   ├── Width: 280, Height: 120
        │   ├── Image: [Customer menu sprite]
        │   ├── Button component
        │   └── SeatNumber (TextMeshProUGUI): "Seat 1"
        │
        ├── CustomerMenu_Seat1 (UI → Image)
        ├── CustomerMenu_Seat2 (UI → Image)
        └── CustomerMenu_Seat3 (UI → Image)
```

### Step 5.4: Create Hand Display

```
UI_Canvas/
├── HandPanel (UI → Panel)
    ├── Anchor: Bottom-Center
    ├── Width: 1600, Height: 250
    │
    └── HandContainer (Empty GameObject)
        ├── Horizontal Layout Group
        │   ├── Spacing: 10
        │   └── Child Alignment: Middle Center
        │
        └── (Cards will spawn here at runtime)
```

### Step 5.5: Create Card UI Prefab

**Super Simple - Just a Button!**

```
Create New GameObject in Hierarchy: CardUI
└── Button (Component with built-in Image)
    ├── RectTransform:
    │   ├── Width: 150 (adjust to your card size)
    │   └── Height: 200 (adjust to your card size)
    │
    └── Image (Built-in with Button):
        ├── Source Image: (will be set at runtime)
        ├── Preserve Aspect: TRUE ✓
        └── Color: White
```

**To Create:**

1. Right-click in Hierarchy
2. **UI → Button**
3. Rename to: `CardUI`
4. Select CardUI, in RectTransform:
   - Width: 150 (your card width)
   - Height: 200 (your card height)
5. In Image component (already on Button):
   - Check ✓ **Preserve Aspect**
   - Source Image: Leave empty (set at runtime)
6. Delete the "Text" child (Button creates text by default - not needed)
7. Drag `CardUI` to `Assets/Prefabs/` folder
8. Delete `CardUI` from Hierarchy

**That's it! Just one Button with its built-in Image.**

Save as Prefab: `Assets/Prefabs/CardUI.prefab`

**Important:** Your card image should already contain:

- Card name
- Focus cost
- Card artwork
- Any other information

The UI will simply display your pre-designed card image!

**To Create:**

1. **GameObject → UI → Image** (in Hierarchy)
2. Rename to: `CardUI`
3. Set Width: 150, Height: 200 (adjust to your card size)
4. **Add Component → Button**
5. Drag from Hierarchy to `Assets/Prefabs/` folder
6. Delete from Hierarchy

Save as Prefab: `Assets/Prefabs/CardUI.prefab`

### Step 5.6: Create Preparation Station Display

```
UI_Canvas/
├── PreparationPanel (UI → Panel)
    ├── Anchor: Middle-Center
    ├── Width: 800, Height: 150
    │
    ├── Title (TextMeshProUGUI): "Kartu Dipilih:"
    │
    └── PreparationText (TextMeshProUGUI)
        ├── Text: "[Belum ada kartu dipilih]"
        └── Font Size: 18
```

### Step 5.7: Create Action Buttons

```
UI_Canvas/
├── ActionButtonsPanel (UI → Panel)
    ├── Anchor: Bottom-Right
    ├── Width: 300, Height: 150
    │
    ├── SubmitOrderButton (UI → Button)
    │   ├── Text: "Submit Order"
    │   ├── Width: 280, Height: 60
    │   └── OnClick: (will be set by MasakuUI)
    │
    └── EndTurnButton (UI → Button)
        ├── Text: "End Turn"
        ├── Width: 280, Height: 60
        └── OnClick: (will be set by MasakuUI)
```

### Step 5.8: Create MasakuUI Manager

1. Select `UI_Canvas`
2. **Add Component → Masaku UI (script)**
3. Configure:

```
Masaku UI (Script)
├── Focus Display:
│   ├── Focus Text: [Drag FocusText]
│   └── Focus Bar: (optional Image for visual bar)
│
├── Reputation Display:
│   ├── Reputation Text: [Drag ReputationText]
│   └── Reputation Hearts (Size: 5):
│       ├── Element 0: [Drag Heart_1]
│       ├── Element 1: [Drag Heart_2]
│       └── ... (all 5 hearts)
│
├── Day Display:
│   └── Day Text: [Drag DayText]
│
├── Hand Display:
│   ├── Hand Container: [Drag HandContainer]
│   └── Card UI Prefab: [Drag CardUI from Prefabs]
│
├── Preparation Station Display:
│   ├── Preparation Container: [Optional]
│   └── Preparation Text: [Drag PreparationText]
│
├── Buttons:
│   ├── Submit Order Button: [Drag SubmitOrderButton]
│   ├── End Turn Button: [Drag EndTurnButton]
│   └── Seat Buttons (Size: 4):
│       ├── Element 0: [Drag CustomerMenu_Seat0]
│       ├── Element 1: [Drag CustomerMenu_Seat1]
│       └── ... (all 4 menu buttons)
│
└── Customer Display:
    └── Customer UI Positions (Size: 4):
        └── (Optional - for displaying customer info)
```

---

## PART 6: SETUP PLAYER CHARACTER

### Step 6.1: Create Player GameObject

```
Hierarchy:
Player (Empty GameObject)
├── Position: (0, -3, 0)
│
└── Sprite (SpriteRenderer)
    ├── Sprite: [Your character sprite]
    └── Order in Layer: 10
```

### Step 6.2: Add Player Components

1. Select `Player`
2. **Add Component → Player Movement (script)**
3. Configure:
   ```
   Player Movement (Script)
   ├── Movement Settings:
   │   ├── Move Speed: 5
   │   ├── Rotation Speed: 10
   │   └── Stopping Distance: 0.5
   │
   └── Animation:
       └── Animator: [Drag Animator after setup]
   ```

### Step 6.3: Assign Player to Managers

1. Select `MasakuCardManager`
2. In **Player Reference** → Drag `Player` from Hierarchy

---

## PART 7: CONFIGURE ANIMATIONS

### Step 7.1: Setup Player Animator

#### Create Animator Controller:

1. Right-click in `Assets/Animations/Character/`
2. **Create → Animator Controller**
3. Name: `PlayerAnimator`

#### Configure Animation States:

1. Double-click `PlayerAnimator` to open Animator window
2. Create states:
   ```
   Animator States:
   ├── Idle (default state)
   │   └── Motion: [Your idle animation clip]
   │
   ├── Walking
   │   └── Motion: [Your walk animation clip]
   │
   ├── PotongSayuran
   │   └── Motion: [Cutting vegetable animation]
   │
   ├── PotongDaging
   │   └── Motion: [Cutting meat animation]
   │
   ├── PanaskanAir
   │   └── Motion: [Heating water animation]
   │
   ├── PanaskanDaging
   │   └── Motion: [Heating meat animation]
   │
   └── PickUp (optional)
       └── Motion: [Picking up animation]
   ```

#### Create Parameters:

```
Parameters:
├── IsWalking (Bool)
├── PotongSayuran (Trigger)
├── PotongDaging (Trigger)
├── PanaskanAir (Trigger)
├── PanaskanDaging (Trigger)
└── PickUp (Trigger)
```

#### Create Transitions:

```
Transitions:
├── Idle → Walking
│   └── Condition: IsWalking == true
│
├── Walking → Idle
│   └── Condition: IsWalking == false
│
├── Idle → PotongSayuran
│   └── Condition: PotongSayuran trigger
│   └── Has Exit Time: false
│
└── (Create similar for all action triggers)
   All actions → Idle (with exit time)
```

#### Attach to Player:

1. Select `Player` in Hierarchy
2. **Add Component → Animator**
3. **Controller**: Drag `PlayerAnimator`
4. Update PlayerMovement script:
   - Drag Animator component to **Animator** field

### Step 7.2: Setup Customer Animations (Optional)

Similar process for customer animations:

1. Create `CustomerAnimator` controller
2. Add Idle, Happy, Angry states
3. Assign to CustomerPrefab's CustomerSprite

---

## PART 8: FINAL CONFIGURATION & TESTING

### Step 8.1: Verify All References

#### Check GameManager:

- ✅ All 4 customers assigned
- ✅ All 4 seat transforms assigned
- ✅ CustomerPrefab assigned
- ✅ Day configurations filled (7 days)
- ✅ Card Manager assigned
- ✅ Special cards assigned

#### Check MasakuCardManager:

- ✅ 5 base action cards assigned
- ✅ Curse card assigned
- ✅ Player Movement assigned

#### Check MasakuUI:

- ✅ All text fields assigned
- ✅ All buttons assigned
- ✅ CardUI prefab assigned
- ✅ Hand container assigned

#### Check Player:

- ✅ Sprite assigned
- ✅ PlayerMovement component attached
- ✅ Animator assigned

### Step 8.2: Test Scene

1. **File → Save Scene**
2. **File → Build Settings**
3. Add `MainGame` scene to build
4. Click **Play ▶**

#### Expected Behavior:

```
✅ 4 customers spawn at seats
✅ Customer sprites visible
✅ Customer patience bars visible
✅ 5 cards appear in hand at bottom
✅ Focus shows "Fokus: 3"
✅ Day shows "Hari 1/7"
✅ Reputation shows 5 hearts
✅ Player character visible
```

### Step 8.3: Test Card Selection

1. **Click a card** → Should highlight yellow
2. **Click again** → Should unhighlight
3. **Select 2 cards** → Both highlighted
4. **Click customer menu** → Seat selected
5. **Click "Submit Order"** → Character should:
   - Walk to storage (if applicable)
   - Walk to workstation
   - Perform action
   - Submit order to customer

### Step 8.4: Test Customer System

1. Watch customer serve
2. Check if correct → Customer happy, gives focus
3. Check if wrong → Customer loses patience
4. Let customer run out of patience → Leaves angry, lose reputation
5. Serve Wizard quickly → Should get boon card
6. Let Barbarian leave angry → Should get curse card

### Step 8.5: Test Turn System

1. **Click "End Turn"**
2. All customers lose patience
3. Empty seats filled with new customers
4. Cards discarded
5. New turn starts with 5 cards, 3 focus

---

## ✅ COMPLETE CHECKLIST

### ScriptableObjects Created:

- [ ] 10 Action Cards (2x each of 5 types)
- [ ] 3 Special Cards (Curse, 2 Boons)
- [ ] 4 Customer types (Knight, Elf, Barbarian, Wizard)

### Scene Hierarchy Setup:

- [ ] Kitchen environment with sprites
- [ ] Storage locations tagged
- [ ] Workstations tagged
- [ ] 4 Customer seats positioned
- [ ] Main Camera configured

### Manager GameObjects:

- [ ] GameManager configured
- [ ] MasakuCardManager configured
- [ ] KitchenLocationManager auto-found locations
- [ ] CustomerPrefab created and assigned

### UI System:

- [ ] Canvas with HUD panel
- [ ] Customer menu display (4 buttons)
- [ ] Hand display area
- [ ] Card UI prefab created
- [ ] Action buttons (Submit Order, End Turn)
- [ ] MasakuUI script configured

### Player Setup:

- [ ] Player GameObject created
- [ ] Sprite assigned
- [ ] PlayerMovement script attached
- [ ] Animator controller created
- [ ] Animations configured

### Testing:

- [ ] Scene plays without errors
- [ ] Customers spawn
- [ ] Cards appear in hand
- [ ] Card selection works
- [ ] Character moves to locations
- [ ] Orders can be submitted
- [ ] Turn system works
- [ ] Day progression works

---

## 🎮 YOU'RE READY TO PLAY!

Press **Play ▶** and enjoy your game!

---

## 🔧 Common Issues & Fixes

### Cards don't appear:

- Check if MasakuCardManager has all 5 base cards assigned
- Verify CardUI prefab is assigned to MasakuUI
- **Check Console**: If you see "Deck dan discard pile kosong!" - the deck is empty
  - Verify all 5 base action cards are assigned in MasakuCardManager inspector
  - Each card should NOT be null

### Player doesn't move:

- Check if PlayerMovement is assigned in MasakuCardManager
- Verify location tags are correct
- Run "Auto-Find Locations" on KitchenLocationManager

### Customers don't spawn:

- Check if CustomerPrefab is assigned to GameManager
- Verify all 4 seats are assigned
- Check if Day Configuration has customers in queue

### UI not updating:

- Verify all text/image fields are assigned in MasakuUI
- Check Console for null reference errors

### Focus/Reputation not working:

- Check if GameManager.Instance exists
- Verify initial values are set correctly

---

_Follow this guide step-by-step and you'll have a fully functional game! Good luck! 🎯_
