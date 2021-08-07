using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using PlayerSpace.Gameplayer;

namespace Photon.Pun.Escape.GM.Game
{
    public enum GameState { Loading, Setting, Gaming, Ending }
    public class Model : MonoBehaviour
    {
        public string gameScene = "";

        #region Transform
        [Header("Trasnform")]
        public Transform hunterSpawn;
        public Transform escaperSpawn;
        #endregion

        #region Gameplayers
        public List<Gameplayer> gameplayers { get; set; }
        public Gameplayer hunter { get; set; }
        public List<Gameplayer> hunters { get; set; }
        public List<Gameplayer> escapers { get; set; }
        public List<Gameplayer> gotStartItemPlayers { get; set; }
        public List<Gameplayer> goalPlayers { get; set; }
        public List<Gameplayer> caughtPlayers { get; set; }
        #endregion

        #region Room Prefabs
        [Header("Room")]
        public int roomSize = 5;
        public List<GameObject> currentGameBlocks { get; set; }
        public BlockContainer blockContainer;
        public Transform destinationRoomUp = null;
        public Transform destinationRoomDown = null;
        public Transform destinationRoomRight = null;
        public Transform destinationRoomLeft = null;
        public Transform destination = null;
        #endregion

        #region StartItems
        [Header("StartItem")]
        public List<GameObject> starItems;
        public List<StartItemContainer> startItemContainers;
        #endregion

        #region Tilemaps
        [Header("Tilemap")]
        public Tilemap startRoomTilemap = null;
        public Vector3Int[] wallDestoryInEscape = null;
        public Vector3Int[] wallDestoryInHunter = null;
        #endregion
    }

    [System.Serializable]
    public class BlockContainer
    {
        public List<GameObject> left = new List<GameObject>();
        public List<GameObject> right = new List<GameObject>();
        public List<GameObject> up = new List<GameObject>();
        public List<GameObject> down = new List<GameObject>();
    }

    [System.Serializable]
    public class StartItemContainer
    {
        public Sprite image;
        public string name;
    }
}