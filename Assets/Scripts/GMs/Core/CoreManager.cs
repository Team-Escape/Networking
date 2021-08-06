using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
using Photon.Pun.Escape.GM.Game;

namespace Photon.Pun.Escape.GM
{
    using Photon.Pun.Escape.Lobby;
    public enum SceneState { LobbyScene, LabScene, ScoreScene, AwardScene };
    public class CoreManager : MonoBehaviour
    {
        public static CoreManager instance;
        SceneState currentScene;

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
                DontDestroyOnLoad(gameObject);

                currentScene = new SceneState();
            }
        }
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentScene.Change(scene.name);
            SceneStateManagement();
        }
        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        #region Public Methods
        public void SceneStateManagement()
        {
            switch (currentScene)
            {
                case SceneState.LobbyScene:
                    ChangeInputMap("Default");
                    break;
                case SceneState.LabScene:
                    Debug.Log("loaded on core");
                    GameManager.instance.Loaded(currentScene.ToString());
                    ChangeInputMap("Gameplay");
                    break;
                case SceneState.ScoreScene:
                    ChangeInputMap("Default");
                    break;
                case SceneState.AwardScene:
                    ChangeInputMap("Default");
                    break;
            }
        }
        public void ChangeInputMap(string mapName)
        {
            Player input = ReInput.players.GetPlayer(0);
            input.SelectTheMap(mapName);

            Debug.Log("Current intput map changes to : " + mapName);
        }
        #endregion
    }
}