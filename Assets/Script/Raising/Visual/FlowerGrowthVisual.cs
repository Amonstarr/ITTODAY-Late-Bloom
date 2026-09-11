using System.Collections;
using UnityEngine;
using LateBloom.Jigsaw;

namespace LateBloom.Raising
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlowerGrowthVisual : MonoBehaviour
    {
        [Header("Sprite Renderer")]
        public SpriteRenderer plantRenderer;

        [Header("4 Phase Sprites")]
        [Tooltip("Sprite untuk Fase 1: Bibit (Seed)")]
        public Sprite seedSprite;

        [Tooltip("Sprite untuk Fase 2: Tunas (Sprout)")]
        public Sprite sproutSprite;

        [Tooltip("Sprite untuk Fase 3: Kuncup (Bud)")]
        public Sprite budSprite;

        [Tooltip("Sprite untuk Fase 4: Mekar (Bloom)")]
        public Sprite bloomSprite;

        [Header("Animation Settings")]
        [SerializeField] private bool playGrowthBounce = true;
        [SerializeField] private float bounceDuration = 0.35f;

        private Vector3 originalScale;
        private Coroutine bounceRoutine;

        private void Awake()
        {
            if (plantRenderer == null)
            {
                plantRenderer = GetComponent<SpriteRenderer>();
            }
            originalScale = transform.localScale;
        }

        private void Start()
        {
            SubscribeToEvents();

            if (PlantGrowthManager.Instance != null)
            {
                UpdateVisual(PlantGrowthManager.Instance.currentStage, false);
            }
            else if (PuzzlePhaseManager.Instance != null)
            {
                UpdateVisual(PuzzlePhaseManager.Instance.currentStage, false);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (PlantGrowthManager.Instance != null)
            {
                PlantGrowthManager.Instance.OnStageChanged += HandleStageChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (PlantGrowthManager.Instance != null)
            {
                PlantGrowthManager.Instance.OnStageChanged -= HandleStageChanged;
            }
        }

        private void HandleStageChanged(FlowerGrowthStage newStage)
        {
            UpdateVisual(newStage, playGrowthBounce);
        }

        /// <summary>
        /// Mengganti sprite tanaman sesuai fase aktifnya saat ini.
        /// </summary>
        public void UpdateVisual(FlowerGrowthStage stage, bool animate = true)
        {
            if (plantRenderer == null) return;

            Sprite targetSprite = null;

            switch (stage)
            {
                case FlowerGrowthStage.Seed:
                    targetSprite = seedSprite;
                    break;
                case FlowerGrowthStage.Sprout:
                    targetSprite = sproutSprite;
                    break;
                case FlowerGrowthStage.Bud:
                    targetSprite = budSprite;
                    break;
                case FlowerGrowthStage.Bloom:
                    targetSprite = bloomSprite;
                    break;
            }

            if (targetSprite != null)
            {
                plantRenderer.sprite = targetSprite;
                Debug.Log($"[FlowerGrowthVisual] Tampilan visual bunga diperbarui ke fase: {stage} ({targetSprite.name})");

                if (animate && gameObject.activeInHierarchy)
                {
                    if (bounceRoutine != null) StopCoroutine(bounceRoutine);
                    bounceRoutine = StartCoroutine(DoGrowthBounce());
                }
            }
        }

        private IEnumerator DoGrowthBounce()
        {
            float elapsed = 0f;
            Vector3 peakScale = originalScale * 1.2f;

            // Membesar (squash/stretch)
            while (elapsed < bounceDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (bounceDuration * 0.5f);
                transform.localScale = Vector3.Lerp(originalScale, peakScale, t);
                yield return null;
            }

            elapsed = 0f;
            // Kembali ke ukuran normal dengan sedikit pantulan
            while (elapsed < bounceDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (bounceDuration * 0.5f);
                transform.localScale = Vector3.Lerp(peakScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
            bounceRoutine = null;
        }
    }
}
