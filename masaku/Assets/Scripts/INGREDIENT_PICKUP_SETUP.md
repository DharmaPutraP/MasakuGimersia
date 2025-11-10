# Ingredient Pickup System Setup

## Overview

Characters now move to storage locations to pick up ingredients BEFORE going to the cooking/cutting station.

---

## 🏷️ Step 1: Create New Tags

Add these tags in Unity (**Edit → Project Settings → Tags and Layers**):

### Required Tags:

- ✅ `StorageVegetable` - For vegetable storage
- ✅ `StorageMeat` - For meat/daging storage
- ✅ `CuttingBoard` - For cutting actions
- ✅ `Stove` - For cooking actions

---

## 🎯 Step 2: Tag Your Scene Objects

### Example Scene Setup:

```
Kitchen
├── Storages
│   ├── VegetableStorage (Tag: "StorageVegetable")
│   └── MeatStorage (Tag: "StorageMeat")
│
├── Workstations
│   ├── CuttingBoard_01 (Tag: "CuttingBoard")
│   ├── CuttingBoard_02 (Tag: "CuttingBoard")
│   ├── Stove_01 (Tag: "Stove")
│   └── Stove_02 (Tag: "Stove")
│
└── ServingCounter (Tag: "ServingCounter")
```

---

## 📋 Step 3: Configure ActionCard ScriptableObjects

Update each ActionCard with both **Pickup Tag** and **Target Tag**:

### Potong Sayuran (Cut Vegetable)

```
Card Name: Potong Sayuran
Card Type: PotongSayuran
Focus Cost: 1
Pickup Tag: StorageVegetable  ← NEW!
Target Tag: CuttingBoard
```

### Potong Daging (Cut Meat)

```
Card Name: Potong Daging
Card Type: PotongDaging
Focus Cost: 1
Pickup Tag: StorageMeat  ← NEW!
Target Tag: CuttingBoard
```

### Panaskan Air (Heat Water)

```
Card Name: Panaskan Air
Card Type: PanaskanAir
Focus Cost: 1
Pickup Tag: (leave empty - water available at stove)  ← UPDATED!
Target Tag: Stove
```

### Panaskan Daging (Heat Meat)

```
Card Name: Panaskan Daging
Card Type: PanaskanDaging
Focus Cost: 1
Pickup Tag: StorageMeat  ← NEW!
Target Tag: Stove
```

### Tarik Nafas (Take Breath)

```
Card Name: Tarik Nafas
Card Type: TarikNafas
Focus Cost: 0
Pickup Tag: (leave empty - no movement)
Target Tag: (leave empty)
Is Special Card: TRUE
```

---

## 🎬 How It Works

### Movement Flow:

```
Player Plays "Potong Daging" Card
    ↓
1. Character moves to StorageMeat
    ├─ Walk animation plays
    ├─ Arrives at meat storage
    ├─ Plays "PickUp" animation (optional)
    └─ Debug: "Mengambil bahan dari: StorageMeat"
    ↓
2. Character moves to CuttingBoard
    ├─ Walk animation continues
    ├─ Arrives at cutting board
    ├─ Walk animation stops
    └─ Debug: "Sampai di lokasi: CuttingBoard"
    ↓
3. Execute cutting action
    ├─ Plays "PotongDaging" animation
    ├─ Wait 2 seconds
    └─ Action complete
```

### Code Flow:

```csharp
MoveToLocation(destination, card)
    ├─ Check: card.pickupTag exists?
    │  └─ YES: Add pickup location to movement queue
    ├─ Add target location to movement queue
    └─ Start moving to first location

ArrivedAtDestination()
    ├─ Check: More locations in queue?
    │  └─ YES:
    │     ├─ Mark ingredient picked up
    │     ├─ Play pickup animation
    │     └─ Move to next location (target)
    │
    └─ NO (all locations visited):
       ├─ Stop movement
       ├─ Play idle animation
       └─ ExecuteActionAtLocation()
```

