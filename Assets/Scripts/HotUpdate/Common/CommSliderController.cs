using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class CommSliderController
    {
        private GeneralWidgetConfig _config;
        private Slider _slider;

        public Action<float> OnValueChanged;

        private enum ExtraObj
        {
            Slider = 0,
        }

        public CommSliderController(GeneralWidgetConfig config)
        {
            _config = config;

            _slider = _config.ExtraObjectList[(int)ExtraObj.Slider].GetComponent<Slider>();

            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        public void Dispose()
        {
            _slider.onValueChanged.RemoveAllListeners();
        }

        private void OnSliderValueChanged(float value)
        {
            OnValueChanged?.Invoke(value);
        }

        public (float, float) GetMinMax()
        {
            return (_slider.minValue, _slider.maxValue);
        }

        public void SetMinMax(float minValue, float maxValue)
        {
            _slider.minValue = minValue;
            _slider.maxValue = maxValue;
        }

        public void SetValueWithoutNotify(int value)
        {
            _slider.SetValueWithoutNotify(value);
        }

        public void SetValue(int value)
        {
            _slider.value = value;
        }
    }
}