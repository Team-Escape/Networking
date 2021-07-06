using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class RoomPlayer : NetworkBehaviour
    {
        public int id = 0;

        public Canvas container;
        public Transform roleUI;
        public Transform mapUI;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                CmdTest();
            }
        }

        [Command]
        public void CmdTest()
        {
            NetworkManagerLobby lobby = NetworkManager.singleton as NetworkManagerLobby;
            lobby.ResetPlayerID();
        }

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