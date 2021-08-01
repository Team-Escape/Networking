using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.Escape.Lobby
{
    using static PhotonSettings;
    [RequireComponent(typeof(PhotonManager))]
    public class PhotonHUD : MonoBehaviour
    {
        public bool showGUI = true;

        public int offsetX;
        public int offsetY;

        private void Awake()
        {
            Debug.Log(this);
        }

        void OnGUI()
        {
#pragma warning disable 618
            if (!showGUI || !PhotonNetwork.IsConnected || !PhotonNetwork.IsConnectedAndReady) return;
#pragma warning restore 618

            GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 215, 9999));
            if (!PhotonNetwork.InRoom)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
            }

            StopButtons();

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (!PhotonNetwork.InRoom)
            {
                // Server + Client
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    if (GUILayout.Button("Host (Server + Client)"))
                    {
                        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
                    }
                }

                // Client + IP
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Client"))
                {
                    PhotonNetwork.JoinRandomRoom();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Cancel Connection Attempt"))
                {
                    PhotonNetwork.LeaveRoom();
                }
            }
        }

        void StatusLabels()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GUILayout.Label($"<b>Host</b>");
            }
            // client only
            else if (PhotonNetwork.InRoom)
            {
                GUILayout.Label($"<b>Client</b>");
            }
        }

        void StopButtons()
        {
            // stop host if host mode
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                if (GUILayout.Button("Stop Host"))
                {
                    PhotonNetwork.LeaveRoom();
                }
            }
            // stop client if client-only
            else if (PhotonNetwork.InRoom)
            {
                if (GUILayout.Button("Stop Client"))
                {
                    PhotonNetwork.LeaveRoom();
                }
            }
            // stop server if server-only
            else if (PhotonNetwork.InRoom)
            {
                if (GUILayout.Button("Stop Server"))
                {
                    PhotonNetwork.LeaveRoom();
                }
            }
        }
    }

}