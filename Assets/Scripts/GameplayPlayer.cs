using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Cinemachine;

namespace Mirror.EscapeGame
{
    public class GameplayPlayer : NetworkBehaviour
    {
        public int id;
        public int teamID;

        Player input = null;

        public void SetCameraFollow() => FindObjectOfType<CinemachineVirtualCamera>().Follow = this.transform;
        [Command]
        public void CmdSetCameraFollow() => RpcSetCameraFollow();
        [ClientRpc]
        public void RpcSetCameraFollow() => SetCameraFollow();

        private void Awake()
        {
            input = ReInput.players.GetPlayer(0);
            Debug.Log(input);
        }

        public void Init(Vector2 spawn)
        {
            if (isLocalPlayer)
            {
                transform.position = spawn;

                SetCameraFollow();
                CmdSetCameraFollow();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isLocalPlayer)
            {
                if (input.GetButton("MoveR"))
                {
                    transform.position += Vector3.right;
                }
                else if (input.GetButton("MoveL"))
                {
                    transform.position += Vector3.left;
                }
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log(this.gameObject.name + " spawn");
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                room.gameplayPlayers.Add(this);
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                room.gameplayPlayers.Remove(this);
            }
        }
    }
}