#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LateBloom.Jigsaw
{
    [CustomEditor(typeof(JigsawManager))]
    public class JigsawManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            JigsawManager mgr = (JigsawManager)target;
            serializedObject.Update();

            // ── MANUAL SETUP TOGGLE ──────────────
            EditorGUILayout.Space(4);
            SerializedProperty useManual = serializedObject.FindProperty("useManualSetup");
            EditorGUILayout.PropertyField(useManual);

            if (mgr.useManualSetup)
            {
                EditorGUILayout.HelpBox(
                    "MODE MANUAL AKTIF\n" +
                    "• Auto-generate board & pieces DINONAKTIFKAN.\n" +
                    "• Atur frame, background, slot, dan piece langsung di scene.\n" +
                    "• Piece dengan 'Use Generated Shape = true' tetap akan di-cut dari foto sumber.",
                    MessageType.Info);

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("── Manual Visual References ──", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("manualBoardFrame"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("manualBackground"));
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "MODE AUTO-GENERATE AKTIF\n" +
                    "• Board dan kepingan dibuat otomatis dari foto sumber.\n" +
                    "• Pastikan 'Puzzle Photo Texture' diisi dan gambar bertanda Read/Write Enabled.",
                    MessageType.Info);

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("── Photo & Grid (Auto) ──", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzlePhotoSprite"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzlePhotoTexture"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shapeStyle"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridRows"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridCols"));
            }

            // ── SHARED FIELDS ────────────────────
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Identity & Metadata ──", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleMetadata"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("instanceId"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Containers ──", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleBoardContainer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("piecesContainer"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Scatter Areas ──", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scatterAreaLeft"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scatterAreaRight"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Snap ──", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snapRadius"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── UI & Events ──", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("progressText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPuzzleCompleted"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPieceSnappedEvent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flashbackSceneName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("delayBeforeFlashback"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Piece & Slot Lists ──", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slots"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pieces"), true);

            serializedObject.ApplyModifiedProperties();

            // ── BUTTONS ──────────────────────────
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("── Editor Actions ──", EditorStyles.boldLabel);

            if (mgr.useManualSetup)
            {
                if (GUILayout.Button("✔ Validate Manual Setup", GUILayout.Height(32)))
                    mgr.ValidateManualSetup();

                if (GUILayout.Button("🔄 Fetch Scene Slots & Pieces", GUILayout.Height(32)))
                    mgr.FetchSceneSlotsAndPieces();

                EditorGUILayout.HelpBox(
                    "Jika kamu juga ingin memotong foto untuk piece yang 'Use Generated Shape = true', " +
                    "klik 'Cut Photo & Assign to Pieces' di bawah.",
                    MessageType.None);

                if (GUILayout.Button("✂ Cut Photo & Assign to Pieces (Generated Shape Only)", GUILayout.Height(32)))
                    mgr.SliceAndAssignPhotoToPieces();
            }
            else
            {
                if (GUILayout.Button("🔄 Fetch Scene Slots & Pieces", GUILayout.Height(32)))
                    mgr.FetchSceneSlotsAndPieces();

                if (GUILayout.Button("⚙ Generate Board Slots (Auto)", GUILayout.Height(32)))
                    mgr.GenerateBoardSlots();

                if (GUILayout.Button("🧩 Generate Pieces (Auto)", GUILayout.Height(32)))
                    mgr.GeneratePieces();

                if (GUILayout.Button("✂ Cut Photo & Assign to Pieces", GUILayout.Height(32)))
                    mgr.SliceAndAssignPhotoToPieces();

                EditorGUILayout.Space(4);
                GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
                if (GUILayout.Button("⚡ ALL-IN-ONE: Generate Board + Pieces + Cut Photo", GUILayout.Height(40)))
                    mgr.GenerateBoardAndPieces();
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("── Save Data ──", EditorStyles.boldLabel);
            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑 Clear Puzzle Save Data", GUILayout.Height(32)))
            {
                mgr.ClearSaveData();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
