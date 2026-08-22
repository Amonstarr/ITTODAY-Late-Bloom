using UnityEngine;

namespace LateBloom.Jigsaw
{
    public class FlowerGrowthController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PuzzlePhaseManager phaseManager;

        [Header("Testing Shortcuts (Unity Editor)")]
        [Tooltip("Tekan tombol keyboard ini saat Play Mode untuk maju ke fase berikutnya")]
        [SerializeField] private KeyCode advanceStageKey = KeyCode.N;
        [Tooltip("Tekan tombol keyboard ini untuk reset progres ke awal")]
        [SerializeField] private KeyCode resetProgressKey = KeyCode.R;

        private void Awake()
        {
            if (phaseManager == null)
            {
                phaseManager = FindObjectOfType<PuzzlePhaseManager>();
            }
        }

        private void Update()
        {
            // Shortcut untuk testing cepat di Unity Editor
            if (Input.GetKeyDown(advanceStageKey))
            {
                AdvanceGrowth();
            }

            if (Input.GetKeyDown(resetProgressKey))
            {
                ResetGrowth();
            }
        }

        /// <summary>
        /// Panggil method ini dari aksi gameplay merawat tanaman (menyiram / memberi sinar matahari).
        /// </summary>
        public void AdvanceGrowth()
        {
            if (phaseManager != null)
            {
                phaseManager.AdvanceGrowthStage();
            }
        }

        public void ResetGrowth()
        {
            if (phaseManager != null)
            {
                phaseManager.ResetProgress();
            }
        }
    }
}
