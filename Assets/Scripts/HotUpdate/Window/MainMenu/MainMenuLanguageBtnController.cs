using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.AssetCore;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public struct MainMenuLanguageBtnData
    {
        public LanguageData LangData;
        public string DisplayName;
        public Action OnClick;

        public MainMenuLanguageBtnData(LanguageData langData, string displayName, Action onClick)
        {
            LangData = langData;
            DisplayName = displayName;
            OnClick = onClick;
        }
    }

    public class MainMenuLanguageBtnController
    {
        private enum ExtraTMP
        {
            Locale = 0,
        }
        
        private enum ExtraBtn
        {
            Clickable = 0,
        }

        private enum ExtraImg
        {
            Flag = 0,
        }

        private enum ExtraAtlas
        {
            Flag = 0,
        }

        private enum ExtraColorGradient
        {
            Unselected = 0,
            Selected = 1,
        }

        public static void SetSelected(GeneralWidgetConfig config, bool isSelected)
        {
            config.ExtraTextMeshProList[(int)ExtraTMP.Locale].colorGradientPreset = isSelected
                ? config.ExtraColorGradientList[(int)ExtraColorGradient.Selected]
                : config.ExtraColorGradientList[(int)ExtraColorGradient.Unselected];
        }

        public static void Show(GeneralWidgetConfig config, MainMenuLanguageBtnData data)
        {
            if (config == null)
            {
                LogHelper.LogError("[MainMenuLanguageBtnController] Config is null");
                return;
            }

            if (data.LangData == null)
            {
                LogHelper.LogError("[MainMenuLanguageBtnController] Language data is null");
                return;
            }

            try
            {
                // Show flag
                var flagImg = config.ExtraImageList[(int)ExtraImg.Flag];
                var flagAtlas = config.ExtraSpriteAtlasList[(int)ExtraAtlas.Flag];
                var flagSprite = flagAtlas.GetSprite(data.LangData.Icon);
                if (flagSprite != null)
                {
                    flagImg.sprite = flagSprite;
                }
                else
                {
                    LogHelper.LogWarning($"[MainMenuLanguageBtnController] Flag sprite not found: {data.LangData.Icon}");
                }

                // Show locale name
                var localeTxt = config.ExtraTextMeshProList[(int)ExtraTMP.Locale];
                if (localeTxt != null)
                {
                    localeTxt.text = data.DisplayName ?? data.LangData.LangCode ?? "Unknown";
                }

                // Set on click
                var clickable = config.ExtraButtonList[(int)ExtraBtn.Clickable];
                if (clickable != null)
                {
                    clickable.onClick.RemoveAllListeners();
                    if (data.OnClick != null)
                    {
                        clickable.onClick.AddListener(() =>
                        {
                            try
                            {
                                data.OnClick.Invoke();
                            }
                            catch (System.Exception ex)
                            {
                                LogHelper.LogError($"[MainMenuLanguageBtnController] Error in button click handler: {ex.Message}");
                            }
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuLanguageBtnController] Error in Show method: {ex.Message}");
            }
        }
    }
}