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
        public struct DropdownOptionData
        {
            public string OriginalText;
            public TMP_Dropdown.OptionData OptionData;

            public DropdownOptionData(string originalText, TMP_Dropdown.OptionData optionData)
            {
                OriginalText = originalText;
                OptionData = optionData;
            }
        }

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
        private List<DropdownOptionData> _resolutionDataList;
        private List<DropdownOptionData> _fullscreenModeDataList;
        private List<DropdownOptionData> _qualityPresetDataList;
        private List<DropdownOptionData> _vsyncDataList;
        private List<DropdownOptionData> _fpsLimitDataList;
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

            _resolutionDataList = new List<DropdownOptionData>();
            _fullscreenModeDataList = new List<DropdownOptionData>();
            _qualityPresetDataList = new List<DropdownOptionData>();
            _fpsLimitDataList = new List<DropdownOptionData>();
            _vsyncDataList = new List<DropdownOptionData>();

            _resolutionController.OnValueChanged += OnResolutionValueChanged;
            _fullscreenModeController.OnValueChanged += OnFullscreenValueChanged;
            _qualityPresetController.OnValueChanged += OnQualityPresetValueChanged;
            _fpsLimitController.OnValueChanged += OnFpsLimitValueChanged;
            _vsyncController.OnValueChanged += OnVsyncValueChanged;

            ModuleRegistry.Get<ILocalizationManager>().OnLanguageChanged += OnLanguageChanged;
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

            ModuleRegistry.Get<ILocalizationManager>().OnLanguageChanged -= OnLanguageChanged;
        }

        public void Refresh()
        {
            InitializeResolutionDropdown();

            // fullscreen mode - async initialization
            InitializeFullscreenModeDropdown();

            // quality preset - async initialization
            InitializeQualityPresetDropdown();

            // fps limit - async initialization
            InitializeFpsLimitDropdown();

            // vsync - async initialization
            InitializeVSyncDropdown();
        }
        #endregion

        private void OnResolutionValueChanged(int index)
        {
            var parts = _resolutionDataList[index].OriginalText.Trim().Split('x');
            var width = int.Parse(parts[0]);
            var height = int.Parse(parts[1]);
            ModuleRegistry.Get<IGraphicManager>().SetResolution(new Vector2Int(width, height));
        }

        private void OnFullscreenValueChanged(int index)
        {
            var fullscreenMode = Enum.Parse<FullScreenMode>(_fullscreenModeDataList[index].OriginalText);
            ModuleRegistry.Get<IGraphicManager>().SetFullScreenMode(fullscreenMode);
        }

        private void OnQualityPresetValueChanged(int index)
        {
            var quality = Enum.Parse<GraphicQuality>(_qualityPresetDataList[index].OriginalText);
            ModuleRegistry.Get<IGraphicManager>().SetQuality(quality);
        }

        private void OnFpsLimitValueChanged(int index)
        {
            var fpsLimit = Enum.Parse<GraphicFPSLimit>(_fpsLimitDataList[index].OriginalText);
            ModuleRegistry.Get<IGraphicManager>().SetFPSLimit(fpsLimit);
        }

        private void OnVsyncValueChanged(int index)
        {
            var vsync = Enum.Parse<GraphicVSync>(_vsyncDataList[index].OriginalText);
            ModuleRegistry.Get<IGraphicManager>().SetVSync(vsync);
        }

        private void OnLanguageChanged(string newLanguage)
        {
            try
            {
                LogHelper.Log($"[MainMenuSettingGraphicsController] Language changed event received: {newLanguage}");
                Refresh();
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingGraphicsController] Error handling language change: {ex.Message}");
            }
        }

        private void InitializeResolutionDropdown()
        {
            // resolution
            _resolutionDataList.Clear();
            var resOptions = new List<TMP_Dropdown.OptionData>();
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

                var optionData = new TMP_Dropdown.OptionData($"{res.width} x {res.height}");
                resOptions.Add(optionData);
                _resolutionDataList.Add(new DropdownOptionData($"{res.width} x {res.height}", optionData));

                if (currentRes.x == res.width && currentRes.y == res.height)
                {
                    currentResIdx = i;
                }
            }
            if (currentResIdx < 0)
            {
                LogHelper.LogError($"No valid Resolution item matched! currentRes: {currentRes}");
            }

            _resolutionController.Refresh(resOptions, currentResIdx);
        }

        private void InitializeFullscreenModeDropdown()
        {
            _fullscreenModeDataList.Clear();
            var currentFullscreenMode = GraphicPreferences.FullScreenMode;
            var fullscreenModes = (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode));

            // Get all localization keys for fullscreen modes
            var keys = new[] {
                LocalizationKeys.FullScreenModeExclusiveFullScreen,
                LocalizationKeys.FullScreenModeFullScreenWindow,
                LocalizationKeys.FullScreenModeMaximizedWindow,
                LocalizationKeys.FullScreenModeWindowed
            };

            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            locManager.GetLocalizedTexts(keys, (results) => {
                var fullscreenOptions = new List<TMP_Dropdown.OptionData>();
                var currentFullscreenIdx = -1;

                for (var i = 0; i < fullscreenModes.Length; ++i)
                {
                    var mode = fullscreenModes[i];
                    var key = GetFullscreenModeKey(mode);
                    var localizedName = results.ContainsKey(key) ? results[key] : mode.ToString();

                    var optionData = new TMP_Dropdown.OptionData(localizedName);
                    fullscreenOptions.Add(optionData);
                    _fullscreenModeDataList.Add(new DropdownOptionData(mode.ToString(), optionData));

                    if (mode == currentFullscreenMode)
                    {
                        currentFullscreenIdx = i;
                    }
                }

                if (currentFullscreenIdx < 0)
                {
                    LogHelper.LogError($"No valid FullScreenMode item matched! currentFullscreenMode: {currentFullscreenMode}");
                }

                _fullscreenModeController.Refresh(fullscreenOptions, currentFullscreenIdx);
            });
        }

        private string GetFullscreenModeKey(FullScreenMode mode)
        {
            return mode switch
            {
                FullScreenMode.ExclusiveFullScreen => LocalizationKeys.FullScreenModeExclusiveFullScreen,
                FullScreenMode.FullScreenWindow => LocalizationKeys.FullScreenModeFullScreenWindow,
                FullScreenMode.MaximizedWindow => LocalizationKeys.FullScreenModeMaximizedWindow,
                FullScreenMode.Windowed => LocalizationKeys.FullScreenModeWindowed,
                _ => LocalizationKeys.FullScreenModeWindowed
            };
        }

        private void InitializeQualityPresetDropdown()
        {
            _qualityPresetDataList.Clear();
            var currentQuality = GraphicPreferences.Quality;
            var qualities = (GraphicQuality[])Enum.GetValues(typeof(GraphicQuality));

            var keys = new[] {
                LocalizationKeys.CommonLow,
                LocalizationKeys.CommonMid,
                LocalizationKeys.CommonHigh
            };

            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            locManager.GetLocalizedTexts(keys, (results) => {
                var qualityOptions = new List<TMP_Dropdown.OptionData>();
                var currentQualityIdx = -1;

                for (var i = 0; i < qualities.Length; ++i)
                {
                    var quality = qualities[i];
                    var key = GetQualityKey(quality);
                    var localizedName = results.ContainsKey(key) ? results[key] : quality.ToString();

                    var optionData = new TMP_Dropdown.OptionData(localizedName);
                    qualityOptions.Add(optionData);
                    _qualityPresetDataList.Add(new DropdownOptionData(quality.ToString(), optionData));

                    if (quality == currentQuality)
                    {
                        currentQualityIdx = i;
                    }
                }

                if (currentQualityIdx < 0)
                {
                    LogHelper.LogError($"No valid QualityPreset item matched! currentQuality: {currentQuality}");
                }

                _qualityPresetController.Refresh(qualityOptions, currentQualityIdx);
            });
        }

        private string GetQualityKey(GraphicQuality quality)
        {
            return quality switch
            {
                GraphicQuality.Low => LocalizationKeys.CommonLow,
                GraphicQuality.Mid => LocalizationKeys.CommonMid,
                GraphicQuality.High => LocalizationKeys.CommonHigh,
                _ => LocalizationKeys.CommonMid
            };
        }

        private void InitializeFpsLimitDropdown()
        {
            _fpsLimitDataList.Clear();
            var currentFpsLimit = GraphicPreferences.FPSLimit;
            var fpsLimits = (GraphicFPSLimit[])Enum.GetValues(typeof(GraphicFPSLimit));

            var keys = new[] {
                LocalizationKeys.GraphicFPSLimitFPSDefault,
                LocalizationKeys.GraphicFPSLimitFPS30,
                LocalizationKeys.GraphicFPSLimitFPS60
            };

            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            locManager.GetLocalizedTexts(keys, (results) => {
                var fpsLimitOptions = new List<TMP_Dropdown.OptionData>();
                var currentFpsLimitIdx = -1;

                for (var i = 0; i < fpsLimits.Length; ++i)
                {
                    var fpsLimit = fpsLimits[i];
                    var key = GetFpsLimitKey(fpsLimit);
                    var localizedName = results.ContainsKey(key) ? results[key] : fpsLimit.ToString();

                    var optionData = new TMP_Dropdown.OptionData(localizedName);
                    fpsLimitOptions.Add(optionData);
                    _fpsLimitDataList.Add(new DropdownOptionData(fpsLimit.ToString(), optionData));

                    if (fpsLimit == currentFpsLimit)
                    {
                        currentFpsLimitIdx = i;
                    }
                }

                if (currentFpsLimitIdx < 0)
                {
                    LogHelper.LogError($"No valid FPSLimit item matched! currentFpsLimit: {currentFpsLimit}");
                }

                _fpsLimitController.Refresh(fpsLimitOptions, currentFpsLimitIdx);
            });
        }

        private string GetFpsLimitKey(GraphicFPSLimit fpsLimit)
        {
            return fpsLimit switch
            {
                GraphicFPSLimit.FPS_Default => LocalizationKeys.GraphicFPSLimitFPSDefault,
                GraphicFPSLimit.FPS_30 => LocalizationKeys.GraphicFPSLimitFPS30,
                GraphicFPSLimit.FPS_60 => LocalizationKeys.GraphicFPSLimitFPS60,
                _ => LocalizationKeys.GraphicFPSLimitFPSDefault
            };
        }

        private void InitializeVSyncDropdown()
        {
            _vsyncDataList.Clear();
            var currentVSync = GraphicPreferences.VSync;
            var vsyncs = (GraphicVSync[])Enum.GetValues(typeof(GraphicVSync));

            var keys = new[] {
                LocalizationKeys.GraphicVSyncOff,
                LocalizationKeys.GraphicVSyncEveryFrame,
                LocalizationKeys.GraphicVSyncEveryTwoFrames
            };

            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            locManager.GetLocalizedTexts(keys, (results) => {
                var vsyncOptions = new List<TMP_Dropdown.OptionData>();
                var currentVsyncIdx = -1;

                for (var i = 0; i < vsyncs.Length; ++i)
                {
                    var vsync = vsyncs[i];
                    var key = GetVSyncKey(vsync);
                    var localizedName = results.ContainsKey(key) ? results[key] : vsync.ToString();

                    var optionData = new TMP_Dropdown.OptionData(localizedName);
                    vsyncOptions.Add(optionData);
                    _vsyncDataList.Add(new DropdownOptionData(vsync.ToString(), optionData));

                    if (vsync == currentVSync)
                    {
                        currentVsyncIdx = i;
                    }
                }

                if (currentVsyncIdx < 0)
                {
                    LogHelper.LogError($"No valid VSync item matched! currentVSync: {currentVSync}");
                }

                _vsyncController.Refresh(vsyncOptions, currentVsyncIdx);
            });
        }

        private string GetVSyncKey(GraphicVSync vsync)
        {
            return vsync switch
            {
                GraphicVSync.Off => LocalizationKeys.GraphicVSyncOff,
                GraphicVSync.EveryFrame => LocalizationKeys.GraphicVSyncEveryFrame,
                GraphicVSync.EveryTwoFrames => LocalizationKeys.GraphicVSyncEveryTwoFrames,
                _ => LocalizationKeys.GraphicVSyncOff
            };
        }
    }

}