using System;
using UnityEngine;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
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
        }

        public void ExecuteWaterAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

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
        }

        public void ExecuteNutrientAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

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
        }

        public void ExecuteRestTeaAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

            energyManager.RestDrinkTea();
            LogAction($"🍵 MC duduk santai di teras, menyeduh teh hangat mendiang istri. Stamina terisi kembali (+{energyManager.TeaRestRestore}).");

            turnSystem.AdvanceDay();
        }

        public void ExecuteRadioMoodAction()
        {
            if (turnSystem.RemainingDays <= 0) return;

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
        }

        // ──────────────────────────────────────────
        // EVALUASI CHECKPOINT (THE "RACE")
        // ──────────────────────────────────────────

        private void HandleEvaluationTime()
        {
            FlowerPhaseRequirement req = plantManager.GetCurrentRequirement();
            EvaluationResult result = evaluator.Evaluate(plantManager, req);

            OnPhaseEvaluated?.Invoke(result);

            if (result.isSuccess)
            {
                LogAction($"🎉 [CHECKPOINT SUKSES] {result.title} {result.detailMessage}");

                // Majukan fase di PuzzlePhaseManager (ini memicu pembagian kepingan puzzle jurnal robek)
                if (puzzlePhaseManager != null)
                {
                    puzzlePhaseManager.AdvanceGrowthStage();
                }

                // Majukan fase internal PlantGrowthManager
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
        }

        private void LogAction(string message)
        {
            Debug.Log($"[RaisingController] {message}");
            OnActionLogged?.Invoke(message);
        }
    }
}
