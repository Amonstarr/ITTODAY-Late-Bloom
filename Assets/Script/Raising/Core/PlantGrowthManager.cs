using System;
using UnityEngine;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
    public class PlantGrowthManager : MonoBehaviour
    {
        public static PlantGrowthManager Instance { get; private set; }

        [Header("Flower Configuration")]
        public FlowerData activeFlowerData;
        public FlowerGrowthStage currentStage = FlowerGrowthStage.Seed;

        [Tooltip("Aktifkan TRUE jika ingin fase pertumbuhan bunga selalu dimulai dari Seed (Benih) saat pertama Play.")]
        public bool startAtSeedOnPlay = true;

        [Header("Accumulated Growth Stats")]
        [SerializeField] private int currentSunlight = 0;
        [SerializeField] private int currentNutrients = 0;
        [SerializeField] private int currentWater = 0;

        public event Action<int, int, int> OnStatsChanged;
        public event Action<FlowerGrowthStage> OnStageChanged;

        public int CurrentSunlight => currentSunlight;
        public int CurrentNutrients => currentNutrients;
        public int CurrentWater => currentWater;

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
            if (startAtSeedOnPlay)
            {
                ResetGrowthStage();
            }
            else
            {
                NotifyStats();
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            // ── TEST SHORTCUTS (Editor Only) ──────────────────
            // N = Advance ke fase pertumbuhan berikutnya (Seed → Sprout → Bud → Bloom)
            // R = Reset ke fase Seed
            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                AdvanceGrowthStage();
                Debug.Log($"[PlantGrowthManager TEST] Stage sekarang: {currentStage}");
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                ResetGrowthStage();
                Debug.Log("[PlantGrowthManager TEST] Progress di-reset ke Seed.");
            }
        }
#endif

        public void SetFlower(FlowerData flower, FlowerGrowthStage startStage = FlowerGrowthStage.Seed)
        {
            activeFlowerData = flower;
            currentStage = startStage;
            ResetPhaseStats();

            // Hubungkan PuzzleMetadata dari FlowerData ke PuzzlePhaseManager jika tersedia
            if (flower != null && flower.puzzleMetadata != null && PuzzlePhaseManager.Instance != null)
            {
                PuzzlePhaseManager.Instance.puzzleMetadata = flower.puzzleMetadata;
                PuzzlePhaseManager.Instance.ApplyMetadataIfAvailable();
                PuzzlePhaseManager.Instance.LoadPhaseProgress();
            }

            OnStageChanged?.Invoke(currentStage);
            if (PuzzlePhaseManager.Instance != null)
            {
                PuzzlePhaseManager.Instance.SetGrowthStage(newStage: startStage);
            }
        }

        public void SetStage(FlowerGrowthStage newStage)
        {
            currentStage = newStage;
            NotifyStats();
            OnStageChanged?.Invoke(currentStage);

            // Otomatis sinkronkan fase pertumbuhan ke PuzzlePhaseManager
            if (PuzzlePhaseManager.Instance != null)
            {
                PuzzlePhaseManager.Instance.SetGrowthStage(newStage);
            }
        }

        public void AdvanceGrowthStage()
        {
            if (currentStage == FlowerGrowthStage.Seed)
            {
                SetStage(FlowerGrowthStage.Sprout);
            }
            else if (currentStage == FlowerGrowthStage.Sprout)
            {
                SetStage(FlowerGrowthStage.Bud);
            }
            else if (currentStage == FlowerGrowthStage.Bud)
            {
                SetStage(FlowerGrowthStage.Bloom);
            }
        }

        public void ResetGrowthStage()
        {
            SetStage(FlowerGrowthStage.Seed);
            ResetPhaseStats();
            if (PuzzlePhaseManager.Instance != null)
            {
                PuzzlePhaseManager.Instance.ResetProgress();
            }
        }

        public void ResetPhaseStats()
        {
            currentSunlight = 0;
            currentNutrients = 0;
            currentWater = 0;
            NotifyStats();
        }

        public void AddSunlight(int amount)
        {
            currentSunlight = Mathf.Clamp(currentSunlight + amount, 0, 300);
            NotifyStats();
        }

        public void AddNutrients(int amount)
        {
            currentNutrients = Mathf.Clamp(currentNutrients + amount, 0, 300);
            NotifyStats();
        }

        public void AddWater(int amount)
        {
            currentWater = Mathf.Clamp(currentWater + amount, 0, 300);
            NotifyStats();
        }

        public FlowerPhaseRequirement GetCurrentRequirement()
        {
            if (activeFlowerData != null)
            {
                FlowerPhaseRequirement req = activeFlowerData.GetRequirementForStage(currentStage);
                if (req != null) return req;
            }

            // Fallback requirement if data is not assigned
            switch (currentStage)
            {
                case FlowerGrowthStage.Seed: return new FlowerPhaseRequirement(currentStage, 30, 30, 45, 10);
                case FlowerGrowthStage.Sprout: return new FlowerPhaseRequirement(currentStage, 75, 60, 65, 10);
                case FlowerGrowthStage.Bud: return new FlowerPhaseRequirement(currentStage, 120, 90, 85, 10);
                case FlowerGrowthStage.Bloom: return new FlowerPhaseRequirement(currentStage, 160, 120, 100, 10);
                default: return new FlowerPhaseRequirement(currentStage, 30, 30, 45, 10);
            }
        }

        private void NotifyStats()
        {
            OnStatsChanged?.Invoke(currentSunlight, currentNutrients, currentWater);
        }
    }
}
