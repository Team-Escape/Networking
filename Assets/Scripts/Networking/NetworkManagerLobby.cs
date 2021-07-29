using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror.EscapeGame.GameplayerSpace;

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
        public List<Gameplayer> gameplayPlayers = new List<Gameplayer>();
        TransitionEffect transition;
        GameLogic gameLogic;
        public List<GameplayContainer> GetGameplayContainers { get { return gameLogic.gameplayContainers; } }

        [SerializeField] GameObject labManager;

        public string lobbyName = "LobbyScene";

        string gameScene = "";

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

        private void Update()
        {
            if (NetworkClient.isConnected && !NetworkClient.ready)
            {
                NetworkClient.Ready();
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
            StartCoroutine(GameSetupCoroutine());
        }

        IEnumerator GameSetupCoroutine()
        {
            gameScene = MapPoll();
            yield return null;
            foreach (RoomPlayer p in roomSlots)
            {
                p.MaskChangeScene(0, () => ServerChangeScene(gameScene));
            }
        }

        public void ResetPlayerID()
        {
            for (int i = 0; i < roomSlots.Count; i++)
            {
                roomSlots[i].id = i;
                roomSlots[i].SyncUI(roomSlots);
            }
        }

        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);
            transition.MaskIn(null);
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
                        NetworkServer.ReplacePlayerForConnection(_conn, player.gameObject, true);
                    }
                }
            }
            else
            {
                gameplayPlayers.Clear();

                switch (sceneName)
                {
                    case "LabScene":
                        GameObject go = Instantiate(labManager);
                        NetworkServer.Spawn(go, roomSlots[0].GetComponent<NetworkIdentity>().connectionToClient);
                        GetComponent<NetworkIdentity>().AssignClientAuthority(roomSlots[0].GetComponent<NetworkIdentity>().connectionToClient);
                        break;
                    default:
                        Debug.Log("Scene not registed yet...");
                        break;
                }

                int index = 0;
                foreach (RoomPlayer player in roomSlots)
                {
                    var _conn = player.connectionToClient;

                    gameLogic.Init(roomSlots);

                    GameObject go = Instantiate(
                        Resources.Load("Roles/" + player.selectedRoleName) as GameObject,
                        gameLogic.gameplayContainers[index].spawnPoint,
                        Quaternion.identity
                    );

                    Gameplayer _gameplayer = go.GetComponent<Gameplayer>();
                    gameLogic.gameplayContainers[index].self = _gameplayer;
                    gameplayPlayers.Add(_gameplayer);

                    NetworkServer.ReplacePlayerForConnection(_conn, go, true);
                    NetworkServer.SetClientReady(_conn);
                    index++;
                }
            }
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
                }
            }
            transition.MaskOut();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            transition = GetComponentInChildren<TransitionEffect>();
            SceneManager.sceneLoaded += OnSceneLoaded;
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

        private void OnEnable()
        {
            if (!instance)
                instance = this;
            else
                Destroy(gameObject);
            gameLogic = GetComponent<GameLogic>();
        }
    }
}