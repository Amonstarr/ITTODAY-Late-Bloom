using System;
using UnityEngine;
using UnityEngine.Events;

namespace LateBloom.Jigsaw
{
    public enum FlowerGrowthStage
    {
        Seed = 0,         // Fase 1: Benih (dapat saat pilih bunga) -> +1 Keping (1/4)
        SmallGrowth = 1,  // Fase 2: Tumbuh dikit -> +1 Keping (2/4)
        BigGrowth = 2,    // Fase 3: Tumbuh gede -> +1 Keping (3/4)
        Bloom = 3         // Fase 4: Berbunga -> +1 Keping (4/4 & Board Unlocked)
    }

    public class PuzzlePhaseManager : MonoBehaviour
    {
        public static PuzzlePhaseManager Instance { get; private set; }

        [Header("Metadata Reference (Optional)")]
        public PuzzleMetadata puzzleMetadata;

        [Header("Current Status")]
        public FlowerGrowthStage currentStage = FlowerGrowthStage.Seed;
        [SerializeField] private int totalPiecesCollected = 1;
        [SerializeField] private bool isPuzzleUnlocked = false;

        [Header("References")]
        public JigsawManager jigsawManager;
        public GameObject puzzleUIContainer;
        public PuzzlePieceAwardedUI pieceAwardedCutsceneUI;

        [Header("Events")]
        [Tooltip("Event saat mendapatkan keping puzzle baru: (pieceIndex, totalPieces, stageName)")]
        public UnityEvent<int, int, string> onPieceAwardedCutscene;
        [Tooltip("Event kompatibilitas (addedPieces, totalPieces)")]
        public UnityEvent<int, int> onPiecesAwarded;
        public UnityEvent onPuzzleUnlocked;

        public const int TotalTargetPieces = 4;
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
            // N = Advance ke fase berikutnya (Seed → SmallGrowth → BigGrowth → Bloom)
            // R = Reset ke fase Seed
            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                AdvanceGrowthStage();
                Debug.Log($"[TEST] Stage sekarang: {currentStage} ({GetStageDisplayName(currentStage)})");
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
                // Metadata loaded
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
                SetGrowthStage(FlowerGrowthStage.SmallGrowth);
            }
            else if (currentStage == FlowerGrowthStage.SmallGrowth)
            {
                SetGrowthStage(FlowerGrowthStage.BigGrowth);
            }
            else if (currentStage == FlowerGrowthStage.BigGrowth)
            {
                SetGrowthStage(FlowerGrowthStage.Bloom);
            }
        }

        public static string GetStageDisplayName(FlowerGrowthStage stage)
        {
            switch (stage)
            {
                case FlowerGrowthStage.Seed:        return "Benih (Seed)";
                case FlowerGrowthStage.SmallGrowth: return "Tumbuh Dikit";
                case FlowerGrowthStage.BigGrowth:   return "Tumbuh Gede";
                case FlowerGrowthStage.Bloom:       return "Berbunga (Bloom)";
                default: return stage.ToString();
            }
        }

        private void RecalculateCollectedPieces()
        {
            int previousCount = totalPiecesCollected;
            
            // Setiap fase memberikan 1 keping (Seed=1, SmallGrowth=2, BigGrowth=3, Bloom=4)
            totalPiecesCollected = (int)currentStage + 1;

            int added = totalPiecesCollected - previousCount;
            if (added > 0)
            {
                string stageName = GetStageDisplayName(currentStage);
                onPiecesAwarded?.Invoke(added, totalPiecesCollected);
                onPieceAwardedCutscene?.Invoke(totalPiecesCollected, TotalTargetPieces, stageName);

                if (pieceAwardedCutsceneUI != null)
                {
                    pieceAwardedCutsceneUI.ShowPieceAwardedPanel(totalPiecesCollected, TotalTargetPieces, stageName);
                }

                Debug.Log($"[PuzzlePhaseManager] Bunga mencapai fase '{stageName}'! Mendapatkan Keping Puzzle #{totalPiecesCollected}/{TotalTargetPieces}");
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

                Debug.Log("[PuzzlePhaseManager] Bunga mekar penuh (Bloom)! Semua 4 keping puzzle didapatkan & Puzzle Flashback dapat dimainkan.");
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
            totalPiecesCollected = 1;
            ApplyCurrentStageStatus();
        }
    }
}
