﻿using System.Collections;
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

        public void SetCameraFollow(Transform target) => FindObjectOfType<CinemachineVirtualCamera>().Follow = target;

        private void Awake()
        {
            input = ReInput.players.GetPlayer(0);
        }

        public void Init(GameplayPlayer target)
        {
            SetCameraFollow(target.transform);
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
    }
}