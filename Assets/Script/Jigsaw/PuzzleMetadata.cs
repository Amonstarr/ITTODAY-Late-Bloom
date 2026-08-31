using System;
using System.Collections.Generic;
using UnityEngine;

namespace LateBloom.Jigsaw
{
    public enum PieceShapeStyle
    {
        SquareGrid,           // Potongan grid kotak lurus biasa
        JigsawInterlocking    // Potongan klasik Jigsaw (Benjolan & Lekukan Interlocking)
    }

    [Serializable]
    public class PieceSaveState
    {
        public int id;
        public bool isSnapped;
        public float posX;
        public float posY;
    }

    [Serializable]
    public class PuzzleSaveData
    {
        public string puzzleId = "sunflower_phase1";
        public bool isCompleted = false;
        public List<PieceSaveState> pieceStates = new List<PieceSaveState>();
    }

    [CreateAssetMenu(fileName = "NewPuzzleMetadata", menuName = "Late Bloom/Puzzle Metadata")]
    public class PuzzleMetadata : ScriptableObject
    {
        [Header("Puzzle Identity")]
        public string puzzleId = "sunflower_phase1";
        public string instanceId = "instance_01";
        public string flowerName = "Bunga Matahari";

        [Header("Visual & Texture")]
        public Texture2D puzzlePhotoTexture;
        public Sprite backgroundSprite;
        public Sprite boardFrameSprite;

        [Header("Phase Configuration")]
        public int seedStagePieces = 4;
        public int budStagePieces = 4;
        public int bloomStagePieces = 8;

        [Header("Saved State (Runtime Data)")]
        public FlowerGrowthStage currentStage = FlowerGrowthStage.Seed;
        public bool isUnlocked = false;
        public bool isCompleted = false;
        public List<PieceSaveState> pieceStates = new List<PieceSaveState>();

        /// <summary>
        /// Menghasilkan Key unik untuk penyimpanan berdasarkan Puzzle ID & Instance ID
        /// (Memungkinkan menanam jenis bunga yang sama berkali-kali tanpa menimpa data)
        /// </summary>
        public string GetFullSaveKey()
        {
            return $"{puzzleId}_{instanceId}";
        }

        public void SaveToDisk()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("PuzzleMeta_" + GetFullSaveKey(), json);
            PlayerPrefs.Save();
        }

        public bool LoadFromDisk()
        {
            string key = "PuzzleMeta_" + GetFullSaveKey();
            if (!PlayerPrefs.HasKey(key)) return false;

            string json = PlayerPrefs.GetString(key);
            JsonUtility.FromJsonOverwrite(json, this);
            return true;
        }

        public void ResetMetadata()
        {
            currentStage = FlowerGrowthStage.Seed;
            isUnlocked = false;
            isCompleted = false;
            pieceStates.Clear();
            PlayerPrefs.DeleteKey("PuzzleMeta_" + GetFullSaveKey());
        }
    }
}
