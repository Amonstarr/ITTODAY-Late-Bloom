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
            NotifyStats();
        }

        public void SetFlower(FlowerData flower, FlowerGrowthStage startStage = FlowerGrowthStage.Seed)
        {
            activeFlowerData = flower;
            currentStage = startStage;
            ResetPhaseStats();
            OnStageChanged?.Invoke(currentStage);
        }

        public void SetStage(FlowerGrowthStage newStage)
        {
            currentStage = newStage;
            NotifyStats();
            OnStageChanged?.Invoke(currentStage);
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
