using Assets.Code.Game;
using System;
using System.Linq;
using System.Collections.Generic;
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

        public int currentTurn { get; private set; }
        //public Dictionary<int, Player>.Enumerator currentTurn;
        //public Dictionary<int, Player>.KeyCollection.Enumerator currentTurn;

        public void Start() {
            started = true;
            GenerateMap();
            foreach(var p in serverManager.lobby.players) {
                Player player = p.Value;
                player.health = Game.defaultHealth;
                player.position = Vector2Int.zero; // should be default anyway
                map[player.position.x, player.position.y].playerIdsInRoom.Add(p.Key);
            }

            //define first turn
            currentTurn = serverManager.lobby.players.First().Key;
            serverManager.SendTurn(currentTurn);
        }

        private void GenerateMap() {
            map = new Room[mapSize, mapSize];
            for(int x = 0; x < mapSize; x++) {
                for(int y = 0; y < mapSize; y++) {
                    //No Room
                    if(x < mapSize - 1 && y < mapSize - 1 && !(x == 0 && y == 0)) {//but not near exit or startLocation
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

        //is cool, maar is dit eigenlijk wel zo handig/efficient
        private void StepTurn() {
            /*if(!currentTurn.MoveNext()) {
                currentTurn = serverManager.lobby.players.Keys.GetEnumerator();
            }*/

        }

        public bool MovePlayer(int playerID, Directions moveDir) {
            //player should not move in muliple directions
            //if it is requested it will only execute one (based on if order)
            Directions possible = GetPossibleDirections(serverManager.lobby.players[playerID].position);

            if((moveDir & possible & Directions.North) != 0)
                serverManager.lobby.players[playerID].position += new Vector2Int(0, -1);
            else if((moveDir & possible & Directions.East) != 0)
                serverManager.lobby.players[playerID].position += new Vector2Int(1, 0);
            else if((moveDir & possible & Directions.South) != 0)
                serverManager.lobby.players[playerID].position += new Vector2Int(0, 1);
            else if((moveDir & possible & Directions.West) != 0)
                serverManager.lobby.players[playerID].position += new Vector2Int(-1, 0);
            else //no possible move entered
                return false;

            return true;
        }

        public RoomInfoMessage GetRoomInfo(int playerId) {
            Player player = serverManager.lobby.players[playerId];
            Room room = GetRoom(player);
            RoomInfoMessage roomInfoMessage = new RoomInfoMessage {
                directions = GetPossibleDirections(player.position),
                TreasureInRoom = (ushort)room.treasure,
                ContainsMonster = room.monster != null,
                ContainsExit = room.exit,
                OtherPlayerIDs = new List<int>(room.playerIdsInRoom)
            };
            //dont send the player itself
            roomInfoMessage.OtherPlayerIDs.Remove(playerId);
            roomInfoMessage.NumberofOtherPlayers = (byte)roomInfoMessage.OtherPlayerIDs.Count;
            return roomInfoMessage;
        }

        private Room GetRoom(Player player) {
            return map[player.position.x, player.position.y];
        }

        private Room GetRoom(Vector2Int position) {
            return map[position.x, position.y];
        }

        private Directions GetPossibleDirections(Vector2Int position) {
            Directions d = 0;
            if(position.y > 0 && map[position.x, position.y - 1] != null)
                d |= Directions.North;
            if(position.x < mapSize && map[position.x + 1, position.y] != null)
                d |= Directions.East;
            if(position.y < mapSize && map[position.x, position.y + 1] != null)
                d |= Directions.South;
            if(position.x > 0 && map[position.x - 1, position.y] != null)
                d |= Directions.West;

            return d;
        }

        [Flags]
        public enum Directions {
            North = 1,
            East = 2,
            South = 4,
            West = 8
        }
    }
}