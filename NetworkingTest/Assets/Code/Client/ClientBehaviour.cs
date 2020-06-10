using UnityEngine;
using UnityEditor;
using System.Collections;
using Unity.Networking.Transport;
using System.IO;
using Assets.Code;
using Unity.Jobs;

public class ClientBehaviour : MonoBehaviour {
    private NetworkDriver networkDriver;
    private NetworkConnection connection;
    private JobHandle networkJobHandle;

    private Lobby lobby = new Lobby();


    public void Connect(string address) {
        networkDriver = NetworkDriver.Create();
        connection = default;

        NetworkEndPoint endpoint;
        if(NetworkEndPoint.TryParse(address, 9000, out endpoint))
            Debug.LogError("Could Not Parse Address");

        connection = networkDriver.Connect(endpoint);
    }

    [ContextMenu("Connect to LocalHost")]
    public void ConnectLocalHost() {
        networkDriver = NetworkDriver.Create();
        connection = default;

        NetworkEndPoint endpoint;
        endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;

        connection = networkDriver.Connect(endpoint);
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
                            var newPlayerMessage = new NewPlayerMessage();
                            newPlayerMessage.DeserializeObject(ref reader);
                            Debug.Log("Received NewPlayerMessage " + newPlayerMessage.PlayerName);

                            Player newPlayer = new Player(newPlayerMessage.PlayerColor);
                            newPlayer.name = newPlayerMessage.PlayerName;
                            lobby.AddPlayer(newPlayerMessage.PlayerID, newPlayer);

                            Menu.Instance.LobbyWindow.UpdatePlayers(lobby);

                            break;
                        }
                    case Message.MessageType.Welcome: {
                            var welcomeMessage = new WelcomeMessage();
                            welcomeMessage.DeserializeObject(ref reader);

                            Player newPlayer = new Player(welcomeMessage.Color);
                            newPlayer.name = Login.Username;
                            lobby.SetPlayer(welcomeMessage.PlayerID, newPlayer);

                            Menu.Instance.LobbyWindow.UpdatePlayers(lobby);

                            Debug.Log("Received Welcome Message", gameObject);
                            SetName(Login.Username);

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

        networkJobHandle = networkDriver.ScheduleUpdate();
    }

    private void OnDestroy() {
        networkJobHandle.Complete();
        networkDriver.Dispose();
    }

    private void SendMessage(Message message) {
        var writer = networkDriver.BeginSend(connection);
        message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);
    }

    public void SetName(string name) {
        var setNameMessage = new SetNameMessage {
            Name = name
        };
        SendMessage(setNameMessage);
    }



}
