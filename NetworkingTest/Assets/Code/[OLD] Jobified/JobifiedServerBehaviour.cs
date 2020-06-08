using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Jobs;

public class JobifiedServerBehaviour : MonoBehaviour {
    public NetworkDriver m_Driver;
    public NativeList<NetworkConnection> m_Connections;
    private JobHandle ServerJobHandle;

    void Start() {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 6969;
        if(m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 6969");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }



    void OnDestroy() {
        ServerJobHandle.Complete();
        m_Connections.Dispose();
        m_Driver.Dispose();
    }



    void Update() {
        ServerJobHandle.Complete();
        var connectionJob = new ServerUpdateConnectionsJob {
            driver = m_Driver,
            connections = m_Connections
        };

        var serverUpdateJob = new ServerUpdateJob {
            driver = m_Driver.ToConcurrent(),
            connections = m_Connections.AsDeferredJobArray()
        };

        ServerJobHandle = m_Driver.ScheduleUpdate();
        ServerJobHandle = connectionJob.Schedule(ServerJobHandle);
        ServerJobHandle = serverUpdateJob.Schedule(m_Connections, 1, ServerJobHandle);
    }
}
