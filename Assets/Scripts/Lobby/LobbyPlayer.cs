using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

namespace Photon.Pun.Escape.Lobby
{
    public class LobbyPlayer : MonoBehaviourPunCallbacks, IPunObservable
    {
        public PhotonView pv;

        Player input;
        public int id = 0;
        public int selectState = 0;
        public int selectIndex = 0;
        public int oldSelectIndex = 0;
        public int oldSelectState = 0;

        bool isSelecting = false;

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(selectState);
                stream.SendNext(selectIndex);
                stream.SendNext(oldSelectIndex);
                stream.SendNext(oldSelectState);
            }
            else
            {
                this.selectState = (int)stream.ReceiveNext();
                this.selectIndex = (int)stream.ReceiveNext();
                this.oldSelectIndex = (int)stream.ReceiveNext();
                this.oldSelectState = (int)stream.ReceiveNext();
            }
        }
        #endregion

        #region Unity APIs
        private void Awake()
        {
            pv = GetComponent<PhotonView>();
        }
        private void Start()
        {
            input = ReInput.players.GetPlayer(0);
            id = PhotonNetwork.CurrentRoom.PlayerCount;

            if (LobbyManager.instance is LobbyManager lobby)
            {
                lobby.OnNewPlayerJoined(this);
            }
        }
        private void Update()
        {
            if (pv.IsMine == false) return;
            Listener();
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
                    selectState++;
                }
                else if (input.GetButtonDown("Cancel"))
                {
                    selectState--;
                }
            }
        }
        #endregion

        #region Raise_Event
        private void OnSelectStateChanged(int newVal)
        {
            if (LobbyManager.instance is LobbyManager lobby)
            {
                selectIndex = 0;
                lobby.StateChange(this, newVal, selectIndex, oldSelectIndex);
                oldSelectState = newVal;
                oldSelectIndex = selectIndex;
            }
        }
        private void OnSelectIndexChanged(int newVal)
        {
            if (LobbyManager.instance is LobbyManager lobby)
            {
                lobby.SelectChange(this, oldSelectIndex, selectIndex);
                oldSelectIndex = newVal;
            }
        }
        private void Listener()
        {
            if (oldSelectState != selectState)
            {
                OnSelectStateChanged(selectState);
            }
            else if (oldSelectIndex != selectIndex)
            {
                OnSelectIndexChanged(selectIndex);
            }
        }
        #endregion
    }
}