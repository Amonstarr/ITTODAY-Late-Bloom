using UnityEngine;
using UnityEngine.SceneManagement;

namespace LateBloom.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [Tooltip("Nama Scene In-Game / Tempat Penanaman Bunga yang akan dibuka saat tombol Play diklik")]
        public string targetInGameSceneName = "RaisingScene";

        [Header("Transition Settings (Opsional)")]
        [Tooltip("Aktifkan true jika ingin memastikan waktu game dalam posisi normal (timeScale = 1) saat masuk ke in-game")]
        public bool resetTimeScaleOnPlay = true;

        /// <summary>
        /// Panggil fungsi ini dari OnClick() pada Tombol Play di Unity Inspector.
        /// </summary>
        public void PlayGame()
        {
            if (resetTimeScaleOnPlay)
            {
                Time.timeScale = 1f;
            }

            if (!string.IsNullOrEmpty(targetInGameSceneName))
            {
                Debug.Log($"[MainMenuController] Membuka Scene In-Game: '{targetInGameSceneName}'");
                SceneManager.LoadScene(targetInGameSceneName);
            }
            else
            {
                Debug.LogWarning("[MainMenuController] 'targetInGameSceneName' masih kosong di Inspector! Isi dengan nama Scene In-Game kamu.");
            }
        }

        /// <summary>
        /// Panggil fungsi ini jika ingin memilih scene tujuan tertentu secara spesifik lewat parameter string.
        /// </summary>
        public void PlayGameCustomScene(string sceneName)
        {
            if (resetTimeScaleOnPlay)
            {
                Time.timeScale = 1f;
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"[MainMenuController] Membuka Scene Custom: '{sceneName}'");
                SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// Panggil fungsi ini dari OnClick() pada Tombol Keluar (Quit Button).
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[MainMenuController] Keluar dari Permainan.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
