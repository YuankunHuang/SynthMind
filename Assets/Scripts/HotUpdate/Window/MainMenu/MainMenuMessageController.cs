using System;
using System.Collections;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;

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