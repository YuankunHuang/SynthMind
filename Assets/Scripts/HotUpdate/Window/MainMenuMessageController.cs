using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.HotUpdate
{
    public enum MainMenuMessageType
    {
        None,
        Self,
        Other,
    }

    public struct MainMenuMessageData
    {
        public string MessageId { get; private set; }
        public IAccount Sender { get; private set; }
        public string Content { get; private set; }
        public MainMenuMessageType Type
        {
            get
            {
                if (Sender == null)
                {
                    return MainMenuMessageType.None;
                }
                else if (Sender.UUID == ModuleRegistry.Get<IAccountManager>().Self.UUID)
                {
                    return MainMenuMessageType.Self;
                }
                else
                {
                    return MainMenuMessageType.Other;
                }
            }
        }

        public MainMenuMessageData(string messageId, IAccount sender, string content)
        {
            MessageId = messageId;
            Sender = sender;
            Content = content;
        }
    }

    public class MainMenuMessageController
    {
        private enum ExtraImg
        {
            Avatar = 0,
        }

        private enum ExtraTMP
        {
            Content = 0,
            Username = 1,
        }

        public static void Show(GeneralWidgetConfig config, MainMenuMessageData data)
        {
            if (config == null)
            {
                return;
            }

            config.ExtraTextMeshProList[(int)ExtraTMP.Content].text = data.Content;
            config.ExtraTextMeshProList[(int)ExtraTMP.Username].text = data.Sender != null
                ? data.Sender.Username
                : "Unknown User";
        }
    }
}