using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.GraphicCore;
using YuankunHuang.Unity.LocalizationCore;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuSettingGraphicsController
    {
        #region UI References
        private enum ExtraConfig
        {
            Resolution = 0,
            FullScreenMode = 1,
            QualityPreset = 2,
            FPSLimit = 3,
            VSync = 4,
        }

        private CommDropdownController _resolutionController;
        private CommDropdownController _fullscreenModeController;
        private CommDropdownController _qualityPresetController;
        private CommDropdownController _fpsLimitController;
        private CommDropdownController _vsyncController;
        #endregion

        #region Fields
        private GeneralWidgetConfig _config;
        private List<TMP_Dropdown.OptionData> _resolutionDataList;
        private List<TMP_Dropdown.OptionData> _fullscreenModeDataList;
        private List<TMP_Dropdown.OptionData> _qualityPresetDataList;
        private List<TMP_Dropdown.OptionData> _vsyncDataList;
        private List<TMP_Dropdown.OptionData> _fpsLimitDataList;
        #endregion

        #region Lifecycle
        public MainMenuSettingGraphicsController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            _resolutionController = new CommDropdownController(_config.ExtraWidgetConfigList[(int)ExtraConfig.Resolution]);
            _fullscreenModeController = new CommDropdownController(_config.ExtraWidgetConfigList[(int)ExtraConfig.FullScreenMode]);
            _qualityPresetController = new CommDropdownController(_config.ExtraWidgetConfigList[(int)ExtraConfig.QualityPreset]);
            _fpsLimitController = new CommDropdownController(_config.ExtraWidgetConfigList[(int)ExtraConfig.FPSLimit]);
            _vsyncController = new CommDropdownController(_config.ExtraWidgetConfigList[(int)ExtraConfig.VSync]);

            _resolutionDataList = new List<TMP_Dropdown.OptionData>();
            _fullscreenModeDataList = new List<TMP_Dropdown.OptionData>();
            _qualityPresetDataList = new List<TMP_Dropdown.OptionData>();
            _fpsLimitDataList = new List<TMP_Dropdown.OptionData>();
            _vsyncDataList = new List<TMP_Dropdown.OptionData>();

            _resolutionController.OnValueChanged += OnResolutionValueChanged;
            _fullscreenModeController.OnValueChanged += OnFullscreenValueChanged;
            _qualityPresetController.OnValueChanged += OnQualityPresetValueChanged;
            _fpsLimitController.OnValueChanged += OnFpsLimitValueChanged;
            _vsyncController.OnValueChanged += OnVsyncValueChanged;
        }

        public void Refresh()
        {
            // resolution
            _resolutionDataList.Clear();
            var currentRes = GraphicPreferences.Resolution;
            var currentResIdx = -1;
            var seenRes = new HashSet<Vector2Int>();
            for (var i = 0; i < Screen.resolutions.Length; ++i)
            {
                var res = Screen.resolutions[i];

                // avoid duplicate
                if (!seenRes.Add(new Vector2Int(res.width, res.height)))
                {
                    continue;
                }

                _resolutionDataList.Add(new TMP_Dropdown.OptionData($"{res.width} x {res.height}"));

                if (currentRes.x == res.width && currentRes.y == res.height)
                {
                    currentResIdx = i;
                }
            }
            if (currentResIdx < 0)
            {
                LogHelper.LogError($"No valid Resolution item matched! currentRes: {currentRes}");
            }
            _resolutionController.Refresh(_resolutionDataList, currentResIdx);

            // fullscreen mode
            _fullscreenModeDataList.Clear();
            var currentFullscreenMode = GraphicPreferences.FullScreenMode;
            var currentFullscreenIdx = -1;
            var fullscreenModes = (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode));
            for (var i = 0; i < fullscreenModes.Length; ++i)
            {
                var mode = fullscreenModes[i];
                var locName = mode.GetLocalizedName();
                
                LogHelper.LogError($"locName: {locName}");

                _fullscreenModeDataList.Add(new TMP_Dropdown.OptionData(locName));

                if (mode == currentFullscreenMode)
                {
                    currentFullscreenIdx = i;
                }
            }
            if (currentFullscreenIdx < 0)
            {
                LogHelper.LogError($"No valid FullScreenMode item matched! currentFullscreenMode: {currentFullscreenMode}");
            }
            _fullscreenModeController.Refresh(_fullscreenModeDataList, currentFullscreenIdx);

            // quality preset
            _qualityPresetDataList.Clear();
            var currentQuality = GraphicPreferences.Quality;
            var currentQualityIdx = -1;
            var qualities = (GraphicQuality[])Enum.GetValues(typeof(GraphicQuality));
            for (var i = 0; i < qualities.Length; ++i)
            {
                var quality = qualities[i];
                var locName = quality.GetLocalizedName();
                
                LogHelper.LogError($"locName: {locName}");

                _qualityPresetDataList.Add(new TMP_Dropdown.OptionData(locName));
                if (quality == currentQuality)
                {
                    currentQualityIdx = i;
                }
            }
            if (currentQualityIdx < 0)
            {
                LogHelper.LogError($"No valid QualityPreset item matched! currentQuality: {currentQuality}");
            }
            _qualityPresetController.Refresh(_qualityPresetDataList, currentQualityIdx);

            // fps limit
            _fpsLimitDataList.Clear();
            var currentFpsLimit = GraphicPreferences.FPSLimit;
            var currentFpsLimitIdx = -1;
            var fpsLimits = (GraphicFPSLimit[])Enum.GetValues(typeof(GraphicFPSLimit));
            for (var i = 0; i < fpsLimits.Length; ++i)
            {
                var fpsLimit = fpsLimits[i];
                var locName = fpsLimit.GetLocalizedName();

                LogHelper.LogError($"locName: {locName}");

                _fpsLimitDataList.Add(new TMP_Dropdown.OptionData(locName));
                if (fpsLimit == currentFpsLimit)
                {
                    currentFpsLimitIdx = i;
                }
            }
            if (currentFpsLimitIdx < 0)
            {
                LogHelper.LogError($"No valid FPSLimit item matched! currentFpsLimit: {currentFpsLimit}");
            }
            _fpsLimitController.Refresh(_fpsLimitDataList, currentFpsLimitIdx);

            // vsync
            _vsyncDataList.Clear();
            var currentVSync = GraphicPreferences.VSync;
            var currentVsyncIdx = -1;
            var vsyncs = (GraphicVSync[])Enum.GetValues(typeof(GraphicVSync));
            for (var i = 0; i < vsyncs.Length; ++i)
            {
                var vsync = vsyncs[i];
                var locName = vsync.GetLocalizedName();

                LogHelper.LogError($"locName: {locName}");

                _vsyncDataList.Add(new TMP_Dropdown.OptionData(locName));
                if (vsync == currentVSync)
                {
                    currentVsyncIdx = i;
                }
            }
            if (currentVsyncIdx < 0)
            {
                LogHelper.LogError($"No valid VSync item matched! currentVSync: {currentVSync}");
            }
            _vsyncController.Refresh(_vsyncDataList, currentVsyncIdx);
        }

        public void Dispose()
        {
            _resolutionDataList = null;
            _fullscreenModeDataList = null;
            _qualityPresetDataList = null;
            _fpsLimitDataList = null;
            _vsyncDataList = null;

            _resolutionController.Dispose();
            _fullscreenModeController.Dispose();
            _qualityPresetController.Dispose();
            _fpsLimitController.Dispose();
            _vsyncController.Dispose();

            _resolutionController.OnValueChanged -= OnResolutionValueChanged;
            _fullscreenModeController.OnValueChanged -= OnFullscreenValueChanged;
            _qualityPresetController.OnValueChanged -= OnQualityPresetValueChanged;
            _fpsLimitController.OnValueChanged -= OnFpsLimitValueChanged;
            _vsyncController.OnValueChanged -= OnVsyncValueChanged;
        }
        #endregion

        private void OnResolutionValueChanged(int index)
        {
            var parts = _resolutionDataList[index].text.Trim().Split('x');
            var width = int.Parse(parts[0]);
            var height = int.Parse(parts[1]);
            ModuleRegistry.Get<IGraphicManager>().SetResolution(new Vector2Int(width, height));
        }

        private void OnFullscreenValueChanged(int index)
        {
            var fullscreenMode = Enum.Parse<FullScreenMode>(_fullscreenModeDataList[index].text);
            ModuleRegistry.Get<IGraphicManager>().SetFullScreenMode(fullscreenMode);
        }

        private void OnQualityPresetValueChanged(int index)
        {
            var quality = Enum.Parse<GraphicQuality>(_qualityPresetDataList[index].text);
            ModuleRegistry.Get<IGraphicManager>().SetQuality(quality);
        }

        private void OnFpsLimitValueChanged(int index)
        {
            var fpsLimit = Enum.Parse<GraphicFPSLimit>(_fpsLimitDataList[index].text);
            ModuleRegistry.Get<IGraphicManager>().SetFPSLimit(fpsLimit);
        }

        private void OnVsyncValueChanged(int index)
        {
            var vsync = Enum.Parse<GraphicVSync>(_vsyncDataList[index].text);
            ModuleRegistry.Get<IGraphicManager>().SetVSync(vsync);
        }
    }

    public static class FullScreenModeExtensions
    {
        public static string GetLocalizedName(this FullScreenMode mode)
        {
            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            switch (mode)
            {
                case FullScreenMode.ExclusiveFullScreen:
                    return locManager.GetLocalizedText(LocalizationKeys.FullScreenModeExclusiveFullScreen);
                case FullScreenMode.FullScreenWindow:
                    return locManager.GetLocalizedText(LocalizationKeys.FullScreenModeFullScreenWindow);
                case FullScreenMode.MaximizedWindow:
                    return locManager.GetLocalizedText(LocalizationKeys.FullScreenModeMaximizedWindow);
                case FullScreenMode.Windowed:
                    return locManager.GetLocalizedText(LocalizationKeys.FullScreenModeWindowed);
                default:
                    return "#UNDEFINED#";
            }
        }
    }

    public static class GraphicQualityExtensions
    {
        public static string GetLocalizedName(this GraphicQuality quality)
        {
            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            switch (quality)
            {
                case GraphicQuality.Low:
                    return locManager.GetLocalizedText(LocalizationKeys.CommonLow);
                case GraphicQuality.Mid:
                    return locManager.GetLocalizedText(LocalizationKeys.CommonMid);
                case GraphicQuality.High:
                    return locManager.GetLocalizedText(LocalizationKeys.CommonHigh);
                default:
                    return "#UNDEFINED#";
            }
        }
    }

    public static class GraphicFPSLimitExtensions
    {
        public static string GetLocalizedName(this GraphicFPSLimit fpsLimit)
        {
            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            switch (fpsLimit)
            {
                case GraphicFPSLimit.FPS_Default:
                    return locManager.GetLocalizedText(LocalizationKeys.GraphicFPSLimitFPSDefault);
                case GraphicFPSLimit.FPS_30:
                    return locManager.GetLocalizedText(LocalizationKeys.GraphicFPSLimitFPS30);
                case GraphicFPSLimit.FPS_60:
                    return locManager.GetLocalizedText(LocalizationKeys.GraphicFPSLimitFPS60);
                default:
                    return "#UNDEFINED#";
            }
        }
    }

    public static class GraphicVSyncExtensions
    {
        public static string GetLocalizedName(this GraphicVSync vsync)
        {
            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            switch (vsync)
            {
                case GraphicVSync.Off:
                    return locManager.GetLocalizedText(LocalizationKeys.GraphicVSyncOff);
                case GraphicVSync.EveryFrame:
                    return locManager.GetLocalizedText(LocalizationKeys.GraphicVSyncEveryFrame);
                case GraphicVSync.EveryTwoFrames:
                    return locManager.GetLocalizedText(LocalizationKeys.GraphicVSyncEveryTwoFrames);
                default:
                    return "#UNDEFINED#";
            }
        }
    }
}