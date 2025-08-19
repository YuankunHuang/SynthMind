using System;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.AssetCore;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class CommPlayerAvatarData
    {
        public int Avatar { get; private set; }
        public string Name { get ; private set; }
        public Action OnClick { get; private set; }

        public CommPlayerAvatarData(int avatar, string name, Action onClick)
        {
            Avatar = avatar;
            Name = name;
            OnClick = onClick;
        }
    }

    public class CommPlayerAvatarController
    {
        private enum ExtraImg
        {
            Avatar = 0,
        }

        private enum ExtraTMP
        {
            Name = 0,
        }

        private enum ExtraBtn
        {
            Clickable = 0,
        }

        public static void Show(GeneralWidgetConfig config, CommPlayerAvatarData data)
        {
            ShowAvatar(config, data.Avatar);
            ShowName(config, data.Name);
            SetOnClick(config, data.OnClick);
        }

        private static void SetOnClick(GeneralWidgetConfig config, Action onClick)
        {
            var btn = config.ExtraButtonList[(int)ExtraBtn.Clickable];
            btn.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick());
            }
        }

        private static void ShowName(GeneralWidgetConfig config, string name)
        {
            var nameTmp = config.ExtraTextMeshProList[(int)ExtraTMP.Name];
            nameTmp.text = name;
        }

        private static void ShowAvatar(GeneralWidgetConfig config, int avatarId)
        {
            var avatarData = AvatarConfig.GetById(avatarId);
            var assetManager = ModuleRegistry.Get<IAssetManager>();
            var sprite = assetManager.GetAsset<Sprite>(avatarData.asset);

            var avatarImg = config.ExtraImageList[(int)ExtraImg.Avatar];
            avatarImg.sprite = sprite;
        }
    }
}