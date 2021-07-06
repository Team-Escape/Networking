using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class NetworkManagerLobby : NetworkManager
    {
        [SerializeField] bool _allPlayersReady;

        public List<RoomPlayer> roomSlots = new List<RoomPlayer>();

        public void ResetPlayerID()
        {
            for (int i = 0; i < roomSlots.Count; i++)
            {
                roomSlots[i].id = i;
                Debug.Log(roomSlots[i].gameObject.name + " id: " + i);
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
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
        }

    }
}