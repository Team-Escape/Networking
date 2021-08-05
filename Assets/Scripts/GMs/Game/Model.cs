using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Photon.Pun.Escape.GM.Game
{
    public class Model : MonoBehaviour
    {
        public List<GameObject> currentGameBlocks { get; set; }
        public int roomSize = 5;

        #region Room Prefabs
        public BlockContainer blockContainer;
        public Transform destinationRoomUp = null;
        public Transform destinationRoomDown = null;
        public Transform destinationRoomRight = null;
        public Transform destinationRoomLeft = null;
        public Transform destination = null;
        #endregion

        public List<GameObject> starItems;
        public List<StartItemContainer> startItemContainers;

        public Tilemap startRoomTilemap = null;
        public Vector3Int[] wallDestoryInEscape = null;
        public Vector3Int[] wallDestoryInHunter = null;
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