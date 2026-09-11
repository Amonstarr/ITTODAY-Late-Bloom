using System;
using UnityEngine;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
    [Serializable]
    public struct ActionFeedbackData
    {
        public string actionTitle;
        public int deltaWater;
        public int totalWater;
        public int deltaSunlight;
        public int totalSunlight;
        public int deltaNutrients;
        public int totalNutrients;
        public int deltaEnergy;
        public int totalEnergy;
        public Sprite plantSprite;
        public Sprite potSprite;
        public bool isSuccess;
        public bool isStageAdvanced;
    }

    [Serializable]
    public struct EvaluationFeedbackData
    {
        public EvaluationResult result;
        public int currentSunlight;
        public int targetSunlight;
        public int currentNutrients;
        public int targetNutrients;
        public int currentWater;
        public int targetWater;
        public Sprite plantSprite;
        public Sprite potSprite;
        public bool isSuccess;
        public string oldStageName;
        public string newStageName;
    }

    public class RaisingController : MonoBehaviour
    {
        public static RaisingController Instance { get; private set; }

        [Header("System References")]
        public PlantGrowthManager plantManager;
        public PlayerEnergyManager energyManager;
        public TurnSystem turnSystem;
        public CheckpointEvaluator evaluator;
        public PuzzlePhaseManager puzzlePhaseManager;

        [Header("Default Flower Config (Optional)")]
        [SerializeField] private FlowerData defaultFlowerData;

        [Header("Events")]
        public Action<string> OnActionLogged;
        public Action<EvaluationResult> OnPhaseEvaluated;
        public Action<ActionFeedbackData> OnActionExecuted;
        public Action<EvaluationFeedbackData> OnPhaseEvaluatedFeedback;

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

            FindDependenciesIfNull();
        }

        private Item[] sceneItems;

        private void Start()
        {
            InitializeGame();
            SubscribeSceneItems();
        }

        private void OnEnable()
        {
            if (turnSystem != null)
            {
                turnSystem.OnEvaluationDayReached += HandleEvaluationTime;
            }
            SubscribeSceneItems();
        }

        private void OnDisable()
        {
            if (turnSystem != null)
            {
                turnSystem.OnEvaluationDayReached -= HandleEvaluationTime;
            }
            UnsubscribeSceneItems();
        }

        private void SubscribeSceneItems()
        {
#if UNITY_2023_1_OR_NEWER
            sceneItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
#else
            sceneItems = FindObjectsOfType<Item>();
#endif
            if (sceneItems != null)
            {
                foreach (Item it in sceneItems)
                {
                    it.OnItemUsed -= HandleItemUsed;
                    it.OnItemUsed += HandleItemUsed;
                }
            }
        }

        private void UnsubscribeSceneItems()
        {
            if (sceneItems != null)
            {
                foreach (Item it in sceneItems)
                {
                    if (it != null) it.OnItemUsed -= HandleItemUsed;
                }
            }
        }

        public PlayerMoodManager moodManager;

        public void HandleItemUsed(ItemType type, float value = 0)
        {
            switch (type)
            {
                case ItemType.Sunlight:
                    ExecuteSunlightAction();
                    break;
                case ItemType.Water:
                    ExecuteWaterAction();
                    break;
                case ItemType.Fertilizer:
                    ExecuteNutrientAction();
                    break;
                case ItemType.Tea:
                    ExecuteRestTeaAction();
                    break;
                case ItemType.Radio:
                    ExecuteRadioMoodAction();
                    break;
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            // Shortcut testing cepat di Unity Editor
            // [1] = Pinset (Cahaya Matahari)
            // [2] = Botol Air (Penyiraman)
            // [3] = Pupuk (Nutrisi)
            // [4] = Cangkir Teh (Rest)
            // [5] = Radio (Mood Boost)
            // [E] = Paksa Evaluasi Sekarang
            if (Input.GetKeyDown(KeyCode.Alpha1)) ExecuteSunlightAction();
            if (Input.GetKeyDown(KeyCode.Alpha2)) ExecuteWaterAction();
            if (Input.GetKeyDown(KeyCode.Alpha3)) ExecuteNutrientAction();
            if (Input.GetKeyDown(KeyCode.Alpha4)) ExecuteRestTeaAction();
            if (Input.GetKeyDown(KeyCode.Alpha5)) ExecuteRadioMoodAction();
            if (Input.GetKeyDown(KeyCode.E)) HandleEvaluationTime();
        }
#endif

        private void FindDependenciesIfNull()
        {
#if UNITY_2023_1_OR_NEWER
            if (plantManager == null) plantManager = FindFirstObjectByType<PlantGrowthManager>();
            if (energyManager == null) energyManager = FindFirstObjectByType<PlayerEnergyManager>();
            if (moodManager == null) moodManager = FindFirstObjectByType<PlayerMoodManager>();
            if (turnSystem == null) turnSystem = FindFirstObjectByType<TurnSystem>();
            if (evaluator == null) evaluator = FindFirstObjectByType<CheckpointEvaluator>();
            if (puzzlePhaseManager == null) puzzlePhaseManager = FindFirstObjectByType<PuzzlePhaseManager>();
#else
            if (plantManager == null) plantManager = FindObjectOfType<PlantGrowthManager>();
            if (energyManager == null) energyManager = FindObjectOfType<PlayerEnergyManager>();
            if (moodManager == null) moodManager = FindObjectOfType<PlayerMoodManager>();
            if (turnSystem == null) turnSystem = FindObjectOfType<TurnSystem>();
            if (evaluator == null) evaluator = FindObjectOfType<CheckpointEvaluator>();
            if (puzzlePhaseManager == null) puzzlePhaseManager = FindObjectOfType<PuzzlePhaseManager>();
#endif
        }

        public void InitializeGame()
        {
            if (defaultFlowerData != null && plantManager != null)
            {
                plantManager.SetFlower(defaultFlowerData, FlowerGrowthStage.Seed);
            }

            FlowerPhaseRequirement req = plantManager?.GetCurrentRequirement();
            int days = (req != null) ? req.daysAllocated : 10;

            if (turnSystem != null)
            {
                turnSystem.StartNewPhase(days);
            }
        }

        // ──────────────────────────────────────────
        // AKSI PERAWATAN (COMMAND ALA UMA MUSUME)
        // ──────────────────────────────────────────

        public void ExecuteSunlightAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

            int prevSun = plantManager != null ? plantManager.CurrentSunlight : 0;
            int prevWater = plantManager != null ? plantManager.CurrentWater : 0;
            int prevNut = plantManager != null ? plantManager.CurrentNutrients : 0;
            int prevEnergy = energyManager != null ? energyManager.CurrentEnergy : 0;
            FlowerGrowthStage prevStage = plantManager != null ? plantManager.currentStage : FlowerGrowthStage.Seed;

            bool isFailed;
            bool actionSuccess = energyManager.TryPerformCareAction(out isFailed);

            if (actionSuccess)
            {
                int baseRoll = UnityEngine.Random.Range(10, 21);
                float multiplier = (moodManager != null) ? moodManager.GetPointMultiplier() : 1.0f;
                int finalSun = Mathf.RoundToInt(baseRoll * multiplier);

                plantManager.AddSunlight(finalSun);

                // Efek Penguapan: Mengurangi air sedikit (-3 s/d -5) jika air sudah ada
                int evap = UnityEngine.Random.Range(3, 6);
                if (plantManager.CurrentWater > 0)
                {
                    plantManager.AddWater(-evap);
                    LogAction($"✂️ Menggunakan pinset untuk merapikan daun & memaparkan sinar matahari hangat. (Cahaya +{finalSun}, Air -{evap} karena penguapan).");
                }
                else
                {
                    LogAction($"✂️ Menggunakan pinset untuk merapikan daun & memaparkan sinar matahari hangat. (Cahaya +{finalSun}).");
                }
            }
            else
            {
                LogAction($"⚠️ MC kelelahan saat merapikan tanaman! Daun tergores dan terpapar panas berlebih.");
            }

            turnSystem.AdvanceDay();

            PublishActionFeedback(actionSuccess ? "Menjemur Tanaman" : "MC Kelelahan Saat Menjemur", actionSuccess, prevSun, prevWater, prevNut, prevEnergy, prevStage);
        }

        public void ExecuteWaterAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

            int prevSun = plantManager != null ? plantManager.CurrentSunlight : 0;
            int prevWater = plantManager != null ? plantManager.CurrentWater : 0;
            int prevNut = plantManager != null ? plantManager.CurrentNutrients : 0;
            int prevEnergy = energyManager != null ? energyManager.CurrentEnergy : 0;
            FlowerGrowthStage prevStage = plantManager != null ? plantManager.currentStage : FlowerGrowthStage.Seed;

            bool isFailed;
            bool actionSuccess = energyManager.TryPerformCareAction(out isFailed);

            if (actionSuccess)
            {
                int baseRoll = UnityEngine.Random.Range(10, 21);
                float multiplier = (moodManager != null) ? moodManager.GetPointMultiplier() : 1.0f;
                int finalWater = Mathf.RoundToInt(baseRoll * multiplier);

                plantManager.AddWater(finalWater);

                // Efek Pelarutan: Membantu melarutkan sedikit nutrisi tanah (+2 s/d +4)
                int bonusNut = UnityEngine.Random.Range(2, 5);
                plantManager.AddNutrients(bonusNut);

                LogAction($"💧 Menyiram air segar secukupnya ke media tanam. (Air +{finalWater}, Nutrisi terlarut +{bonusNut}).");
            }
            else
            {
                LogAction($"⚠️ MC tangan gemetar karena lelah. Air tumpah berlebihan!");
            }

            turnSystem.AdvanceDay();

            PublishActionFeedback(actionSuccess ? "Menyiram Tanaman" : "MC Kelelahan Saat Menyiram", actionSuccess, prevSun, prevWater, prevNut, prevEnergy, prevStage);
        }

        public void ExecuteNutrientAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

            int prevSun = plantManager != null ? plantManager.CurrentSunlight : 0;
            int prevWater = plantManager != null ? plantManager.CurrentWater : 0;
            int prevNut = plantManager != null ? plantManager.CurrentNutrients : 0;
            int prevEnergy = energyManager != null ? energyManager.CurrentEnergy : 0;
            FlowerGrowthStage prevStage = plantManager != null ? plantManager.currentStage : FlowerGrowthStage.Seed;

            bool isFailed;
            bool actionSuccess = energyManager.TryPerformCareAction(out isFailed);

            if (actionSuccess)
            {
                int baseRoll = UnityEngine.Random.Range(10, 21);
                float multiplier = (moodManager != null) ? moodManager.GetPointMultiplier() : 1.0f;
                int finalNut = Mathf.RoundToInt(baseRoll * multiplier);

                plantManager.AddNutrients(finalNut);
                LogAction($"🌿 Memberikan pupuk bernutrisi ke tanah. (Nutrisi +{finalNut}).");
            }
            else
            {
                LogAction($"⚠️ MC salah menakar dosis pupuk karena kelelahan!");
            }

            turnSystem.AdvanceDay();

            PublishActionFeedback(actionSuccess ? "Memberikan Pupuk" : "MC Kelelahan Saat Memupuk", actionSuccess, prevSun, prevWater, prevNut, prevEnergy, prevStage);
        }

        public void ExecuteRestTeaAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

            int prevSun = plantManager != null ? plantManager.CurrentSunlight : 0;
            int prevWater = plantManager != null ? plantManager.CurrentWater : 0;
            int prevNut = plantManager != null ? plantManager.CurrentNutrients : 0;
            int prevEnergy = energyManager != null ? energyManager.CurrentEnergy : 0;
            FlowerGrowthStage prevStage = plantManager != null ? plantManager.currentStage : FlowerGrowthStage.Seed;

            energyManager.RestDrinkTea();
            LogAction($"🍵 MC duduk santai di teras, menyeduh teh hangat mendiang istri. Stamina terisi kembali (+{energyManager.TeaRestRestore}).");

            turnSystem.AdvanceDay();

            PublishActionFeedback("Minum Teh & Istirahat", true, prevSun, prevWater, prevNut, prevEnergy, prevStage);
        }

        public void ExecuteRadioMoodAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

            int prevSun = plantManager != null ? plantManager.CurrentSunlight : 0;
            int prevWater = plantManager != null ? plantManager.CurrentWater : 0;
            int prevNut = plantManager != null ? plantManager.CurrentNutrients : 0;
            int prevEnergy = energyManager != null ? energyManager.CurrentEnergy : 0;
            FlowerGrowthStage prevStage = plantManager != null ? plantManager.currentStage : FlowerGrowthStage.Seed;

            if (moodManager != null)
            {
                moodManager.BoostMoodFromRadio();
                LogAction($"📻 MC memutar siaran musik klasik kesukaan istri di radio tua. Suasana hati membaik! (Mood: {moodManager.CurrentMood}).");
            }
            else
            {
                LogAction($"📻 MC mendengarkan siaran radio dengan tenang.");
            }

            turnSystem.AdvanceDay();

            PublishActionFeedback("Memutar Musik Radio", true, prevSun, prevWater, prevNut, prevEnergy, prevStage);
        }

        private void PublishActionFeedback(string defaultTitle, bool actionSuccess, int prevSun, int prevWater, int prevNut, int prevEnergy, FlowerGrowthStage prevStage)
        {
            int curSun = plantManager != null ? plantManager.CurrentSunlight : 0;
            int curWater = plantManager != null ? plantManager.CurrentWater : 0;
            int curNut = plantManager != null ? plantManager.CurrentNutrients : 0;
            int curEnergy = energyManager != null ? energyManager.CurrentEnergy : 0;
            FlowerGrowthStage curStage = plantManager != null ? plantManager.currentStage : FlowerGrowthStage.Seed;
            bool stageAdvanced = curStage != prevStage;

            string title = defaultTitle;
            if (stageAdvanced)
            {
                title = "Bibit Tumbuh";
            }

            Sprite plantSpr = null;
#if UNITY_2023_1_OR_NEWER
            FlowerGrowthVisual visual = FindFirstObjectByType<FlowerGrowthVisual>();
#else
            FlowerGrowthVisual visual = FindObjectOfType<FlowerGrowthVisual>();
#endif
            if (visual != null && visual.plantRenderer != null)
            {
                plantSpr = visual.plantRenderer.sprite;
            }

            Sprite potSpr = GameObject.Find("Pot_Bunga")?.GetComponent<SpriteRenderer>()?.sprite;

            ActionFeedbackData feedback = new ActionFeedbackData
            {
                actionTitle = title,
                deltaWater = curWater - prevWater,
                totalWater = curWater,
                deltaSunlight = curSun - prevSun,
                totalSunlight = curSun,
                deltaNutrients = curNut - prevNut,
                totalNutrients = curNut,
                deltaEnergy = curEnergy - prevEnergy,
                totalEnergy = curEnergy,
                plantSprite = plantSpr,
                potSprite = potSpr,
                isSuccess = actionSuccess,
                isStageAdvanced = stageAdvanced
            };

            OnActionExecuted?.Invoke(feedback);
        }

        // ──────────────────────────────────────────
        // EVALUASI CHECKPOINT (THE "RACE")
        // ──────────────────────────────────────────

        private void HandleEvaluationTime()
        {
            FlowerPhaseRequirement req = plantManager.GetCurrentRequirement();
            EvaluationResult result = evaluator.Evaluate(plantManager, req);

            OnPhaseEvaluated?.Invoke(result);

            int curSun = (plantManager != null) ? plantManager.CurrentSunlight : 0;
            int curNut = (plantManager != null) ? plantManager.CurrentNutrients : 0;
            int curWater = (plantManager != null) ? plantManager.CurrentWater : 0;
            int targetSun = (req != null) ? req.targetSunlight : 30;
            int targetNut = (req != null) ? req.targetNutrients : 30;
            int targetWater = (req != null) ? req.targetWater : 45;

            string oldStage = (plantManager != null) ? plantManager.currentStage.ToString() : "Seed";

            if (result.isSuccess)
            {
                LogAction($"🎉 [CHECKPOINT SUKSES] {result.title} {result.detailMessage}");

                // Majukan fase di PuzzlePhaseManager (ini memicu pembagian kepingan puzzle jurnal robek)
                if (puzzlePhaseManager != null)
                {
                    puzzlePhaseManager.AdvanceGrowthStage();
                }

                // Majukan fase internal PlantGrowthManager (poin kumulatif tetap ada, tidak di-nolkan)
                if (plantManager.currentStage == FlowerGrowthStage.Seed)
                {
                    plantManager.SetStage(FlowerGrowthStage.Sprout);
                }
                else if (plantManager.currentStage == FlowerGrowthStage.Sprout)
                {
                    plantManager.SetStage(FlowerGrowthStage.Bud);
                }
                else if (plantManager.currentStage == FlowerGrowthStage.Bud)
                {
                    plantManager.SetStage(FlowerGrowthStage.Bloom);
                }

                // Mulai fase baru di TurnSystem
                FlowerPhaseRequirement nextReq = plantManager.GetCurrentRequirement();
                int nextDays = (nextReq != null) ? nextReq.daysAllocated : 10;
                turnSystem.StartNewPhase(nextDays);
            }
            else
            {
                LogAction($"❌ [CHECKPOINT KURANG OPTIMAL] {result.detailMessage} MC butuh 2 hari tambahan untuk menstabilkan kondisi tanaman.");
                // Berikan 2 hari kompensasi untuk membenahi stat
                turnSystem.StartNewPhase(2);
            }

            Sprite plantSpr = null;
#if UNITY_2023_1_OR_NEWER
            FlowerGrowthVisual visual = FindFirstObjectByType<FlowerGrowthVisual>();
#else
            FlowerGrowthVisual visual = FindObjectOfType<FlowerGrowthVisual>();
#endif
            if (visual != null && visual.plantRenderer != null)
            {
                plantSpr = visual.plantRenderer.sprite;
            }

            Sprite potSpr = GameObject.Find("Pot_Bunga")?.GetComponent<SpriteRenderer>()?.sprite;

            EvaluationFeedbackData feedback = new EvaluationFeedbackData
            {
                result = result,
                currentSunlight = curSun,
                targetSunlight = targetSun,
                currentNutrients = curNut,
                targetNutrients = targetNut,
                currentWater = curWater,
                targetWater = targetWater,
                plantSprite = plantSpr,
                potSprite = potSpr,
                isSuccess = result.isSuccess,
                oldStageName = oldStage,
                newStageName = (plantManager != null) ? plantManager.currentStage.ToString() : oldStage
            };

            OnPhaseEvaluatedFeedback?.Invoke(feedback);
        }

        private void LogAction(string message)
        {
            Debug.Log($"[RaisingController] {message}");
            OnActionLogged?.Invoke(message);
        }
    }
}
