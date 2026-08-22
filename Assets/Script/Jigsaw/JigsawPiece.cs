using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LateBloom.Jigsaw
{
    public enum PieceState
    {
        Idle,
        Dragging,
        Snapped
    }

    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class JigsawPiece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Identity")]
        [Tooltip("ID unik kepingan ini (harus sama dengan pieceId di JigsawSlot pasangannya)")]
        public int pieceId;

        [Header("State")]
        public PieceState currentState = PieceState.Idle;

        [Header("Drag Visual Settings")]
        public float dragScaleMultiplier = 1.05f;
        public float idleScale = 1.0f;

        [HideInInspector] public RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private Vector2 previousIdlePosition;
        private Transform originalParent;
        private JigsawManager manager;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();
        }

        public void Initialize(JigsawManager jigsawManager, int id)
        {
            manager = jigsawManager;
            pieceId = id;
            currentState = PieceState.Idle;
            previousIdlePosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;
            transform.SetAsLastSibling(); // Bawa ke layer paling depan
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;

            currentState = PieceState.Dragging;
            canvasGroup.blocksRaycasts = false;
            rectTransform.localScale = Vector3.one * dragScaleMultiplier;

            if (manager != null)
            {
                manager.OnPiecePickup();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;

            if (canvas != null)
            {
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
            else
            {
                rectTransform.anchoredPosition += eventData.delta;
            }

            if (manager != null)
            {
                manager.CheckHoverSlot(this);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;

            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale = Vector3.one * idleScale;

            if (manager != null && manager.TrySnapPiece(this))
            {
                SnapToSlot(manager.GetSlot(pieceId));
            }
            else
            {
                currentState = PieceState.Idle;
                previousIdlePosition = rectTransform.anchoredPosition;
            }
        }

        public void SnapToSlot(JigsawSlot slot)
        {
            if (slot == null) return;

            currentState = PieceState.Snapped;
            canvasGroup.blocksRaycasts = false; // Kunci kepingan agar tidak bisa di-drag lagi
            rectTransform.position = slot.rectTransform.position;
            rectTransform.localScale = Vector3.one * idleScale;
            slot.SetOccupied(true);

            if (manager != null)
            {
                manager.OnPieceSnapped(this);
            }
        }

        public void SetSavedPosition(Vector2 anchoredPos, bool isSnapped, JigsawSlot slot)
        {
            rectTransform.anchoredPosition = anchoredPos;
            if (isSnapped && slot != null)
            {
                SnapToSlot(slot);
            }
            else
            {
                currentState = PieceState.Idle;
                previousIdlePosition = anchoredPos;
            }
        }
    }
}
