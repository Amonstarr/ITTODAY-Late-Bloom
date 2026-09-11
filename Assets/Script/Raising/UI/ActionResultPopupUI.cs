using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LateBloom.Raising;

namespace LateBloom.Raising.UI
{
    /// <summary>
    /// Popup UI for displaying care action feedback and phase evaluation checkpoints.
    /// </summary>
    public class ActionResultPopupUI : MonoBehaviour
    {
        public static ActionResultPopupUI Instance { get; private set; }
        public static bool IsOpen { get; private set; }

        [Header("Custom Icons")]
        [SerializeField] private Sprite waterIconSprite;
        [SerializeField] private Sprite sunIconSprite;
        [SerializeField] private Sprite nutrientIconSprite;
        [SerializeField] private Sprite staminaIconSprite;
        [SerializeField] private Sprite potFallbackSprite;

        // UI references
        private GameObject popupRoot;
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI actionTitleText;
        private Image plantImage;
        private Image potImage;

        // Stat row references
        private TextMeshProUGUI waterDeltaText;
        private TextMeshProUGUI waterArrowText;
        private TextMeshProUGUI waterTotalText;
        private TextMeshProUGUI sunDeltaText;
        private TextMeshProUGUI sunArrowText;
        private TextMeshProUGUI sunTotalText;
        private TextMeshProUGUI nutDeltaText;
        private TextMeshProUGUI nutArrowText;
        private TextMeshProUGUI nutTotalText;
        private TextMeshProUGUI staminaDeltaText;
        private TextMeshProUGUI staminaArrowText;
        private TextMeshProUGUI staminaTotalText;

        private TextMeshProUGUI continueHintText;
        private float canDismissTime;
        private bool pointerMustBeReleased;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterSceneLoadedListener()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureInstanceForActiveScene();
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            EnsureInstanceForActiveScene();
        }

