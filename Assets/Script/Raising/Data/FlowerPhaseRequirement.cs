using System;
using UnityEngine;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
    [Serializable]
    public class FlowerPhaseRequirement
    {
        public FlowerGrowthStage stage;

        [Header("Target Poin Pertumbuhan (Skala 0 - 150)")]
        [Tooltip("Target poin Cahaya Matahari yang harus dicapai")]
        public int targetSunlight = 30;

        [Tooltip("Target poin Nutrisi Tanah yang harus dicapai")]
        public int targetNutrients = 30;

        [Tooltip("Target poin Air yang harus dicapai")]
        public int targetWater = 45;

        [Header("Alokasi Waktu Fase")]
        [Tooltip("Jumlah turn / hari dalam fase ini sebelum evaluasi checkpoint")]
        public int daysAllocated = 10;

        // Backward compatibility
        public int requiredSunlight => targetSunlight;
        public int requiredNutrients => targetNutrients;
        public int requiredWater => targetWater;

        public FlowerPhaseRequirement() { }

        public FlowerPhaseRequirement(FlowerGrowthStage stage, int sun, int nut, int water, int days = 10)
        {
            this.stage = stage;
            this.targetSunlight = sun;
            this.targetNutrients = nut;
            this.targetWater = water;
            this.daysAllocated = days;
        }
    }
}
