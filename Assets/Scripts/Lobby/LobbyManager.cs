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
        public void SpawnPlayer()
        {
            PhotonNetwork.Instantiate(lobbyPlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);
        }
        public void DestroyPlayer(Player targetPlayer)
        {
            PhotonNetwork.DestroyPlayerObjects(targetPlayer);
            OnPlayerLeft(lobbyPlayers.Find(x => x.id == targetPlayer.ActorNumber));
        }
        public void OnNewPlayerJoined(LobbyPlayer newPlayer)
        {
            Debug.Log(newPlayer.id + " id");
            lobbyPlayers.Add(newPlayer);

            if (PhotonNetwork.IsMasterClient)
            {
                pv.RPC("AssignID", RpcTarget.All, new object[] { });
            }
        }
        public void OnPlayerLeft(LobbyPlayer leftPlayer)
        {
            Debug.Log(leftPlayer.id + " id");
            lobbyPlayers.Remove(leftPlayer);

            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    pv.RPC("AssignID", RpcTarget.All, new object[] { });
                }
            }
        }
        #endregion

        #region Virtual Methods
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
        public void AssignID()
        {
            CloseAllUIs();
            int index = 1;
            foreach (LobbyPlayer p in lobbyPlayers)
            {
                Debug.Log(p.photonView.IsMine);
                p.id = index;
                switch (p.selectState)
                {
                    case 0:
                        ActiveRoleUI(p.id, p.selectIndex, true);
                        break;
                    case 1:
                        ActiveMapUI(p.id, p.selectIndex, true);
                        break;
                    case 2:
                        break;
                    default:
                        Debug.Log("Current state unregistered.");
                        break;
                }
                index++;
            }
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