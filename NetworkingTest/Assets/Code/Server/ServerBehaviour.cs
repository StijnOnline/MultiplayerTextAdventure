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
using Assets.Code.Server;
using System.Linq;

public class ServerBehaviour {
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;
    private JobHandle networkJobHandle;

    private Queue<AdressedMessage> receivedMessagesQueue;
    private Queue<AdressedMessage> sendMessagesQueue;

    //Define Event
    public class AdressedMessageEvent : UnityEvent<AdressedMessage> { }
    public AdressedMessageEvent[] ServerCallbacks = new AdressedMessageEvent[(int)Message.MessageType.Count ];

    private ServerManager serverManager;

    /// <summary>
    /// KEY: Connection ID
    /// VALUE: Last time a message has been sent
    /// </summary>
    private Dictionary<int, float> lastSendTimes = new Dictionary<int, float>(ServerManager.maxPlayers);
    private const float STAY_ALIVE_AFTER_SECONDS = 20;

    public ServerBehaviour(ServerManager serverManager) {
        this.serverManager = serverManager;
    }

    public void Start() {

        networkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if(networkDriver.Bind(endpoint) != 0) {
            Debug.Log("Failed to bind port");
        } else {
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        receivedMessagesQueue = new Queue<AdressedMessage>();
        sendMessagesQueue = new Queue<AdressedMessage>();

        //assign all eventlisteners
        for(int i = 0; i < ServerCallbacks.Length; i++) {
            ServerCallbacks[i] = new AdressedMessageEvent();
        }
        ServerCallbacks[(int)Message.MessageType.SetName].AddListener(serverManager.HandleSetName);
        ServerCallbacks[(int)Message.MessageType.MoveRequest].AddListener(serverManager.HandleMoveRequest);
        ServerCallbacks[(int)Message.MessageType.AttackRequest].AddListener(serverManager.HandleAttackRequest);
        ServerCallbacks[(int)Message.MessageType.DefendRequest].AddListener(serverManager.HandleDefendRequest);
        ServerCallbacks[(int)Message.MessageType.ClaimTreasureRequest].AddListener(serverManager.HandleClaimTreasureRequest);
        ServerCallbacks[(int)Message.MessageType.LeaveDungeonRequest].AddListener(serverManager.HandleLeaveDungeonRequest);

        Debug.Log("Host Started");
    }

    public void Update() {
        networkJobHandle.Complete();

        for(int i = 0; i < connections.Length; ++i) {
            if(!connections[i].IsCreated) {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        NetworkConnection c;
        while(!serverManager.game.started && serverManager.lobby.players.Count < ServerManager.maxPlayers && (c = networkDriver.Accept()) != default) {
            connections.Add(c);
            Debug.Log("Accepted connection");
            serverManager.HandleNewConnection(c.InternalId);
        }

        DataStreamReader reader;
        for(int i = 0; i < connections.Length; ++i) {
            if(!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty) {
                if(cmd == NetworkEvent.Type.Data) {
                    var messageType = (Message.MessageType)reader.ReadUShort();
                    Debug.Log("Host Received: " + messageType + " from " + connections[i].InternalId);

                    Message message = null;
                    switch(messageType) {
                        case Message.MessageType.None: break; //stay alive
                        case Message.MessageType.SetName: message = new SetNameMessage(); break;
                        case Message.MessageType.MoveRequest: message = new MoveRequestMessage(); break;
                        case Message.MessageType.AttackRequest: message = new AttackRequestMessage(); break;
                        case Message.MessageType.DefendRequest: message = new DefendRequestMessage(); break;
                        case Message.MessageType.ClaimTreasureRequest: message = new ClaimTreasureRequestMessage(); break;
                        case Message.MessageType.LeaveDungeonRequest: message = new LeaveDungeonRequestMessage(); break;
                        default: break;
                    }
                    if(message != null) {
                        message.DeserializeObject(ref reader);
                        receivedMessagesQueue.Enqueue(new AdressedMessage(message, connections[i].InternalId));
                    }
                } else if(cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client disconnected");
                    serverManager.HandleDisconnect(connections[i].InternalId);
                    connections[i] = default;
                }
            }
        }

        ProcessMessagesQueue();

        //order slightly wrong but will be sent next cycle
        foreach(KeyValuePair<int, float> lastSendTime in lastSendTimes) {
            if(Time.time - lastSendTime.Value > STAY_ALIVE_AFTER_SECONDS) {
                QeueMessage(new AdressedMessage(new NoneMessage(), lastSendTime.Key));
            }
        }
        networkJobHandle = networkDriver.ScheduleUpdate();

    }

    private void ProcessMessagesQueue() {
        while(receivedMessagesQueue.Count > 0) {
            AdressedMessage receivedMessage = receivedMessagesQueue.Dequeue();
            ServerCallbacks[(int)receivedMessage.message.Type].Invoke(receivedMessage);
        }
        while(sendMessagesQueue.Count > 0) {
            AdressedMessage sendMessage = sendMessagesQueue.Dequeue();
            SendMessage(sendMessage);
        }
    }

    public void Dispose() {
        networkJobHandle.Complete();
        networkDriver.Dispose();
        connections.Dispose();
    }

    public void QeueMessage(AdressedMessage sendMessage) {
        sendMessagesQueue.Enqueue(sendMessage);
    }

    private void SendMessage(AdressedMessage adressedMessage) {
        var writer = networkDriver.BeginSend(connections[adressedMessage.connectionID]);
        adressedMessage.message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);

        if(!lastSendTimes.ContainsKey(adressedMessage.connectionID))
            lastSendTimes.Add(adressedMessage.connectionID, Time.time);
        else
            lastSendTimes[adressedMessage.connectionID] = Time.time;

        Debug.Log("Host Sending Message: " + adressedMessage.message.Type + ", to " + adressedMessage.connectionID);
    }
}