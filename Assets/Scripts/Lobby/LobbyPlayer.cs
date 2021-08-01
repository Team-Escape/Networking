using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

namespace Photon.Pun.Escape.Lobby
{
    public class LobbyPlayer : MonoBehaviour
    {
        Player input;

        private void OnEnable()
        {
            input = ReInput.players.GetPlayer(0);
        }
    }
}