        public static void EnsureInstanceForActiveScene()
        {
            if (Instance == null || Instance.gameObject == null)
            {
                Canvas canvas = null;
                GameObject canvasObj = GameObject.Find("UI Canvas");
                if (canvasObj != null) canvas = canvasObj.GetComponent<Canvas>();
                if (canvas == null)
                {
#if UNITY_2023_1_OR_NEWER
                    canvas = Object.FindFirstObjectByType<Canvas>();
#else
                    canvas = Object.FindObjectOfType<Canvas>();
#endif
                }

                if (canvas != null)
                {
                    GameObject go = new GameObject("ActionResultPopupUI", typeof(ActionResultPopupUI));
                    go.transform.SetParent(canvas.transform, false);
                }
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            IsOpen = false;
            BuildUIHierarchy();
        }

        private void OnEnable()
        {
            if (RaisingController.Instance != null)
            {
                RaisingController.Instance.OnActionExecuted -= HandleActionExecuted;
                RaisingController.Instance.OnActionExecuted += HandleActionExecuted;

                RaisingController.Instance.OnPhaseEvaluatedFeedback -= HandlePhaseEvaluated;
                RaisingController.Instance.OnPhaseEvaluatedFeedback += HandlePhaseEvaluated;
            }
        }

        private void OnDisable()
        {
            if (RaisingController.Instance != null)
            {
                RaisingController.Instance.OnActionExecuted -= HandleActionExecuted;
                RaisingController.Instance.OnPhaseEvaluatedFeedback -= HandlePhaseEvaluated;
            }
            IsOpen = false;
        }

        private void Start()
        {
            if (RaisingController.Instance != null)
            {
                RaisingController.Instance.OnActionExecuted -= HandleActionExecuted;
                RaisingController.Instance.OnActionExecuted += HandleActionExecuted;

                RaisingController.Instance.OnPhaseEvaluatedFeedback -= HandlePhaseEvaluated;
                RaisingController.Instance.OnPhaseEvaluatedFeedback += HandlePhaseEvaluated;
            }
        }

        private void Update()
        {
            if (!IsOpen) return;

            // Hint breathing animation
            if (continueHintText != null)
            {
                float alpha = Mathf.PingPong(Time.unscaledTime * 1.5f, 0.5f) + 0.45f;
                Color c = continueHintText.color;
                c.a = alpha;
                continueHintText.color = c;
            }

            // Wait until pointer click is released before allowing dismissal
            if (pointerMustBeReleased)
            {
                if (!Input.GetMouseButton(0))
                {
                    pointerMustBeReleased = false;
                }
                return;
            }

            // Dismiss popup on user click or confirm key
            if (Time.unscaledTime >= canDismissTime)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    Dismiss();
                }
            }
        }

        public void HandleActionExecuted(ActionFeedbackData data)
        {
            ShowPopup(data);
        }

        public void HandlePhaseEvaluated(EvaluationFeedbackData data)
        {
            ShowEvaluationPopup(data);
        }

        public void ShowPopup(ActionFeedbackData data)
        {
            if (popupRoot == null)
            {
                BuildUIHierarchy();
            }

            if (actionTitleText != null)
            {
                actionTitleText.text = string.IsNullOrEmpty(data.actionTitle) ? "Bibit Tumbuh" : data.actionTitle;
            }

            if (plantImage != null)
            {
                if (data.plantSprite != null)
                {
                    plantImage.sprite = data.plantSprite;
                    plantImage.gameObject.SetActive(true);
                }
                else
                {
                    plantImage.gameObject.SetActive(false);
                }
            }

            if (potImage != null)
            {
                Sprite potSpr = data.potSprite != null ? data.potSprite : potFallbackSprite;
                if (potSpr != null)
                {
                    potImage.sprite = potSpr;
                    potImage.gameObject.SetActive(true);
                }
                else
                {
                    potImage.gameObject.SetActive(false);
                }
            }

            if (waterArrowText != null) waterArrowText.text = "->";
            if (sunArrowText != null) sunArrowText.text = "->";
            if (nutArrowText != null) nutArrowText.text = "->";
            if (staminaArrowText != null) staminaArrowText.text = "->";
            if (continueHintText != null) continueHintText.text = "Click to continue";

            SetStatRow(waterDeltaText, waterTotalText, data.deltaWater, data.totalWater, false);
            SetStatRow(sunDeltaText, sunTotalText, data.deltaSunlight, data.totalSunlight, false);
            SetStatRow(nutDeltaText, nutTotalText, data.deltaNutrients, data.totalNutrients, false);
            SetStatRow(staminaDeltaText, staminaTotalText, data.deltaEnergy, data.totalEnergy, true);

            IsOpen = true;
            popupRoot.SetActive(true);
            canDismissTime = Time.unscaledTime + 0.35f;
            pointerMustBeReleased = true;

            if (canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeInRoutine());
            }
        }

        public void ShowEvaluationPopup(EvaluationFeedbackData data)
        {
            if (popupRoot == null)
            {
                BuildUIHierarchy();
            }

            if (actionTitleText != null)
            {
                actionTitleText.text = string.IsNullOrEmpty(data.result.title) ? "Evaluasi Fase Selesai" : data.result.title;
            }

            if (plantImage != null)
            {
                if (data.plantSprite != null)
                {
                    plantImage.sprite = data.plantSprite;
                    plantImage.gameObject.SetActive(true);
                }
                else
                {
                    plantImage.gameObject.SetActive(false);
                }
            }

            if (potImage != null)
            {
                Sprite potSpr = data.potSprite != null ? data.potSprite : potFallbackSprite;
                if (potSpr != null)
                {
                    potImage.sprite = potSpr;
                    potImage.gameObject.SetActive(true);
                }
                else
                {
                    potImage.gameObject.SetActive(false);
                }
            }

            if (waterArrowText != null) waterArrowText.text = "/";
            if (sunArrowText != null) sunArrowText.text = "/";
            if (nutArrowText != null) nutArrowText.text = "/";
            if (staminaArrowText != null) staminaArrowText.text = "->";

            SetEvaluationRow(waterDeltaText, waterTotalText, data.currentWater, data.targetWater);
            SetEvaluationRow(sunDeltaText, sunTotalText, data.currentSunlight, data.targetSunlight);
            SetEvaluationRow(nutDeltaText, nutTotalText, data.currentNutrients, data.targetNutrients);

            if (staminaDeltaText != null)
            {
                staminaDeltaText.text = data.result.grade.ToString();
                staminaDeltaText.color = data.isSuccess ? new Color(0.38f, 0.85f, 0.5f) : new Color(0.96f, 0.45f, 0.45f);
            }
            if (staminaTotalText != null)
            {
                staminaTotalText.text = data.isSuccess ? data.newStageName : "+2 Hari";
                staminaTotalText.color = Color.white;
            }

            if (continueHintText != null)
            {
                continueHintText.text = "Klik di mana saja untuk lanjut ke fase baru";
            }

            IsOpen = true;
            popupRoot.SetActive(true);
            canDismissTime = Time.unscaledTime + 0.35f;
            pointerMustBeReleased = true;

            if (canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeInRoutine());
            }
        }

        private void SetEvaluationRow(TextMeshProUGUI currentLabel, TextMeshProUGUI targetLabel, int current, int target)
        {
            if (currentLabel != null)
            {
                currentLabel.text = current.ToString();
                currentLabel.color = (current >= target) ? new Color(0.38f, 0.85f, 0.5f) : new Color(0.95f, 0.95f, 0.95f);
            }

            if (targetLabel != null)
            {
                targetLabel.text = target.ToString();
                targetLabel.color = new Color(0.85f, 0.85f, 0.85f);
            }
        }

        public void Dismiss()
        {
            if (!IsOpen) return;
            IsOpen = false;

            if (canvasGroup != null && gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(FadeOutRoutine());
            }
            else
            {
                if (popupRoot != null) popupRoot.SetActive(false);
            }
        }

        private void SetStatRow(TextMeshProUGUI deltaLabel, TextMeshProUGUI totalLabel, int delta, int total, bool isStamina)
        {
            if (deltaLabel != null)
            {
                if (delta > 0)
                {
                    deltaLabel.text = "+" + delta;
                    deltaLabel.color = isStamina ? new Color(0.38f, 0.85f, 0.5f) : new Color(0.95f, 0.95f, 0.95f);
                }
                else if (delta < 0)
                {
                    deltaLabel.text = delta.ToString();
                    deltaLabel.color = new Color(0.96f, 0.45f, 0.45f);
                }
                else
                {
                    deltaLabel.text = "0";
                    deltaLabel.color = new Color(0.75f, 0.75f, 0.75f);
                }
            }

            if (totalLabel != null)
            {
                totalLabel.text = total.ToString();
            }
        }

        private IEnumerator FadeInRoutine()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            float duration = 0.15f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutRoutine()
        {
            float elapsed = 0f;
            float duration = 0.12f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            if (popupRoot != null) popupRoot.SetActive(false);
        }

        // UI hierarchy construction
        private void BuildUIHierarchy()
        {
            if (popupRoot != null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = GameObject.Find("UI Canvas");
                if (canvasObj != null) canvas = canvasObj.GetComponent<Canvas>();
            }
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            Transform parentTransform = canvas != null ? canvas.transform : transform;

            TMP_FontAsset fontAsset = null;
            TextMeshProUGUI existingTmp = FindFirstObjectByType<TextMeshProUGUI>();
            if (existingTmp != null)
            {
                fontAsset = existingTmp.font;
            }

            EnsureSprites();

            // Root overlay
            popupRoot = new GameObject("ActionResultPopup", typeof(RectTransform), typeof(CanvasGroup));
            popupRoot.transform.SetParent(parentTransform, false);
            RectTransform rootRect = popupRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            canvasGroup = popupRoot.GetComponent<CanvasGroup>();

            GameObject dimmerObj = new GameObject("DimmerOverlay", typeof(RectTransform), typeof(Image));
            dimmerObj.transform.SetParent(popupRoot.transform, false);
            RectTransform dimmerRect = dimmerObj.GetComponent<RectTransform>();
            dimmerRect.anchorMin = Vector2.zero;
            dimmerRect.anchorMax = Vector2.one;
            dimmerRect.offsetMin = Vector2.zero;
            dimmerRect.offsetMax = Vector2.zero;

            Image dimmerImg = dimmerObj.GetComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.65f);
            dimmerImg.raycastTarget = true;

            // Center banner
            GameObject bannerObj = new GameObject("CenterWhiteBanner", typeof(RectTransform), typeof(Image));
            bannerObj.transform.SetParent(popupRoot.transform, false);
            RectTransform bannerRect = bannerObj.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0f, 0.45f);
            bannerRect.anchorMax = new Vector2(1f, 0.67f);
            bannerRect.offsetMin = Vector2.zero;
            bannerRect.offsetMax = Vector2.zero;

            Image bannerImg = bannerObj.GetComponent<Image>();
            bannerImg.color = Color.white;
            bannerImg.raycastTarget = false;

            CreateLine(bannerObj.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -3f), new Vector2(0f, 0f), new Color(0.12f, 0.12f, 0.15f, 1f));
            CreateLine(bannerObj.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 3f), new Color(0.12f, 0.12f, 0.15f, 1f));

            // Banner title & plant visual
            GameObject titleObj = new GameObject("ActionTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(bannerObj.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.15f, 0f);
            titleRect.anchorMax = new Vector2(0.62f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            actionTitleText = titleObj.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) actionTitleText.font = fontAsset;
            actionTitleText.text = "Bibit Tumbuh";
            actionTitleText.fontSize = 44;
            actionTitleText.fontStyle = FontStyles.Bold;
            actionTitleText.color = new Color(0.12f, 0.14f, 0.18f);
            actionTitleText.alignment = TextAlignmentOptions.MidlineLeft;
            actionTitleText.raycastTarget = false;

            GameObject plantContainer = new GameObject("PlantBannerContainer", typeof(RectTransform));
            plantContainer.transform.SetParent(bannerObj.transform, false);
            RectTransform plantContRect = plantContainer.GetComponent<RectTransform>();
            plantContRect.anchorMin = new Vector2(0.68f, 0.05f);
            plantContRect.anchorMax = new Vector2(0.85f, 0.95f);
            plantContRect.offsetMin = Vector2.zero;
            plantContRect.offsetMax = Vector2.zero;

            GameObject potObj = new GameObject("PotImage", typeof(RectTransform), typeof(Image));
            potObj.transform.SetParent(plantContainer.transform, false);
            RectTransform potRect = potObj.GetComponent<RectTransform>();
            potRect.anchorMin = new Vector2(0.5f, 0f);
            potRect.anchorMax = new Vector2(0.5f, 0f);
            potRect.pivot = new Vector2(0.5f, 0f);
            potRect.sizeDelta = new Vector2(130f, 90f);
            potRect.anchoredPosition = new Vector2(0f, 10f);

            potImage = potObj.GetComponent<Image>();
            potImage.preserveAspect = true;
            potImage.raycastTarget = false;
            if (potFallbackSprite != null) potImage.sprite = potFallbackSprite;

            GameObject plantSprObj = new GameObject("PlantImage", typeof(RectTransform), typeof(Image));
            plantSprObj.transform.SetParent(plantContainer.transform, false);
            RectTransform plantSprRect = plantSprObj.GetComponent<RectTransform>();
            plantSprRect.anchorMin = new Vector2(0.5f, 0f);
            plantSprRect.anchorMax = new Vector2(0.5f, 0f);
            plantSprRect.pivot = new Vector2(0.5f, 0f);
            plantSprRect.sizeDelta = new Vector2(130f, 120f);
            plantSprRect.anchoredPosition = new Vector2(0f, 60f);

            plantImage = plantSprObj.GetComponent<Image>();
            plantImage.preserveAspect = true;
            plantImage.raycastTarget = false;

            // Stats section
            GameObject statsContainer = new GameObject("StatsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            statsContainer.transform.SetParent(popupRoot.transform, false);
            RectTransform statsRect = statsContainer.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.36f, 0.13f);
            statsRect.anchorMax = new Vector2(0.64f, 0.42f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = statsContainer.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 8f;

            CreateStatRow("WaterRow", statsContainer.transform, waterIconSprite, fontAsset, out waterDeltaText, out waterArrowText, out waterTotalText);
            CreateStatRow("SunRow", statsContainer.transform, sunIconSprite, fontAsset, out sunDeltaText, out sunArrowText, out sunTotalText);
            CreateStatRow("NutrientRow", statsContainer.transform, nutrientIconSprite, fontAsset, out nutDeltaText, out nutArrowText, out nutTotalText);
            CreateStatRow("StaminaRow", statsContainer.transform, staminaIconSprite, fontAsset, out staminaDeltaText, out staminaArrowText, out staminaTotalText);

            // Continue prompt
            GameObject continueObj = new GameObject("ClickToContinueText", typeof(RectTransform), typeof(TextMeshProUGUI));
            continueObj.transform.SetParent(popupRoot.transform, false);
            RectTransform continueRect = continueObj.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.65f, 0.03f);
            continueRect.anchorMax = new Vector2(0.96f, 0.10f);
            continueRect.offsetMin = Vector2.zero;
            continueRect.offsetMax = Vector2.zero;

            continueHintText = continueObj.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) continueHintText.font = fontAsset;
            continueHintText.text = "Click to continue";
            continueHintText.fontSize = 24;
            continueHintText.color = new Color(0.92f, 0.94f, 0.98f, 0.9f);
            continueHintText.alignment = TextAlignmentOptions.BottomRight;
            continueHintText.raycastTarget = false;

            popupRoot.SetActive(false);
        }

        private void CreateStatRow(string name, Transform parent, Sprite iconSprite, TMP_FontAsset fontAsset, out TextMeshProUGUI deltaText, out TextMeshProUGUI arrowTextOut, out TextMeshProUGUI totalText)
        {
            GameObject rowObj = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowObj.transform.SetParent(parent, false);

            HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 18f;

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(rowObj.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(30f, 30f);

            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            GameObject deltaObj = new GameObject("Delta", typeof(RectTransform), typeof(TextMeshProUGUI));
            deltaObj.transform.SetParent(rowObj.transform, false);
            RectTransform deltaRect = deltaObj.GetComponent<RectTransform>();
            deltaRect.sizeDelta = new Vector2(75f, 32f);

            deltaText = deltaObj.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) deltaText.font = fontAsset;
            deltaText.text = "+0";
            deltaText.fontSize = 26;
            deltaText.fontStyle = FontStyles.Bold;
            deltaText.alignment = TextAlignmentOptions.MidlineRight;
            deltaText.color = Color.white;
            deltaText.raycastTarget = false;

            GameObject arrowObj = new GameObject("Arrow", typeof(RectTransform), typeof(TextMeshProUGUI));
            arrowObj.transform.SetParent(rowObj.transform, false);
            RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(50f, 32f);

            TextMeshProUGUI arrowText = arrowObj.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) arrowText.font = fontAsset;
            arrowText.text = "->";
            arrowText.fontSize = 24;
            arrowText.fontStyle = FontStyles.Bold;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.color = new Color(0.8f, 0.85f, 0.9f);
            arrowText.raycastTarget = false;
            arrowTextOut = arrowText;

            GameObject totalObj = new GameObject("Total", typeof(RectTransform), typeof(TextMeshProUGUI));
            totalObj.transform.SetParent(rowObj.transform, false);
            RectTransform totalRect = totalObj.GetComponent<RectTransform>();
            totalRect.sizeDelta = new Vector2(75f, 32f);

            totalText = totalObj.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) totalText.font = fontAsset;
            totalText.text = "0";
            totalText.fontSize = 26;
            totalText.fontStyle = FontStyles.Bold;
            totalText.alignment = TextAlignmentOptions.MidlineLeft;
            totalText.color = Color.white;
            totalText.raycastTarget = false;
        }

        private void CreateLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject line = new GameObject("BorderLine", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = line.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }

        // Procedural fallback icon generation
        private void EnsureSprites()
        {
            if (waterIconSprite == null) waterIconSprite = CreateDropletSprite();
            if (sunIconSprite == null) sunIconSprite = CreateSunSprite();
            if (nutrientIconSprite == null) nutrientIconSprite = CreateSproutSprite();
            if (staminaIconSprite == null) staminaIconSprite = CreateHeartSprite();

            if (potFallbackSprite == null)
            {
                GameObject potObj = GameObject.Find("Pot_Bunga");
                if (potObj != null)
                {
                    SpriteRenderer sr = potObj.GetComponent<SpriteRenderer>();
                    if (sr != null) potFallbackSprite = sr.sprite;
                }
            }
        }

        private Sprite CreateDropletSprite()
        {
            int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = (x / (float)res) * 2f - 1f;
                    float v = (y / (float)res) * 2f - 1f;

                    float distCircle = Mathf.Sqrt(u * u + (v + 0.25f) * (v + 0.25f));
                    bool inCircle = distCircle <= 0.48f;
                    bool inCone = (v >= -0.25f && v <= 0.72f) && (Mathf.Abs(u) <= (0.72f - v) * 0.5f);

                    if (inCircle || inCone)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite CreateSunSprite()
        {
            int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = (x / (float)res) * 2f - 1f;
                    float v = (y / (float)res) * 2f - 1f;
                    float dist = Mathf.Sqrt(u * u + v * v);

                    bool inCore = dist <= 0.38f;
                    float angle = Mathf.Atan2(v, u);
                    float rayPattern = Mathf.Cos(angle * 8f);
                    bool inRay = (dist >= 0.35f && dist <= 0.75f) && (rayPattern > 0.65f);

                    if (inCore || inRay)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite CreateSproutSprite()
        {
            int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = (x / (float)res) * 2f - 1f;
                    float v = (y / (float)res) * 2f - 1f;

                    bool inStem = (Mathf.Abs(u) <= 0.08f) && (v >= -0.7f && v <= 0.15f);
                    float lu = (u + 0.3f);
                    float lv = (v - 0.2f);
                    bool inLeftLeaf = (lu * lu * 2.5f + lv * lv * 5f) <= 0.25f && (u < 0.05f);

                    float ru = (u - 0.3f);
                    float rv = (v - 0.28f);
                    bool inRightLeaf = (ru * ru * 2.5f + rv * rv * 5f) <= 0.25f && (u > -0.05f);

                    if (inStem || inLeftLeaf || inRightLeaf)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite CreateHeartSprite()
        {
            int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = (x / (float)res) * 2.6f - 1.3f;
                    float v = (y / (float)res) * 2.6f - 1.15f;

                    float a = u * u + v * v - 0.7f;
                    bool inHeart = (a * a * a - u * u * v * v * v) <= 0f;

                    if (inHeart)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
