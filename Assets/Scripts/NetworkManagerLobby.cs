using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror.EscapeGame
{
    public class NetworkManagerLobby : NetworkManager
    {
        public static NetworkManagerLobby instance;
        public GameObject mainUI;
        public Canvas container;
        public Transform roleUI;
        public Transform mapUI;
        public List<RoomPlayer> roomSlots = new List<RoomPlayer>();
        public List<GameplayPlayer> gameplayPlayers = new List<GameplayPlayer>();
        TransitionEffect transition;

        public string lobbyName = "LobbyScene";

        string gameScene = "";

        public void ChangeScene(string name) => transition.MaskIn(() => ServerChangeScene(gameScene));

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            transition.MaskOut();
            if (scene.name == lobbyName)
            {
                mainUI.SetActive(true);
            }
            else
            {
                mainUI.SetActive(false);
            }
        }

        public bool CheckAllPlayerReady
        {
            get
            {
                foreach (RoomPlayer player in roomSlots)
                {
                    if (player.isReady == false) return false;
                }
                return true;
            }
        }

        public string MapPoll()
        {
            var polls = new Dictionary<string, int>();
            foreach (RoomPlayer p in roomSlots)
            {
                string key = p.selectedMapName;
                if (polls.ContainsKey(key)) continue;
                polls.Add(key, roomSlots.FindAll(x => x.selectedMapName == key).Count);
            }

            string mapName = polls.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            return mapName;
        }

        public void NextLevel()
        {
            if (CheckAllPlayerReady == false) return;
            gameScene = MapPoll();
            ChangeScene(gameScene);
        }

        public void ResetPlayerID()
        {
            for (int i = 0; i < roomSlots.Count; i++)
            {
                roomSlots[i].id = i;
                roomSlots[i].SyncUI(roomSlots);
            }
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);

            if (sceneName == lobbyName)
            {
                if (gameplayPlayers.Count > 0)
                {
                    foreach (RoomPlayer player in roomSlots)
                    {
                        var _conn = player.connectionToClient;
                        NetworkServer.SetClientReady(_conn);
                        NetworkServer.ReplacePlayerForConnection(_conn, player.gameObject);
                    }
                }
            }
            else
            {
                gameplayPlayers.Clear();
                foreach (RoomPlayer player in roomSlots)
                {
                    var _conn = player.connectionToClient;
                    NetworkServer.SetClientReady(_conn);

                    GameObject go = Instantiate(Resources.Load(player.selectedRoleName) as GameObject);
                    NetworkServer.ReplacePlayerForConnection(_conn, go);
                    gameplayPlayers.Add(go.GetComponentInChildren<GameplayPlayer>());
                }
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            transition.MaskIn(null);
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            base.OnClientSceneChanged(conn);

            if (IsSceneActive(lobbyName))
            {
                foreach (RoomPlayer player in roomSlots)
                {
                    player.ChangeInputMap("Default");
                    player.GetSelectUI(this);
                }
            }
            else
            {
                mainUI.SetActive(false);
                foreach (RoomPlayer player in roomSlots)
                {
                    player.ChangeInputMap("Gameplay");
                    Vector2 point = FindObjectOfType<RoomBlockData>().escapeSpawn.position;
                    foreach (GameplayPlayer go in gameplayPlayers)
                    {
                        go.Init(point);
                    }
                }

            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
        }

        private void Awake()
        {
            transition = GetComponentInChildren<TransitionEffect>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (!instance)
                instance = this;
            else
                Destroy(gameObject);
        }
    }
}