---

## 🎨 Step 4: Animation Setup (Optional)

### Add Animation Trigger:

- **PickUp** - Plays when character picks up ingredient

### Example Animation States:

```
Animator Parameters:
├─ Bool: IsWalking
└─ Trigger: PickUp

Transitions:
Idle → (IsWalking = true) → Walking
Walking → (Trigger: PickUp) → PickUp Animation → Walking
Walking → (IsWalking = false) → Idle
```

---

## 🧪 Testing

### Test Case 1: Card with Pickup

1. Play "Potong Daging" card
2. Watch character move to **StorageMeat** first
3. Character picks up ingredient
4. Character moves to **CuttingBoard**
5. Character performs cutting action

**Expected Console Output:**

```
Akan mengambil bahan dari: StorageMeat
Bergerak ke lokasi: StorageMeat
Mengambil bahan dari: StorageMeat
Melanjutkan ke lokasi: CuttingBoard
Sampai di lokasi: CuttingBoard
Memotong daging...
```

### Test Case 2: Card without Pickup

1. Play "Tarik Nafas" card
2. No movement occurs
3. Special card effect executes immediately

---

## 🔧 Troubleshooting

### Character doesn't pick up ingredient:

- Check if **Pickup Tag** is set on the ActionCard
- Verify GameObject with that tag exists in scene
- Ensure tag spelling matches exactly

### Character skips pickup location:

- Check if location exists with correct tag
- Verify KitchenLocationManager has the location registered
- Run **Auto-Find Locations** context menu

### Character goes directly to target:

- Pickup Tag might be empty on ActionCard
- Check if GetLocationPosition returns valid position

---

## 📊 Card Configuration Reference

| Card Name       | Pickup Tag       | Target Tag   | Notes                        |
| --------------- | ---------------- | ------------ | ---------------------------- |
| Potong Sayuran  | StorageVegetable | CuttingBoard | Pick vegetable → Cut         |
| Potong Daging   | StorageMeat      | CuttingBoard | Pick meat → Cut              |
| Panaskan Air    | (empty)          | Stove        | Go directly to stove         |
| Panaskan Daging | StorageMeat      | Stove        | Pick meat → Heat             |
| Tarik Nafas     | (empty)          | (empty)      | No movement (special action) |

---

## 🎯 Advanced: Multiple Storage Locations

If you have multiple vegetable storage areas, the character will go to the first one found. To choose the closest:

```csharp
// In KitchenLocationManager.cs
public Vector3 GetClosestLocationPosition(string tag, Vector3 fromPosition)
{
    Transform closest = null;
    float closestDistance = Mathf.Infinity;

    foreach (KitchenLocation location in locations)
    {
        if (location.locationTag == tag)
        {
            float distance = Vector3.Distance(fromPosition,
                                            location.locationTransform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = location.locationTransform;
            }
        }
    }

    return closest != null ? closest.position : Vector3.zero;
}
```

Then update MasakuCardManager.PlayCard() to use `GetClosestLocationPosition()`.

---

## ✅ Checklist

- [ ] Created new tags (StorageVegetable, StorageMeat)
- [ ] Tagged scene objects with appropriate tags
- [ ] Updated ActionCard ScriptableObjects with Pickup Tags
  - [ ] Potong Sayuran: pickupTag = "StorageVegetable"
  - [ ] Potong Daging: pickupTag = "StorageMeat"
  - [ ] Panaskan Air: pickupTag = (empty)
  - [ ] Panaskan Daging: pickupTag = "StorageMeat"
- [ ] Verified KitchenLocationManager has all locations
- [ ] Tested card playing with sequential movement
- [ ] (Optional) Added PickUp animation trigger
- [ ] Verified console logs show correct movement sequence

---

_This system creates more realistic gameplay where characters must gather ingredients before cooking!_
