using System;
using UnityEngine;

namespace LateBloom.Raising
{
    public class TurnSystem : MonoBehaviour
    {
        public static TurnSystem Instance { get; private set; }

        [Header("Turn / Day Configuration")]
        [Tooltip("Jumlah hari dalam 1 fase (bawaan: 10 hari)")]
        [SerializeField] private int daysPerPhase = 10;

        [Header("Runtime Day Status")]
        [SerializeField] private int currentDayInPhase = 1;

        public event Action<int, int> OnDayChanged; // currentDay, remainingDays
        public event Action OnEvaluationDayReached;

        public int DaysPerPhase => daysPerPhase;
        public int CurrentDayInPhase => currentDayInPhase;
        public int RemainingDays => Mathf.Max(0, daysPerPhase - currentDayInPhase + 1);
        public bool IsEvaluationDay => RemainingDays <= 1;

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
            NotifyDay();
        }

        /// <summary>
        /// Menginisialisasi fase baru dengan jumlah hari tertentu.
        /// </summary>
        public void StartNewPhase(int days)
        {
            daysPerPhase = days;
            currentDayInPhase = 1;
            NotifyDay();
            Debug.Log($"[TurnSystem] Memulai fase baru! Total hari: {daysPerPhase} hari.");
        }

        /// <summary>
        /// Memajukan 1 hari setelah aksi (Jemur, Siram, Pupuk, Minum Teh) diambil.
        /// </summary>
        public void AdvanceDay()
        {
            currentDayInPhase++;
            NotifyDay();

            Debug.Log($"[TurnSystem] Masuk ke Hari {currentDayInPhase}. Sisa hari menuju evaluasi: {RemainingDays}.");

            if (currentDayInPhase > daysPerPhase)
            {
                Debug.Log("[TurnSystem] Waktu fase ini habis! Memulai evaluasi checkpoint...");
                OnEvaluationDayReached?.Invoke();
            }
        }

        private void NotifyDay()
        {
            OnDayChanged?.Invoke(currentDayInPhase, RemainingDays);
        }
    }
}
