# Sistem Kartu Aksi - Panduan Setup

## 📋 Deskripsi

Sistem kartu aksi untuk game masak dengan 10 kartu tetap (5 jenis, masing-masing 2x):

- 2x "Potong Sayuran" (Biaya: 1 Fokus)
- 2x "Potong Daging" (Biaya: 1 Fokus)
- 2x "Panaskan Air" (Biaya: 1 Fokus)
- 2x "Panaskan Daging" (Biaya: 1 Fokus)
- 2x "Tarik Nafas" (Buang 1x Kartu, Tarik 1x Kartu)

## 🎯 File yang Dibuat

1. **ActionCard.cs** - ScriptableObject untuk data kartu
2. **CardManager.cs** - Mengelola deck, hand, dan gameplay
3. **PlayerMovement.cs** - Menggerakkan player ke lokasi berdasarkan tag
4. **CardUI.cs** - UI untuk menampilkan dan memilih kartu (opsional)
5. **GameSetup.cs** - Helper untuk testing

## 🔧 Cara Setup

### Langkah 1: Buat Action Cards (ScriptableObjects)

1. Di Project window, klik kanan → **Create → Cards → Action Card**
2. Buat 5 kartu dengan setting berikut:

**Potong Sayuran:**

- Card Name: "Potong Sayuran"
- Card Type: PotongSayuran
- Focus Cost: 1
- Target Tag: "PotongSayuran"
- Is Special Card: false

**Potong Daging:**

- Card Name: "Potong Daging"
- Card Type: PotongDaging
- Focus Cost: 1
- Target Tag: "PotongDaging"
- Is Special Card: false

**Panaskan Air:**

- Card Name: "Panaskan Air"
- Card Type: PanaskanAir
- Focus Cost: 1
- Target Tag: "PanaskanAir"
- Is Special Card: false

**Panaskan Daging:**

- Card Name: "Panaskan Daging"
- Card Type: PanaskanDaging
- Focus Cost: 1
- Target Tag: "PanaskanDaging"
- Is Special Card: false

**Tarik Nafas:**

- Card Name: "Tarik Nafas"
- Card Type: TarikNafas
- Is Special Card: true
- Discard Count: 1
- Draw Count: 1

### Langkah 2: Setup Tags

1. Buka **Edit → Project Settings → Tags and Layers**
2. Tambahkan tags baru:
   - PotongSayuran
   - PotongDaging
   - PanaskanAir
   - PanaskanDaging

### Langkah 3: Setup Scene

1. **Buat lokasi di scene** untuk setiap aksi
2. Assign tag yang sesuai ke setiap GameObject lokasi

Contoh:

```
Scene Hierarchy:
├── Player (dengan script PlayerMovement)
├── CardManager (dengan script CardManager)
├── Locations
│   ├── Meja_Potong_Sayuran (Tag: PotongSayuran)
│   ├── Meja_Potong_Daging (Tag: PotongDaging)
│   ├── Kompor_Air (Tag: PanaskanAir)
│   └── Kompor_Daging (Tag: PanaskanDaging)
```

### Langkah 4: Setup Card Manager

1. Buat Empty GameObject bernama **"CardManager"**
2. Tambahkan component **CardManager**
3. Di Inspector:
   - **All Action Cards:** Drag & drop semua 5 ActionCard yang dibuat (sistem otomatis duplikat 2x)
   - **Player Movement:** Drag Player GameObject
   - **Current Focus:** 3 (atau sesuai keinginan)
   - **Max Focus:** 5

### Langkah 5: Setup Player

1. Pilih Player GameObject
2. Tambahkan component **PlayerMovement**
3. Sesuaikan settings:
   - Move Speed: 5
   - Rotation Speed: 10
   - Stopping Distance: 0.5
   - Animator: (opsional, drag Animator component jika ada)

### Langkah 6: Testing (Opsional)

1. Buat Empty GameObject bernama **"GameSetup"**
2. Tambahkan component **GameSetup**
3. Jalankan game dan gunakan:
   - **D** = Draw kartu
   - **1** = Play kartu pertama di tangan
   - **2** = Play kartu kedua di tangan
   - **3** = Play kartu ketiga di tangan

## 🎮 Cara Menggunakan di Script Lain

```csharp
// Mainkan kartu dari index tangan
CardManager.Instance.PlayCardByIndex(0);

// Ambil kartu dari deck
CardManager.Instance.DrawCards(2);

// Tambah fokus
CardManager.Instance.AddFocus(1);

// Reset fokus ke maksimal
CardManager.Instance.ResetFocus();

// Lihat kartu di tangan
List<ActionCard> hand = CardManager.Instance.GetHand();
```

## 🎨 Setup UI (Opsional)

Jika ingin menampilkan kartu sebagai UI:

1. Buat Canvas
2. Buat Panel untuk "Hand Container"
3. Buat Prefab untuk Card UI dengan structure:
   ```
   CardUI_Prefab
   ├── CardImage (Image)
   ├── CardName (TextMeshProUGUI)
   ├── FocusCost (TextMeshProUGUI)
   └── Button (Button component)
   ```
4. Tambahkan script CardUI ke Canvas
5. Assign references di Inspector

## 🔍 Fitur Sistem

✅ Deck tetap 10 kartu (5 jenis × 2)
✅ Sistem fokus untuk biaya kartu
✅ Auto-shuffle discard pile ke deck saat deck habis
✅ Kartu spesial "Tarik Nafas" (discard & draw)
✅ Player otomatis bergerak ke lokasi tag
✅ Animasi support (jika ada Animator)
✅ Action execution di lokasi tujuan

## 🐛 Troubleshooting

**Player tidak bergerak:**

- Pastikan tag sudah diassign ke GameObject lokasi
- Cek PlayerMovement component sudah di Player
- Cek CardManager sudah reference Player Movement

**Kartu tidak muncul:**

- Pastikan ActionCard sudah dibuat dan assign ke CardManager
- Cek array "All Action Cards" di CardManager Inspector

**Error "Tag tidak ditemukan":**

- Buka Edit → Project Settings → Tags and Layers
- Tambahkan tag yang sesuai dengan Target Tag di ActionCard

## 📞 Support

Jika ada error atau pertanyaan, cek:

1. Console untuk error message
2. Pastikan semua script sudah dicompile
3. Pastikan semua reference sudah diassign di Inspector
