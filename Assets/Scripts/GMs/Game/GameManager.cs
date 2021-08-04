using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Escape.GM.Game
{
    public class GameManager : MonoBehaviour
    {
        Model model;

        private void Awake()
        {
            model = GetComponent<Model>();
        }
    }
}