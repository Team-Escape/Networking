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
        GameLogic gameLogic;

        [SerializeField] GameObject labManager;

        public string lobbyName = "LobbyScene";

        string gameScene = "";

        public void ChangeScene(string name) => ServerChangeScene(name);

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
            // transition.MaskIn(() => ChangeScene(gameScene));
            StartCoroutine(GameSetupCoroutine());
        }

        IEnumerator GameSetupCoroutine()
        {
            gameScene = MapPoll();
            yield return null;
            foreach (RoomPlayer p in roomSlots)
            {
                p.MaskChangeScene(0, () => ChangeScene(gameScene));
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
                        NetworkServer.Spawn(Instantiate(labManager), roomSlots[0].GetComponent<NetworkIdentity>().connectionToClient);
                        break;
                }

                int index = 0;
                foreach (RoomPlayer player in roomSlots)
                {
                    var _conn = player.connectionToClient;

                    gameLogic.Init(roomSlots);

                    GameObject go = Instantiate(
                        Resources.Load(player.selectedRoleName) as GameObject,
                        gameLogic.gameplayContainers[index].spawnPoint,
                        Quaternion.identity
                    );

                    GameplayPlayer _gameplayer = go.GetComponent<GameplayPlayer>();
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