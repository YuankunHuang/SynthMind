using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
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
                _fullscreenModeDataList.Add(new TMP_Dropdown.OptionData(mode.ToString()));
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
                _qualityPresetDataList.Add(new TMP_Dropdown.OptionData(quality.ToString()));
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
                _fpsLimitDataList.Add(new TMP_Dropdown.OptionData(fpsLimit.ToString()));
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
                _vsyncDataList.Add(new TMP_Dropdown.OptionData(vsync.ToString()));
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
}