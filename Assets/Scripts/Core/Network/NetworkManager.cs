using System;

namespace YuankunHuang.Unity.Core
{
    public class NetworkManager : INetworkManager
    {
        public void Connect(string address, int port)
        {
            // Implement connection logic here
            LogHelper.Log($"Connecting to {address}:{port}");
            OnConnected?.Invoke();
        }

        public void Disconnect()
        {
            // Implement disconnection logic here
            LogHelper.Log("Disconnecting from network");
            OnDisconnected?.Invoke();
        }

        public bool IsConnected { get; private set; } = false;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;

        public RestApiClient RestApi { get; private set; }

        public NetworkManager()
        {
            RestApi = new RestApiClient();
            LogHelper.Log("NetworkManager initialized");
        }

        public void Dispose()
        {
            RestApi.Dispose();
            RestApi = null;

            Disconnect();

            LogHelper.Log("NetworkManager disposed");
        }
    }
}