using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace.Gameplayer;
using Photon.Realtime;

namespace Photon.Pun.Escape.GM.Game
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager instance;

        #region Player Getter/Setter in model
        List<Gameplayer> GetGameplayers { get { return model.gameplayers; } }
        Gameplayer SetGameplayers
        {
            set
            {
                if (model.gameplayers == null)
                    model.gameplayers = new List<Gameplayer>();
                model.gameplayers.Add(value);
            }
        }
        List<Gameplayer> GetHunters { get { return model.hunters; } }
        Gameplayer SetHunters
        {
            set
            {
                if (model.hunters == null)
                    model.hunters = new List<Gameplayer>();
                model.hunters.Add(value);
            }
        }
        List<Gameplayer> GetEscapers { get { return model.escapers; } }
        Gameplayer SetEscapers
        {
            set
            {
                if (model.escapers == null)
                    model.escapers = new List<Gameplayer>();
                model.escapers.Add(value);
            }
        }
        List<Gameplayer> GetGotStartItemPlayers { get { return model.gotStartItemPlayers; } }
        Gameplayer SetGotStartItemPlayers
        {
            set
            {
                if (model.gotStartItemPlayers == null)
                    model.gotStartItemPlayers = new List<Gameplayer>();
                model.gotStartItemPlayers.Add(value);
            }
        }
        List<Gameplayer> GetGoledPlayers { get { return model.goaledPlayers; } }
        Gameplayer SetGoledPlayers
        {
            set
            {
                if (model.goaledPlayers == null)
                    model.goaledPlayers = new List<Gameplayer>();
                model.goaledPlayers.Add(value);
            }
        }
        List<Gameplayer> GetCaughtPlayers { get { return model.caughtPlayers; } }
        Gameplayer SetCaughtPlayers
        {
            set
            {
                if (model.caughtPlayers == null)
                    model.caughtPlayers = new List<Gameplayer>();
                model.caughtPlayers.Add(value);
            }
        }
        #endregion

        Model model;
        PhotonView pv;

        int joinedPlayer = 0;

        public void TeleportNext(Gameplayer role)
        {
            role.currentRoomID++;
            MapObjectData m_data = model.currentGameBlocks[role.currentRoomID].GetComponent<MapObjectData>();
            role.transform.position = m_data.entrance.position;
        }
        public void TeleportPrev(Gameplayer role)
        {
            role.currentRoomID--;
            MapObjectData m_data = model.currentGameBlocks[role.currentRoomID].GetComponent<MapObjectData>();
            role.transform.position = m_data.entrance.position;
        }
        #region Unity APIs
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            else
            {
                instance = this;
                model = GetComponent<Model>();
                pv = GetComponent<PhotonView>();
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        #endregion

        #region Public Methods
        public void Init()
        {
            Debug.Log("my scene is : " + name);
            if (PhotonNetwork.IsMasterClient)
                GameSetup();
        }
        public void OnPlayerSpawned(Gameplayer gameplayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetGameplayers = gameplayer;
            }
        }
        public void Loaded(string name)
        {
            model.gameScene = name;
            pv.RPC("RpcLoaded", RpcTarget.MasterClient);
        }
        #endregion

        #region Coroutine
        IEnumerator GameSetupCoroutine()
        {
            yield return StartCoroutine(RandomStartItems());
            // yield return StartCoroutine(RandomRoomsList());
            // yield return StartCoroutine(RoomsInstantiate());
            yield return StartCoroutine(SpawnPlayerCoroutine());
        }
        IEnumerator RandomStartItems()
        {
            List<int> items = new List<int>();
            items = model.startItemContainers.RandomSeedInt(3);

            yield return null;

            object[] data = new object[] { items[0], items[1], items[2] };

            pv.RPC("AssignStartItems", RpcTarget.All, data);
        }
        IEnumerator RandomRoomsList()
        {
            int index = 1;
            List<GameObject> blocks = new List<GameObject>();
            blocks.Add(FindObjectOfType<StartRoomData>().gameObject);
            blocks.Add(model.blockContainer.left.Random());
            while (true)
            {
                if (index > model.roomSize - 1) break;
                GameObject go = null;
                string name = blocks[index].name;
                switch (name.Split(',')[2])
                {
                    case "left":
                        go = model.blockContainer.right.Random();
                        break;
                    case "right":
                        go = model.blockContainer.left.Random();
                        break;
                    case "up":
                        go = model.blockContainer.down.Random();
                        break;
                    case "down":
                        go = model.blockContainer.up.Random();
                        break;
                }
                blocks.Add(go);
                index++;
                yield return null;
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

            model.currentGameBlocks = blocks;
            Debug.Log(model.currentGameBlocks.Count);
        }
        IEnumerator RoomsInstantiate()
        {
            for (int i = 1; i < model.currentGameBlocks.Count; i++)
            {
                model.currentGameBlocks[i].GetComponent<MapObjectData>().id = i;
                Vector2 pos = (i == 1) ? (model.currentGameBlocks[i - 1].GetComponent<StartRoomData>().endPoint.position) + new Vector3(100, 100, 0) : (model.currentGameBlocks[i - 1].GetComponent<MapObjectData>().endpoint.position) + new Vector3(100, 100, 0);

                string name;
                if (i < model.currentGameBlocks.Count - 1)
                {
                    name = "Game/" + model.gameScene + "/Rooms/" + model.currentGameBlocks[i].name;
                }
                else
                {
                    name = "Game/" + model.gameScene + "/Destination/" + model.currentGameBlocks[i].name;
                }

                object[] data = new object[] { name, i, pos.x, pos.y };
                pv.RPC("SpawnRoomBlock", RpcTarget.All, data);
            }

            yield return null;
        }
        IEnumerator SpawnPlayerCoroutine()
        {
            foreach (Role r in CoreModel.instance.avatarsDataStorage)
            {
                object[] data = new object[] { r.avatars };
                pv.RPC("SpawnMyGameplayer", r.player, data);
                yield return null;
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        public void RpcLoaded()
        {
            joinedPlayer++;
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log(joinedPlayer);
                if (joinedPlayer == CoreModel.instance.avatarsDataStorage.Count)
                {
                    Init();
                }
            }
        }
        [PunRPC]
        public void GameSetup()
        {
            if (PhotonNetwork.IsMasterClient == false)
                return;

            StartCoroutine(GameSetupCoroutine());
        }
        [PunRPC]
        public void SpawnRoomBlock(object[] data)
        {
            if (pv.IsMine)
            {
                string name = (string)data[0];
                int id = (int)data[1];
                float x = (float)data[2];
                float y = (float)data[3];

                GameObject go = Instantiate(Resources.Load(name) as GameObject);
                go.transform.position = new Vector3(x, y, 0);
                go.GetComponent<MapObjectData>().id = id;
                model.currentGameBlocks[id] = go;
            }
        }
        [PunRPC]
        public void AssignStartItems(object[] data)
        {
            for (int i = 0; i < model.starItems.Count; i++)
            {
                int id = (int)data[i];
                model.starItems[i].GetComponent<SpriteRenderer>().sprite = model.startItemContainers[id].image;
                model.starItems[i].name = model.startItemContainers[id].name;
            }
        }
        [PunRPC]
        public void SpawnMyGameplayer(string go)
        {
            Debug.Log("mine player is here");
            Vector2 spawn = Vector2.zero;
            PhotonNetwork.Instantiate(go, spawn, Quaternion.identity);
        }
        #endregion
    }
}