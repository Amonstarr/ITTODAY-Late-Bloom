using UnityEngine;
using TMPro;
using UnityEngine.UI;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
    /// <summary>
    /// Menampilkan HUD Raising ala Uma Musume:
    /// - Header: Nama Bunga, Fase Aktif, Hari saat ini / Total Hari
    /// - Indikator Poin: Cahaya, Nutrisi, Air vs Target Fase
    /// - Kondisi MC: Stamina (0-100) & Mood & Failure Rate
    /// - Log Aksi Terakhir
    /// Dilengkapi OnGUI otomatis agar langsung tampak di layar tanpa setup manual yang rumit.
    /// </summary>
    public class RaisingHUD : MonoBehaviour
    {
        public static RaisingHUD Instance { get; private set; }

        [Header("Optional UI Elements (TMPro)")]
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI mcStatusText;
        [SerializeField] private TextMeshProUGUI actionLogText;

        private string lastActionMessage = "Siap merawat tanaman hari ini.";

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

        private void OnEnable()
        {
            if (TurnSystem.Instance != null)
            {
                TurnSystem.Instance.OnDayChanged += HandleDayChanged;
            }

            if (PlantGrowthManager.Instance != null)
            {
                PlantGrowthManager.Instance.OnStatsChanged += HandleStatsChanged;
                PlantGrowthManager.Instance.OnStageChanged += HandleStageChanged;
            }

            if (PlayerEnergyManager.Instance != null)
            {
                PlayerEnergyManager.Instance.OnEnergyChanged += HandleEnergyChanged;
            }

            if (PlayerMoodManager.Instance != null)
            {
                PlayerMoodManager.Instance.OnMoodChanged += HandleMoodChanged;
            }

            if (RaisingController.Instance != null)
            {
                RaisingController.Instance.OnActionLogged += HandleActionLogged;
            }
        }

        private void OnDisable()
        {
            if (TurnSystem.Instance != null)
            {
                TurnSystem.Instance.OnDayChanged -= HandleDayChanged;
            }

            if (PlantGrowthManager.Instance != null)
            {
                PlantGrowthManager.Instance.OnStatsChanged -= HandleStatsChanged;
                PlantGrowthManager.Instance.OnStageChanged -= HandleStageChanged;
            }

            if (PlayerEnergyManager.Instance != null)
            {
                PlayerEnergyManager.Instance.OnEnergyChanged -= HandleEnergyChanged;
            }

            if (PlayerMoodManager.Instance != null)
            {
                PlayerMoodManager.Instance.OnMoodChanged -= HandleMoodChanged;
            }

            if (RaisingController.Instance != null)
            {
                RaisingController.Instance.OnActionLogged -= HandleActionLogged;
            }
        }

        private void Start()
        {
            UpdateAllDisplays();
        }

        private void HandleDayChanged(int day, int remaining)
        {
            UpdateAllDisplays();
        }

        private void HandleStatsChanged(int sun, int nut, int water)
        {
            UpdateAllDisplays();
        }

        private void HandleStageChanged(FlowerGrowthStage stage)
        {
            UpdateAllDisplays();
        }

        private void HandleEnergyChanged(int current, int max)
        {
            UpdateAllDisplays();
        }

        private void HandleMoodChanged(PlayerMood mood)
        {
            UpdateAllDisplays();
        }

        private void HandleActionLogged(string msg)
        {
            lastActionMessage = msg;
            if (actionLogText != null)
            {
                actionLogText.text = msg;
            }
        }

        public void UpdateAllDisplays()
        {
            var turn = TurnSystem.Instance;
            var plant = PlantGrowthManager.Instance;
            var energy = PlayerEnergyManager.Instance;
            var mood = PlayerMoodManager.Instance;

            if (plant == null) return;

            string flowerName = plant.activeFlowerData != null ? plant.activeFlowerData.flowerName : "Bunga";
            string stageName = GetStageName(plant.currentStage);

            FlowerPhaseRequirement req = plant.GetCurrentRequirement();
            int targetSun = req != null ? req.targetSunlight : 30;
            int targetNut = req != null ? req.targetNutrients : 30;
            int targetWater = req != null ? req.targetWater : 45;
            int totalDays = turn != null ? turn.DaysPerPhase : 10;
            int currentDay = turn != null ? turn.CurrentDayInPhase : 1;
            int remaining = turn != null ? turn.RemainingDays : 10;

            if (phaseText != null)
            {
                phaseText.text = $"🌻 {flowerName} — {stageName}";
            }

            if (dayText != null)
            {
                dayText.text = $"Hari {currentDay} / {totalDays} (Sisa {remaining} Hari)";
            }

            if (statsText != null)
            {
                statsText.text = $"☀️ Cahaya: {plant.CurrentSunlight}/{targetSun}\n" +
                                 $"🌿 Nutrisi: {plant.CurrentNutrients}/{targetNut}\n" +
                                 $"💧 Air: {plant.CurrentWater}/{targetWater}";
            }

            if (mcStatusText != null && energy != null && mood != null)
            {
                float failRate = energy.GetFailureRate();
                mcStatusText.text = $"⚡ Stamina: {energy.CurrentEnergy}/{energy.MaxEnergy}\n" +
                                   $"📻 Mood: {mood.CurrentMood} (Gagal: {failRate:0}%)";
            }
        }

        private string GetStageName(FlowerGrowthStage stage)
        {
            switch (stage)
            {
                case FlowerGrowthStage.Seed: return "Fase 1: Bibit (Seed)";
                case FlowerGrowthStage.Sprout: return "Fase 2: Tunas (Sprout)";
                case FlowerGrowthStage.Bud: return "Fase 3: Kuncup (Bud)";
                case FlowerGrowthStage.Bloom: return "Fase 4: Mekar (Bloom)";
                default: return stage.ToString();
            }
        }
    }
}