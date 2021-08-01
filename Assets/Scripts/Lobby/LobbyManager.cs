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
        public void OnNewPlayerJoined(int newPlayer, LobbyPlayer lbbyplayer)
        {
            lobbyPlayers.Add(lbbyplayer);

            pv.RPC("SyncUI", RpcTarget.All);
        }
        [PunRPC]
        public void SyncUI()
        {
            int index = 0;
            foreach (var p in lobbyPlayers)
            {
                int id = lobbyPlayers[index].id;
                int selectIndex = p.selectIndex;
                switch (p.selectState)
                {
                    case 0:
                        ActiveRoleUI(id, selectIndex, true);
                        break;
                    case 1:
                        ActiveMapUI(id, selectIndex, true);
                        break;
                    default:
                        Debug.Log($"State not registered in {p.selectState}");
                        break;
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