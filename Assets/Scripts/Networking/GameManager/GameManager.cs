using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class GameManager : NetworkBehaviour
    {
        Model model;

        #region Rooms
        [Command] public void CmdSetupRooms() => SetupRooms();
        public void SetupRooms()
        {
            SpawnRooms(RandomRooms());
        }

        public List<GameObject> RandomRooms()
        {
            int index = 1;
            List<GameObject> blocks = new List<GameObject>();
            blocks.Add(model.startRoom.gameObject);
            blocks.Add(model.blocks.left.Random());
            while (true)
            {
                if (index > model.roomSize - 1) break;
                GameObject go = null;
                string name = blocks[index].name;
                switch (name.Split(',')[2])
                {
                    case "left":
                        go = model.blocks.right.Random();
                        break;
                    case "right":
                        go = model.blocks.left.Random();
                        break;
                    case "up":
                        go = model.blocks.down.Random();
                        break;
                    case "down":
                        go = model.blocks.up.Random();
                        break;
                }
                blocks.Add(go);
                index++;
            }

            GameObject go1 = null;
            string name1 = blocks[index].name;
            switch (name1.Split(',')[2])
            {
                case "left":
                    go1 = model.destinationRoomRight.gameObject;
                    break;
                case "right":
                    go1 = model.destinationRoomLeft.gameObject;
                    break;
                case "up":
                    go1 = model.destinationRoomDown.gameObject;
                    break;
                case "down":
                    go1 = model.destinationRoomUp.gameObject;
                    break;
            }
            blocks.Add(go1);

            return blocks;
        }

        public void SpawnRooms(List<GameObject> blocks)
        {
            model.blocksList = blocks;
            for (int i = 1; i < model.blocksList.Count; i++)
            {
                Vector2 pos = (i == 1) ? model.blocksList[i - 1].GetComponent<RoomBlockData>().endPoint.position + new Vector3(100, 100, 0) : model.blocksList[i - 1].GetComponent<MapObjectData>().endpoint.position + new Vector3(100, 100, 0);
                GameObject go = Instantiate(blocks[i], pos, Quaternion.identity);
                NetworkServer.Spawn(go);
                model.blocksList[i] = go;
            }
        }
        #endregion

        #region StartItems
        [Command] public void CmdSetupStartItems() => SetupStartItems();
        [ClientRpc] public void RpcSpawnStartItems(List<int> _startItems) => SpawnStartItems(_startItems);
        public void SetupStartItems()
        {
            RpcSpawnStartItems(RandomStartItem());
        }
        public List<int> RandomStartItem()
        {
            int length = 3;
            return model.startItemContainers.RandomSeedInt(length);
        }
        public void SpawnStartItems(List<int> _startItems)
        {
            for (int i = 0; i < model.starItems.Count; i++)
            {
                int rnd = _startItems[i];
                var item = model.startItemContainers[rnd];
                model.starItems[i].GetComponent<SpriteRenderer>().sprite = item.image;
                model.starItems[i].name = item.name;
            }
        }
        #endregion

        #region NativeAPIs
        private void Awake()
        {
            model = GetComponent<Model>();
            model.startRoom = FindObjectOfType<RoomBlockData>().transform;
            model.starItems = GameObject.FindGameObjectsWithTag("StartItem").ToList();
            Debug.Log(model.starItems.Count);
        }

        private void Start()
        {
            GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
            CmdSetupRooms();
            CmdSetupStartItems();
        }
        #endregion
    }
}