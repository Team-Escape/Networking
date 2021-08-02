using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Photon.Pun.Escape.Lobby
{
    public class LobbyManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public static LobbyManager instance;

        public int RoleLength { get { return roleContainer.childCount; } }
        public int MapLength { get { return mapContainer.childCount; } }
        [Header("UI")]
        [SerializeField] Transform roleContainer;
        [SerializeField] Transform mapContainer;

        [Header("Photon Related")]
        [SerializeField] GameObject lobbyPlayerPrefab;

        public PhotonView pv;
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

        private void Start()
        {
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventRecevied_Select;
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventRecevied_State;
        }


        #region Raise_Event in Selecting Role/Map
        public const byte SelectChangeEvent = 0;
        void NetworkingClient_EventRecevied_Select(EventData obj)
        {
            if (obj.Code == SelectChangeEvent)
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
            PhotonNetwork.RaiseEvent(SelectChangeEvent, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }
        #endregion

        #region Raise_Event in switching Role/Map
        public const byte SwitchStateEvent = 1;
        void NetworkingClient_EventRecevied_State(EventData obj)
        {
            if (obj.Code == SwitchStateEvent)
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
            PhotonNetwork.RaiseEvent(SwitchStateEvent, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }
        #endregion

        #region Public Methods
        public void SpawnPlayer()
        {
            PhotonNetwork.Instantiate(lobbyPlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);
        }
        public void OnNewPlayerJoined(LobbyPlayer newPlayer)
        {
            lobbyPlayers.Add(newPlayer);
            // pv.RPC("SyncUI", RpcTarget.All);
        }
        public override void OnJoinedRoom()
        {
            PhotonNetwork.Instantiate(lobbyPlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);
            int id = PhotonNetwork.CurrentRoom.PlayerCount;
            pv.RPC("ActiveRoleUI", RpcTarget.All, id, 0, true);
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            LobbyPlayer lobbyPlayer = lobbyPlayers.Find(x => x.photonView.IsMine);
            switch (lobbyPlayer.selectState)
            {
                case 0:
                    pv.RPC("ActiveRoleUI", newPlayer, lobbyPlayer.id, lobbyPlayer.selectIndex, true);
                    break;
                case 1:
                    pv.RPC("ActiveMapUI", newPlayer, lobbyPlayer.id, lobbyPlayer.selectIndex, true);
                    break;
            }
        }
        [PunRPC]
        public void ChangeState(int id, int newState, int newSelect, int oldSelect)
        {
            switch (newState)
            {
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
            Debug.Log("map : " + id + "," + index + "," + isActive);
            mapContainer.GetChild(index).GetChild(id).gameObject.SetActive(isActive);
        }
        #endregion
    }
}