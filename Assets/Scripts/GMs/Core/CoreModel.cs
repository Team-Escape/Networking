using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Escape.GM
{
    public class CoreModel : MonoBehaviour
    {
        public static CoreModel instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            else
            {
                instance = this;
            }
        }

        #region Game Settings
        /// <summary>
        /// Player first to this point wins the game.
        /// </summary>
        public int pointToWin = 3;
        #endregion

        #region In-Game datas
        /// <summary>
        /// Prefabs of players' roles.
        /// </summary>
        public List<GameObject> playerAvatars { get; set; }
        public GameObject winnersAvatar { get; set; }
        /// <summary>
        /// Store each player's total score.
        /// </summary>
        public List<int> scores { get; set; }
        /// <summary>
        /// After each round finish, store each player's obtain score then additive to scores list.
        /// </summary>
        public List<int> additiveScores { get; set; }
        #endregion
    }
}