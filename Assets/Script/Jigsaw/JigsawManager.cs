using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace LateBloom.Jigsaw
{

    public class JigsawManager : MonoBehaviour
    {
        // ─────────────────────────────────────────
        //  1. MANUAL SETUP MODE
        // ─────────────────────────────────────────
        [Header("=== MANUAL SETUP MODE ===")]
        [Tooltip("Aktifkan mode manual: semua elemen (frame, background, piece, slot) diatur langsung di Scene. " +
                 "Auto-generate DINONAKTIFKAN saat flag ini ON.")]
        public bool useManualSetup = true;

        [Tooltip("(Manual Mode) GameObject Image untuk frame/border board puzzle.")]
        public Image manualBoardFrame;

        [Tooltip("(Manual Mode) GameObject Image untuk background/latar board puzzle.")]
        public Image manualBackground;

        // ─────────────────────────────────────────
        //  2. METADATA & IDENTITY
        // ─────────────────────────────────────────
        [Header("Metadata System (Opsional / Recommended)")]
        [Tooltip("ScriptableObject metadata untuk bunga/instance ini.")]
        public PuzzleMetadata puzzleMetadata;

        [Header("Identity & Instance Setup")]
        [Tooltip("ID jenis bunga (misal: 'sunflower_phase1')")]
        public string puzzleId = "sunflower_phase1";
        [Tooltip("ID unik per penanaman/pot (misal: 'pot_01'). Memungkinkan menanam bunga sama 2x+ tanpa menimpa data.")]
        public string instanceId = "instance_01";

        // ─────────────────────────────────────────
        //  3. PHOTO & GRID (auto-generate saja)
        // ─────────────────────────────────────────
        [Header("Photo & Grid Configuration (dipakai bila useManualSetup = false)")]
        [Tooltip("Foto Utuh Kenangan (Sprite)")]
        public Sprite puzzlePhotoSprite;
        [Tooltip("Foto Utuh Kenangan (Texture2D)")]
        public Texture2D puzzlePhotoTexture;

        public const int gridRows = 2;
        public const int gridCols = 2;

        // ─────────────────────────────────────────
        //  4. BOARD & PIECE CONTAINERS
        // ─────────────────────────────────────────
        [Header("Board & Piece Containers")]
        [Tooltip("Container tempat Slot target di Canvas (misal: Board_Puzzle)")]
        public RectTransform puzzleBoardContainer;

        [Tooltip("Container tempat Kepingan Puzzle di Canvas (misal: Pieces_Container)")]
        public RectTransform piecesContainer;

        // ─────────────────────────────────────────
        //  5. SCATTER AREA
        // ─────────────────────────────────────────
        [Header("Scatter Area (Area Kepingan Berserakan)")]
        [Tooltip("Area kiri untuk kepingan berserakan")]
        public RectTransform scatterAreaLeft;
        [Tooltip("Area kanan untuk kepingan berserakan")]
        public RectTransform scatterAreaRight;

        // ─────────────────────────────────────────
        //  6. SNAP CONFIG
        // ─────────────────────────────────────────
        [Header("Snap Configuration")]
        [Tooltip("Toleransi jarak snap ke slot target (pixel)")]
        public float snapRadius = 60f;

        // ─────────────────────────────────────────
        //  7. UI & EVENTS
        // ─────────────────────────────────────────
        [Header("UI References")]
        public TextMeshProUGUI progressText;

        [Header("Events & Transition")]
        public UnityEvent onPuzzleCompleted;
        public UnityEvent onPieceSnappedEvent;
        public string flashbackSceneName = "Flashback_Phase1";
        public float delayBeforeFlashback = 1.5f;

        [Header("Game Pause Setting")]
        [Tooltip("Aktifkan true jika ingin menghentikan/pause waktu game (Time.timeScale = 0) saat puzzle jigsaw aktif, dan unpause (Time.timeScale = 1) saat jigsaw selesai/ditutup.")]
        public bool pauseGameWhenActive = true;

        // ─────────────────────────────────────────
        //  8. PIECE & SLOT LISTS (isi manual atau auto-fetch)
        // ─────────────────────────────────────────
        [Header("Piece & Slot Lists (Terisi Otomatis dari Scene)")]
        [Tooltip("Daftar JigsawSlot di scene. Klik kanan > 'Fetch Scene Slots & Pieces' untuk mengisi otomatis.")]
        public List<JigsawSlot> slots = new List<JigsawSlot>();
        [Tooltip("Daftar JigsawPiece di scene. Klik kanan > 'Fetch Scene Slots & Pieces' untuk mengisi otomatis.")]
        public List<JigsawPiece> pieces = new List<JigsawPiece>();

        // ─────────────────────────────────────────
        //  PRIVATE STATE
        // ─────────────────────────────────────────
        private int snappedCount = 0;
        private bool isCompleted = false;

        // ══════════════════════════════════════════
        //  SAVE KEY HELPER
        // ══════════════════════════════════════════
        public string GetFullSaveKey()
        {
            if (puzzleMetadata != null) return puzzleMetadata.GetFullSaveKey();
            return $"{puzzleId}_{instanceId}";
        }

        // ══════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ══════════════════════════════════════════
        private void Start()
        {
            ApplyMetadataIfAvailable();

            if (useManualSetup)
            {
                // Validasi elemen manual lalu langsung init
                ValidateManualSetup();
                FetchSceneSlotsAndPieces();
            }
            else
            {
                FetchSceneSlotsAndPieces();
            }

            InitializePuzzle();
        }

        private void OnDisable()  
        { 
            SaveProgress(); 
            if (pauseGameWhenActive) Time.timeScale = 1f; 
        }
        private void OnApplicationPause(bool p) { if (p) SaveProgress(); }
        private void OnDestroy()  
        { 
            SaveProgress(); 
            if (pauseGameWhenActive) Time.timeScale = 1f; 
        }

        // ══════════════════════════════════════════
        //  METADATA
        // ══════════════════════════════════════════
        public void ApplyMetadataIfAvailable()
        {
            if (puzzleMetadata != null)
            {
                puzzleId = puzzleMetadata.puzzleId;
                instanceId = puzzleMetadata.instanceId;

                // Terapkan foto/sprite dari Metadata ke JigsawManager
                if (puzzleMetadata.puzzlePhotoSprite != null)
                {
                    puzzlePhotoSprite = puzzleMetadata.puzzlePhotoSprite;
                    if (puzzlePhotoTexture == null && puzzlePhotoSprite.texture != null)
                    {
                        puzzlePhotoTexture = puzzlePhotoSprite.texture;
                    }
                }
                if (puzzleMetadata.puzzlePhotoTexture != null)
                {
                    puzzlePhotoTexture = puzzleMetadata.puzzlePhotoTexture;
                    if (puzzlePhotoSprite == null && puzzlePhotoTexture != null)
                    {
                        puzzlePhotoSprite = Sprite.Create(
                            puzzlePhotoTexture,
                            new Rect(0, 0, puzzlePhotoTexture.width, puzzlePhotoTexture.height),
                            new Vector2(0.5f, 0.5f)
                        );
                    }
                }

                // Terapkan Background & Frame jika diisi di Metadata
                if (puzzleMetadata.backgroundSprite != null && manualBackground != null)
                {
                    manualBackground.sprite = puzzleMetadata.backgroundSprite;
                }
                if (puzzleMetadata.boardFrameSprite != null && manualBoardFrame != null)
                {
                    manualBoardFrame.sprite = puzzleMetadata.boardFrameSprite;
                }

                puzzleMetadata.LoadFromDisk();

                // Potong dan terapkan foto ke kepingan puzzle jika kepingan ada di scene
                if (puzzlePhotoTexture != null && pieces != null && pieces.Count > 0)
                {
                    SliceAndAssignPhotoToPieces();
                }
            }
        }

        // ══════════════════════════════════════════
        //  MANUAL SETUP VALIDATION
        // ══════════════════════════════════════════
        /// <summary>
        /// Memvalidasi semua referensi manual dan menampilkan peringatan bila ada yang kurang.
        /// Dipanggil saat useManualSetup = true.
        /// </summary>
        [ContextMenu("Validate Manual Setup")]
        public void ValidateManualSetup()
        {
            if (!useManualSetup)
            {
                Debug.Log("[JigsawManager] useManualSetup = false. Mode manual tidak aktif.");
                return;
            }

            bool ok = true;

            if (puzzleBoardContainer == null)
            {
                Debug.LogWarning("[JigsawManager] Manual Setup: 'Puzzle Board Container' (tempat Slot) belum diisi! " +
                                 "Drag GameObject Board_Puzzle ke field ini.");
                ok = false;
            }

            if (piecesContainer == null)
            {
                Debug.LogWarning("[JigsawManager] Manual Setup: 'Pieces Container' (tempat Piece) belum diisi! " +
                                 "Drag GameObject Pieces_Root ke field ini.");
                ok = false;
            }

            if (manualBoardFrame == null)
                Debug.LogWarning("[JigsawManager] Manual Setup: 'Manual Board Frame' belum diisi (opsional, tapi disarankan).");

            if (manualBackground == null)
                Debug.LogWarning("[JigsawManager] Manual Setup: 'Manual Background' belum diisi (opsional, tapi disarankan).");

            if (slots.Count < 4)
            {
                Debug.LogWarning($"[JigsawManager] Manual Setup: Hanya ditemukan {slots.Count} Slot di scene! Dibutuhkan 4 Slot (Slot_0, Slot_1, Slot_2, Slot_3) untuk puzzle 2x2.");
                ok = false;
            }

            if (pieces.Count < 4)
            {
                Debug.LogWarning($"[JigsawManager] Manual Setup: Hanya ditemukan {pieces.Count} Piece di scene! Dibutuhkan 4 Piece (Piece_0, Piece_1, Piece_2, Piece_3) untuk puzzle 2x2.");
                ok = false;
            }

            if (ok)
                Debug.Log("[JigsawManager] ✅ Manual Setup: Semua referensi wajib & 4 keping puzzle sudah terisi dengan benar!");
        }

        // ══════════════════════════════════════════
        //  FETCH SLOTS & PIECES FROM SCENE
        // ══════════════════════════════════════════
        /// <summary>
        /// Mengambil dan menghubungkan seluruh JigsawSlot &amp; JigsawPiece yang ditaruh manual di Scene Canvas.
        /// </summary>
        [ContextMenu("Fetch Scene Slots & Pieces")]
        public void FetchSceneSlotsAndPieces()
        {
            slots.Clear();
            pieces.Clear();

            // ── Ambil Slots (Maksimal 4) ──────────────────────
            if (puzzleBoardContainer != null)
            {
                JigsawSlot[] foundSlots = puzzleBoardContainer.GetComponentsInChildren<JigsawSlot>(true);

                // Jika ada sisa kepingan lama > 4 (misal sisa 9 keping), hapus sisa kepingan 4-8
                if (foundSlots.Length > 4)
                {
                    for (int i = foundSlots.Length - 1; i >= 4; i--)
                    {
                        if (Application.isEditor && !Application.isPlaying)
                            DestroyImmediate(foundSlots[i].gameObject);
                        else
                            Destroy(foundSlots[i].gameObject);
                    }
                    foundSlots = puzzleBoardContainer.GetComponentsInChildren<JigsawSlot>(true);
                }

                if (foundSlots.Length == 0 && !useManualSetup)
                {
                    GenerateBoardSlots();
                    foundSlots = puzzleBoardContainer.GetComponentsInChildren<JigsawSlot>(true);
                }
                else if (foundSlots.Length == 0 && useManualSetup)
                {
                    Debug.LogWarning("[JigsawManager] Manual Setup: Tidak ada JigsawSlot di dalam Board Container! " +
                                     "Tambahkan 4 child GameObject dengan komponen JigsawSlot secara manual di Scene.");
                }

                int count = Mathf.Min(foundSlots.Length, 4);
                for (int i = 0; i < count; i++)
                {
                    foundSlots[i].pieceId = i;
                    slots.Add(foundSlots[i]);
                }
            }

            // ── Ambil Pieces (Maksimal 4) ─────────────────────
            if (piecesContainer != null)
            {
                JigsawPiece[] foundPieces = piecesContainer.GetComponentsInChildren<JigsawPiece>(true);

                // Jika ada sisa kepingan lama > 4 (misal sisa 9 keping), hapus sisa kepingan 4-8
                if (foundPieces.Length > 4)
                {
                    for (int i = foundPieces.Length - 1; i >= 4; i--)
                    {
                        if (Application.isEditor && !Application.isPlaying)
                            DestroyImmediate(foundPieces[i].gameObject);
                        else
                            Destroy(foundPieces[i].gameObject);
                    }
                    foundPieces = piecesContainer.GetComponentsInChildren<JigsawPiece>(true);
                }

                int count = Mathf.Min(foundPieces.Length, 4);
                for (int i = 0; i < count; i++)
                {
                    foundPieces[i].pieceId = i;
                    pieces.Add(foundPieces[i]);
                }
            }

            // ── Auto-slice foto (hanya kalau bukan manual atau foto diberikan & useGeneratedShape aktif) ──
            if (!useManualSetup)
            {
                if (puzzlePhotoTexture == null && puzzlePhotoSprite != null)
                    puzzlePhotoTexture = puzzlePhotoSprite.texture;

                if (puzzlePhotoTexture != null && pieces.Count > 0)
                    SliceAndAssignPhotoToPieces();
            }
            else
            {
                // Manual mode: cukup assign foto ke piece yang minta (useGeneratedShape = true)
                if (puzzlePhotoTexture == null && puzzlePhotoSprite != null)
                    puzzlePhotoTexture = puzzlePhotoSprite.texture;

                bool anyNeedSlice = false;
                foreach (var p in pieces)
                    if (p != null && p.useGeneratedShape) { anyNeedSlice = true; break; }

                if (anyNeedSlice && puzzlePhotoTexture != null)
                    SliceAndAssignPhotoToPieces();
            }

            Debug.Log($"[JigsawManager] Berhasil mengambil {slots.Count} Slot dan {pieces.Count} Piece dari Scene.");
        }

        /// <summary>
        /// Menghapus Slot dan Piece berlebih di Scene Hierarchy jika lebih dari target 4 keping (index >= 4).
        /// </summary>
        [ContextMenu("Trim Extra Scene Slots & Pieces (Keep 4 Only)")]
        public void TrimExtraSlotsAndPiecesTo4()
        {
            int targetCount = gridRows * gridCols; // 2x2 = 4

            if (puzzleBoardContainer != null)
            {
                JigsawSlot[] foundSlots = puzzleBoardContainer.GetComponentsInChildren<JigsawSlot>(true);
                for (int i = foundSlots.Length - 1; i >= targetCount; i--)
                {
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(foundSlots[i].gameObject);
                    else
                        Destroy(foundSlots[i].gameObject);
                }
            }

            if (piecesContainer != null)
            {
                JigsawPiece[] foundPieces = piecesContainer.GetComponentsInChildren<JigsawPiece>(true);
                for (int i = foundPieces.Length - 1; i >= targetCount; i--)
                {
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(foundPieces[i].gameObject);
                    else
                        Destroy(foundPieces[i].gameObject);
                }
            }

            FetchSceneSlotsAndPieces();
            Debug.Log($"[JigsawManager] Berhasil merapikan Scene. Tersisa {slots.Count} Slot dan {pieces.Count} Piece.");
        }

        // ══════════════════════════════════════════
        //  AUTO-GENERATE BOARD SLOTS (non-manual saja)
        // ══════════════════════════════════════════
        /// <summary>
        /// Membuat grid Slot secara otomatis di dalam Board_Puzzle. Hanya dipakai saat useManualSetup = false.
        /// </summary>
        [ContextMenu("Generate Board Slots (Auto)")]
        public void GenerateBoardSlots()
        {
            if (useManualSetup)
            {
                Debug.LogWarning("[JigsawManager] Generate Board Slots tidak tersedia saat useManualSetup = true. " +
                                 "Matikan flag 'Use Manual Setup' dulu.");
                return;
            }

            if (puzzleBoardContainer == null) return;

            JigsawSlot[] oldSlots = puzzleBoardContainer.GetComponentsInChildren<JigsawSlot>(true);
            for (int i = oldSlots.Length - 1; i >= 0; i--)
            {
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(oldSlots[i].gameObject);
                else
                    Destroy(oldSlots[i].gameObject);
            }

            int totalPieces = gridRows * gridCols;

            Vector2 boardSize = puzzleBoardContainer.rect.width > 0
                ? puzzleBoardContainer.rect.size : new Vector2(480f, 480f);
            float pieceWidth  = boardSize.x / gridCols;
            float pieceHeight = boardSize.y / gridRows;

            GridLayoutGroup gridLayout = puzzleBoardContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
                gridLayout = puzzleBoardContainer.gameObject.AddComponent<GridLayoutGroup>();

            gridLayout.cellSize        = new Vector2(pieceWidth, pieceHeight);
            gridLayout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridCols;
            gridLayout.childAlignment  = TextAnchor.MiddleCenter;

            for (int i = 0; i < totalPieces; i++)
            {
                GameObject slotObj = new GameObject($"Slot_{i}",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(JigsawSlot));
                slotObj.transform.SetParent(puzzleBoardContainer, false);

                Image slotImage = slotObj.GetComponent<Image>();
                slotImage.color = new Color(0f, 0f, 0f, 0.2f);

                JigsawSlot slot = slotObj.GetComponent<JigsawSlot>();
                slot.pieceId = i;
                slot.ghostHighlightImage = slotImage;
            }

            Debug.Log($"[JigsawManager] Berhasil membuat {totalPieces} Slot otomatis di {puzzleBoardContainer.name}.");
        }

        // ══════════════════════════════════════════
        //  AUTO-GENERATE PIECES
        // ══════════════════════════════════════════
        /// <summary>
        /// Membuat GameObject JigsawPiece secara otomatis di dalam Pieces Container.
        /// Hanya dipakai saat useManualSetup = false.
        /// </summary>
        [ContextMenu("Generate Pieces (Auto)")]
        public void GeneratePieces()
        {
            if (useManualSetup)
            {
                Debug.LogWarning("[JigsawManager] Generate Pieces tidak tersedia saat useManualSetup = true.");
                return;
            }

            if (piecesContainer == null)
            {
                Debug.LogWarning("[JigsawManager] 'Pieces Container' belum diisi di Inspector!");
                return;
            }

            // Hapus piece lama kalau ada
            JigsawPiece[] oldPieces = piecesContainer.GetComponentsInChildren<JigsawPiece>(true);
            for (int i = oldPieces.Length - 1; i >= 0; i--)
            {
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(oldPieces[i].gameObject);
                else
                    Destroy(oldPieces[i].gameObject);
            }
            pieces.Clear();

            int total = gridRows * gridCols;
            Vector2 boardSize = puzzleBoardContainer != null && puzzleBoardContainer.rect.width > 0
                ? puzzleBoardContainer.rect.size : new Vector2(480f, 480f);
            float pieceW = boardSize.x / gridCols;
            float pieceH = boardSize.y / gridRows;

            for (int i = 0; i < total; i++)
            {
                GameObject pieceObj = new GameObject($"Piece_{i}",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                    typeof(CanvasGroup), typeof(JigsawPiece));
                pieceObj.transform.SetParent(piecesContainer, false);

                RectTransform rt = pieceObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(pieceW, pieceH);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);

                // Posisi awal di tengah canvas — nanti di-scatter saat Play
                rt.anchoredPosition = Vector2.zero;

                Image img = pieceObj.GetComponent<Image>();
                img.color = Color.white;
                img.raycastTarget = true;

                JigsawPiece piece = pieceObj.GetComponent<JigsawPiece>();
                piece.pieceId = i;
                piece.useGeneratedShape = true; // Auto mode selalu pakai generated shape

                pieces.Add(piece);
            }

            Debug.Log($"[JigsawManager] Berhasil membuat {total} Piece di {piecesContainer.name}.");
        }

        // ══════════════════════════════════════════
        //  ONE-CLICK: GENERATE BOARD + PIECES + CUT PHOTO
        // ══════════════════════════════════════════
        /// <summary>
        /// Satu klik untuk generate slot, piece, dan potong foto sekaligus.
        /// </summary>
        [ContextMenu("⚡ Generate Board + Pieces + Cut Photo (All-in-One)")]
        public void GenerateBoardAndPieces()
        {
            if (useManualSetup)
            {
                Debug.LogWarning("[JigsawManager] All-in-One Generate tidak tersedia saat useManualSetup = true.");
                return;
            }
            GenerateBoardSlots();
            GeneratePieces();
            SliceAndAssignPhotoToPieces();
            Debug.Log("[JigsawManager] ✅ Board, Pieces, dan Photo selesai di-generate!");
        }

        // ══════════════════════════════════════════
        //  SLICE PHOTO & ASSIGN TO PIECES
        // ══════════════════════════════════════════
        /// <summary>
        /// Memotong 1 Foto Utuh dan memasang potongan sprite-nya ke kepingan puzzle.
        /// Di manual mode, hanya piece dengan useGeneratedShape = true yang diproses.
        /// </summary>
        [ContextMenu("Cut Photo & Assign to Pieces")]
        public void SliceAndAssignPhotoToPieces()
        {
            if (puzzlePhotoTexture == null && puzzlePhotoSprite != null)
                puzzlePhotoTexture = puzzlePhotoSprite.texture;

            if (puzzlePhotoTexture == null)
            {
                Debug.LogWarning("[JigsawManager] Harap masukkan 'Puzzle Photo Texture' atau 'Puzzle Photo Sprite' di Inspector!");
                return;
            }

            if (pieces.Count == 0)
            {
                // Auto mode: coba generate pieces dulu sebelum menyerah
                if (!useManualSetup)
                {
                    GeneratePieces();
                }
                else
                {
                    FetchSceneSlotsAndPieces();
                }
                if (pieces.Count == 0)
                {
                    Debug.LogWarning("[JigsawManager] Tidak ada Piece ditemukan. " +
                        (useManualSetup
                            ? "Tambahkan JigsawPiece di scene lalu klik 'Fetch Scene Slots & Pieces'."
                            : "Pastikan 'Pieces Container' sudah diisi, lalu klik 'Generate Pieces (Auto)'."));
                    return;
                }
            }

            float baseTexW = (float)puzzlePhotoTexture.width  / gridCols;
            float baseTexH = (float)puzzlePhotoTexture.height / gridRows;

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null) continue;

                // Manual mode: skip pieces yang tidak minta generated shape
                if (useManualSetup && !pieces[i].useGeneratedShape) continue;

                int r = i / gridCols;
                int c = i % gridCols;

                float texX = c * baseTexW;
                float texY = (gridRows - 1 - r) * baseTexH;
                Rect cropRect = new Rect(texX, texY, baseTexW, baseTexH);
                Texture2D croppedTex = CropTexture(puzzlePhotoTexture, cropRect);
                Sprite pieceSprite = Sprite.Create(croppedTex,
                    new Rect(0, 0, croppedTex.width, croppedTex.height), new Vector2(0.5f, 0.5f));

                Image pieceImg = pieces[i].GetComponent<Image>();
                if (pieceImg != null)
                {
                    pieceImg.sprite = pieceSprite;
                    pieceImg.color  = Color.white;
                }
            }

            Debug.Log($"[JigsawManager] Foto '{puzzlePhotoTexture.name}' berhasil dipotong menjadi {pieces.Count} kepingan persegi.");
        }

        private Texture2D CropTexture(Texture2D source, Rect cropRect)
        {
            int width  = Mathf.FloorToInt(cropRect.width);
            int height = Mathf.FloorToInt(cropRect.height);
            int x = Mathf.FloorToInt(cropRect.x);
            int y = Mathf.FloorToInt(cropRect.y);

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = source.GetPixels(x, y, width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        // ══════════════════════════════════════════
        //  INITIALIZE PUZZLE
        // ══════════════════════════════════════════
        public void InitializePuzzle()
        {
            snappedCount = 0;
            isCompleted  = false;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null) continue;
                slots[i].pieceId = i;
                slots[i].SetOccupied(false);

                // Samakan ukuran piece dengan slot-nya
                if (i < pieces.Count && pieces[i] != null)
                {
                    RectTransform slotRect  = slots[i].rectTransform;
                    RectTransform pieceRect = pieces[i].rectTransform;
                    if (slotRect != null && pieceRect != null)
                        pieceRect.sizeDelta = slotRect.sizeDelta;
                }
            }

            for (int i = 0; i < pieces.Count; i++)
                if (pieces[i] != null)
                    pieces[i].Initialize(this, i);

            // Load progres; jika belum ada, acak kepingan ke area scatter (hanya untuk auto mode)
            if (!LoadProgress())
            {
                if (!useManualSetup)
                {
                    ScatterPieces();
                }
            }

            UpdateProgressUI();

            if (pauseGameWhenActive)
            {
                Time.timeScale = 0f;
            }
        }

        // ══════════════════════════════════════════
        //  SCATTER PIECES
        // ══════════════════════════════════════════
        public void ScatterPieces()
        {
            StartCoroutine(ScatterPiecesDelayed());
        }

        private IEnumerator ScatterPiecesDelayed()
        {
            // Tunggu 1 frame agar Canvas layout selesai dihitung dulu
            yield return null;

            List<RectTransform> validAreas = new List<RectTransform>();
            if (scatterAreaLeft  != null) validAreas.Add(scatterAreaLeft);
            if (scatterAreaRight != null) validAreas.Add(scatterAreaRight);

            if (validAreas.Count == 0)
            {
                Debug.LogWarning("[JigsawManager] Scatter Area Left/Right belum diisi! Kepingan berada di posisi default Canvas.");
                yield break;
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null || pieces[i].currentState == PieceState.Snapped) continue;

                RectTransform targetArea = validAreas[i % validAreas.Count];

                // Pakai GetWorldCorners agar dapat ukuran NYATA setelah Canvas layout
                Vector3[] corners = new Vector3[4];
                targetArea.GetWorldCorners(corners);
                // corners: [0]=bottom-left [1]=top-left [2]=top-right [3]=bottom-right

                float minX = corners[0].x;
                float maxX = corners[2].x;
                float minY = corners[0].y;
                float maxY = corners[2].y;

                if (Mathf.Approximately(minX, maxX) || Mathf.Approximately(minY, maxY))
                {
                    Debug.LogWarning($"[JigsawManager] '{targetArea.name}' punya ukuran 0 — pastikan Width & Height diisi di RectTransform!");
                    continue;
                }

                float margin = 40f;
                float rx = UnityEngine.Random.Range(minX + margin, maxX - margin);
                float ry = UnityEngine.Random.Range(minY + margin, maxY - margin);
                pieces[i].rectTransform.position = new Vector3(rx, ry, 0f);
            }
        }


        // ══════════════════════════════════════════
        //  SLOT HELPERS
        // ══════════════════════════════════════════
        public JigsawSlot GetSlot(int pieceId)
        {
            if (pieceId >= 0 && pieceId < slots.Count) return slots[pieceId];
            return null;
        }

        public float GetSnapDistance(JigsawPiece piece, JigsawSlot slot)
        {
            Canvas canvas = piece.GetComponentInParent<Canvas>();
            float scaleFactor = (canvas != null && canvas.scaleFactor > 0) ? canvas.scaleFactor : 1f;
            return Vector2.Distance(piece.rectTransform.position, slot.rectTransform.position) / scaleFactor;
        }

        public void CheckHoverSlot(JigsawPiece piece)
        {
            JigsawSlot targetSlot = GetSlot(piece.pieceId);
            if (targetSlot == null || targetSlot.isOccupied) return;
            targetSlot.SetHover(GetSnapDistance(piece, targetSlot) <= snapRadius);
        }

        public bool TrySnapPiece(JigsawPiece piece)
        {
            JigsawSlot targetSlot = GetSlot(piece.pieceId);
            if (targetSlot == null || targetSlot.isOccupied) return false;
            targetSlot.SetHover(false);
            return GetSnapDistance(piece, targetSlot) <= snapRadius;
        }

        // ══════════════════════════════════════════
        //  PIECE EVENTS
        // ══════════════════════════════════════════
        public void OnPiecePickup()
        {
            if (JigsawAudioManager.Instance != null)
                JigsawAudioManager.Instance.PlayPickupSFX();
        }

        public void OnPieceSnapped(JigsawPiece piece)
        {
            snappedCount++;
            UpdateProgressUI();
            SaveProgress();

            if (JigsawAudioManager.Instance != null)
                JigsawAudioManager.Instance.PlaySnapSFX();

            onPieceSnappedEvent?.Invoke();

            if (snappedCount >= pieces.Count && !isCompleted)
                CompletePuzzle();
        }

        private void CompletePuzzle()
        {
            isCompleted = true;
            SaveProgress();

            if (JigsawAudioManager.Instance != null)
                JigsawAudioManager.Instance.PlayCompleteSFX();

            onPuzzleCompleted?.Invoke();
            StartCoroutine(FlashbackRoutine());
        }

        private IEnumerator FlashbackRoutine()
        {
            Time.timeScale = 1f;
            yield return new WaitForSecondsRealtime(delayBeforeFlashback);
            if (!string.IsNullOrEmpty(flashbackSceneName))
                UnityEngine.SceneManagement.SceneManager.LoadScene(flashbackSceneName);
        }

        private void UpdateProgressUI()
        {
            if (progressText != null)
                progressText.text = $"Pieces: {snappedCount} / {pieces.Count}";
        }

        // ══════════════════════════════════════════
        //  SAVE / LOAD
        // ══════════════════════════════════════════
        public void SaveProgress()
        {
            if (puzzleMetadata != null)
            {
                puzzleMetadata.isCompleted = this.isCompleted;
                puzzleMetadata.pieceStates.Clear();
                foreach (var piece in pieces)
                {
                    if (piece == null) continue;
                    puzzleMetadata.pieceStates.Add(new PieceSaveState
                    {
                        id        = piece.pieceId,
                        isSnapped = piece.currentState == PieceState.Snapped,
                        posX      = piece.rectTransform.anchoredPosition.x,
                        posY      = piece.rectTransform.anchoredPosition.y
                    });
                }
                puzzleMetadata.SaveToDisk();
                return;
            }

            PuzzleSaveData data = new PuzzleSaveData
            {
                puzzleId    = this.puzzleId,
                isCompleted = this.isCompleted
            };
            foreach (var piece in pieces)
            {
                if (piece == null) continue;
                data.pieceStates.Add(new PieceSaveState
                {
                    id        = piece.pieceId,
                    isSnapped = piece.currentState == PieceState.Snapped,
                    posX      = piece.rectTransform.anchoredPosition.x,
                    posY      = piece.rectTransform.anchoredPosition.y
                });
            }
            PlayerPrefs.SetString("PuzzleSave_" + GetFullSaveKey(), JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public bool LoadProgress()
        {
            if (puzzleMetadata != null)
            {
                if (!puzzleMetadata.LoadFromDisk()) return false;

                snappedCount = 0;
                var statesCopy = puzzleMetadata.pieceStates.ToArray();
                foreach (var pieceState in statesCopy)
                {
                    if (pieceState.id < 0 || pieceState.id >= pieces.Count) continue;
                    JigsawPiece piece = pieces[pieceState.id];
                    JigsawSlot  slot  = GetSlot(pieceState.id);
                    piece.SetSavedPosition(new Vector2(pieceState.posX, pieceState.posY), pieceState.isSnapped, slot);
                    if (pieceState.isSnapped) snappedCount++;
                }
                isCompleted = puzzleMetadata.isCompleted;
                UpdateProgressUI();
                return true;
            }

            string key = "PuzzleSave_" + GetFullSaveKey();
            if (!PlayerPrefs.HasKey(key)) return false;

            PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(PlayerPrefs.GetString(key));
            if (data == null || data.pieceStates == null) return false;

            snappedCount = 0;
            var playerPrefsStatesCopy = data.pieceStates.ToArray();
            foreach (var pieceState in playerPrefsStatesCopy)
            {
                if (pieceState.id < 0 || pieceState.id >= pieces.Count) continue;
                JigsawPiece piece = pieces[pieceState.id];
                JigsawSlot  slot  = GetSlot(pieceState.id);
                piece.SetSavedPosition(new Vector2(pieceState.posX, pieceState.posY), pieceState.isSnapped, slot);
                if (pieceState.isSnapped) snappedCount++;
            }
            isCompleted = data.isCompleted;
            UpdateProgressUI();
            return true;
        }

        // ══════════════════════════════════════════
        //  REPLAY
        // ══════════════════════════════════════════
        public void ReplayPuzzle()
        {
            ClearSaveData();
            InitializePuzzle();
        }

        [ContextMenu("Clear Save Data")]
        public void ClearSaveData()
        {
            if (puzzleMetadata != null)
                puzzleMetadata.ResetMetadata();
            else
                PlayerPrefs.DeleteKey("PuzzleSave_" + GetFullSaveKey());

            PlayerPrefs.Save();
            Debug.Log("[JigsawManager] Save data puzzle berhasil dihapus!");
        }
    }

}
