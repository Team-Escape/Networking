using System;
using System.Linq;
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
        List<Gameplayer> Gameplayers { get { return model.gameplayers; } set { model.gameplayers = value; } }
        Gameplayer SetGameplayer
        {
            set
            {
                if (model.gameplayers == null)
                    model.gameplayers = new List<Gameplayer>();
                model.gameplayers.Add(value);
            }
        }
        Gameplayer Hunter { get { return model.hunter; } set { model.hunter = value; } }
        List<Gameplayer> Hunters { get { return model.hunters; } set { model.hunters = value; } }
        Gameplayer SetHunter
        {
            set
            {
                if (model.hunters == null)
                    model.hunters = new List<Gameplayer>();
                model.hunters.Add(value);
            }
        }
        List<Gameplayer> Escapers { get { return model.escapers; } set { model.escapers = value; } }
        Gameplayer SetEscaper
        {
            set
            {
                if (model.escapers == null)
                    model.escapers = new List<Gameplayer>();
                model.escapers.Add(value);
            }
        }
        List<Gameplayer> GotStartItemPlayers { get { return model.gotStartItemPlayers; } set { model.gotStartItemPlayers = value; } }
        Gameplayer SetGotStartItemPlayer
        {
            set
            {
                if (model.gotStartItemPlayers == null)
                    model.gotStartItemPlayers = new List<Gameplayer>();
                model.gotStartItemPlayers.Add(value);
            }
        }
        List<Gameplayer> GoalPlayers { get { return model.goalPlayers; } set { model.goalPlayers = value; } }
        Gameplayer SetGoalPlayer
        {
            set
            {
                if (model.goalPlayers == null)
                    model.goalPlayers = new List<Gameplayer>();
                model.goalPlayers.Add(value);
            }
        }
        List<Gameplayer> CaughtPlayers { get { return model.caughtPlayers; } set { model.caughtPlayers = value; } }
        Gameplayer SetCaughtPlayer
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
        GameState gameState = new GameState();

        int ActivePlayerCounts { get { return model.gameplayers.Count; } }
        int joinedPlayer = 0;
        int playerGotStartItemCounts = 0;

        bool isStarted = false;
        bool isGoaled = false;

        #region Actions
        public void TeleportNextAction(Gameplayer role)
        {
            role.currentRoomID++;
            MapObjectData m_data = model.currentGameBlocks[role.currentRoomID].GetComponent<MapObjectData>();
            role.transform.position = m_data.entrance.position;
        }
        public void TeleportPrevAction(Gameplayer role)
        {
            role.currentRoomID--;
            MapObjectData m_data = model.currentGameBlocks[role.currentRoomID].GetComponent<MapObjectData>();
            role.transform.position = m_data.entrance.position;
        }
        public void GetStartItemAction(Gameplayer role)
        {
            if (isStarted) return;
            playerGotStartItemCounts++;
            if (playerGotStartItemCounts >= ActivePlayerCounts - 1)
            {
                GameLogic("Gaming");
            }
        }
        public void CaughtAction(Gameplayer role)
        {
            SetCaughtPlayer = role;
            if (CaughtPlayers.Count >= ActivePlayerCounts - 1)
            {
                GameLogic("Ending");
            }
            else Hunter.HunterDebuff(ActivePlayerCounts - 1);
        }
        public void GoalAction(Gameplayer role)
        {
            if (GoalPlayers.Any(x => x.playerID == role.playerID) == false)
                SetGoalPlayer = role;

            if (isGoaled) return;

            StartCoroutine(GoalCountDownCoroutine());
        }
        #endregion

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
                GameLogic("Loading");
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        #endregion

        #region Public Methods
        public void GameLogic(string state)
        {
            gameState.Change(state);
            switch (gameState)
            {
                case GameState.Loading:
                    // Pending feature, loading coroutine.
                    break;
                case GameState.Setting:
                    GameSetup();
                    break;
                case GameState.Gaming:
                    break;
                case GameState.Ending:
                    break;
                default:
                    break;
            }
        }
        public void GameSetup()
        {
            if (PhotonNetwork.IsMasterClient == false)
                return;

            StartCoroutine(GameSetupCoroutine());
        }
        public void OnPlayerSpawned(Gameplayer gameplayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetGameplayer = gameplayer;
            }
        }
        public void Loaded(string name)
        {
            model.gameScene = name;
            pv.RPC("RpcLoaded", RpcTarget.MasterClient);
        }
        #endregion

        #region Coroutine
        IEnumerator GoalCountDownCoroutine()
        {
            yield return null;
        }
        IEnumerator GameSetupCoroutine()
        {
            yield return StartCoroutine(RandomStartItems());
            yield return StartCoroutine(RandomRoomsList());
            yield return StartCoroutine(RoomsInstantiate());
            yield return StartCoroutine(SpawnPlayerCoroutine());
            yield return StartCoroutine(RadnomTeam());
        }
        IEnumerator RadnomTeam()
        {
            List<Action<Gameplayer>> gameActions = new List<Action<Gameplayer>>();
            gameActions.Add(GetStartItemAction);
            gameActions.Add(CaughtAction);
            gameActions.Add(GoalAction);

            List<Action<Gameplayer>> teleportActions = new List<Action<Gameplayer>>();
            teleportActions.Add(TeleportNextAction);
            teleportActions.Add(TeleportPrevAction);

            yield return null;

            Hunter = Gameplayers.Random();
            Hunter.transform.position = model.hunterSpawn.position;
            Hunter.AssignTeam(1, gameActions, teleportActions);

            yield return null;

            Escapers = Gameplayers.FindAll(x => (x != Hunter));
            Escapers.ForEach(x => x.AssignTeam(0, gameActions, teleportActions));
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
                pv.RPC("SpawnMyGameplayer", r.player, r.avatars);
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
                if (joinedPlayer == CoreModel.instance.avatarsDataStorage.Count)
                {
                    GameLogic("Setting");
                }
            }
        }
        [PunRPC]
        public void SpawnRoomBlock(object[] data)
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
            Vector2 spawn = model.escaperSpawn.position;
            PhotonNetwork.Instantiate(go, spawn, Quaternion.identity);
        }
        #endregion
    }
}