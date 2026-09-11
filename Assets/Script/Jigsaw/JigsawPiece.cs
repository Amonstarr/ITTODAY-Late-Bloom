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

    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
    public class JigsawPiece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // ─────────────────────────────────────────
        //  IDENTITY
        // ─────────────────────────────────────────
        [Header("Identity")]
        [Tooltip("ID unik kepingan ini (harus sama dengan pieceId di JigsawSlot pasangannya). " +
                 "Diisi otomatis saat FetchSceneSlotsAndPieces() dipanggil.")]
        public int pieceId;

        // ─────────────────────────────────────────
        //  MANUAL SETUP OPTIONS
        // ─────────────────────────────────────────
        [Header("Manual Setup Options")]
        [Tooltip("TRUE  → Gambar kepingan ini akan di-cut otomatis dari foto sumber (useManualSetup di JigsawManager).\n" +
                 "FALSE → Sprite sudah di-assign manual di komponen Image ini; tidak akan di-overwrite oleh generator.")]
        public bool useGeneratedShape = false;

        // ─────────────────────────────────────────
        //  STATE
        // ─────────────────────────────────────────
        [Header("State")]
        public PieceState currentState = PieceState.Idle;

        // ─────────────────────────────────────────
        //  DRAG VISUAL
        // ─────────────────────────────────────────
        [Header("Drag Visual Settings")]
        public float dragScaleMultiplier = 1.05f;
        public float idleScale = 1.0f;

        // ─────────────────────────────────────────
        //  PRIVATE REFS
        // ─────────────────────────────────────────
        [HideInInspector] public RectTransform rectTransform;
        private Canvas       canvas;
        private CanvasGroup  canvasGroup;
        private Image        pieceImage;
        private JigsawManager manager;

        [HideInInspector] public Vector2 initialAnchoredPosition;
        [HideInInspector] public bool hasInitialPositionSaved = false;

        // ══════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ══════════════════════════════════════════
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup   = GetComponent<CanvasGroup>();
            pieceImage    = GetComponent<Image>();
            canvas        = GetComponentInParent<Canvas>();

            if (rectTransform != null && !hasInitialPositionSaved)
            {
                initialAnchoredPosition = rectTransform.anchoredPosition;
                hasInitialPositionSaved = true;
            }

            // Pastikan Raycast Target aktif agar mouse/touch bisa menarik kepingan ini
            if (pieceImage != null)
                pieceImage.raycastTarget = true;
        }

        public void ResetToInitialPosition()
        {
            currentState = PieceState.Idle;
            if (rectTransform != null && hasInitialPositionSaved)
            {
                rectTransform.anchoredPosition = initialAnchoredPosition;
            }
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            if (pieceImage  != null) pieceImage.raycastTarget   = true;
        }

        // ══════════════════════════════════════════
        //  INITIALIZE
        // ══════════════════════════════════════════
        public void Initialize(JigsawManager jigsawManager, int id)
        {
            manager = jigsawManager;
            pieceId = id;

            if (currentState != PieceState.Snapped)
            {
                currentState = PieceState.Idle;
                if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            }

            if (pieceImage == null) pieceImage = GetComponent<Image>();
            if (pieceImage != null) pieceImage.raycastTarget = (currentState != PieceState.Snapped);
        }

        private Vector2 dragOffset;

        // ══════════════════════════════════════════
        //  DRAG & DROP HANDLERS
        // ══════════════════════════════════════════
        public void OnPointerDown(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;

            if (manager != null && manager.piecesContainer != null)
            {
                manager.piecesContainer.SetAsLastSibling();
            }
            transform.SetAsLastSibling();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;

            currentState = PieceState.Dragging;
            rectTransform.localScale = Vector3.one * dragScaleMultiplier;

            if (manager != null && manager.piecesContainer != null)
            {
                manager.piecesContainer.SetAsLastSibling();
            }
            transform.SetAsLastSibling();

            RectTransform parentRect = rectTransform.parent as RectTransform;
            if (parentRect != null)
            {
                Canvas parentCanvas = canvas != null ? canvas : GetComponentInParent<Canvas>();
                Camera cam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? parentCanvas.worldCamera : null;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out Vector2 localPointer))
                {
                    dragOffset = rectTransform.anchoredPosition - localPointer;
                }
                else
                {
                    dragOffset = Vector2.zero;
                }
            }

            manager?.OnPiecePickup();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;

            RectTransform parentRect = rectTransform.parent as RectTransform;
            if (parentRect != null)
            {
                Canvas parentCanvas = canvas != null ? canvas : GetComponentInParent<Canvas>();
                Camera cam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? parentCanvas.worldCamera : null;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out Vector2 localPointer))
                {
                    rectTransform.anchoredPosition = localPointer + dragOffset;
                }
            }

            manager?.CheckHoverSlot(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (currentState == PieceState.Snapped) return;

            rectTransform.localScale = Vector3.one * idleScale;

            if (manager != null && manager.TrySnapPiece(this))
                SnapToSlot(manager.GetSlot(pieceId));
            else
                currentState = PieceState.Idle;
        }

        // ══════════════════════════════════════════
        //  SNAP
        // ══════════════════════════════════════════
        public void SnapToSlot(JigsawSlot slot, bool triggerEvents = true)
        {
            if (slot == null) return;

            currentState = PieceState.Snapped;

            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            if (pieceImage  != null) pieceImage.raycastTarget   = false;

            rectTransform.position   = slot.rectTransform.position;
            rectTransform.localScale = Vector3.one * idleScale;
            slot.SetOccupied(true);

            if (triggerEvents)
            {
                manager?.OnPieceSnapped(this);
            }
        }

        // ══════════════════════════════════════════
        //  SAVE / LOAD POSITION
        // ══════════════════════════════════════════
        public void SetSavedPosition(Vector2 anchoredPos, bool isSnapped, JigsawSlot slot)
        {
            rectTransform.anchoredPosition = anchoredPos;

            if (isSnapped && slot != null)
            {
                SnapToSlot(slot, false);
            }
            else
            {
                currentState = PieceState.Idle;
                if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
                if (pieceImage  != null) pieceImage.raycastTarget   = true;
            }
        }
    }
}
