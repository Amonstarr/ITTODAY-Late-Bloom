using System;
using UnityEngine;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
    public enum EvaluationGrade
    {
        Perfect, // S-Rank: Sangat presisi dengan kebutuhan botani
        Good,    // A-Rank: Sangat sehat dengan sedikit variasi
        Pass,    // B-Rank: Berhasil mekar/tumbuh
        Failed   // Terlalu jauh dari kebutuhan (layu / butuh pemulihan)
    }

    [Serializable]
    public struct EvaluationResult
    {
        public EvaluationGrade grade;
        public bool isSuccess;
        public string title;
        public string detailMessage;
        public int sunDiff;
        public int nutDiff;
        public int waterDiff;
    }

    public class CheckpointEvaluator : MonoBehaviour
    {
        public static CheckpointEvaluator Instance { get; private set; }

        public event Action<EvaluationResult> OnEvaluationCompleted;

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

        /// <summary>
        /// Mengevaluasi kondisi tanaman di akhir batas hari fase terhadap target GDD.
        /// </summary>
        public EvaluationResult Evaluate(PlantGrowthManager plant, FlowerPhaseRequirement requirement)
        {
            EvaluationResult result = new EvaluationResult();

            if (requirement == null)
            {
                // Fallback requirement botani standar jika data requirement kosong
                FlowerGrowthStage currentStage = (plant != null) ? plant.currentStage : FlowerGrowthStage.Seed;
                switch (currentStage)
                {
                    case FlowerGrowthStage.Seed: requirement = new FlowerPhaseRequirement(currentStage, 30, 30, 45, 10); break;
                    case FlowerGrowthStage.Sprout: requirement = new FlowerPhaseRequirement(currentStage, 75, 60, 65, 10); break;
                    case FlowerGrowthStage.Bud: requirement = new FlowerPhaseRequirement(currentStage, 120, 90, 85, 10); break;
                    case FlowerGrowthStage.Bloom: requirement = new FlowerPhaseRequirement(currentStage, 160, 120, 100, 10); break;
                    default: requirement = new FlowerPhaseRequirement(currentStage, 30, 30, 45, 10); break;
                }
            }

            int currentSun = (plant != null) ? plant.CurrentSunlight : 0;
            int currentNut = (plant != null) ? plant.CurrentNutrients : 0;
            int currentWater = (plant != null) ? plant.CurrentWater : 0;

            int sunTarget = requirement.targetSunlight;
            int nutTarget = requirement.targetNutrients;
            int waterTarget = requirement.targetWater;

            int sunDiff = currentSun - sunTarget;
            int nutDiff = currentNut - nutTarget;
            int waterDiff = currentWater - waterTarget;

            result.sunDiff = Mathf.Abs(sunDiff);
            result.nutDiff = Mathf.Abs(nutDiff);
            result.waterDiff = Mathf.Abs(waterDiff);

            // Cek rasio pemenuhan target
            float sunRatio = (sunTarget > 0) ? (float)plant.CurrentSunlight / sunTarget : 1f;
            float nutRatio = (nutTarget > 0) ? (float)plant.CurrentNutrients / nutTarget : 1f;
            float waterRatio = (waterTarget > 0) ? (float)plant.CurrentWater / waterTarget : 1f;

            float minRatio = Mathf.Min(sunRatio, Mathf.Min(nutRatio, waterRatio));
            float avgRatio = (sunRatio + nutRatio + waterRatio) / 3f;

            if (minRatio >= 0.95f && avgRatio <= 1.35f)
            {
                result.grade = EvaluationGrade.Perfect;
                result.isSuccess = true;
                result.title = "Sempurna (S-Rank)!";
                result.detailMessage = "Kebutuhan cahaya, nutrisi, dan air sangat ideal! Bunga mekar dengan kualitas paling prima.";
            }
            else if (minRatio >= 0.80f)
            {
                result.grade = EvaluationGrade.Good;
                result.isSuccess = true;
                result.title = "Bagus (A-Rank)!";
                result.detailMessage = "Bunga tumbuh sehat dan kuat melewati fase ini.";
            }
            else if (minRatio >= 0.65f)
            {
                result.grade = EvaluationGrade.Pass;
                result.isSuccess = true;
                result.title = "Cukup (B-Rank)";
                result.detailMessage = "Bunga berhasil bertahan dan tumbuh meski beberapa kebutuhan sedikit kurang.";
            }
            else
            {
                result.grade = EvaluationGrade.Failed;
                result.isSuccess = false;
                result.title = "Gagal / Layu";
                result.detailMessage = "Tanaman layu karena kekurangan asupan penting. MC membutuhkan 2 hari tambahan untuk merawatnya.";
            }

            int totalDiff = result.sunDiff + result.nutDiff + result.waterDiff;
            Debug.Log($"[CheckpointEvaluator] Hasil Evaluasi: {result.title} (Diff: {totalDiff}). Sukses: {result.isSuccess}");
            OnEvaluationCompleted?.Invoke(result);
            return result;
        }
    }
}
