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
        public int selectState;
        public int selectIndex;

        PhotonView pv;

        private void Awake()
        {
            pv = GetComponent<PhotonView>();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(selectState);
                stream.SendNext(selectIndex);
            }
            else
            {
                this.selectState = (int)stream.ReceiveNext();
                this.selectIndex = (int)stream.ReceiveNext();
            }
        }

        public override void OnEnable()
        {
            input = ReInput.players.GetPlayer(0);

            id = PhotonNetwork.CurrentRoom.PlayerCount;
            if (LobbyManager.instance is LobbyManager lobby)
            {
                lobby.OnNewPlayerJoined(this);
                selectState = 0;
                selectIndex = 0;
            }
        }
    }
}