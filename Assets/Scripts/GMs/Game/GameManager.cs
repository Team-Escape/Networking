using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Escape.GM.Game
{
    using PlayerSpace.Gameplayer;
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager instance;

        Model model;
        PhotonView pv;

        List<Gameplayer> gameplayers = new List<Gameplayer>();

        #region Unity APIs
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
                model = GetComponent<Model>();
                pv = GetComponent<PhotonView>();
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log(CoreModel.instance.avatarsDataStorage.Count);
                foreach (Role r in CoreModel.instance.avatarsDataStorage)
                {
                    object[] data = new object[] { r.avatars, 1 };
                    pv.RPC("SpawnMyGameplayer", r.player, data);
                }
            }
        }
        #endregion

        #region Public Methods
        public void OnPlayerSpawned(Gameplayer gameplayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                gameplayers.Add(gameplayer);
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        public void SpawnMyGameplayer(string go, int teamID)
        {
            Debug.Log("mine player is here");
            Vector2 spawn = Vector2.zero;
            PhotonNetwork.Instantiate(go, spawn, Quaternion.identity);
        }
        #endregion
    }
}