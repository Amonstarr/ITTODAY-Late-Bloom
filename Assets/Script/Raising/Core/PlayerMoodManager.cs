using System;
using UnityEngine;

namespace LateBloom.Raising
{
    public enum PlayerMood
    {
        Grief,   // Duka / Terpuruk: Point -20%, Failure +15%
        Calm,    // Tenang / Netral: Standard
        Devoted  // Hangat / Penuh Kasih: Point +25%, Failure -10%
    }

    public class PlayerMoodManager : MonoBehaviour
    {
        public static PlayerMoodManager Instance { get; private set; }

        [Header("Mood Status")]
        [SerializeField] private PlayerMood currentMood = PlayerMood.Calm;

        public event Action<PlayerMood> OnMoodChanged;

        public PlayerMood CurrentMood => currentMood;

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

        private void Start()
        {
            OnMoodChanged?.Invoke(currentMood);
        }

        public float GetPointMultiplier()
        {
            switch (currentMood)
            {
                case PlayerMood.Grief:
                    return 0.8f;
                case PlayerMood.Devoted:
                    return 1.25f;
                case PlayerMood.Calm:
                default:
                    return 1.0f;
            }
        }

        public int GetFailureRateModifier()
        {
            switch (currentMood)
            {
                case PlayerMood.Grief:
                    return 15;
                case PlayerMood.Devoted:
                    return -10;
                case PlayerMood.Calm:
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Aksi memutar radio tua kenangan istri menaikkan suasana hati MC.
        /// </summary>
        public bool BoostMoodFromRadio()
        {
            if (currentMood == PlayerMood.Grief)
            {
                currentMood = PlayerMood.Calm;
                OnMoodChanged?.Invoke(currentMood);
                Debug.Log("[PlayerMoodManager] Musik radio menenangkan hati MC. Mood sekarang: Tenang (Calm).");
                return true;
            }
            else if (currentMood == PlayerMood.Calm)
            {
                currentMood = PlayerMood.Devoted;
                OnMoodChanged?.Invoke(currentMood);
                Debug.Log("[PlayerMoodManager] Kenangan indah membuat MC merasa dekat dengan mendiang istri. Mood sekarang: Hangat (Devoted)!");
                return true;
            }

            Debug.Log("[PlayerMoodManager] Mood MC sudah berada di puncaknya (Hangat).");
            return false;
        }

        public void SetMood(PlayerMood mood)
        {
            currentMood = mood;
            OnMoodChanged?.Invoke(currentMood);
        }
    }
}
