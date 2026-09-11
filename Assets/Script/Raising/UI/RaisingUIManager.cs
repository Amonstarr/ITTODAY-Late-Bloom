using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LateBloom.Raising
{
    public class RaisingUIManager : MonoBehaviour
    {
        [Header("System Reference")]
        public RaisingController raisingController;

        [Header("Turn / Day UI")]
        public TextMeshProUGUI dayInfoText;
        public TextMeshProUGUI remainingDaysText;

        [Header("Energy & Failure Rate UI")]
        public Slider energySlider;
        public TextMeshProUGUI energyText;
        public TextMeshProUGUI failureRateText;

        [Header("Plant Stats UI")]
        public TextMeshProUGUI flowerNameText;
        public TextMeshProUGUI stageText;
        public TextMeshProUGUI sunlightStatText;
        public TextMeshProUGUI nutrientStatText;
        public TextMeshProUGUI waterStatText;

        [Header("Action Feedback UI")]
        public TextMeshProUGUI actionLogText;

        [Header("Action Buttons (Optional - can be hooked up via Inspector)")]
        public Button sunlightButton;
        public Button waterButton;
        public Button nutrientButton;
        public Button restTeaButton;

        private void Awake()
        {
            if (raisingController == null)
            {
#if UNITY_2023_1_OR_NEWER
                raisingController = FindFirstObjectByType<RaisingController>();
#else
                raisingController = FindObjectOfType<RaisingController>();
#endif
            }
        }

        private void Start()
        {
            BindButtons();
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void BindButtons()
        {
            if (sunlightButton != null) sunlightButton.onClick.AddListener(() => raisingController?.ExecuteSunlightAction());
            if (waterButton != null) waterButton.onClick.AddListener(() => raisingController?.ExecuteWaterAction());
            if (nutrientButton != null) nutrientButton.onClick.AddListener(() => raisingController?.ExecuteNutrientAction());
            if (restTeaButton != null) restTeaButton.onClick.AddListener(() => raisingController?.ExecuteRestTeaAction());
        }

        private void SubscribeEvents()
        {
            if (raisingController == null) return;

            if (raisingController.turnSystem != null)
            {
                raisingController.turnSystem.OnDayChanged += UpdateDayUI;
            }

            if (raisingController.energyManager != null)
            {
                raisingController.energyManager.OnEnergyChanged += UpdateEnergyUI;
                raisingController.energyManager.OnFailureRateUpdated += UpdateFailureRateUI;
            }

            if (raisingController.plantManager != null)
            {
                raisingController.plantManager.OnStatsChanged += UpdatePlantStatsUI;
                raisingController.plantManager.OnStageChanged += (stage) => UpdatePlantInfoUI();
            }

            raisingController.OnActionLogged += UpdateLogUI;
        }

        private void UnsubscribeEvents()
        {
            if (raisingController == null) return;

            if (raisingController.turnSystem != null)
            {
                raisingController.turnSystem.OnDayChanged -= UpdateDayUI;
            }

            if (raisingController.energyManager != null)
            {
                raisingController.energyManager.OnEnergyChanged -= UpdateEnergyUI;
                raisingController.energyManager.OnFailureRateUpdated -= UpdateFailureRateUI;
            }

            if (raisingController.plantManager != null)
            {
                raisingController.plantManager.OnStatsChanged -= UpdatePlantStatsUI;
            }

            raisingController.OnActionLogged -= UpdateLogUI;
        }

        private void UpdateDayUI(int currentDay, int remainingDays)
        {
            if (dayInfoText != null) dayInfoText.text = $"Hari ke-{currentDay}";
            if (remainingDaysText != null) remainingDaysText.text = $"Sisa Hari: {remainingDays}";
        }

        private void UpdateEnergyUI(int current, int max)
        {
            if (energySlider != null)
            {
                energySlider.maxValue = max;
                energySlider.value = current;
            }

            if (energyText != null)
            {
                energyText.text = $"Energi: {current}/{max}";
            }
        }

        private void UpdateFailureRateUI(int failureRate)
        {
            if (failureRateText != null)
            {
                if (failureRate > 0)
                {
                    failureRateText.text = $"Risiko Gagal: <color=#FF4444>{failureRate}%</color>";
                }
                else
                {
                    failureRateText.text = $"Risiko Gagal: <color=#44FF44>0% (Aman)</color>";
                }
            }
        }

        private void UpdatePlantInfoUI()
        {
            if (raisingController?.plantManager == null) return;

            if (flowerNameText != null && raisingController.plantManager.activeFlowerData != null)
            {
                flowerNameText.text = raisingController.plantManager.activeFlowerData.flowerName;
            }

            if (stageText != null)
            {
                stageText.text = $"Fase: {raisingController.plantManager.currentStage}";
            }

            UpdatePlantStatsUI(
                raisingController.plantManager.CurrentSunlight,
                raisingController.plantManager.CurrentNutrients,
                raisingController.plantManager.CurrentWater
            );
        }

        private void UpdatePlantStatsUI(int sun, int nut, int water)
        {
            FlowerPhaseRequirement req = raisingController?.plantManager?.GetCurrentRequirement();

            int targetSun = (req != null) ? req.requiredSunlight : 0;
            int targetNut = (req != null) ? req.requiredNutrients : 0;
            int targetWater = (req != null) ? req.requiredWater : 0;

            if (sunlightStatText != null) sunlightStatText.text = $"☀️ Cahaya: {sun}/{targetSun}";
            if (nutrientStatText != null) nutrientStatText.text = $"🌿 Nutrisi: {nut}/{targetNut}";
            if (waterStatText != null) waterStatText.text = $"💧 Air: {water}/{targetWater}";
        }

        private void UpdateLogUI(string message)
        {
            if (actionLogText != null)
            {
                actionLogText.text = message;
            }
        }
    }
}
