using Assets.Code.Game;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using System.Diagnostics;

namespace Assets.Code.Server {
    public class Game {
        private ServerManager serverManager;
        public Game(ServerManager serverManager) {
            this.serverManager = serverManager;
        }
        public bool started { get; private set; } = false;
        public static ushort DEFEND_REGEN_HEALTH_AMOUNT = 3;

        public Room[,] map;

        const int mapSize = 3;
        const float noRoomChance = 0.15f;
        const float treasureChance = 0.15f;
        const int treasureMin = 50;
        const int treasureMax = 80;
        const float monsterChance = 0.15f;
        const int monsterTreasure = 100;
        public const int defaultHealth = 10;

        //iterator of playerKeys integer list
        private int currentTurnIndex;
        private List<int> playerKeys;


        public void Start() {
            started = true;
            GenerateMap();
            foreach(var p in serverManager.lobby.players) {
                Player player = p.Value;
                player.health = Game.defaultHealth;
                player.position = Vector2Int.zero; // should be default anyway
                map[player.position.x, player.position.y].playerIdsInRoom.Add(p.Key);
            }

            playerKeys = serverManager.lobby.players.Keys.ToList();
            //define first turn
            currentTurnIndex = -1;
            StepTurn();
        }

        private void GenerateMap() {
            map = new Room[mapSize, mapSize];
            for(int x = 0; x < mapSize; x++) {
                for(int y = 0; y < mapSize; y++) {
                    //No Room
                    if((x < mapSize - 1 || y < mapSize - 1) && (x > 1 || y > 1)) {//but not near exit or startLocation
                        if(UnityEngine.Random.value < noRoomChance)
                            continue;
                    }
                    map[x, y] = new Room();

                    //Exit (always in corner)
                    if(x == mapSize - 1 && y == mapSize - 1) {
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


        private void StepTurn() {
            if(started == false) return;

            if(serverManager.lobby.players.Count > 1)
                currentTurnIndex = ++currentTurnIndex % serverManager.lobby.players.Count;
            else
                currentTurnIndex = 0;
            //skip left players
            if(serverManager.lobby.players[GetCurrentTurnPlayerID()].leftDungeon)
                StepTurn();
            serverManager.SendTurn(GetCurrentTurnPlayerID());
        }


        public int GetCurrentTurnPlayerID() {
            return playerKeys[currentTurnIndex];
        }

        public bool MovePlayer(int playerID, Directions moveDir) {
            if(GetRoom(playerID).monster != null)
                return false;
            //player should not move in muliple directions
            //if it is requested it will only execute one (based on if order)
            Directions possible = GetPossibleDirections(serverManager.lobby.players[playerID].position);
            Vector2Int move;
            if((moveDir & possible & Directions.North) != 0)
                move = new Vector2Int(0, -1);
            else if((moveDir & possible & Directions.East) != 0)
                move = new Vector2Int(1, 0);
            else if((moveDir & possible & Directions.South) != 0)
                move = new Vector2Int(0, 1);
            else if((moveDir & possible & Directions.West) != 0)
                move = new Vector2Int(-1, 0);
            else //no possible move entered
                return false;

            GetRoom(playerID).playerIdsInRoom.Remove(playerID);
            serverManager.lobby.players[playerID].position += move;
            GetRoom(playerID).playerIdsInRoom.Add(playerID);

            StepTurn();
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

        private Room GetRoom(int playerID) {
            return map[serverManager.lobby.players[playerID].position.x, serverManager.lobby.players[playerID].position.y];
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
            if(position.x < mapSize - 1 && map[position.x + 1, position.y] != null)
                d |= Directions.East;
            if(position.y < mapSize - 1 && map[position.x, position.y + 1] != null)
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

        public bool Attack(int connectionID) {
            Room room = GetRoom(connectionID);
            Player player = serverManager.lobby.players[connectionID];
            ushort damage = Player.DEFAULT_DAMAGE;
            if(room.monster == null)
                return false;

            if(room.monster.health - damage > 0) {
                room.monster.health -= damage;
                if(player.health - room.monster.damage > 0) {
                    player.health -= room.monster.damage;
                    serverManager.SendHitByMonsterMessage(connectionID, player.health);
                } else {
                    serverManager.SendPlayerDiedMessage(connectionID);
                }
            } else
                room.monster = null;             
                    //TODO no message in protocol to send monster died

            serverManager.SendHitMonsterMessage(connectionID, damage);


            StepTurn();
            return true;
        }

        public bool Defend(int connectionID) {
            Player player = serverManager.lobby.players[connectionID];
            if(GetRoom(connectionID).monster == null)
                return false;

            player.health += DEFEND_REGEN_HEALTH_AMOUNT;
            serverManager.SendPlayerDefendsMessage(connectionID, player.health);

            StepTurn();
            return true;
        }

        internal bool ClaimTreasure(int connectionID) {
            Room room = GetRoom(connectionID);
            if(room.treasure <= 0 || room.monster != null)
                return false;

            foreach(int playerID in room.playerIdsInRoom) {
                if(playerID == connectionID) continue;
                 serverManager.lobby.players[playerID].gold += (ushort)(room.treasure / room.playerIdsInRoom.Count);
                serverManager.SendObtainGold(playerID, (ushort)(room.treasure / room.playerIdsInRoom.Count));
            }
            if(room.playerIdsInRoom.Count > 0) serverManager.lobby.players[connectionID].gold += (ushort)(room.treasure / room.playerIdsInRoom.Count);
            serverManager.lobby.players[connectionID].gold += (ushort)(room.treasure % room.playerIdsInRoom.Count); //give the player that claimed the treasure the remaining goldserverManager
            serverManager.SendObtainGold(connectionID, (ushort)(room.treasure / room.playerIdsInRoom.Count + room.treasure % room.playerIdsInRoom.Count));
            room.treasure = 0;

            StepTurn();
            return true;
        }

        internal bool LeaveDungeon(int connectionID) {
            if(!GetRoom(connectionID).exit)
                return false;

            serverManager.lobby.players[connectionID].leftDungeon = true;

            bool allLeft = true;
            foreach(KeyValuePair<int, Player> player in serverManager.lobby.players) {
                if(!player.Value.leftDungeon)
                    allLeft = false;
                if(player.Key != connectionID)
                    serverManager.SendPlayerLeftDungeon(connectionID);                
            }

            if(allLeft) 
                EndGame();
            else
                StepTurn();
            return true;
        }

        private void EndGame() {
            started = false;
            serverManager.GameEnded();
            //idk
        }
    }
}