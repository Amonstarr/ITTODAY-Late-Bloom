using UnityEngine;
using UnityEngine.UI;

namespace LateBloom.Jigsaw
{
    [RequireComponent(typeof(RectTransform))]
    public class JigsawSlot : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [Tooltip("ID kepingan yang cocok untuk slot ini (0, 1, 2, ...)")]
        public int pieceId;

        [Header("Visual Feedback (Optional)")]
        [Tooltip("Image border/ghost highlight saat keping didekatkan")]
        public Image ghostHighlightImage;
        public Color normalColor = new Color(1f, 1f, 1f, 0.2f);
        public Color hoverColor = new Color(0.2f, 1f, 0.4f, 0.5f);

        [HideInInspector] public RectTransform rectTransform;
        public bool isOccupied { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            SetHover(false);
        }

        public void SetHover(bool hovering)
        {
            if (ghostHighlightImage != null && !isOccupied)
            {
                ghostHighlightImage.color = hovering ? hoverColor : normalColor;
            }
        }

        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
            if (ghostHighlightImage != null)
            {
                ghostHighlightImage.enabled = !occupied;
            }
        }
    }
}
