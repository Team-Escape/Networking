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

        [SerializeField] float speed = 10f;

        Player input = null;
        Rigidbody2D rb;

        [Command]
        public void CmdBroadCastToAll(string msg) => RpcBroadCastToAll(msg);
        [ClientRpc]
        public void RpcBroadCastToAll(string msg) => Debug.Log(msg);

        public void SetCameraFollow() => FindObjectOfType<CinemachineVirtualCamera>().Follow = transform;
        public void SetCameraFollow(Transform target) => FindObjectOfType<CinemachineVirtualCamera>().Follow = target;
        [Command]
        public void CmdSetCameraFollow() => RpcSetCameraFollow();
        [ClientRpc]
        public void RpcSetCameraFollow() => SetCameraFollow();

        private void Awake()
        {
            input = ReInput.players.GetPlayer(0);
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {

        }

        public void Init()
        {
            if (isLocalPlayer)
            {
                SetCameraFollow();
                CmdSetCameraFollow();
                RpcSetCameraFollow();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isLocalPlayer)
            {
                if (input.GetButton("MoveR"))
                {
                    rb.velocity = Vector2.right * speed;
                }
                else if (input.GetButton("MoveL"))
                {
                    rb.velocity = Vector2.left * speed;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    CmdBroadCastToAll("oqfjopj");
                }
            }
        }
    }
}