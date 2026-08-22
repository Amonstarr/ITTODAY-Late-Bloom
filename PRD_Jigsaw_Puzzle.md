# PRD — Jigsaw Puzzle Mechanic
**Late Bloom** | Roti o Stasiun Gubeng | Telkom University

---

## 1. Overview

| Field | Detail |
|---|---|
| Fitur | Flashback Jigsaw Puzzle |
| Fase Cerita | Fase 1 — Bunga Matahari (Kesetiaan) |
| Prioritas | P0 — Core gameplay loop |
| Status | Final Draft |
| Dibuat | Minggu ke-1 |

### Problem Statement
Pemain membutuhkan cara untuk "merasakan" kepingan kenangan, bukan sekadar menonton cutscene pasif. Jigsaw puzzle dipilih karena secara tematik mencerminkan ingatan yang tersebar dan perlu disusun kembali — seperti kenangan yang datang sepotong demi sepotong seiring merawat tanaman.

### Goal
Menyediakan mekanik puzzle interaktif yang secara naratif kohesif: mengumpulkan kepingan di setiap fase pertumbuhan bunga (Seed, Bud, Bloom), lalu menyusun kembali foto kenangan Bu Sari sebagai representasi Pak Wira yang sedang "mengingat kembali" momen yang telah lama terlupakan setelah bunga mekar penuh.

---

## 2. User Story

```
Sebagai pemain,
setiap kali bunga melewati fase pertumbuhan (Seed -> Bud -> Bloom),
saya mendapatkan kepingan puzzle foto kenangan,
dan setelah bunga mekar penuh (Bloom),
saya dapat menyusun seluruh kepingan tersebut
agar saya bisa melihat flashback dan merasakan cerita di balik bunga tersebut.
```

### Acceptance Criteria
- [ ] Kepingan puzzle diberikan secara bertahap di setiap fase pertumbuhan bunga (Seed, Bud, Bloom) dengan jumlah yang dapat dikonfigurasi di Unity
- [ ] Puzzle scene / UI hanya dapat dibuka/dimainkan setelah bunga mencapai fase **Bloom (Mekar)**
- [ ] Puzzle menampilkan foto kenangan yang terpecah menjadi kepingan-kepingan puzzle
- [ ] Pemain dapat mengambil (drag), memindahkan, dan menempatkan (drop) keping ke posisi yang benar
- [ ] Keping yang benar di tempat yang benar "terkunci" (snap) dengan feedback visual & audio
- [ ] Puzzle selesai ketika semua keping terpasang → trigger flashback scene
- [ ] Pemain bisa keluar dari puzzle dan melanjutkan di lain waktu (progress tersimpan)
- [ ] Puzzle dapat di-replay setelah selesai

---

## 3. Scope

### In Scope
- Sistem distribusi kepingan berbasis fase pertumbuhan bunga (dapat diatur per fase di Unity)
- Puzzle grid berbasis posisi snap
- Drag & drop mechanic untuk keping puzzle (Mouse / Touch)
- Sistem deteksi keping benar / salah
- Visual feedback: keping terkunci, hover highlight, preview ghosting / frame
- Audio feedback: SFX pickup, SFX snap, SFX complete, BGM puzzle
- Trigger ke flashback scene / event setelah puzzle selesai
- Save/load progress puzzle & keping yang sudah terkumpul
- Replay puzzle feature

### Out of Scope (untuk fase ini)
- Hint system (ditiadakan agar puzzle murni berbasis eksplorasi pemain)
- Rotasi keping puzzle
- Keping puzzle yang bisa dibalik
- Multiplayer / co-op puzzle
- Procedural generation potongan puzzle
- Timer / leaderboard
- Puzzle fase 2–5 (dibuat terpisah, menggunakan komponen ini sebagai base)

---

## 4. Gameplay Flow

```
[Fase 1: Seed]  ──► Dapat kepingan puzzle tahap 1 (tersimpan di inventory/manager)
      │
      ▼
[Fase 2: Bud]   ──► Dapat kepingan puzzle tahap 2 (tersimpan di inventory/manager)
      │
      ▼
[Fase 3: Bloom] ──► Dapat kepingan puzzle tahap 3 (semua keping terkumpul lengkap)
      │
      ▼
[Cutscene singkat: Pak Wira menemukan kotak foto lama]
      │
      ▼
[Buka Layar Jigsaw Puzzle]
      │
      ▼
[Keping foto tersebar di area scatter]
      │
      ├─► Pemain drag keping → hover di slot grid
      │         │
      │         ├─ Posisi benar (dalam snap radius) → snap + kunci + SFX klik
      │         └─ Posisi salah / di luar snap radius → keping kembali ke posisi idle terakhir
      │
      └─► Semua keping terpasang
                │
                ▼
      [Animasi foto menyatu + SFX complete]
                │
                ▼
      [Fade out → Flashback Scene: Hari pertama tinggal bersama]
                │
                ▼
      [Flashback selesai → kembali ke dunia game / unlock album]
```

