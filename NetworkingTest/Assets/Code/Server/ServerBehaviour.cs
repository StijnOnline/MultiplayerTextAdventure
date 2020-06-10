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

public class ServerBehaviour : MonoBehaviour {
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;
    private JobHandle networkJobHandle;

    private Queue<AdressedMessage> receivedMessagesQueue;
    private Queue<AdressedMessage> sendMessagesQueue;

    //Define Event
    public class AdressedMessageEvent : UnityEvent<AdressedMessage> { }
    public AdressedMessageEvent[] ServerCallbacks = new AdressedMessageEvent[(int)Message.MessageType.Count - 1];

    private ServerManager serverManager;

    void Start() {
        serverManager = new ServerManager(this);


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

        Debug.Log("Host Started");
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
        while(serverManager.lobby.players.Count < ServerManager.maxPlayers && (c = networkDriver.Accept()) != default) {
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
                    switch(messageType) {
                        case Message.MessageType.None: //stay alive
                            break;
                        case Message.MessageType.SetName:
                            var message = new SetNameMessage();
                            message.DeserializeObject(ref reader);
                            receivedMessagesQueue.Enqueue(new AdressedMessage(message, connections[i].InternalId));
                            break;
                        default:
                            break;
                    }
                } else if(cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        ProcessMessagesQueue();
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

    private void OnDestroy() {
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

        Debug.Log("Host Sending Message: " + adressedMessage.message.Type + ", to " + adressedMessage.connectionID);
    }
}