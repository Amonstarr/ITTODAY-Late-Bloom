using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace LateBloom.Jigsaw
{
    public class MemoryStoryCutsceneController : MonoBehaviour
    {
        [Header("References")]
        public JigsawManager jigsawManager;

        [Header("Cutscene Panel UI")]
        public GameObject memoryPanelRoot;
        public CanvasGroup memoryCanvasGroup;

        [Tooltip("UI Image component di Canvas/Hierarchy yang menampilkan gambar cerita kenangan")]
        public Image storyImage;

        [Tooltip("Sprite foto kenangan (bisa di-drag langsung dari Project Window/Assets). Jika diisi, sprite ini yang dipakai; jika kosong, otomatis memakai foto dari JigsawManager.")]
        public Sprite customStorySprite;

        public TextMeshProUGUI titleText;
        [TextArea(3, 10)]
        public string storyTextContent = "Kenangan manis terpancar indah di ingatan...";
        public TextMeshProUGUI storyText;
        public Button closeOrNextButton;

        [Header("Scene Transition (Opsional)")]
        public bool loadSceneAfterCutscene = true;
        public string flashbackSceneName = "Flashback_Phase1";
        public float delayBeforeSceneLoad = 2f;

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (jigsawManager == null)
            {
#if UNITY_2023_1_OR_NEWER
                jigsawManager = FindFirstObjectByType<JigsawManager>();
#else
                jigsawManager = FindObjectOfType<JigsawManager>();
#endif
            }

            if (closeOrNextButton != null)
            {
                closeOrNextButton.onClick.AddListener(OnCloseOrNextClicked);
            }

            if (memoryPanelRoot != null)
            {
                memoryPanelRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (jigsawManager != null)
            {
                jigsawManager.onPuzzleCompleted.AddListener(PlayMemoryStoryCutscene);
            }
        }

        private void OnDisable()
        {
            if (jigsawManager != null)
            {
                jigsawManager.onPuzzleCompleted.RemoveListener(PlayMemoryStoryCutscene);
            }
        }

        /// <summary>
        /// Memutar cutscene cerita ingatan setelah 4 keping puzzle berhasil disusun.
        /// </summary>
        public void PlayMemoryStoryCutscene()
        {
            Debug.Log("[MemoryStoryCutsceneController] Memutar Cutscene Cerita Ingatan!");

            if (memoryPanelRoot != null)
            {
                if (titleText != null)
                    titleText.text = "Kenangan Terbuka";

                if (storyText != null)
                    storyText.text = storyTextContent;

                if (storyImage != null)
                {
                    if (customStorySprite != null)
                    {
                        storyImage.sprite = customStorySprite;
                    }
                    else if (jigsawManager != null && jigsawManager.puzzlePhotoSprite != null)
                    {
                        storyImage.sprite = jigsawManager.puzzlePhotoSprite;
                    }
                }

                memoryPanelRoot.SetActive(true);

                if (memoryCanvasGroup != null)
                {
                    if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                    fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f));
                }
            }
            else if (loadSceneAfterCutscene && !string.IsNullOrEmpty(flashbackSceneName))
            {
                StartCoroutine(LoadFlashbackSceneRoutine());
            }
        }

        public void OnCloseOrNextClicked()
        {
            if (loadSceneAfterCutscene && !string.IsNullOrEmpty(flashbackSceneName))
            {
                StartCoroutine(LoadFlashbackSceneRoutine());
            }
            else
            {
                HidePanel();
            }
        }

        public void HidePanel()
        {
            if (memoryPanelRoot == null) return;

            if (memoryCanvasGroup != null && gameObject.activeInHierarchy)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0f, () => memoryPanelRoot.SetActive(false)));
            }
            else
            {
                memoryPanelRoot.SetActive(false);
            }
        }

        private IEnumerator LoadFlashbackSceneRoutine()
        {
            yield return new WaitForSeconds(delayBeforeSceneLoad);
            if (!string.IsNullOrEmpty(flashbackSceneName))
            {
                SceneManager.LoadScene(flashbackSceneName);
            }
        }

        private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, System.Action onComplete = null)
        {
            memoryCanvasGroup.alpha = startAlpha;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                memoryCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            memoryCanvasGroup.alpha = endAlpha;
            onComplete?.Invoke();
        }
    }
}
