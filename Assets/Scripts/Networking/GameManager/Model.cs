using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class Model : MonoBehaviour
    {
        public List<GameObject> blocksList { get; set; }
        public int roomSize = 5;
        public Transform startRoom = null;
        public BlockContainer blocks;

        public Transform destinationRoomUp = null;
        public Transform destinationRoomDown = null;
        public Transform destinationRoomRight = null;
        public Transform destinationRoomLeft = null;
        public Transform destination = null;

        public List<GameObject> starItems;
        public List<StartItemContainer> startItemContainers;
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