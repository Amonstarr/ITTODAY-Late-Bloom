using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LateBloom.Jigsaw
{
    public class PuzzlePieceAwardedUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Root Container/Panel Modal untuk Cutscene Keping Puzzle")]
        public GameObject panelRoot;

        [Tooltip("Teks Judul (Misal: 'Keping Puzzle Didapatkan!')")]
        public TextMeshProUGUI titleText;

        [Tooltip("Teks Subtitle/Detail (Misal: 'Fase: Tumbuh Dikit (2/4)')")]
        public TextMeshProUGUI detailText;

        [Tooltip("Teks Progres Kepingan (Misal: '2 / 4 Keping')")]
        public TextMeshProUGUI progressText;

        [Tooltip("Gambar Pratinjau Kepingan Puzzle yang Didapat")]
        public Image piecePreviewImage;

        [Tooltip("Daftar Sprite Kepingan Puzzle (Index 0-3 untuk Keping 1-4)")]
        public Sprite[] pieceSprites;

        [Tooltip("Tombol Tutup / Lanjutkan")]
        public Button continueButton;

        [Header("Animation Settings")]
        public CanvasGroup canvasGroup;
        public float fadeDuration = 0.3f;

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HidePanel);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Menampilkan panel cutscene saat 1 keping puzzle didapatkan.
        /// </summary>
        public void ShowPieceAwardedPanel(int pieceNumber, int totalPieces, string stageName)
        {
            if (panelRoot == null) return;

            if (titleText != null)
                titleText.text = "Keping Puzzle Didapatkan!";

            if (detailText != null)
                detailText.text = $"Fase Pertumbuhan: {stageName}";

            if (progressText != null)
                progressText.text = $"Kepingan: {pieceNumber} / {totalPieces}";

            if (piecePreviewImage != null && pieceSprites != null && pieceSprites.Length >= pieceNumber && pieceNumber > 0)
            {
                piecePreviewImage.sprite = pieceSprites[pieceNumber - 1];
                piecePreviewImage.gameObject.SetActive(pieceSprites[pieceNumber - 1] != null);
            }

            panelRoot.SetActive(true);

            if (canvasGroup != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f));
            }
        }

        public void HidePanel()
        {
            if (panelRoot == null) return;

            if (canvasGroup != null && gameObject.activeInHierarchy)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0f, () => panelRoot.SetActive(false)));
            }
            else
            {
                panelRoot.SetActive(false);
            }
        }

        private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, System.Action onComplete = null)
        {
            canvasGroup.alpha = startAlpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            onComplete?.Invoke();
        }
    }
}
