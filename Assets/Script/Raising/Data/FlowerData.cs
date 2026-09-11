using System;
using System.Collections.Generic;
using UnityEngine;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
    public enum FlowerType
    {
        Sunflower,
        RedChrysanthemum,
        ForgetMeNot,
        Edelweiss,
        Dahlia
    }

    [CreateAssetMenu(fileName = "NewFlowerData", menuName = "Late Bloom/Raising/Flower Data")]
    public class FlowerData : ScriptableObject
    {
        [Header("Identitas Bunga")]
        public FlowerType flowerType = FlowerType.Sunflower;
        public string flowerName = "Sunflower";
        public string flowerMeaning = "Kesetiaan";
        public string flowerColor = "Kuning";

        [TextArea(2, 4)]
        public string narrativeTheme = "Kesetiaan yang salah arah. MC membangun rumah sebagai bentuk cinta, tapi lupa istrinya hanya ingin ia hadir.";

        [Header("Konfigurasi 4 Fase (Bibit, Tunas, Kuncup, Mekar)")]
        public List<FlowerPhaseRequirement> phaseRequirements = new List<FlowerPhaseRequirement>();

        public FlowerPhaseRequirement GetRequirementForStage(FlowerGrowthStage stage)
        {
            if (phaseRequirements == null || phaseRequirements.Count == 0)
            {
                PopulateDefaultsAccordingToGDD();
            }

            FlowerPhaseRequirement req = phaseRequirements != null ? phaseRequirements.Find(p => p.stage == stage) : null;
            if (req == null)
            {
                switch (stage)
                {
                    case FlowerGrowthStage.Seed: req = new FlowerPhaseRequirement(stage, 30, 30, 45, 10); break;
                    case FlowerGrowthStage.Sprout: req = new FlowerPhaseRequirement(stage, 75, 60, 65, 10); break;
                    case FlowerGrowthStage.Bud: req = new FlowerPhaseRequirement(stage, 120, 90, 85, 10); break;
                    case FlowerGrowthStage.Bloom: req = new FlowerPhaseRequirement(stage, 160, 120, 100, 10); break;
                    default: req = new FlowerPhaseRequirement(stage, 30, 30, 45, 10); break;
                }
            }
            return req;
        }

        private void Reset()
        {
            PopulateDefaultsAccordingToGDD();
        }

        [ContextMenu("Populate Defaults Sesuai GDD")]
        public void PopulateDefaultsAccordingToGDD()
        {
            phaseRequirements.Clear();

            switch (flowerType)
            {
                case FlowerType.Sunflower:
                    flowerName = "Sunflower";
                    flowerMeaning = "Kesetiaan";
                    flowerColor = "Kuning";
                    narrativeTheme = "Kesetiaan yang salah arah. MC membangun rumah sebagai bentuk cinta, tapi lupa hadir di dalamnya.";
                    // 10 Hari / Fase (Milestone Kumulatif)
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Seed, 30, 30, 45, 10));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Sprout, 75, 60, 65, 10));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bud, 120, 90, 85, 10));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bloom, 160, 120, 100, 10));
                    break;

                case FlowerType.RedChrysanthemum:
                    flowerName = "Red Chrysanthemum";
                    flowerMeaning = "Rasa Cinta";
                    flowerColor = "Merah";
                    narrativeTheme = "Hadiah material sebagai kompensasi ketidakhadiran. Mekar dalam gelap.";
                    // 8 Hari / Fase (Sedang)
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Seed, 30, 30, 60, 8));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Sprout, 60, 60, 45, 8));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bud, 60, 60, 45, 8));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bloom, 45, 45, 45, 8));
                    break;

                case FlowerType.ForgetMeNot:
                    flowerName = "Forget-Me-Not";
                    flowerMeaning = "Untuk Selalu Mengingat";
                    flowerColor = "Biru";
                    narrativeTheme = "Mengejar kesembuhan lewat uang dan usaha, bukan kehadiran.";
                    // 7 Hari / Fase (Kebutuhan pupuk rendah)
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Seed, 30, 15, 60, 7));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Sprout, 45, 30, 60, 7));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bud, 45, 30, 60, 7));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bloom, 45, 30, 60, 7));
                    break;

                case FlowerType.Edelweiss:
                    flowerName = "Edelweiss";
                    flowerMeaning = "Cinta Sejati / Keabadian";
                    flowerColor = "Putih";
                    narrativeTheme = "Dua orang yang sama-sama menghindar dari kenyataan. Bertahan sendirian di kondisi minim dukungan.";
                    // 6 Hari / Fase (Unik: minim air & pupuk)
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Seed, 45, 15, 30, 6));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Sprout, 60, 15, 30, 6));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bud, 60, 15, 30, 6));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bloom, 80, 15, 30, 6));
                    break;

                case FlowerType.Dahlia:
                    flowerName = "Dahlia";
                    flowerMeaning = "Kegigihan";
                    flowerColor = "Pink";
                    narrativeTheme = "Pengampunan dan penerimaan. Surat yang sengaja disiapkan istrinya untuk ditemukan setelah tiada.";
                    // 5 Hari / Fase (Kebutuhan stat tinggi)
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Seed, 45, 30, 15, 5));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Sprout, 75, 45, 60, 5));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bud, 75, 60, 60, 5));
                    phaseRequirements.Add(new FlowerPhaseRequirement(FlowerGrowthStage.Bloom, 80, 80, 80, 5));
                    break;
            }
        }
    }
}
