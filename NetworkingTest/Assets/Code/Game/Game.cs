using Assets.Code.Game;
using System;
using System.Numerics;
using UnityEngine;

namespace Assets.Code.Server {
    public class Game {
        private ServerManager serverManager;
        public Game(ServerManager serverManager) {
            this.serverManager = serverManager;
        }
        public bool started { get; private set; } = false;
        public Room[,] map;

        const int mapSize = 4;
        const float noRoomChance = 0.15f;
        const float treasureChance = 0.15f;
        const int treasureMin = 50;
        const int treasureMax = 80;
        const float monsterChance = 0.15f;
        const int monsterTreasure = 100;
        public const int defaultHealth = 10;      
        
        public void Start() {
            started = true;
            foreach(var p in serverManager.lobby.players) {
                Player player = p.Value;
                player.health = Game.defaultHealth;
                player.position = Vector2Int.zero; // should be default anyway
            }
            GenerateMap();
        }

        private void GenerateMap() {
            map = new Room[mapSize, mapSize];
            for(int x = 0; x < mapSize; x++) {
                for(int y = 0; y < mapSize; y++) {
                    //No Room
                    if(x < mapSize - 1 && y < mapSize - 1) {//but not near exit
                        if(UnityEngine.Random.value < noRoomChance)
                            continue;
                    }
                    map[x, y] = new Room();

                    //Exit (always in corner)
                    if(x == mapSize && y == mapSize) {
                        map[x, y].exit = true;
                        continue;
                    }

                    //Treasure
                    if(UnityEngine.Random.value < treasureChance) {
                        map[x, y].treasure = UnityEngine.Random.Range(treasureMin, treasureMax);
                    }

                    //Monster
                    if(UnityEngine.Random.value < monsterChance) {
                        map[x, y].monster = new Monster();
                        map[x, y].treasure = monsterTreasure;
                    }
                }
            }
        }

        [Flags]
        public enum Direction {
            North = 1,
            East = 2,
            South = 4,
            West = 8
        }
    }
}