using System;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class CommPlayerAvatarData
    {
        public object Owner { get; private set; }
        public int Avatar { get; private set; }
        public string Name { get ; private set; }
        public Action OnClick { get; private set; }

        public GeneralWidgetConfig Config { get; set; }
        public string AssetKey => string.Format(AddressablePaths.PlayerAvatar, Avatar);

        public CommPlayerAvatarData(object owner, int avatar, string name, Action onClick)
        {
            Owner = owner;
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

        private static Dictionary<object, Dictionary<GeneralWidgetConfig, CommPlayerAvatarData>> _loaded = new();

        public static async void Show(GeneralWidgetConfig config, CommPlayerAvatarData data)
        {
            // if loaded
            if (_loaded.TryGetValue(data.Owner, out var loadedDict) && loadedDict.TryGetValue(config, out var loadedData))
            {
                // check if updated
                // if updated, reload (release old + load new)
                // otherwise, retain + return
                var toUpdate = false;

                //if (Config && data.Avatar != 0 ||
                //    !string.IsNullOrEmpty(loadedData.AssetKey) && data.Avatar == 0 ||
                //    loadedData.Avatar != data.Avatar)
                //{

                //}
            }


            // otherwise,
            // load & save

            var key = string.Format(AddressablePaths.PlayerAvatar, data.Avatar);
            var avatarSprtie = await ResManager.LoadAssetAsync<Sprite>(key);
        }

        public static void Release()
        {
            // release all data with the same owner

        }
    }
}