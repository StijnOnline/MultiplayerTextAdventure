using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

struct ServerUpdateConnectionsJob : IJob {
    public NetworkDriver driver;
    public NativeList<NetworkConnection> connections;

    public void Execute() {
        // Clean up connections
        for(int i = 0; i < connections.Length; i++) {
            if(!connections[i].IsCreated) {
                connections.RemoveAtSwapBack(i);
                --i;
                Debug.Log("Cleaned a connection");
            }
        }
        // Accept new connections
        NetworkConnection c;
        while((c = driver.Accept()) != default(NetworkConnection)) {
            connections.Add(c);
            Debug.Log("Accepted a connection");
        }
    }
}