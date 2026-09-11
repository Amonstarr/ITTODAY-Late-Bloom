# 🧩 Panduan Setup Jigsaw 4-Fase & Cutscene Cerita Ingatan

Panduan langkah demi langkah untuk mengatur sistem Jigsaw 4 Fase Pertumbuhan Bunga, Pop-up Cutscene Keping Puzzle, dan Cutscene Cerita Ingatan di Unity Scene.

---

## 1. Struktur Hierarchy Canvas di Scene

```
Canvas
├─ PuzzlePhaseManager          ← PuzzlePhaseManager (script)
├─ FlowerGrowthController      ← FlowerGrowthController (script testing)
│
├─ Jigsaw_System               ← JigsawManager (script)
│   ├─ ManualBackground        ← Image (background latar board)
│   ├─ ManualBoardFrame        ← Image (frame/border board)
│   ├─ Board_Puzzle            ← RectTransform (puzzleBoardContainer)
│   │   ├─ Slot_0              ← JigsawSlot (pieceId = 0)
│   │   ├─ Slot_1              ← JigsawSlot (pieceId = 1)
│   │   ├─ Slot_2              ← JigsawSlot (pieceId = 2)
│   │   └─ Slot_3              ← JigsawSlot (pieceId = 3)
│   ├─ Pieces_Root             ← RectTransform (piecesContainer)
│   │   ├─ Piece_0             ← JigsawPiece (pieceId = 0)
│   │   ├─ Piece_1             ← JigsawPiece (pieceId = 1)
│   │   ├─ Piece_2             ← JigsawPiece (pieceId = 2)
│   │   └─ Piece_3             ← JigsawPiece (pieceId = 3)
│   ├─ ScatterArea_Left        ← RectTransform (scatterAreaLeft)
│   └─ ScatterArea_Right       ← RectTransform (scatterAreaRight)
│
├─ PieceAwarded_Panel          ← PuzzlePieceAwardedUI (script modal pop-up kepingan)
│   ├─ Title_Text              ← TextMeshProUGUI ("Keping Puzzle Didapatkan!")
│   ├─ Detail_Text             ← TextMeshProUGUI ("Fase: Tumbuh Dikit")
│   ├─ Progress_Text           ← TextMeshProUGUI ("Kepingan: 2 / 4")
│   ├─ Piece_Preview_Image     ← Image (preview sprite kepingan)
│   └─ Continue_Button         ← Button (tombol tutup/lanjut)
│
└─ MemoryStory_Panel           ← MemoryStoryCutsceneController (script cutscene cerita)
    ├─ Story_Title_Text        ← TextMeshProUGUI ("Kenangan Terbuka")
    ├─ Story_Photo_Image       ← Image (foto utuh kenangan)
    ├─ Story_Description_Text  ← TextMeshProUGUI (isi cerita kenangan)
    └─ CloseOrNext_Button      ← Button (tombol lanjut ke scene flashback)
```

---

## 2. Langkah Demi Langkah Setup Komponen

### Langkah 1: Setup `PuzzlePhaseManager`
1. Buat Empty GameObject bernama **`PuzzlePhaseManager`** di Scene (atau di dalam Canvas).
2. Tambahkan komponen **`PuzzlePhaseManager`**.
3. **Inspector Assignments**:
   - **`Puzzle Metadata`**: Assign file asset `PuzzleMetadata` bunga kamu (Opsional).
   - **`Jigsaw Manager`**: Drag GameObject **`Jigsaw_System`**.
   - **`Puzzle UI Container`**: Drag GameObject **`Board_Puzzle`** (atau `Jigsaw_System`).
   - **`Piece Awarded Cutscene UI`**: Drag GameObject **`PieceAwarded_Panel`** (komponen `PuzzlePieceAwardedUI`).

---

### Langkah 2: Setup UI Pop-up Keping Puzzle (`PuzzlePieceAwardedUI`)
1. Buat UI Panel bernama **`PieceAwarded_Panel`** di Canvas.
2. Tambahkan komponen **`PuzzlePieceAwardedUI`** pada GameObject tersebut.
3. Buat child element UI: Title Text, Detail Text, Progress Text, Preview Image, dan Continue Button.
4. **Inspector Assignments**:
   - **`Panel Root`**: Drag **`PieceAwarded_Panel`**.
   - **`Title Text`**: Drag `Title_Text`.
   - **`Detail Text`**: Drag `Detail_Text`.
   - **`Progress Text`**: Drag `Progress_Text`.
   - **`Piece Preview Image`**: Drag `Piece_Preview_Image`.
   - **`Piece Sprites`**: Set size = `4`, lalu drag sprite 4 kepingan puzzle (indeks 0=keping 1, 1=keping 2, dst.).
   - **`Continue Button`**: Drag `Continue_Button`.

