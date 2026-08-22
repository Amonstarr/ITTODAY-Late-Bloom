using UnityEngine;

namespace LateBloom.Jigsaw
{
    public class JigsawAudioManager : MonoBehaviour
    {
        public static JigsawAudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource bgmSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip pickupSFX;
        [SerializeField] private AudioClip snapSFX;
        [SerializeField] private AudioClip puzzleCompleteSFX;
        [SerializeField] private AudioClip puzzleBGM;

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
            PlayBGM();
        }

        public void PlayBGM()
        {
            if (bgmSource != null && puzzleBGM != null)
            {
                bgmSource.clip = puzzleBGM;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void PlayPickupSFX()
        {
            PlaySFX(pickupSFX);
        }

        public void PlaySnapSFX()
        {
            PlaySFX(snapSFX);
        }

        public void PlayCompleteSFX()
        {
            PlaySFX(puzzleCompleteSFX);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}
