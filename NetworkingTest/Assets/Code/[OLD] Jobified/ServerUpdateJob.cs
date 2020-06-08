using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Jobs;

struct ServerUpdateJob : IJobParallelForDefer {
    public NetworkDriver.Concurrent driver;
    public NativeArray<NetworkConnection> connections;
    public void Execute(int index) {
        DataStreamReader stream;
        Assert.IsTrue(connections[index].IsCreated);

        NetworkEvent.Type cmd;
        while((cmd = driver.PopEventForConnection(connections[index], out stream)) !=
        NetworkEvent.Type.Empty) {
            if(cmd == NetworkEvent.Type.Data) {


                NativeString64 message = stream.ReadString();
                if(message.ToString() != "Message Confirmed") {
                    Debug.Log("Received Message: " + message);

                    var writer = driver.BeginSend(connections[index]);
                    writer.WriteString("Message Confirmed");
                    driver.EndSend(writer);
                }



            } else if(cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client disconnected from server");
                connections[index] = default(NetworkConnection);
            }
        }
    }
}

