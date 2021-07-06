using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class RoomPlayer : NetworkBehaviour
    {
        public int id = 0;

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                DontDestroyOnLoad(this);
                room.roomSlots.Add(this);
                room.ResetPlayerID();
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
        }
    }

}