---

## 5. Distribusi Kepingan Berbasis Fase (Unity Configurable)

Jumlah kepingan per fase dapat diatur langsung di Unity Inspector melalui component `PuzzlePhaseManager` / `JigsawGameManager`:

| Fase Pertumbuhan | Default Kepingan | Status Kepingan |
|---|---|---|
| **Seed (Benih)** | 4 keping | Terkumpul & tersimpan, puzzle belum bisa dimainkan |
| **Bud (Kuncup)** | 4 keping | Terkumpul & tersimpan, puzzle belum bisa dimainkan |
| **Bloom (Mekar)**| 8 keping | Seluruh keping lengkap (Total 16 keping) & Puzzle Mode terbuka |

> **Catatan:** Nilai ini dapat diubah di Unity Inspector (misal: 2, 2, 4 untuk total 8 keping, atau custom distribution lainnya).

---

## 6. Spesifikasi Mekanik

### 6.1 Grid & Keping

| Parameter | Nilai Default | Catatan |
|---|---|---|
| Ukuran grid default | 4×4 | 16 keping; dapat disesuaikan di Unity |
| Ukuran keping | 120×120 px | Pada canvas UI / resolusi target |
| Snap radius | 35 px | Jarak toleransi agar keping otomatis menempel pas |
| Area Board / Slot | Area tengah board | Tempat menyusun kepingan puzzle |
| Area Scatter | Tepi / sisi board | Area kepingan awal berserakan |

### 6.2 State Keping

```
IDLE          → keping diam di posisi scatter / luar slot
DRAGGING      → keping sedang dipegang pemain (z-index / sorting order tertinggi)
HOVERING      → keping berada di dekat slot target
SNAPPED       → keping benar dan terkunci permanen (tidak bisa di-drag lagi)
```

### 6.3 Drag & Drop Rules
- Hanya satu keping yang dapat di-drag pada satu waktu
- Keping yang sudah `SNAPPED` tidak bisa di-drag ulang
- Jika keping di-drop di luar snap radius slot yang benar, keping tetap berada di posisi drop (atau kembali ke idle scatter)
- Deteksi snap menggunakan jarak Euclidean antara anchor keping dan slot target

### 6.4 Completion Condition
- Semua keping dalam state `SNAPPED`
- Trigger animasi menyatu / efek flash lembut
- Delay 1.5 detik → trigger callback `OnPuzzleCompleted` / load Flashback Scene

---

## 7. Visual & Audio

### 7.1 Visual
| Elemen | Deskripsi |
|---|---|
| Background puzzle | Meja kayu tua dengan pencahayaan hangat |
| Slot Grid | Outline transparan / grid border tipis sebagai panduan visual |
| Keping IDLE | Sedikit shadow / border |
| Keping DRAGGING | Scale up sedikit (1.05x), sorting order paling depan |
| Keping SNAPPED | Outline memudar, terkunci sempurna di posisi slot |
| Progress Indicator | Text counter di UI: "Pieces: X / Total" |

### 7.2 Audio
| Event | SFX |
|---|---|
| Keping di-pick up | Suara sentuhan / kertas ringan |
| Keping SNAPPED | Suara "klik" kayu / snap yang memuaskan |
| Puzzle Selesai | Chord piano / jingle hangat |
| BGM Puzzle | Musik santai / melankolis |

---

## 8. Save & Load System

Progress puzzle dan status pengumpulan keping per fase disimpan (PlayerPrefs / JSON save):
```json
{
  "puzzle_id": "sunflower_phase1",
  "seed_pieces_collected": 4,
  "bud_pieces_collected": 4,
  "bloom_pieces_collected": 8,
  "is_unlocked": true,
  "is_completed": false,
  "pieces_state": [
    { "id": 0, "is_snapped": true, "pos_x": 0.0, "pos_y": 0.0 },
    { "id": 1, "is_snapped": false, "pos_x": -320.0, "pos_y": 150.0 }
  ]
}
```

---

## 9. Arsitektur Script Unity

1. `JigsawPiece.cs` — Mengatur interaksi drag-and-drop, snapping logic, visual state, dan collision/touch detection.
2. `JigsawSlot.cs` — Menentukan titik target snap koordinat untuk tiap ID kepingan puzzle.
3. `JigsawManager.cs` — Mengelola inisialisasi board, pembuatan slot & keping, pengecekan kondisi menang, save/load, dan audio.
4. `PuzzlePhaseManager.cs` — Menghubungkan fase pertumbuhan tanaman (`Seed`, `Bud`, `Bloom`) ke pengumpulan kepingan puzzle & unlock puzzle saat Bloom.
5. `FlowerGrowthController.cs` (Mock/Integration) — Komponen pengatur pertumbuhan bunga untuk trigger transisi fase dan testing.

---

*PRD ini adalah living document — diperbarui seiring progres pengembangan.*  
*Late Bloom | Roti o Stasiun Gubeng | Telkom University*

