using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

struct ClientUpdateJob : IJob {
    public NetworkDriver driver;
    public NativeArray<NetworkConnection> connection;
    public NativeArray<byte> done;

    public void Execute() {

        if(!connection[0].IsCreated) {
            if(done[0] != 1)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while((cmd = connection[0].PopEvent(driver, out stream)) != NetworkEvent.Type.Empty) {
            if(cmd == NetworkEvent.Type.Connect) {
                Debug.Log("We are now connected to the server");


                string TestMessage = "Test";

                DataStreamWriter writer = driver.BeginSend(connection[0]);
                writer.WriteString(TestMessage);
                driver.EndSend(writer);
            } else if(cmd == NetworkEvent.Type.Data) {
                string value = stream.ReadString().ToString();
                Debug.Log("Received: " + value);
                done[0] = 1;
                connection[0].Disconnect(driver);
                connection[0] = default(NetworkConnection);
            } else if(cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client got disconnected from server");
                connection[0] = default(NetworkConnection);
            }


        }
    }
}
