using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

namespace Photon.Pun.Escape.Lobby
{
    public class LobbyPlayer : MonoBehaviourPunCallbacks, IPunObservable
    {
        Player input;
        public int id = 0;
        public int selectState = 0;
        public int selectIndex = 0;
        public int oldSelectIndex = 0;

        PhotonView pv;

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(selectState);
                stream.SendNext(selectIndex);
                stream.SendNext(oldSelectIndex);
            }
            else
            {
                this.selectState = (int)stream.ReceiveNext();
                this.selectIndex = (int)stream.ReceiveNext();
                this.oldSelectIndex = (int)stream.ReceiveNext();
            }
        }
        #endregion

        #region Unity APIs
        private void Awake()
        {
            pv = GetComponent<PhotonView>();
        }
        private void Update()
        {
            if (pv.IsMine == false) return;
            SelectIndexListener();
            if (LobbyManager.instance is LobbyManager lobby)
            {
                if (input.GetButtonDown("SelectR"))
                {
                    if ((selectIndex + 1) >= (selectState == 0 ? lobby.RoleLength : lobby.MapLength))
                        return;
                    selectIndex++;
                }
                else if (input.GetButtonDown("SelectL"))
                {
                    if ((selectIndex - 1) < 0)
                        return;
                    selectIndex--;
                }
                else if (input.GetButtonDown("Confirm"))
                {

                }
                else if (input.GetButtonDown("Cancel"))
                {

                }
            }
        }
        #endregion

        private void OnSelectIndexChanged()
        {
            if (LobbyManager.instance is LobbyManager lobby)
            {
                Debug.Log("change check");
                lobby.pv.RPC("SyncUI", RpcTarget.All);
                oldSelectIndex = selectIndex;
            }
        }

        private void SelectIndexListener()
        {
            if (pv.IsMine)
            {
                if (oldSelectIndex != selectIndex)
                {
                    OnSelectIndexChanged();
                }
            }
        }

        public override void OnEnable()
        {
            input = ReInput.players.GetPlayer(0);

            id = PhotonNetwork.CurrentRoom.PlayerCount;
            if (LobbyManager.instance is LobbyManager lobby)
            {
                lobby.OnNewPlayerJoined(this);
            }
        }
    }
}