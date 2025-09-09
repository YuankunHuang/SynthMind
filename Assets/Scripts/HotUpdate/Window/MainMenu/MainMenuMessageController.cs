using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Util;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.AccountCore;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.AssetCore;

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
        public DateTime DeliveryTime { get; private set; }

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

        public MainMenuMessageData(string messageId, IAccount sender, string content, DateTime deliveryTime)
        {
            MessageId = messageId;
            Sender = sender;
            Content = content;
            DeliveryTime = deliveryTime;
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
            Nickname = 1,
        }

        public static void Show(GeneralWidgetConfig config, MainMenuMessageData data)
        {
            if (config == null)
            {
                return;
            }

            config.ExtraTextMeshProList[(int)ExtraTMP.Content].text = data.Content;
            config.ExtraTextMeshProList[(int)ExtraTMP.Nickname].text = data.Sender != null
                ? data.Sender.Nickname
                : "Unknown User";
            config.ExtraImageList[(int)ExtraImg.Avatar].sprite = ModuleRegistry.Get<IAssetManager>()
                .GetAsset<Sprite>(AvatarConfig.GetById(data.Sender?.Avatar ?? 0).Asset);

            config.CanvasGroup.CanvasGroupOn();
        }

        public static void Hide(GeneralWidgetConfig config)
        {
            if (config == null)
            {
                return;
            }

            config.CanvasGroup.CanvasGroupOff();
        }
    }
}