using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.AudioCore;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuSettingSoundController
    {
        #region UI References
        private enum ExtraConfig
        {
            MasterVolumeSlider = 0,
            BGMVolumeSlider = 1,
            SFXVolumeSlider = 2,
            MasterMuteToggle = 3,
        }

        private CommSliderController _masterVolumeController;
        private CommSliderController _bgmVolumeController;
        private CommSliderController _sfxVolumeController;
        private CommToggleController _masterMuteController;
        #endregion

        #region Fields
        private GeneralWidgetConfig _config;
        #endregion

        #region Lifecycle
        public MainMenuSettingSoundController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            _masterVolumeController = new CommSliderController(_config.ExtraWidgetConfigList[(int)ExtraConfig.MasterVolumeSlider]);
            _bgmVolumeController = new CommSliderController(_config.ExtraWidgetConfigList[(int)ExtraConfig.BGMVolumeSlider]);
            _sfxVolumeController = new CommSliderController(_config.ExtraWidgetConfigList[(int)ExtraConfig.SFXVolumeSlider]);
            _masterMuteController = new CommToggleController(_config.ExtraWidgetConfigList[(int)ExtraConfig.MasterMuteToggle]);

            _masterVolumeController.OnValueChanged += OnMasterVolumeChanged;
            _bgmVolumeController.OnValueChanged += OnBGMVolumeChanged;
            _sfxVolumeController.OnValueChanged += OnSFXVolumeChanged;
            _masterMuteController.OnValueChanged += OnMasterMuteChanged;
        }

        public void Refresh()
        {
            _masterVolumeController.SetValue(AudioPreferences.MasterVolume);
            _bgmVolumeController.SetValue(AudioPreferences.BGMVolume);
            _sfxVolumeController.SetValue(AudioPreferences.SFXVolume);
            _masterMuteController.SetValue(AudioPreferences.MasterMuted);
        }

        public void Dispose()
        {
            _masterVolumeController.OnValueChanged -= OnMasterVolumeChanged;
            _bgmVolumeController.OnValueChanged -= OnBGMVolumeChanged;
            _sfxVolumeController.OnValueChanged -= OnSFXVolumeChanged;
            _masterMuteController.OnValueChanged -= OnMasterMuteChanged;

            _masterVolumeController.Dispose();
            _bgmVolumeController.Dispose();
            _sfxVolumeController.Dispose();
        }
        #endregion

        #region UI Binding
        private void OnMasterVolumeChanged(float value)
        {
            ModuleRegistry.Get<IAudioManager>().SetMasterVolume(Mathf.RoundToInt(value));
        }

        private void OnBGMVolumeChanged(float value)
        {
            ModuleRegistry.Get<IAudioManager>().SetBGMVolume(Mathf.RoundToInt(value));
        }

        private void OnSFXVolumeChanged(float value)
        {
            ModuleRegistry.Get<IAudioManager>().SetSFXVolume(Mathf.RoundToInt(value));
        }

        private void OnMasterMuteChanged(bool value)
        {
            ModuleRegistry.Get<IAudioManager>().SetMasterMuted(value);
        }
        #endregion
    }
}