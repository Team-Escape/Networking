﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Cinemachine;
using Mirror.EscapeGame.Gameplayer;

namespace Mirror.EscapeGame
{
    public class GameplayPlayer : NetworkBehaviour
    {
        public int id;
        public int teamID;
        Control control;

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
            control = GetComponent<Control>();
        }

        public void Init()
        {
            if (isLocalPlayer)
            {
                SetCameraFollow();
            }
        }

        void Update()
        {
            if (isLocalPlayer)
            {
                MoveInput();
            }
        }

        public void MoveInput()
        {
            float movement = input.GetAxis("Move Horizontal");
            control.Move(movement);

            if (input.GetButtonDown("Jump")) control.Jump(true);
            else if (input.GetButtonUp("Jump")) control.Jump(false);

            if (input.GetButtonDown("Run")) control.Run(true);
            else if (input.GetButtonUp("Run")) control.Run(false);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (isLocalPlayer)
            {
                Init();
            }
        }
    }
}