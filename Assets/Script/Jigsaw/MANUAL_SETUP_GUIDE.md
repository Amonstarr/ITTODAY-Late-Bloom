# 🧩 Manual Jigsaw Setup Guide

Panduan ini menjelaskan cara mengatur tampilan jigsaw puzzle (frame, background, dan kepingan) **secara manual di Unity Scene** tanpa menggunakan auto-generate.

---

## 1. Hierarchy yang Disarankan

```
Canvas
└─ Jigsaw_System            ← JigsawManager (script)
    ├─ ManualBackground     ← Image (background latar board)
    ├─ ManualBoardFrame     ← Image (frame/border board)
    ├─ Board_Puzzle         ← RectTransform (puzzleBoardContainer)
    │   ├─ Slot_0           ← JigsawSlot (pieceId = 0)
    │   ├─ Slot_1           ← JigsawSlot (pieceId = 1)
    │   └─ ...
    ├─ Pieces_Root          ← RectTransform (piecesContainer)
    │   ├─ Piece_0          ← JigsawPiece (pieceId = 0, sprite manual)
    │   ├─ Piece_1          ← JigsawPiece (pieceId = 1, sprite manual)
    │   └─ ...
    ├─ ScatterArea_Left     ← RectTransform (scatterAreaLeft)
    └─ ScatterArea_Right    ← RectTransform (scatterAreaRight)
```

---

## 2. Setup JigsawManager (Inspector)

| Field | Nilai |
|---|---|
| **Use Manual Setup** | ✅ Centang (ON) |
| **Manual Board Frame** | Drag GameObject `ManualBoardFrame` |
| **Manual Background** | Drag GameObject `ManualBackground` |
| **Puzzle Board Container** | Drag `Board_Puzzle` |
| **Pieces Container** | Drag `Pieces_Root` |
| **Scatter Area Left** | Drag `ScatterArea_Left` |
| **Scatter Area Right** | Drag `ScatterArea_Right` |
| **Snap Radius** | `60` (default, bisa disesuaikan) |

---

## 3. Setup Setiap Slot (JigsawSlot)

1. Buat child GameObject di dalam **`Board_Puzzle`** (misal `Slot_0`, `Slot_1`, dst.)
2. Tambahkan komponen **`JigsawSlot`**.
3. Set **`Piece Id`** sesuai urutan: Slot_0 → `0`, Slot_1 → `1`, dst.
4. Atur **RectTransform** (posisi & ukuran) sesuai layout puzzle yang kamu inginkan.
5. Opsional: isi `Ghost Highlight Image` dengan Image komponen untuk efek highlight saat hover.

---

## 4. Setup Setiap Piece (JigsawPiece)

1. Buat child GameObject di dalam **`Pieces_Root`** (misal `Piece_0`, `Piece_1`, dst.)
2. Tambahkan komponen **`JigsawPiece`**.
3. Set **`Piece Id`** sesuai pasangan slot: Piece_0 → `0`, dst.
4. **Pilih salah satu cara memberi gambar:**

### A. Sprite Manual (Disarankan untuk Manual Setup)
- Matikan **`Use Generated Shape`** (OFF/false) di JigsawPiece.
- Assign sprite jigsaw custom kamu langsung di komponen **Image** (field `Source Image`).
- Generator tidak akan meng-overwrite sprite ini.

### B. Gambar Otomatis Di-cut dari Foto
- Aktifkan **`Use Generated Shape`** (ON/true) di JigsawPiece.
- Isi **`Puzzle Photo Texture`** atau **`Puzzle Photo Sprite`** di JigsawManager.
- Klik tombol **"✂ Cut Photo & Assign to Pieces (Generated Shape Only)"** di Inspector.

---

## 5. Action Buttons di Inspector

Setelah semua objek di-setup di scene, gunakan tombol-tombol ini di Inspector JigsawManager:

| Tombol | Fungsi |
|---|---|
| **✔ Validate Manual Setup** | Cek apakah semua referensi wajib sudah diisi |
| **🔄 Fetch Scene Slots & Pieces** | Mengambil semua JigsawSlot & JigsawPiece dari scene secara otomatis |
| **✂ Cut Photo & Assign to Pieces** | Memotong foto dan assign ke piece yang `Use Generated Shape = true` |

---

## 6. Ukuran Piece vs Slot

JigsawManager secara otomatis menyamakan `sizeDelta` setiap piece dengan slot pasangannya saat `InitializePuzzle()` dijalankan (saat Play).

Atur ukuran slot di RectTransform, lalu piece akan mengikuti.

---

## 7. Tips Desain

- **Background** (`ManualBackground`): gunakan Image dengan sprite background bertekstur, set ke `Stretch` agar memenuhi area board.
- **Frame** (`ManualBoardFrame`): gunakan Image dengan sprite frame/border, set `Image Type = Sliced` untuk stretch yang rapi.
- **Slot**: buat semi-transparan (warna `0,0,0,0.15`) agar slot terlihat samar sebagai panduan.
- **Piece**: pastikan `Raycast Target = true` di Image agar bisa di-drag.

---

## 8. Checklist Sebelum Play

- [ ] `Use Manual Setup` dicentang di JigsawManager
- [ ] `Puzzle Board Container` & `Pieces Container` terisi
- [ ] Setiap `JigsawSlot` punya `Piece Id` yang unik dan berurutan
- [ ] Setiap `JigsawPiece` punya `Piece Id` yang cocok dengan slot pasangannya
- [ ] Scatter Area Left/Right terisi (opsional tapi disarankan)
- [ ] Tekan **"Fetch Scene Slots & Pieces"** sebelum Play
