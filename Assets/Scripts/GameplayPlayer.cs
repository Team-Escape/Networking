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
                FindObjectOfType<CinemachineVirtualCamera>().Follow = this.transform;
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
    }

}