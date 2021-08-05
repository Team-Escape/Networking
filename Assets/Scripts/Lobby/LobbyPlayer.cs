using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Photon.Pun.Escape.Lobby
{
    public class LobbyPlayer : MonoBehaviourPunCallbacks, IPunObservable
    {
        public PhotonView pv;

        Rewired.Player input;
        public int id = 1;

        public int selectState = 0;
        public int selectIndex = 0;
        public int oldSelectIndex = 0;
        public int oldSelectState = 0;
        public int roleSelection = 0;
        public int mapSelection = 0;

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(selectState);
                stream.SendNext(selectIndex);
                stream.SendNext(oldSelectIndex);
                stream.SendNext(oldSelectState);
                stream.SendNext(roleSelection);
                stream.SendNext(mapSelection);
            }
            else
            {
                this.selectState = (int)stream.ReceiveNext();
                this.selectIndex = (int)stream.ReceiveNext();
                this.oldSelectIndex = (int)stream.ReceiveNext();
                this.oldSelectState = (int)stream.ReceiveNext();
                this.roleSelection = (int)stream.ReceiveNext();
                this.mapSelection = (int)stream.ReceiveNext();
            }
        }
        public override void OnPlayerPropertiesUpdate(Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (!pv.IsMine && targetPlayer == pv.Owner)
            {
                id = (int)changedProps["NewID"];
                OnSelectIndexChanged(selectIndex);
            }
        }
        public void SyncMyID(int id)
        {
            if (pv.IsMine)
            {
                Debug.Log("id syncing : " + id);
                this.id = id;
                OnSelectIndexChanged(selectIndex);

                Hashtable hash = new Hashtable();
                hash.Add("NewID", id);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
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
        public override void OnDisable()
        {
            base.OnDisable();
            if (LobbyManager.instance is LobbyManager lobby)
            {
                lobby.OnPlayerLeft(this);
            }
        }
        #endregion

        #region Raise_Event
        private void OnSelectStateChanged(int newVal)
        {
            if (LobbyManager.instance is LobbyManager lobby)
            {
                switch (newVal)
                {
                    case 0:
                        roleSelection = 0;
                        break;
                    case 1:
                        roleSelection = selectIndex;
                        mapSelection = 0;
                        break;
                    case 2:
                        mapSelection = selectIndex;
                        break;
                }
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