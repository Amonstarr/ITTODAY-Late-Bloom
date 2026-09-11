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

        [Header("Accumulated Stats in Current Phase (Skala 1 - 5)")]
        [SerializeField] private int currentSunlight = 0;
        [SerializeField] private int currentNutrients = 0;
        [SerializeField] private int currentWater = 0;

        public event Action<int, int, int> OnStatsChanged; // sun, nut, water
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
            ResetPhaseStats();
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
            currentSunlight = Mathf.Clamp(currentSunlight + amount, 0, 200);
            NotifyStats();
        }

        public void AddNutrients(int amount)
        {
            currentNutrients = Mathf.Clamp(currentNutrients + amount, 0, 200);
            NotifyStats();
        }

        public void AddWater(int amount)
        {
            currentWater = Mathf.Clamp(currentWater + amount, 0, 200);
            NotifyStats();
        }

        public FlowerPhaseRequirement GetCurrentRequirement()
        {
            if (activeFlowerData == null) return null;
            return activeFlowerData.GetRequirementForStage(currentStage);
        }

        private void NotifyStats()
        {
            OnStatsChanged?.Invoke(currentSunlight, currentNutrients, currentWater);
        }
    }
}
