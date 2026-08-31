using System;
using UnityEngine;
using UnityEngine.Events;

namespace LateBloom.Jigsaw
{
    public enum FlowerGrowthStage
    {
        Seed = 0,
        Bud = 1,
        Bloom = 2
    }

    public class PuzzlePhaseManager : MonoBehaviour
    {
        public static PuzzlePhaseManager Instance { get; private set; }

        [Header("Metadata Reference (Optional)")]
        public PuzzleMetadata puzzleMetadata;

        [Header("Phase Piece Configuration (Configurable in Inspector)")]
        [Tooltip("Jumlah kepingan yang didapat saat fase Seed (Benih)")]
        public int seedStagePieces = 4;

        [Tooltip("Jumlah kepingan yang didapat saat fase Bud (Kuncup)")]
        public int budStagePieces = 4;

        [Tooltip("Jumlah kepingan yang didapat saat fase Bloom (Mekar)")]
        public int bloomStagePieces = 8;

        [Header("Current Status")]
        public FlowerGrowthStage currentStage = FlowerGrowthStage.Seed;
        [SerializeField] private int totalPiecesCollected = 0;
        [SerializeField] private bool isPuzzleUnlocked = false;

        [Header("References")]
        public JigsawManager jigsawManager;
        public GameObject puzzleUIContainer;

        [Header("Events")]
        public UnityEvent<int, int> onPiecesAwarded;
        public UnityEvent onPuzzleUnlocked;

        public int TotalTargetPieces => seedStagePieces + budStagePieces + bloomStagePieces;
        public int TotalPiecesCollected => totalPiecesCollected;
        public bool IsPuzzleUnlocked => isPuzzleUnlocked;

        public string GetSaveKey()
        {
            if (puzzleMetadata != null) return puzzleMetadata.GetFullSaveKey();
            if (jigsawManager != null) return jigsawManager.GetFullSaveKey();
            return "default_instance";
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ApplyMetadataIfAvailable();
            LoadPhaseProgress();
            ApplyCurrentStageStatus();
        }

#if UNITY_EDITOR
        private void Update()
        {
            // ── TEST SHORTCUTS (Editor Only) ──────────────────
            // N = Advance ke fase berikutnya (Seed → Bud → Bloom)
            // R = Reset ke fase Seed
            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                AdvanceGrowthStage();
                Debug.Log($"[TEST] Stage sekarang: {currentStage}");
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                ResetProgress();
                Debug.Log("[TEST] Stage di-reset ke Seed.");
            }
        }
#endif

        private void OnDisable()
        {
            SavePhaseProgress();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SavePhaseProgress();
            }
        }

        private void OnDestroy()
        {
            SavePhaseProgress();
        }

        public void ApplyMetadataIfAvailable()
        {
            if (puzzleMetadata != null)
            {
                seedStagePieces = puzzleMetadata.seedStagePieces;
                budStagePieces = puzzleMetadata.budStagePieces;
                bloomStagePieces = puzzleMetadata.bloomStagePieces;
            }
        }

        public void SetGrowthStage(FlowerGrowthStage newStage)
        {
            currentStage = newStage;
            RecalculateCollectedPieces();
            SavePhaseProgress();
            ApplyCurrentStageStatus();
        }

        public void AdvanceGrowthStage()
        {
            if (currentStage == FlowerGrowthStage.Seed)
            {
                SetGrowthStage(FlowerGrowthStage.Bud);
            }
            else if (currentStage == FlowerGrowthStage.Bud)
            {
                SetGrowthStage(FlowerGrowthStage.Bloom);
            }
        }

        private void RecalculateCollectedPieces()
        {
            int previousCount = totalPiecesCollected;
            totalPiecesCollected = 0;

            if (currentStage >= FlowerGrowthStage.Seed)
            {
                totalPiecesCollected += seedStagePieces;
            }
            if (currentStage >= FlowerGrowthStage.Bud)
            {
                totalPiecesCollected += budStagePieces;
            }
            if (currentStage >= FlowerGrowthStage.Bloom)
            {
                totalPiecesCollected += bloomStagePieces;
            }

            int added = totalPiecesCollected - previousCount;
            if (added > 0)
            {
                onPiecesAwarded?.Invoke(added, totalPiecesCollected);
                Debug.Log($"[PuzzlePhaseManager] Bunga mencapai fase {currentStage}! Mendapatkan +{added} keping. Total keping: {totalPiecesCollected}/{TotalTargetPieces}");
            }
        }

        private void ApplyCurrentStageStatus()
        {
            if (currentStage == FlowerGrowthStage.Bloom)
            {
                isPuzzleUnlocked = true;
                onPuzzleUnlocked?.Invoke();

                if (puzzleUIContainer != null)
                {
                    puzzleUIContainer.SetActive(true);
                }

                if (jigsawManager != null)
                {
                    jigsawManager.gameObject.SetActive(true);
                    jigsawManager.InitializePuzzle();
                }

                Debug.Log("[PuzzlePhaseManager] Bunga mekar penuh (Bloom)! Puzzle Flashback sekarang dapat dimainkan.");
            }
            else
            {
                isPuzzleUnlocked = false;

                if (puzzleUIContainer != null)
                {
                    puzzleUIContainer.SetActive(false);
                }
            }
        }

        public void SavePhaseProgress()
        {
            if (puzzleMetadata != null)
            {
                puzzleMetadata.currentStage = this.currentStage;
                puzzleMetadata.isUnlocked = this.isPuzzleUnlocked;
                puzzleMetadata.SaveToDisk();
                return;
            }

            PlayerPrefs.SetInt("GrowthStage_" + GetSaveKey(), (int)currentStage);
            PlayerPrefs.Save();
        }

        public void LoadPhaseProgress()
        {
            if (puzzleMetadata != null)
            {
                if (puzzleMetadata.LoadFromDisk())
                {
                    currentStage = puzzleMetadata.currentStage;
                }
                RecalculateCollectedPieces();
                return;
            }

            string key = "GrowthStage_" + GetSaveKey();
            if (PlayerPrefs.HasKey(key))
            {
                currentStage = (FlowerGrowthStage)PlayerPrefs.GetInt(key, 0);
            }
            RecalculateCollectedPieces();
        }

        public void ResetProgress()
        {
            if (puzzleMetadata != null)
            {
                puzzleMetadata.ResetMetadata();
            }
            else
            {
                string key = "GrowthStage_" + GetSaveKey();
                PlayerPrefs.DeleteKey(key);
            }
            currentStage = FlowerGrowthStage.Seed;
            RecalculateCollectedPieces();
            ApplyCurrentStageStatus();
        }
    }
}
