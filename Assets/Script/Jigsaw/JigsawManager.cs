using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace LateBloom.Jigsaw
{
    public enum PieceShapeStyle
    {
        SquareGrid,           // Potongan grid kotak lurus biasa
        JigsawInterlocking,   // Potongan klasik Jigsaw (Benjolan & Lekukan Interlocking)
        CustomSprites         // Menggunakan daftar Sprite kepingan buatan artist secara manual
    }

    public class JigsawManager : MonoBehaviour
    {
        [Header("0. Metadata System (Opsional / Recommended)")]
        [Tooltip("ScriptableObject metadata untuk bunga/instance ini. Jika diisi, data visual & save akan otomatis diambil dari metadata.")]
        public PuzzleMetadata puzzleMetadata;

        [Header("1. Visual Assets (Drag & Drop)")]
        [Tooltip("Image UI untuk background ruangan / kayu")]
        public Image backgroundImage;
        public Sprite backgroundSprite;

        [Space(5)]
        [Tooltip("Image UI untuk bingkai / frame tempat menyusun puzzle")]
        public Image boardFrameImage;
        public Sprite boardFrameSprite;

        [Space(5)]
        [Tooltip("Foto Utuh Kenangan yang akan dipotong otomatis menjadi kepingan puzzle")]
        public Texture2D puzzlePhotoTexture;

        [Header("2. Identity & Instance Setup")]
        [Tooltip("ID jenis bunga (misal: 'sunflower_phase1')")]
        public string puzzleId = "sunflower_phase1";
        [Tooltip("ID unik per penanaman/pot (misal: 'pot_01', 'inst_02'). Memungkinkan menanam bunga sama 2x+ tanpa menimpa data.")]
        public string instanceId = "instance_01";

        [Header("3. Shape & Grid Configuration")]
        [Tooltip("Pilih bentuk kepingan puzzle: Kotak (SquareGrid), Klasik Jigsaw (JigsawInterlocking), atau Custom Sprites")]
        public PieceShapeStyle shapeStyle = PieceShapeStyle.JigsawInterlocking;

        [Tooltip("Daftar sprite kepingan buatan artist (Hanya digunakan jika Shape Style = CustomSprites)")]
        public List<Sprite> customPieceSprites = new List<Sprite>();

        [Range(2, 8)] public int gridRows = 4;
        [Range(2, 8)] public int gridCols = 4;
        public Vector2 boardDimensions = new Vector2(480f, 480f);
        [Tooltip("Toleransi jarak snap ke slot dalam pixel")]
        public float snapRadius = 35f;

        [Header("4. Containers (Otomatis dibuat jika kosong)")]
        public RectTransform puzzleBoardContainer;
        public RectTransform piecesContainer;
        public RectTransform scatterAreaLeft;
        public RectTransform scatterAreaRight;

        [Header("5. UI References")]
        public TextMeshProUGUI progressText;

        [Header("6. Events & Transition")]
        public UnityEvent onPuzzleCompleted;
        public UnityEvent onPieceSnappedEvent;
        public string flashbackSceneName = "Flashback_Phase1";
        public float delayBeforeFlashback = 1.5f;

        [HideInInspector] public List<JigsawSlot> slots = new List<JigsawSlot>();
        [HideInInspector] public List<JigsawPiece> pieces = new List<JigsawPiece>();

        private int snappedCount = 0;
        private bool isCompleted = false;

        public string GetFullSaveKey()
        {
            if (puzzleMetadata != null) return puzzleMetadata.GetFullSaveKey();
            return $"{puzzleId}_{instanceId}";
        }

        private void Start()
        {
            ApplyMetadataIfAvailable();
            SetupVisualAssets();
            GeneratePuzzleBoardAndPieces();
            InitializePuzzle();
        }

        private void OnDisable()
        {
            SaveProgress();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveProgress();
            }
        }

        private void OnDestroy()
        {
            SaveProgress();
        }

        public void ApplyMetadataIfAvailable()
        {
            if (puzzleMetadata != null)
            {
                puzzleId = puzzleMetadata.puzzleId;
                instanceId = puzzleMetadata.instanceId;

                if (puzzleMetadata.puzzlePhotoTexture != null) puzzlePhotoTexture = puzzleMetadata.puzzlePhotoTexture;
                if (puzzleMetadata.backgroundSprite != null) backgroundSprite = puzzleMetadata.backgroundSprite;
                if (puzzleMetadata.boardFrameSprite != null) boardFrameSprite = puzzleMetadata.boardFrameSprite;

                puzzleMetadata.LoadFromDisk();
            }
        }

        public void SetupVisualAssets()
        {
            if (backgroundImage != null && backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite;
            }

            if (boardFrameImage != null && boardFrameSprite != null)
            {
                boardFrameImage.sprite = boardFrameSprite;
            }
        }

        [ContextMenu("Generate Puzzle Board & Pieces")]
        public void GeneratePuzzleBoardAndPieces()
        {
            ClearGeneratedObjects();
            EnsureContainersExist();

            int totalPieces = gridRows * gridCols;
            float pieceWidth = boardDimensions.x / gridCols;
            float pieceHeight = boardDimensions.y / gridRows;

            GridLayoutGroup gridLayout = puzzleBoardContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null) gridLayout = puzzleBoardContainer.gameObject.AddComponent<GridLayoutGroup>();

            gridLayout.cellSize = new Vector2(pieceWidth, pieceHeight);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridCols;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            int[,] horizEdges = new int[gridRows + 1, gridCols];
            int[,] vertEdges = new int[gridRows, gridCols + 1];

            if (shapeStyle == PieceShapeStyle.JigsawInterlocking)
            {
                GenerateInterlockingEdgeData(horizEdges, vertEdges);
            }

            for (int r = 0; r < gridRows; r++)
            {
                for (int c = 0; c < gridCols; c++)
                {
                    int id = r * gridCols + c;

                    // 1. Slot GameObject
                    GameObject slotObj = new GameObject($"Slot_{id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(JigsawSlot));
                    slotObj.transform.SetParent(puzzleBoardContainer, false);

                    Image slotImage = slotObj.GetComponent<Image>();
                    slotImage.color = new Color(1f, 1f, 1f, 0.15f);

                    JigsawSlot slot = slotObj.GetComponent<JigsawSlot>();
                    slot.pieceId = id;
                    slot.ghostHighlightImage = slotImage;
                    slots.Add(slot);

                    // 2. Sprite Selection
                    Sprite pieceSprite = null;

                    if (shapeStyle == PieceShapeStyle.CustomSprites)
                    {
                        if (id < customPieceSprites.Count)
                        {
                            pieceSprite = customPieceSprites[id];
                        }
                    }
                    else if (puzzlePhotoTexture != null)
                    {
                        float texPieceWidth = (float)puzzlePhotoTexture.width / gridCols;
                        float texPieceHeight = (float)puzzlePhotoTexture.height / gridRows;

                        float texX = c * texPieceWidth;
                        float texY = (gridRows - 1 - r) * texPieceHeight;

                        Rect cropRect = new Rect(texX, texY, texPieceWidth, texPieceHeight);
                        Texture2D croppedTex = CropTexture(puzzlePhotoTexture, cropRect);

                        if (shapeStyle == PieceShapeStyle.JigsawInterlocking)
                        {
                            int topEdge = horizEdges[r, c];
                            int rightEdge = vertEdges[r, c + 1];
                            int bottomEdge = horizEdges[r + 1, c];
                            int leftEdge = vertEdges[r, c];

                            Texture2D jigsawTex = ApplyJigsawInterlockingMask(croppedTex, topEdge, rightEdge, bottomEdge, leftEdge);
                            pieceSprite = Sprite.Create(jigsawTex, new Rect(0, 0, jigsawTex.width, jigsawTex.height), new Vector2(0.5f, 0.5f));
                        }
                        else // SquareGrid
                        {
                            pieceSprite = Sprite.Create(croppedTex, new Rect(0, 0, croppedTex.width, croppedTex.height), new Vector2(0.5f, 0.5f));
                        }
                    }

                    // 3. Piece GameObject
                    GameObject pieceObj = new GameObject($"Piece_{id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(JigsawPiece));
                    pieceObj.transform.SetParent(piecesContainer, false);

                    RectTransform pieceRect = pieceObj.GetComponent<RectTransform>();
                    pieceRect.sizeDelta = new Vector2(pieceWidth, pieceHeight);

                    Image pieceImage = pieceObj.GetComponent<Image>();
                    pieceImage.sprite = pieceSprite;

                    JigsawPiece piece = pieceObj.GetComponent<JigsawPiece>();
                    piece.pieceId = id;
                    pieces.Add(piece);
                }
            }
        }

        private void GenerateInterlockingEdgeData(int[,] horizEdges, int[,] vertEdges)
        {
            for (int r = 1; r < gridRows; r++)
            {
                for (int c = 0; c < gridCols; c++)
                {
                    horizEdges[r, c] = UnityEngine.Random.value > 0.5f ? 1 : -1;
                }
            }

            for (int r = 0; r < gridRows; r++)
            {
                for (int c = 1; c < gridCols; c++)
                {
                    vertEdges[r, c] = UnityEngine.Random.value > 0.5f ? 1 : -1;
                }
            }
        }

        private Texture2D CropTexture(Texture2D source, Rect cropRect)
        {
            int x = Mathf.FloorToInt(cropRect.x);
            int y = Mathf.FloorToInt(cropRect.y);
            int width = Mathf.FloorToInt(cropRect.width);
            int height = Mathf.FloorToInt(cropRect.height);

            Color[] pixels = source.GetPixels(x, y, width, height);
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private Texture2D ApplyJigsawInterlockingMask(Texture2D source, int top, int right, int bottom, int left)
        {
            int w = source.width;
            int h = source.height;

            Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = source.GetPixels();

            float centerX = w * 0.5f;
            float centerY = h * 0.5f;
            float tabRadius = Mathf.Min(w, h) * 0.18f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * w + x;
                    Color c = pixels[index];

                    bool insideMask = true;

                    if (top != 0)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, h));
                        if (top == -1 && dist < tabRadius) insideMask = false;
                    }

                    if (bottom != 0)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, 0));
                        if (bottom == -1 && dist < tabRadius) insideMask = false;
                    }

                    if (right != 0)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(w, centerY));
                        if (right == -1 && dist < tabRadius) insideMask = false;
                    }

                    if (left != 0)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(0, centerY));
                        if (left == -1 && dist < tabRadius) insideMask = false;
                    }

                    if (!insideMask)
                    {
                        c.a = 0f;
                    }

                    pixels[index] = c;
                }
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private void EnsureContainersExist()
        {
            Transform parentCanvas = transform.parent != null ? transform.parent : transform;

            if (puzzleBoardContainer == null)
            {
                GameObject boardObj = new GameObject("Generated_BoardContainer", typeof(RectTransform));
                boardObj.transform.SetParent(parentCanvas, false);
                puzzleBoardContainer = boardObj.GetComponent<RectTransform>();
                puzzleBoardContainer.sizeDelta = boardDimensions;
            }

            if (piecesContainer == null)
            {
                GameObject piecesObj = new GameObject("Generated_PiecesContainer", typeof(RectTransform));
                piecesObj.transform.SetParent(parentCanvas, false);
                piecesContainer = piecesObj.GetComponent<RectTransform>();
            }

            if (scatterAreaLeft == null)
            {
                GameObject scatterL = new GameObject("Generated_ScatterLeft", typeof(RectTransform));
                scatterL.transform.SetParent(parentCanvas, false);
                scatterAreaLeft = scatterL.GetComponent<RectTransform>();
                scatterAreaLeft.sizeDelta = new Vector2(350, boardDimensions.y);
                scatterAreaLeft.anchoredPosition = new Vector2(-boardDimensions.x / 2f - 220f, 0);
            }

            if (scatterAreaRight == null)
            {
                GameObject scatterR = new GameObject("Generated_ScatterRight", typeof(RectTransform));
                scatterR.transform.SetParent(parentCanvas, false);
                scatterAreaRight = scatterR.GetComponent<RectTransform>();
                scatterAreaRight.sizeDelta = new Vector2(350, boardDimensions.y);
                scatterAreaRight.anchoredPosition = new Vector2(boardDimensions.x / 2f + 220f, 0);
            }
        }

        private void ClearGeneratedObjects()
        {
            foreach (var piece in pieces)
            {
                if (piece != null && piece.gameObject != null)
                {
                    if (Application.isPlaying) Destroy(piece.gameObject);
                    else DestroyImmediate(piece.gameObject);
                }
            }
            pieces.Clear();

            foreach (var slot in slots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    if (Application.isPlaying) Destroy(slot.gameObject);
                    else DestroyImmediate(slot.gameObject);
                }
            }
            slots.Clear();
        }

        public void InitializePuzzle()
        {
            snappedCount = 0;
            isCompleted = false;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].pieceId = i;
                    slots[i].SetOccupied(false);
                }
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] != null)
                {
                    pieces[i].Initialize(this, i);
                }
            }

            if (!LoadProgress())
            {
                ScatterPieces();
            }

            UpdateProgressUI();
        }

        public void ScatterPieces()
        {
            RectTransform[] scatterAreas = new RectTransform[] { scatterAreaLeft, scatterAreaRight };

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null || pieces[i].currentState == PieceState.Snapped) continue;

                RectTransform targetArea = scatterAreas[i % scatterAreas.Length];
                if (targetArea == null) continue;

                Rect areaRect = targetArea.rect;
                float randomX = UnityEngine.Random.Range(areaRect.xMin + 20f, areaRect.xMax - 20f);
                float randomY = UnityEngine.Random.Range(areaRect.yMin + 20f, areaRect.yMax - 20f);

                pieces[i].rectTransform.position = targetArea.TransformPoint(new Vector2(randomX, randomY));
            }
        }

        public JigsawSlot GetSlot(int pieceId)
        {
            if (pieceId >= 0 && pieceId < slots.Count)
            {
                return slots[pieceId];
            }
            return null;
        }

        public void CheckHoverSlot(JigsawPiece piece)
        {
            JigsawSlot targetSlot = GetSlot(piece.pieceId);
            if (targetSlot == null || targetSlot.isOccupied) return;

            float distance = Vector2.Distance(piece.rectTransform.position, targetSlot.rectTransform.position);
            targetSlot.SetHover(distance <= snapRadius);
        }

        public bool TrySnapPiece(JigsawPiece piece)
        {
            JigsawSlot targetSlot = GetSlot(piece.pieceId);
            if (targetSlot == null || targetSlot.isOccupied) return false;

            float distance = Vector2.Distance(piece.rectTransform.position, targetSlot.rectTransform.position);
            targetSlot.SetHover(false);

            return distance <= snapRadius;
        }

        public void OnPiecePickup()
        {
            if (JigsawAudioManager.Instance != null)
            {
                JigsawAudioManager.Instance.PlayPickupSFX();
            }
        }

        public void OnPieceSnapped(JigsawPiece piece)
        {
            snappedCount++;
            UpdateProgressUI();
            SaveProgress();

            if (JigsawAudioManager.Instance != null)
            {
                JigsawAudioManager.Instance.PlaySnapSFX();
            }

            onPieceSnappedEvent?.Invoke();

            if (snappedCount >= pieces.Count && !isCompleted)
            {
                CompletePuzzle();
            }
        }

        private void CompletePuzzle()
        {
            isCompleted = true;
            SaveProgress();

            if (JigsawAudioManager.Instance != null)
            {
                JigsawAudioManager.Instance.PlayCompleteSFX();
            }

            onPuzzleCompleted?.Invoke();
            StartCoroutine(FlashbackRoutine());
        }

        private IEnumerator FlashbackRoutine()
        {
            yield return new WaitForSeconds(delayBeforeFlashback);

            if (!string.IsNullOrEmpty(flashbackSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(flashbackSceneName);
            }
        }

        private void UpdateProgressUI()
        {
            if (progressText != null)
            {
                progressText.text = $"Pieces: {snappedCount} / {pieces.Count}";
            }
        }

        public void SaveProgress()
        {
            if (puzzleMetadata != null)
            {
                puzzleMetadata.isCompleted = this.isCompleted;
                puzzleMetadata.pieceStates.Clear();

                foreach (var piece in pieces)
                {
                    if (piece != null)
                    {
                        puzzleMetadata.pieceStates.Add(new PieceSaveState
                        {
                            id = piece.pieceId,
                            isSnapped = piece.currentState == PieceState.Snapped,
                            posX = piece.rectTransform.anchoredPosition.x,
                            posY = piece.rectTransform.anchoredPosition.y
                        });
                    }
                }
                puzzleMetadata.SaveToDisk();
                return;
            }

            PuzzleSaveData data = new PuzzleSaveData
            {
                puzzleId = this.puzzleId,
                isCompleted = this.isCompleted
            };

            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    data.pieceStates.Add(new PieceSaveState
                    {
                        id = piece.pieceId,
                        isSnapped = piece.currentState == PieceState.Snapped,
                        posX = piece.rectTransform.anchoredPosition.x,
                        posY = piece.rectTransform.anchoredPosition.y
                    });
                }
            }

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("PuzzleSave_" + GetFullSaveKey(), json);
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
                    if (pieceState.id >= 0 && pieceState.id < pieces.Count)
                    {
                        JigsawPiece piece = pieces[pieceState.id];
                        JigsawSlot slot = GetSlot(pieceState.id);
                        piece.SetSavedPosition(new Vector2(pieceState.posX, pieceState.posY), pieceState.isSnapped, slot);

                        if (pieceState.isSnapped) snappedCount++;
                    }
                }
                isCompleted = puzzleMetadata.isCompleted;
                return true;
            }

            string key = "PuzzleSave_" + GetFullSaveKey();
            if (!PlayerPrefs.HasKey(key)) return false;

            string json = PlayerPrefs.GetString(key);
            PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(json);
            if (data == null || data.pieceStates == null) return false;

            snappedCount = 0;
            foreach (var pieceState in data.pieceStates)
            {
                if (pieceState.id >= 0 && pieceState.id < pieces.Count)
                {
                    JigsawPiece piece = pieces[pieceState.id];
                    JigsawSlot slot = GetSlot(pieceState.id);
                    piece.SetSavedPosition(new Vector2(pieceState.posX, pieceState.posY), pieceState.isSnapped, slot);

                    if (pieceState.isSnapped) snappedCount++;
                }
            }

            isCompleted = data.isCompleted;
            return true;
        }

        public void ReplayPuzzle()
        {
            if (puzzleMetadata != null)
            {
                puzzleMetadata.ResetMetadata();
            }
            else
            {
                PlayerPrefs.DeleteKey("PuzzleSave_" + GetFullSaveKey());
            }
            InitializePuzzle();
        }
    }
}
