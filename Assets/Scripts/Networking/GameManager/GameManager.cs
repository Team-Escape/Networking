using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror.EscapeGame.GameplayerSpace;

namespace Mirror.EscapeGame
{
    public class GameManager : NetworkBehaviour
    {
        Model model;
        StartRoomData startRoomData;
        int gotStartItemPlayers = 0;

        [Command]
        public void CmdHunterGameInit()
        {
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                NetworkConnection conn = room.GetHunterGameplayer().connectionToClient;
                RpcHunterGameInit(conn);
            }
        }

        [TargetRpc]
        public void RpcHunterGameInit(NetworkConnection conn)
        {
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                Gameplayer p = room.GetHunterGameplayer();
                p.hunterGameSetup.Generator(p.input, CmdGameStart);
            }
        }

        public void GameStart() => CmdGameStart();
        [Command] public void CmdGameStart() => RpcGameStart();
        [ClientRpc]
        public void RpcGameStart()
        {
            Debug.Log("qopfjwopqfjpo");
            DesroyDoorsTile();
        }

        public void DesroyDoorsTile()
        {
            StartCoroutine(OpenEscaperRoomsDoor());
            StartCoroutine(OpenHunterRoomsDoor());
        }
        public IEnumerator OpenEscaperRoomsDoor()
        {
            foreach (Vector3Int pos in model.wallDestoryInEscape)
            {
                yield return new WaitForSeconds(0.1f);
                model.startRoomTilemap.SetTile(pos, null);
            }
            yield return null;
        }
        public IEnumerator OpenHunterRoomsDoor()
        {
            foreach (Vector3Int pos in model.wallDestoryInHunter)
            {
                yield return new WaitForSeconds(0.1f);
                model.startRoomTilemap.SetTile(pos, null);
            }
            yield return null;
        }

        public void GetStartItemCallback(Gameplayer role)
        {
            gotStartItemPlayers++;
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                if (gotStartItemPlayers >= (room.roomSlots.Count - 1))
                {
                    Gameplayer _game = room.GetHunterGameplayer();
                    _game.hunterGameSetup.Generator(_game.input, RpcGameStart);
                }
            }
        }
        public void GetCaught(Gameplayer role)
        {

        }
        public void GetGoal(Gameplayer role)
        {

        }

        public void ItemTeleportNext(Gameplayer role, CinemachineConfiner confiner)
        {
            // role.currentRoomID--;
            // // MapObjectData m_data = model.blocks[role.currentRoomID].GetComponent<MapObjectData>();
            // // confiner.m_BoundingShape2D = m_data.polygonCollider2D;
            // // role.transform.position = m_data.entrance.position;
        }
        public void TeleportNext(Gameplayer role, CinemachineConfiner confiner)
        {
            // role.currentRoomID++;
            // MapObjectData m_data = model.blocks[role.currentRoomID].GetComponent<MapObjectData>();
            // confiner.m_BoundingShape2D = m_data.polygonCollider2D;
            // role.transform.position = m_data.entrance.position;
        }
        public void TeleportPrev(Gameplayer role, CinemachineConfiner confiner)
        {
            // role.currentRoomID--;
            // MapObjectData m_data = model.blocks[role.currentRoomID].GetComponent<MapObjectData>();
            // confiner.m_BoundingShape2D = m_data.polygonCollider2D;
            // role.transform.position = m_data.exit.position;
        }

        #region Gameplayers
        [Command]
        public void CmdInitPlayers()
        {
            RpcInitPlayers();
        }

        [ClientRpc]
        public void RpcInitPlayers()
        {
            List<System.Action<Gameplayer>> actions = new List<System.Action<Gameplayer>>();
            actions.Add(GetStartItemCallback);
            actions.Add(GetCaught);
            actions.Add(GetGoal);

            List<System.Action<Gameplayer, CinemachineConfiner>> changeLevelActions = new List<System.Action<Gameplayer, CinemachineConfiner>>();
            changeLevelActions.Add(TeleportNext);
            changeLevelActions.Add(TeleportPrev);
            changeLevelActions.Add(ItemTeleportNext);

            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                foreach (var p in room.GetGameplayContainers)
                {
                    p.self.AssignTeam(p.teamID, actions, changeLevelActions);
                }
            }
        }
        #endregion

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
                Vector2 pos = (i == 1) ? model.blocksList[i - 1].GetComponent<StartRoomData>().endPoint.position + new Vector3(100, 100, 0) : model.blocksList[i - 1].GetComponent<MapObjectData>().endpoint.position + new Vector3(100, 100, 0);
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
            Debug.Log(_startItems);
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
            startRoomData = FindObjectOfType<StartRoomData>();
            model.startRoom = startRoomData.transform;
            model.starItems = GameObject.FindGameObjectsWithTag("StartItem").ToList();
            model.startRoomTilemap = startRoomData.startRoomTilemap;
        }

        private void Start()
        {
            if (isServer)
            {
                CmdSetupRooms();
                CmdSetupStartItems();
                CmdInitPlayers();
            }
        }
        #endregion
    }
}