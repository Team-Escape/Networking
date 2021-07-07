using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class NetworkManagerLobby : NetworkManager
    {
        public Canvas container;
        public Transform roleUI;
        public Transform mapUI;
        public List<RoomPlayer> roomSlots = new List<RoomPlayer>();

        public void ResetPlayerID()
        {
            for (int i = 0; i < roomSlots.Count; i++)
            {
                roomSlots[i].id = i;
                roomSlots[i].SyncUI(roomSlots);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            ResetPlayerID();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
        }

    }
}