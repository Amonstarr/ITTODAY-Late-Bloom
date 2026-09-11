using System;
using UnityEngine;

namespace LateBloom.Raising
{
    public class PlayerEnergyManager : MonoBehaviour
    {
        public static PlayerEnergyManager Instance { get; private set; }

        [Header("Energy Configuration")]
        [SerializeField] private int maxEnergy = 100;
        [SerializeField] private int currentEnergy = 100;

        [Header("Energy Costs & Rest")]
        [Tooltip("Biaya energi untuk 1x aksi perawatan (Jemur / Siram / Pupuk)")]
        [SerializeField] private int careActionCost = 20;

        [Tooltip("Jumlah pemulihan energi saat mengambil aksi Minum Teh (Rest)")]
        [SerializeField] private int teaRestRestore = 40;

        public event Action<int, int> OnEnergyChanged;
        public event Action<int> OnFailureRateUpdated;
        public event Action<string> OnActionFailed;

        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => maxEnergy;
        public int CareActionCost => careActionCost;
        public int TeaRestRestore => teaRestRestore;

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
            NotifyState();
        }


        public int GetFailureRate()
        {
            float percent = (float)currentEnergy / maxEnergy;
            int baseRate = 0;

            if (percent >= 0.5f)
            {
                baseRate = 0;
            }
            else if (percent >= 0.3f)
            {
                float t = (0.5f - percent) / 0.2f;
                baseRate = Mathf.RoundToInt(Mathf.Lerp(0, 20, t));
            }
            else if (percent >= 0.1f)
            {
                float t = (0.3f - percent) / 0.2f;
                baseRate = Mathf.RoundToInt(Mathf.Lerp(20, 55, t));
            }
            else
            {
                float t = (0.1f - percent) / 0.1f;
                baseRate = Mathf.RoundToInt(Mathf.Lerp(55, 85, t));
            }

            int moodMod = (PlayerMoodManager.Instance != null) ? PlayerMoodManager.Instance.GetFailureRateModifier() : 0;
            return Mathf.Clamp(baseRate + moodMod, 0, 95);
        }

        /// <summary>
        /// Mengecek apakah aksi perawatan berhasil atau gagal karena kelelahan (Failure Rate roll).
        /// </summary>
        public bool TryPerformCareAction(out bool isFailed)
        {
            int failureRate = GetFailureRate();
            int roll = UnityEngine.Random.Range(0, 100);

            isFailed = (roll < failureRate);

            // Energi tetap berkurang terlepas dari berhasil atau gagal
            currentEnergy = Mathf.Clamp(currentEnergy - careActionCost, 0, maxEnergy);
            NotifyState();

            if (isFailed)
            {
                OnActionFailed?.Invoke($"MC terlalu lelah! (Peluang gagal tadi {failureRate}%). Aksi perawatan terganggu.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Aksi Minum Teh (Rest) untuk memulihkan energi pemain.
        /// </summary>
        public void RestDrinkTea()
        {
            currentEnergy = Mathf.Clamp(currentEnergy + teaRestRestore, 0, maxEnergy);
            NotifyState();
            Debug.Log($"[PlayerEnergyManager] MC beristirahat sambil minum teh. Energi pulih menjadi {currentEnergy}/{maxEnergy}.");
        }

        public void SetEnergy(int amount)
        {
            currentEnergy = Mathf.Clamp(amount, 0, maxEnergy);
            NotifyState();
        }

        private void NotifyState()
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            OnFailureRateUpdated?.Invoke(GetFailureRate());
        }
    }
}
