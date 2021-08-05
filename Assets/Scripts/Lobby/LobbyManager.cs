using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Escape.Lobby
{
    using static PhotonSettings;
    using Photon.Realtime;
    using Photon.Pun.Escape.GM;
    using ExitGames.Client.Photon;
    public class LobbyManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public static LobbyManager instance;

        public int RoleLength { get { return roleContainer.childCount; } }
        public int MapLength { get { return mapContainer.childCount; } }

        [Header("UI")]
        [SerializeField] Transform roleContainer;
        [SerializeField] Transform mapContainer;

        [Header("Photon Related")]
        [SerializeField] string lobbyPlayerPath = "Lobby/LobbyPlayer";

        PhotonView pv;
        [SerializeField] List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
        #endregion

        #region Unity APIs
        private void Awake()
        {
            pv = GetComponent<PhotonView>();

            if (instance != null)
            {
                Destroy(this);
                return;
            }
            else
            {
                instance = this;
            }
        }
        #endregion

        #region Raise_Event in Selecting Role/Map
        public const byte SelectChangeEventCode = 1;
        void NetworkingClient_EventRecevied_Select(EventData obj)
        {
            if (obj.Code == SelectChangeEventCode)
            {
                object[] data = (object[])obj.CustomData;
                int id = (int)data[0];
                int state = (int)data[1];
                int oldVal = (int)data[2];
                int newVal = (int)data[3];

                ActiveNewUI(id, state, newVal, oldVal);
            }
        }
        public void SelectChange(LobbyPlayer target, int oldVal, int newVal)
        {
            ActiveNewUI(target.id, target.selectState, newVal, oldVal);

            object[] data = new object[] { target.id, target.selectState, oldVal, newVal };
            PhotonNetwork.RaiseEvent(SelectChangeEventCode, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }
        #endregion

        #region Raise_Event in Switching Role/Map
        public const byte SwitchStateEventCode = 2;
        void NetworkingClient_EventRecevied_State(EventData obj)
        {
            if (obj.Code == SwitchStateEventCode)
            {
                object[] data = (object[])obj.CustomData;

                int id = (int)data[0];
                int newState = (int)data[1];
                int selectIndex = (int)data[2];
                int oldSelectIndex = (int)data[3];

                ChangeState(id, newState, selectIndex, oldSelectIndex);
            }
        }
        public void StateChange(LobbyPlayer target, int newState, int selectIndex, int oldSelectIndex)
        {
            ChangeState(target.id, newState, selectIndex, oldSelectIndex);

            object[] data = new object[] { target.id, newState, selectIndex, oldSelectIndex };
            PhotonNetwork.RaiseEvent(SwitchStateEventCode, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }
        #endregion

        #region Public Methods
        public void CheckAllPlayerReady()
        {
            int numOfReady = lobbyPlayers.Where(x => x.selectState == 2).ToList().Count;
            Debug.Log(numOfReady + " players is Ready");

            if (PhotonNetwork.IsMasterClient)
            {
                if (numOfReady >= lobbyPlayers.Count && lobbyPlayers.Count >= minPlayersToStartGame)
                {
                    LoadGame();
                }
            }
        }
        public void LoadGame()
        {
            List<Role> avatars = new List<Role>();

            foreach (LobbyPlayer p in lobbyPlayers)
            {
                if (p.selectState == 2)
                {
                    Role r = new Role();
                    r.player = p.pv.Owner;
                    r.avatars = "Game/Gameplayers/" + roleContainer.GetChild(p.roleSelection).name;
                    avatars.Add(r);
                }
                else
                {
                    avatars.Add(new Role());
                }
            }

            CoreModel.instance.avatarsDataStorage = avatars;
            foreach (var a in CoreModel.instance.avatarsDataStorage)
            {
                Debug.Log("Data storage :\n" + a.avatars + ",\n" + a.player);
            }

            PhotonNetwork.LoadLevel(MapPoll());
        }
        public string MapPoll()
        {
            var polls = new Dictionary<string, int>();
            List<string> maps = new List<string>();

            foreach (LobbyPlayer p in lobbyPlayers)
            {
                maps.Add(mapContainer.GetChild(p.mapSelection).name);
            }

            foreach (string s in maps)
            {
                string key = s;
                if (polls.ContainsKey(key)) continue;
                polls.Add(key, maps.FindAll(x => x == key).Count);
            }

            string mapName = polls.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            return mapName;
        }
        public void SpawnPlayer()
        {
            PhotonNetwork.Instantiate(lobbyPlayerPath, Vector3.zero, Quaternion.identity, 0);
        }
        public void DestroyPlayer(Player targetPlayer)
        {
            PhotonNetwork.DestroyPlayerObjects(targetPlayer);
        }
        public void OnNewPlayerJoined(LobbyPlayer newPlayer)
        {
            lobbyPlayers.Add(newPlayer);
            if (PhotonNetwork.IsMasterClient)
            {
                SyncAll();
            }
        }
        public void OnPlayerLeft(LobbyPlayer _player)
        {
            lobbyPlayers.Remove(_player);

            if (PhotonNetwork.IsMasterClient)
            {
                SyncAll();
            }
        }
        /// <summary>
        /// To sync UI and ID 
        /// </summary>
        public void SyncAll()
        {
            pv.RPC("CloseAllUIs", RpcTarget.All, new object[] { });
            ChangeMineID();
        }
        public void ChangeMineID()
        {
            int index = 1;
            foreach (LobbyPlayer p in lobbyPlayers)
            {
                p.id = index;
                pv.RPC("AssignIDToMine", p.pv.Owner, index);
                index++;
            }
        }
        #endregion

        #region Virtual Methods
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);

            if (PhotonNetwork.IsMasterClient)
            {
                SyncAll();
            }
        }
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventRecevied_Select;
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventRecevied_State;

            SpawnPlayer();
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            DestroyPlayer(otherPlayer);
        }
        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventRecevied_Select;
            PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventRecevied_State;

            CloseAllUIs();
        }
        #endregion

        #region RPCs
        [PunRPC]
        public void AssignIDToMine(int id)
        {
            LobbyPlayer p = lobbyPlayers.Find(x => x.pv.IsMine);
            p.SyncMyID(id);
        }
        [PunRPC]
        public void CloseAllUIs()
        {
            foreach (Transform c in roleContainer)
            {
                int index = 0;
                foreach (Transform c1 in c)
                {
                    if (index > 0)
                    {
                        c1.gameObject.SetActive(false);
                    }
                    index++;
                }
            }
            foreach (Transform c in mapContainer)
            {
                int index = 0;
                foreach (Transform c1 in c)
                {
                    if (index > 0)
                    {
                        c1.gameObject.SetActive(false);
                    }
                    index++;
                }
            }
        }
        [PunRPC]
        public void ChangeState(int id, int newState, int newSelect, int oldSelect)
        {
            switch (newState)
            {
                case -1:
                    CloseAllUIs();
                    break;
                case 0:
                    ActiveMapUI(id, oldSelect, false);
                    ActiveRoleUI(id, newSelect, true);
                    break;
                case 1:
                    ActiveRoleUI(id, oldSelect, false);
                    ActiveMapUI(id, newSelect, true);
                    break;
                case 2:
                    ActiveMapUI(id, oldSelect, false);
                    CheckAllPlayerReady();
                    break;
                default:
                    Debug.Log("Current state unregistered.");
                    break;
            }
        }
        [PunRPC]
        public void ActiveNewUI(int id, int state, int newSelect, int oldSelect)
        {
            switch (state)
            {
                case 0:
                    ActiveRoleUI(id, oldSelect, false);
                    ActiveRoleUI(id, newSelect, true);
                    break;
                case 1:
                    ActiveMapUI(id, oldSelect, false);
                    ActiveMapUI(id, newSelect, true);
                    break;
                default:
                    Debug.Log("Current state unregistered.");
                    break;
            }
        }
        [PunRPC]
        public void ActiveRoleUI(int id, int index, bool isActive)
        {
            roleContainer.GetChild(index).GetChild(id).gameObject.SetActive(isActive);
        }
        [PunRPC]
        public void ActiveMapUI(int id, int index, bool isActive)
        {
            mapContainer.GetChild(index).GetChild(id).gameObject.SetActive(isActive);
        }
        #endregion
    }
}