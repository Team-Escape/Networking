using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.EscapeGame.GameplayerSpace;

namespace Mirror.EscapeGame
{
    public class GameLogic : MonoBehaviour
    {
        public List<GameplayContainer> gameplayContainers = new List<GameplayContainer>();

        public void Init(List<RoomPlayer> players)
        {
            AddContainers(players);
        }

        public void AddContainers(List<RoomPlayer> players)
        {
            int rnd = players.RandomInt();
            RoomBlockData roomBlockData = FindObjectOfType<RoomBlockData>();
            for (int i = 0; i < players.Count; i++)
            {
                GameplayContainer container = new GameplayContainer();
                if (i == rnd)
                {
                    container.teamID = 1;
                    container.spawnPoint = roomBlockData.hunterSpawn.position;
                }
                else
                {
                    container.teamID = 0;
                    container.spawnPoint = roomBlockData.escapeSpawn.position;
                }
                gameplayContainers.Add(container);
            }
        }
    }

    public class GameplayContainer
    {
        public Gameplayer self;
        public int id = 0;
        public int teamID = 0;
        public Vector2 spawnPoint = Vector2.zero;
    }
}