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

        [Tooltip("Pilih bentuk kepingan: Kotak atau Klasik Jigsaw")]
        public PieceShapeStyle shapeStyle = PieceShapeStyle.JigsawInterlocking;

        [Range(2, 10)] public int gridRows = 3;
        [Range(2, 10)] public int gridCols = 3;

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

        private void OnDisable()  { SaveProgress(); }
        private void OnApplicationPause(bool p) { if (p) SaveProgress(); }
        private void OnDestroy()  { SaveProgress(); }

        // ══════════════════════════════════════════
        //  METADATA
        // ══════════════════════════════════════════
        public void ApplyMetadataIfAvailable()
        {
            if (puzzleMetadata != null)
            {
                puzzleId = puzzleMetadata.puzzleId;
                instanceId = puzzleMetadata.instanceId;
                puzzleMetadata.LoadFromDisk();
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

            if (ok)
                Debug.Log("[JigsawManager] ✅ Manual Setup: Semua referensi wajib sudah terisi dengan benar!");
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

            // ── Ambil Slots ──────────────────────
            if (puzzleBoardContainer != null)
            {
                JigsawSlot[] foundSlots = puzzleBoardContainer.GetComponentsInChildren<JigsawSlot>(true);

                if (foundSlots.Length == 0)
                {
                    Canvas parentCanvas = puzzleBoardContainer.GetComponentInParent<Canvas>();
                    if (parentCanvas != null)
                        foundSlots = parentCanvas.GetComponentsInChildren<JigsawSlot>(true);
                }

                // Jika manual setup dinonaktifkan dan tidak ada slot, generate otomatis
                if (foundSlots.Length == 0 && !useManualSetup)
                {
                    GenerateBoardSlots();
                    foundSlots = puzzleBoardContainer.GetComponentsInChildren<JigsawSlot>(true);
                }
                else if (foundSlots.Length == 0 && useManualSetup)
                {
                    Debug.LogWarning("[JigsawManager] Manual Setup: Tidak ada JigsawSlot di dalam Board Container! " +
                                     "Tambahkan child GameObject dengan komponen JigsawSlot secara manual di Scene.");
                }

                for (int i = 0; i < foundSlots.Length; i++)
                {
                    foundSlots[i].pieceId = i;
                    slots.Add(foundSlots[i]);
                }
            }

            // ── Ambil Pieces ─────────────────────
            if (piecesContainer != null)
            {
                JigsawPiece[] foundPieces = piecesContainer.GetComponentsInChildren<JigsawPiece>(true);
                for (int i = 0; i < foundPieces.Length; i++)
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

            int totalPieces = pieces.Count > 0 ? pieces.Count : (gridRows * gridCols);
            int side = Mathf.RoundToInt(Mathf.Sqrt(totalPieces));
            if (side * side == totalPieces) { gridRows = side; gridCols = side; }

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

            int total = pieces.Count;
            int side = Mathf.RoundToInt(Mathf.Sqrt(total));
            if (side * side == total) { gridRows = side; gridCols = side; }

            float baseTexW = (float)puzzlePhotoTexture.width  / gridCols;
            float baseTexH = (float)puzzlePhotoTexture.height / gridRows;

            int[,] horizEdges = new int[gridRows + 1, gridCols];
            int[,] vertEdges  = new int[gridRows, gridCols + 1];

            if (shapeStyle == PieceShapeStyle.JigsawInterlocking)
                GenerateInterlockingEdgeData(horizEdges, vertEdges);

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null) continue;

                // Manual mode: skip pieces yang tidak minta generated shape
                if (useManualSetup && !pieces[i].useGeneratedShape) continue;

                int r = i / gridCols;
                int c = i % gridCols;

                Sprite pieceSprite;

                if (shapeStyle == PieceShapeStyle.JigsawInterlocking)
                {
                    float padX = baseTexW * 0.25f;
                    float padY = baseTexH * 0.25f;

                    float texX = c * baseTexW - padX;
                    float texY = (gridRows - 1 - r) * baseTexH - padY;
                    float croppedW = baseTexW + padX * 2f;
                    float croppedH = baseTexH + padY * 2f;

                    Rect cropRect = new Rect(texX, texY, croppedW, croppedH);
                    Texture2D croppedTex = CropTextureWithBorderPadding(puzzlePhotoTexture, cropRect);

                    int topEdge    = horizEdges[Mathf.Clamp(r,     0, gridRows),     Mathf.Clamp(c,     0, gridCols - 1)];
                    int rightEdge  = vertEdges [Mathf.Clamp(r,     0, gridRows - 1), Mathf.Clamp(c + 1, 0, gridCols)];
                    int bottomEdge = horizEdges[Mathf.Clamp(r + 1, 0, gridRows),     Mathf.Clamp(c,     0, gridCols - 1)];
                    int leftEdge   = vertEdges [Mathf.Clamp(r,     0, gridRows - 1), Mathf.Clamp(c,     0, gridCols)];

                    Texture2D jigsawTex = ApplyJigsawInterlockingMaskPadded(croppedTex, padX, padY,
                        topEdge, rightEdge, bottomEdge, leftEdge);
                    pieceSprite = Sprite.Create(jigsawTex,
                        new Rect(0, 0, jigsawTex.width, jigsawTex.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    float texX = c * baseTexW;
                    float texY = (gridRows - 1 - r) * baseTexH;
                    Rect cropRect = new Rect(texX, texY, baseTexW, baseTexH);
                    Texture2D croppedTex = CropTexture(puzzlePhotoTexture, cropRect);
                    pieceSprite = Sprite.Create(croppedTex,
                        new Rect(0, 0, croppedTex.width, croppedTex.height), new Vector2(0.5f, 0.5f));
                }

                Image pieceImg = pieces[i].GetComponent<Image>();
                if (pieceImg != null)
                {
                    pieceImg.sprite = pieceSprite;
                    pieceImg.color  = Color.white;
                }
            }

            Debug.Log($"[JigsawManager] Foto '{puzzlePhotoTexture.name}' berhasil dipotong menjadi {pieces.Count} kepingan.");
        }

        // ══════════════════════════════════════════
        //  INTERLOCKING EDGE HELPERS
        // ══════════════════════════════════════════
        private void GenerateInterlockingEdgeData(int[,] horizEdges, int[,] vertEdges)
        {
            for (int r = 1; r < gridRows; r++)
                for (int c = 0; c < gridCols; c++)
                    horizEdges[r, c] = UnityEngine.Random.value > 0.5f ? 1 : -1;

            for (int r = 0; r < gridRows; r++)
                for (int c = 1; c < gridCols; c++)
                    vertEdges[r, c] = UnityEngine.Random.value > 0.5f ? 1 : -1;
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

        private Texture2D CropTextureWithBorderPadding(Texture2D source, Rect cropRect)
        {
            int width  = Mathf.FloorToInt(cropRect.width);
            int height = Mathf.FloorToInt(cropRect.height);
            int srcX   = Mathf.FloorToInt(cropRect.x);
            int srcY   = Mathf.FloorToInt(cropRect.y);

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            if (!source.isReadable)
                Debug.LogError($"[JigsawManager] Texture '{source.name}' belum dicentang 'Read/Write Enabled'! " +
                               "Buka file gambar di Inspector → Advanced → centang 'Read/Write Enabled' → Apply.");

            try
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int realX = srcX + x;
                        int realY = srcY + y;
                        pixels[y * width + x] = (realX >= 0 && realX < source.width && realY >= 0 && realY < source.height)
                            ? source.GetPixel(realX, realY)
                            : new Color(0, 0, 0, 0);
                    }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JigsawManager] Gagal membaca pixel texture. Pastikan 'Read/Write Enabled' dicentang! Detail: {ex.Message}");
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private Texture2D ApplyJigsawInterlockingMaskPadded(Texture2D source, float padX, float padY,
            int top, int right, int bottom, int left)
        {
            int w = source.width;
            int h = source.height;

            Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = source.GetPixels();

            float innerLeft   = padX;
            float innerRight  = w - padX;
            float innerBottom = padY;
            float innerTop    = h - padY;
            float innerW = innerRight - innerLeft;
            float innerH = innerTop - innerBottom;

            float knobRadius = Mathf.Min(innerW, innerH) * 0.16f;

            Vector2 topCenter    = new Vector2(innerLeft + innerW * 0.5f, innerTop);
            Vector2 bottomCenter = new Vector2(innerLeft + innerW * 0.5f, innerBottom);
            Vector2 rightCenter  = new Vector2(innerRight, innerBottom + innerH * 0.5f);
            Vector2 leftCenter   = new Vector2(innerLeft,  innerBottom + innerH * 0.5f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * w + x;
                    Color c = pixels[index];
                    Vector2 pt = new Vector2(x, y);
                    bool keepPixel = true;

                    bool insideBaseRect = (x >= innerLeft && x <= innerRight && y >= innerBottom && y <= innerTop);

                    float distTop    = Vector2.Distance(pt, topCenter);
                    float distBottom = Vector2.Distance(pt, bottomCenter);
                    float distRight  = Vector2.Distance(pt, rightCenter);
                    float distLeft   = Vector2.Distance(pt, leftCenter);

                    // Top edge
                    if      (top ==  1 && distTop <= knobRadius)               keepPixel = true;
                    else if (top == -1 && distTop <= knobRadius && insideBaseRect) keepPixel = false;
                    else if (top !=  1 && y > innerTop)                        keepPixel = false;

                    // Bottom edge
                    if      (bottom ==  1 && distBottom <= knobRadius)               keepPixel = true;
                    else if (bottom == -1 && distBottom <= knobRadius && insideBaseRect) keepPixel = false;
                    else if (bottom !=  1 && y < innerBottom)                        keepPixel = false;

                    // Right edge
                    if      (right ==  1 && distRight <= knobRadius)               keepPixel = true;
                    else if (right == -1 && distRight <= knobRadius && insideBaseRect) keepPixel = false;
                    else if (right !=  1 && x > innerRight)                        keepPixel = false;

                    // Left edge
                    if      (left ==  1 && distLeft <= knobRadius)               keepPixel = true;
                    else if (left == -1 && distLeft <= knobRadius && insideBaseRect) keepPixel = false;
                    else if (left !=  1 && x < innerLeft)                        keepPixel = false;

                    if (!keepPixel) c.a = 0f;
                    pixels[index] = c;
                }
            }

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
            yield return new WaitForSeconds(delayBeforeFlashback);
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
                foreach (var pieceState in puzzleMetadata.pieceStates)
                {
                    if (pieceState.id < 0 || pieceState.id >= pieces.Count) continue;
                    JigsawPiece piece = pieces[pieceState.id];
                    JigsawSlot  slot  = GetSlot(pieceState.id);
                    piece.SetSavedPosition(new Vector2(pieceState.posX, pieceState.posY), pieceState.isSnapped, slot);
                    if (pieceState.isSnapped) snappedCount++;
                }
                isCompleted = puzzleMetadata.isCompleted;
                return true;
            }

            string key = "PuzzleSave_" + GetFullSaveKey();
            if (!PlayerPrefs.HasKey(key)) return false;

            PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(PlayerPrefs.GetString(key));
            if (data == null || data.pieceStates == null) return false;

            snappedCount = 0;
            foreach (var pieceState in data.pieceStates)
            {
                if (pieceState.id < 0 || pieceState.id >= pieces.Count) continue;
                JigsawPiece piece = pieces[pieceState.id];
                JigsawSlot  slot  = GetSlot(pieceState.id);
                piece.SetSavedPosition(new Vector2(pieceState.posX, pieceState.posY), pieceState.isSnapped, slot);
                if (pieceState.isSnapped) snappedCount++;
            }
            isCompleted = data.isCompleted;
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
