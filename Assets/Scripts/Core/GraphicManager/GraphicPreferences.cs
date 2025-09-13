using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.GraphicCore
{
    public static class GraphicPreferences
    {
        private const string RESOLUTION_WIDTH_KEY = "Graphic_Resolution_Width";
        private const string RESOLUTION_HEIGHT_KEY = "Graphic_Resolution_Height";
        private const string FULLSCREEN_MODE_KEY = "Graphic_FullScreenMode";
        private const string QUALITY_KEY = "Graphic_Quality";
        private const string FPS_LIMIT_KEY = "Graphic_FpsLimit";
        private const string VSYNC_KEY = "Graphic_VSync";

        public static Vector2Int Resolution
        {
            get
            {
                if (PlayerPrefsUtil.HasKey(RESOLUTION_WIDTH_KEY) && PlayerPrefsUtil.HasKey(RESOLUTION_HEIGHT_KEY))
                {
                    return new Vector2Int(PlayerPrefsUtil.GetInt(RESOLUTION_WIDTH_KEY), PlayerPrefsUtil.GetInt(RESOLUTION_HEIGHT_KEY));
                }

                var resolution = Screen.currentResolution;
                Resolution = new Vector2Int(resolution.width, resolution.height);
                LogHelper.Log($"No stored Resolution key. Current: {Screen.currentResolution}. Initialized to {Resolution}");
                return Resolution;
            }
            set
            {
                PlayerPrefsUtil.TrySetInt(RESOLUTION_WIDTH_KEY, value.x);
                PlayerPrefsUtil.TrySetInt(RESOLUTION_HEIGHT_KEY, value.y);
            }
        }

        public static FullScreenMode FullScreenMode
        {
            get
            {
                if (PlayerPrefsUtil.HasKey(FULLSCREEN_MODE_KEY))
                {
                    return (FullScreenMode)PlayerPrefsUtil.GetInt(FULLSCREEN_MODE_KEY);
                }

                FullScreenMode = Screen.fullScreenMode;

                LogHelper.Log($"No stored FullScreenMode key. Current: {Screen.fullScreenMode}. Initialized to {FullScreenMode}");
                return FullScreenMode;
            }
            set => PlayerPrefsUtil.TrySetInt(FULLSCREEN_MODE_KEY, (int)value);
        }

        public static GraphicQuality Quality
        {
            get
            {
                if (PlayerPrefsUtil.HasKey(QUALITY_KEY))
                {
                    return (GraphicQuality)PlayerPrefsUtil.GetInt(QUALITY_KEY);
                }

                Quality = (GraphicQuality)QualitySettings.GetQualityLevel();

                LogHelper.Log($"No stored Quality key. Current: {QualitySettings.GetQualityLevel()}. Initialized to {Quality}");
                return Quality;
            }
            set => PlayerPrefsUtil.TrySetInt(QUALITY_KEY, (int)value);
        }

        public static GraphicFPSLimit FPSLimit
        {
            get
            {
                if (PlayerPrefsUtil.HasKey(FPS_LIMIT_KEY))
                {
                    return (GraphicFPSLimit)PlayerPrefsUtil.GetInt(FPS_LIMIT_KEY);
                }

                switch (Application.targetFrameRate)
                {
                    case 30:
                        FPSLimit = GraphicFPSLimit.FPS_30;
                        break;
                    case 60:
                        FPSLimit = GraphicFPSLimit.FPS_60;
                        break;
                    default:
                        FPSLimit = GraphicFPSLimit.FPS_Default;
                        break;
                }

                LogHelper.Log($"No stored FPSLimit key. Current: {Application.targetFrameRate}. Initialized to {FPSLimit}");
                return FPSLimit;
            }
            set => PlayerPrefsUtil.TrySetInt(FPS_LIMIT_KEY, (int)value);
        }

        public static GraphicVSync VSync
        {
            get
            {
                if (PlayerPrefsUtil.HasKey(VSYNC_KEY))
                {
                    return (GraphicVSync)PlayerPrefsUtil.GetInt(VSYNC_KEY);
                }

                VSync = (GraphicVSync)QualitySettings.vSyncCount;
                LogHelper.Log($"No stored VSync key. Current: {QualitySettings.vSyncCount}. Initialized to {VSync}");
                return VSync;
            }
            set => PlayerPrefsUtil.TrySetInt(VSYNC_KEY, (int)value);
        }
    }
}