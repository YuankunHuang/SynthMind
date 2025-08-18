using System;
using System.Collections.Generic;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.NetworkCore
{
    public enum ServerType
    {
        ChatAI,
        ChatPlayer,
    }

    public interface INetworkManager : IModule
    {
        void Connect(string address, int port);
        void Disconnect();
        bool IsConnected { get; }
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<string> OnError;

        void SendMessage(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata, ServerType server, Action<string> onSuccess, Action<string> onError);
    }
}