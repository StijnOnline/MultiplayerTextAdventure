using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

public class JobifiedClientBehaviour : MonoBehaviour {
    public NetworkDriver m_Driver;
    public NativeArray<NetworkConnection> m_Connection;
    public NativeArray<byte> m_Done;
    public JobHandle ClientJobHandle;
    public string TestMessage = "Test";

    void Start() {
        m_Driver = NetworkDriver.Create();
        m_Connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
        m_Done = new NativeArray<byte>(1, Allocator.Persistent);

        NetworkEndPoint endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 6969;
        m_Connection[0] = m_Driver.Connect(endpoint);
    }
    public void OnDestroy() {
        ClientJobHandle.Complete();

        m_Connection.Dispose();
        m_Driver.Dispose();
        m_Done.Dispose();
    }
    void Update() {
        ClientJobHandle.Complete();
        var job = new ClientUpdateJob {
            driver = m_Driver,
            connection = m_Connection,
            done = m_Done
        };

        ClientJobHandle = m_Driver.ScheduleUpdate();
        ClientJobHandle = job.Schedule(ClientJobHandle);
    }
}