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
        }

        public void Init(Vector2 spawn)
        {
            Debug.Log("Init GamePlayer");
            transform.position = spawn;
            if (isClient)
                CmdInit(spawn);
            else if (isServer)
                RpcInit(spawn);

            SetCameraFollow();
            CmdSetCameraFollow();
        }

        [Command]
        public void CmdInit(Vector2 spawn) => RpcInit(spawn);
        [ClientRpc]
        public void RpcInit(Vector2 spawn) => Init(spawn);

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
    }
}