---

### Langkah 3: Setup Board & Keping Puzzle (`JigsawManager`)
1. Pilih GameObject **`Jigsaw_System`** yang memiliki komponen **`JigsawManager`**.
2. **Inspector Configuration**:
   - **`Use Manual Setup`**: Centang `true` (jika menggunakan Slot & Piece manual di scene).
   - **`Grid Rows`**: `2`
   - **`Grid Cols`**: `2` (Total 4 kepingan persegi).
   - **`Puzzle Board Container`**: Drag **`Board_Puzzle`** (berisi 4 `JigsawSlot` dengan `pieceId` = 0, 1, 2, 3).
   - **`Pieces Container`**: Drag **`Pieces_Root`** (berisi 4 `JigsawPiece` dengan `pieceId` = 0, 1, 2, 3).
   - **`Scatter Area Left` & `Right`**: Drag GameObject area sebaran kepingan.
3. **Fetch Slots & Pieces**: Klik tombol **"🔄 Fetch Scene Slots & Pieces"** pada Inspector `JigsawManager`.

---

### Langkah 4: Setup Cutscene Cerita Ingatan (`MemoryStoryCutsceneController`)
1. Buat UI Panel bernama **`MemoryStory_Panel`** di Canvas.
2. Tambahkan komponen **`MemoryStoryCutsceneController`**.
3. Buat child elements: Title Text, Photo Image, Story Text, dan Button.
4. **Inspector Assignments**:
   - **`Jigsaw Manager`**: Drag GameObject **`Jigsaw_System`**.
   - **`Memory Panel Root`**: Drag **`MemoryStory_Panel`**.
   - **`Story Image`**: Drag Image untuk menampilkan foto kenangan utuh.
   - **`Story Text`**: Drag TextMeshProUGUI untuk isi cerita.
   - **`Story Text Content`**: Tulis teks narasi kenangan manis.
   - **`Close Or Next Button`**: Drag Tombol Lanjut.
   - **`Load Scene After Cutscene`**: Centang `true` jika ingin pindah ke Scene Flashback setelah cutscene ditutup.
   - **`Flashback Scene Name`**: Isi nama scene tujuan (misal: `"Flashback_Phase1"`).

---

## 3. Alur Gameplay Saat Dijalankan (Play Mode)

1. **Fase 1: Seed (Benih)** (Awal milih bunga):
   - Tanaman berada di fase **Seed**.
   - Pemain otomatis mendapatkan **Keping Puzzle #1 (1/4)** -> Pop-up `PieceAwarded_Panel` muncul.
2. **Fase 2: SmallGrowth (Tumbuh Dikit)** (Setelah disiram/dirawat):
   - Tanaman naik ke fase **Tumbuh Dikit**.
   - Pemain mendapatkan **Keping Puzzle #2 (2/4)** -> Pop-up `PieceAwarded_Panel` muncul.
3. **Fase 3: BigGrowth (Tumbuh Gede)**:
   - Tanaman naik ke fase **Tumbuh Gede**.
   - Pemain mendapatkan **Keping Puzzle #3 (3/4)** -> Pop-up `PieceAwarded_Panel` muncul.
4. **Fase 4: Bloom (Berbunga)**:
   - Tanaman mekar penuh (**Bloom**).
   - Pemain mendapatkan **Keping Puzzle #4 (4/4)** -> Pop-up `PieceAwarded_Panel` muncul.
   - **Board Puzzle Unlocked & Tampil**: Pemain menyusun 4 keping puzzle persegi ke slot yang sesuai.
5. **Menyusun 4 Keping Puzzle Selesai**:
   - Setelah kepingan ke-4 tersnap dengan benar di board, **Cutscene Cerita Ingatan** (`MemoryStory_Panel`) otomatis terbuka.
   - Pemain membaca cerita kenangan dan mengklik tombol untuk masuk ke **Scene Flashback**.

---

## 4. Shortcut Keyboard untuk Testing (Editor Only)

Gunakan tombol keyboard ini pada Play Mode di Unity Editor untuk melakukan testing cepat:
- **`N`**: Maju ke fase pertumbuhan berikutnya (`Seed` → `SmallGrowth` → `BigGrowth` → `Bloom`).
- **`R`**: Reset progres pertumbuhan kembali ke fase `Seed`.

