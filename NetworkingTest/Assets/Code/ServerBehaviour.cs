using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using Assets.Code;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;
using System;

public class ServerBehaviour : MonoBehaviour {
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;
    private JobHandle networkJobHandle;
    private Queue<ReceivedMessage> receivedMessagesQueue;
    public MessageEvent[] ServerCallbacks = new MessageEvent[(int)Message.MessageType.Count - 1];

    private Lobby lobby = new Lobby();

    void Start() {
        networkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if(networkDriver.Bind(endpoint) != 0) {
            Commandline.Instance.Output("Failed to bind port");
        } else {
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        receivedMessagesQueue = new Queue<ReceivedMessage>();

        //assign all eventlisteners
        for(int i = 0; i < ServerCallbacks.Length; i++) {
            ServerCallbacks[i] = new MessageEvent();
        }
        ServerCallbacks[(int)Message.MessageType.SetName].AddListener(HandleSetName);

        Commandline.Instance.Output("Host Started");
    }

    void Update() {
        networkJobHandle.Complete();

        for(int i = 0; i < connections.Length; ++i) {
            if(!connections[i].IsCreated) {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        NetworkConnection c;
        while(lobby.players.Count<4 &&  (c = networkDriver.Accept()) != default) {
            connections.Add(c);
            Commandline.Instance.Output("Accepted connection");

            SendWelcome(c);
        }

        DataStreamReader reader;
        for(int i = 0; i < connections.Length; ++i) {
            if(!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty) {
                if(cmd == NetworkEvent.Type.Data) {
                    var messageType = (Message.MessageType)reader.ReadUShort();
                    switch(messageType) {
                        case Message.MessageType.None: //stay alive
                            break;
                        case Message.MessageType.SetName:
                            var message = new SetNameMessage();
                            message.DeserializeObject(ref reader);
                            receivedMessagesQueue.Enqueue(new ReceivedMessage(message, connections[i].InternalId));
                            break;
                        case Message.MessageType.Count:
                            break;
                        default:
                            break;
                    }
                } else if(cmd == NetworkEvent.Type.Disconnect) {
                    Commandline.Instance.Output("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        ProcessMessagesQueue();
        networkJobHandle = networkDriver.ScheduleUpdate();

    }

    private void ProcessMessagesQueue() {
        while(receivedMessagesQueue.Count > 0) {
            ReceivedMessage receivedMessage = receivedMessagesQueue.Dequeue();
            ServerCallbacks[(int)receivedMessage.message.Type].Invoke(receivedMessage);
        }
    }

    private void OnDestroy() {
        networkJobHandle.Complete();
        networkDriver.Dispose();
        connections.Dispose();
    }

    private void SendMessage(NetworkConnection connection, Message message) {
        //networkJobHandle.Complete();

        var writer = networkDriver.BeginSend(connection);
        message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);

        Debug.Log("Host Sending Message: " + message.Type +", to " + connection.InternalId);
        //networkJobHandle = networkDriver.ScheduleUpdate();
    }


    private void HandleSetName(ReceivedMessage receivedMessage) {
        SetNameMessage setNameMessage = (SetNameMessage)receivedMessage.message;
        lobby.players[receivedMessage.connectionID].name = setNameMessage.Name;
        Debug.Log($"Got a name: {setNameMessage.Name}");

        var newPlayerMessage = new NewPlayerMessage {
            PlayerID = receivedMessage.connectionID,
            PlayerColor = lobby.players[receivedMessage.connectionID].color.ToUInt(),
            PlayerName = lobby.players[receivedMessage.connectionID].name
        };

        for(int i = 0; i < connections.Length; ++i) {
            if(!connections[i].IsCreated) continue;
            if(connections[i].InternalId == receivedMessage.connectionID) continue;
            SendMessage(connections[i], newPlayerMessage);
        }
    }

    private void SendWelcome(NetworkConnection newConnection) {
        Debug.Log("Sending Welcome");

        //welcome to new player
        var color = Color.magenta;
        var color32 = (Color32)color;
        var welcomeMessage = new WelcomeMessage {
            PlayerID = newConnection.InternalId,
            Color = ((uint)color32.r << 24) | ((uint)color32.g << 16) | ((uint)color32.b << 8) | color32.a
        };

        SendMessage(newConnection, welcomeMessage);

        //send allready joined players to new player
        foreach(KeyValuePair<int, Player> player in lobby.players) {
            if(player.Key == newConnection.InternalId) continue;
            if(player.Value.name == null) continue;
            SendMessage(newConnection, new NewPlayerMessage {
                    PlayerID = player.Key,
                    PlayerColor = player.Value.color.ToUInt(),
                    PlayerName = player.Value.name
                });
            
        }

        lobby.AddPlayer(newConnection.InternalId, new Player(color));

        
    }
}