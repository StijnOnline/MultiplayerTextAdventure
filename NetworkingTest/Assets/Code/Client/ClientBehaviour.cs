﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using Unity.Networking.Transport;
using System.IO;
using Assets.Code;
using Unity.Jobs;
using System.Collections.Generic;
using Assets.Code.Client;
using UnityEngine.Events;

public class ClientBehaviour {
    private NetworkDriver networkDriver;
    private NetworkConnection connection;
    private JobHandle networkJobHandle;

    private Queue<Message> receivedMessagesQueue;
    private Queue<Message> sendMessagesQueue;

    //Define Event
    public class MessageEvent : UnityEvent<Message> { }
    public MessageEvent[] ClientCallbacks = new MessageEvent[(int)Message.MessageType.Count - 1];

    private ClientManager clientManager;

    private float lastSendTime = 0;
    private const float STAY_ALIVE_AFTER_SECONDS = 20;

    public ClientBehaviour(ClientManager clientManager) {
        this.clientManager = clientManager;
    }

    public void Connect(string address) {
        Init();

        NetworkEndPoint endpoint;
        if(NetworkEndPoint.TryParse(address, 9000, out endpoint))
            Debug.LogError("Could Not Parse Address");

        connection = networkDriver.Connect(endpoint);
    }

    public void ConnectLocalHost() {
        Init();

        NetworkEndPoint endpoint;
        endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;

        connection = networkDriver.Connect(endpoint);
    }

    private void Init() {
        networkDriver = NetworkDriver.Create();
        connection = default;

        receivedMessagesQueue = new Queue<Message>();
        sendMessagesQueue = new Queue<Message>();

        //assign all eventlisteners
        for(int i = 0; i < ClientCallbacks.Length; i++) {
            ClientCallbacks[i] = new MessageEvent();
        }
        ClientCallbacks[(int)Message.MessageType.NewPlayer].AddListener(clientManager.HandleNewPlayer);
        ClientCallbacks[(int)Message.MessageType.Welcome].AddListener(clientManager.HandleWelcome);
        ClientCallbacks[(int)Message.MessageType.RequestDenied].AddListener(clientManager.HandleRequestDenied);
        ClientCallbacks[(int)Message.MessageType.PlayerLeft].AddListener(clientManager.HandlePlayerLeft);
        ClientCallbacks[(int)Message.MessageType.StartGame].AddListener(clientManager.HandleStartGame);
        ClientCallbacks[(int)Message.MessageType.PlayerTurn].AddListener(clientManager.HandlePlayerTurn);
        ClientCallbacks[(int)Message.MessageType.RoomInfo].AddListener(clientManager.HandleRoomInfo);
        ClientCallbacks[(int)Message.MessageType.PlayerEnterRoom].AddListener(clientManager.HandlePlayerEnterRoom);
        ClientCallbacks[(int)Message.MessageType.PlayerLeaveRoom].AddListener(clientManager.HandlePlayerLeaveRoom);
        ClientCallbacks[(int)Message.MessageType.ObtainTreasure].AddListener(clientManager.HandleObtainTreasure);
        ClientCallbacks[(int)Message.MessageType.HitMonster].AddListener(clientManager.HandleHitMonster);
        ClientCallbacks[(int)Message.MessageType.HitByMonster].AddListener(clientManager.HandleHitByMonster);
        ClientCallbacks[(int)Message.MessageType.PlayerDefends].AddListener(clientManager.HandlePlayerDefends);
        ClientCallbacks[(int)Message.MessageType.PlayerLeftDungeon].AddListener(clientManager.HandlePlayerLeftDungeon);
        ClientCallbacks[(int)Message.MessageType.PlayerDies].AddListener(clientManager.HandlePlayerDies);
        ClientCallbacks[(int)Message.MessageType.EndGame].AddListener(clientManager.HandleEndGame);
    }

    public void Update() {
        networkJobHandle.Complete();

        if(!connection.IsCreated) {
            return;
        }

        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while((cmd = connection.PopEvent(networkDriver, out reader)) != NetworkEvent.Type.Empty) {
            if(cmd == NetworkEvent.Type.Connect) {
                Debug.Log("Connected to server");
            } else if(cmd == NetworkEvent.Type.Data) {
                var messageType = (Message.MessageType)reader.ReadUShort();
                Debug.Log("Client Received: " + messageType + " from Host");

                Message message = null;
                switch(messageType) {
                    case Message.MessageType.None: break;
                    case Message.MessageType.NewPlayer: message = new NewPlayerMessage(); break;
                    case Message.MessageType.Welcome: message = new WelcomeMessage(); break;
                    case Message.MessageType.RequestDenied: message = new RequestDeniedMessage(); break;
                    case Message.MessageType.PlayerLeft: message = new PlayerLeftMessage(); break;
                    case Message.MessageType.StartGame: message = new StartGameMessage(); break;
                    case Message.MessageType.PlayerTurn: message = new PlayerTurnMessage(); break;
                    case Message.MessageType.RoomInfo: message = new RoomInfoMessage(); break;
                    case Message.MessageType.PlayerEnterRoom: message = new PlayerEnterRoomMessage(); break;
                    case Message.MessageType.PlayerLeaveRoom: message = new PlayerLeaveRoomMessage(); break;
                    case Message.MessageType.ObtainTreasure: message = new ObtainTreasureMessage(); break;
                    case Message.MessageType.HitMonster: message = new HitMonserMessage(); break;
                    case Message.MessageType.HitByMonster: message = new HitByMonserMessage(); break;
                    case Message.MessageType.PlayerDefends: message = new PlayerDefendsMessage(); break;
                    case Message.MessageType.PlayerLeftDungeon: message = new PlayerLeftDungeonMessage(); break;
                    case Message.MessageType.PlayerDies: message = new PlayerDiesMessage(); break;
                    case Message.MessageType.EndGame:   break;
                    default: break;
                }
                if(message != null) {
                    message.DeserializeObject(ref reader);
                    receivedMessagesQueue.Enqueue(message);
                }
            } else if(cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Disconnected from server");
                connection = default;
            }
        }
        ProcessMessagesQueue();

        if(Time.time - lastSendTime > STAY_ALIVE_AFTER_SECONDS) {
            SendMessage(new NoneMessage());
        }
        networkJobHandle = networkDriver.ScheduleUpdate();
    }

    public void Dispose() {
        networkJobHandle.Complete();
        networkDriver.Dispose();
    }

    private void ProcessMessagesQueue() {
        while(receivedMessagesQueue.Count > 0) {
            Message receivedMessage = receivedMessagesQueue.Dequeue();
            ClientCallbacks[(int)receivedMessage.Type].Invoke(receivedMessage);
        }
        while(sendMessagesQueue.Count > 0) {
            Message sendMessage = sendMessagesQueue.Dequeue();
            SendMessage(sendMessage);
        }
    }

    private void SendMessage(Message sendMessage) {
        var writer = networkDriver.BeginSend(connection);
        sendMessage.SerializeObject(ref writer);
        networkDriver.EndSend(writer);
        lastSendTime = Time.time;

        Debug.Log("Client Sending Message: " + sendMessage.Type + " to Host");
    }

    public void QeueMessage(Message sendMessage) {
        sendMessagesQueue.Enqueue(sendMessage);
    }



}
