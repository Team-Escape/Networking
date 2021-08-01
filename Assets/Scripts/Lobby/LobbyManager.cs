using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace Photon.Pun.Escape.Lobby
{
    using static Photon.Pun.Escape.PhotonSettings;
    public class LobbyManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public static LobbyManager instance;

        [Header("UI")]
        [SerializeField] Transform roleContainer;
        [SerializeField] Transform mapContainer;

        [Header("Photon Related")]
        [SerializeField] GameObject lobbyPlayerPrefab;

        PhotonView pv;
        List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // stream.SendNext(punPlayers);
            }
            else
            {
                // this.punPlayers = (List<Realtime.Player>)stream.ReceiveNext();
            }
        }
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

        #region Public Methods
        public void SpawnPlayer()
        {
            PhotonNetwork.Instantiate(lobbyPlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);
        }
        public void OnNewPlayerJoined(LobbyPlayer newPlayer)
        {
            lobbyPlayers.Add(newPlayer);
            pv.RPC("SyncUI", RpcTarget.All);
        }
        public override void OnJoinedRoom()
        {
            PhotonNetwork.Instantiate(lobbyPlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);
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
        public void SyncUI()
        {
            CloseAllUI();
            lobbyPlayers.ForEach(x =>
            {
                switch (x.selectState)
                {
                    case 0:
                        ActiveRoleUI(x.id, x.selectIndex, true);
                        break;
                    case 1:
                        ActiveMapUI(x.id, x.selectIndex, true);
                        break;
                }
            });
        }
        public void CloseAllUI()
        {
            CloseUIBySearchingChildren(roleContainer);
            CloseUIBySearchingChildren(mapContainer);
        }
        public void CloseUIBySearchingChildren(Transform t)
        {
            foreach (Transform c in t)
            {
                int index = 0;
                foreach (Transform c1 in c)
                {
                    if (index == 0) continue;
                    c1.gameObject.SetActive(false);
                }
                index++;
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