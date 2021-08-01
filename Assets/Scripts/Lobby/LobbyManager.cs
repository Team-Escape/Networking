﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace Photon.Pun.Escape.Lobby
{
    using static Photon.Pun.Escape.PhotonSettings;
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] Transform uiContainer;
        [SerializeField] Transform roleContainer;
        [SerializeField] Transform mapContainer;

        #region Unity APIs
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        #endregion

        #region Public Methods
        public void CreateRoom()
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }
        #endregion

        #region  PhotonCallbacks
        public override void OnConnectedToMaster()
        {
            Debug.Log("Pun connected");
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Pun Disconnected");
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            CreateRoom();
        }
        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        }
        #endregion
    }
}