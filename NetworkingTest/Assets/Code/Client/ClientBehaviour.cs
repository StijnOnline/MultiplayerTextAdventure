using UnityEngine;
using UnityEditor;
using System.Collections;
using Unity.Networking.Transport;
using System.IO;
using Assets.Code;
using Unity.Jobs;
using System.Collections.Generic;
using Assets.Code.Client;
using UnityEditor.PackageManager;
using UnityEngine.Events;

public class ClientBehaviour : MonoBehaviour {
    private NetworkDriver networkDriver;
    private NetworkConnection connection;
    private JobHandle networkJobHandle;

    private Queue<Message> receivedMessagesQueue;
    private Queue<Message> sendMessagesQueue;

    //Define Event
    public class MessageEvent : UnityEvent<Message> { }
    public MessageEvent[] ClientCallbacks = new MessageEvent[(int)Message.MessageType.Count - 1];

    private ClientManager clientManager;

    public void Connect(string address) {
        Init();

        NetworkEndPoint endpoint;
        if(NetworkEndPoint.TryParse(address, 9000, out endpoint))
            Debug.LogError("Could Not Parse Address");

        connection = networkDriver.Connect(endpoint);
    }

    [ContextMenu("Connect to LocalHost")]
    public void ConnectLocalHost() {
        Init();

        NetworkEndPoint endpoint;
        endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;

        connection = networkDriver.Connect(endpoint);
    }

    private void Init() {
        clientManager = new ClientManager(this);
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
    }

    void Update() {
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
                switch(messageType) {
                    case Message.MessageType.None:
                        break;
                    case Message.MessageType.NewPlayer: {
                        var message = new NewPlayerMessage();
                        message.DeserializeObject(ref reader);
                        receivedMessagesQueue.Enqueue(message);
                        break;
                    }
                    case Message.MessageType.Welcome: {
                        var message = new WelcomeMessage();
                        message.DeserializeObject(ref reader);
                        receivedMessagesQueue.Enqueue(message);
                        break;
                    }
                    case Message.MessageType.RequestDenied:
                        break;
                    case Message.MessageType.PlayerLeft:
                        break;
                    case Message.MessageType.StartGame:
                        break;
                    default:
                        break;
                }
            } else if(cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Disconnected from server");
                connection = default;
            }
        }
        ProcessMessagesQueue();
        networkJobHandle = networkDriver.ScheduleUpdate();
    }

    private void OnDestroy() {
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

        Debug.Log("Client Sending Message: " + sendMessage.Type + ", to Host");
    }

    public void QeueMessage(Message sendMessage) {
        sendMessagesQueue.Enqueue(sendMessage);
    }



}
