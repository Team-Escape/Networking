using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

namespace Mirror.EscapeGame
{
    public class GameplayPlayer : NetworkBehaviour
    {
        Player input = null;

        private void Awake()
        {
            input = ReInput.players.GetPlayer(0);
            Debug.Log(input);
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