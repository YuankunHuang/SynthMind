using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class CommToggleController
    {
        private GeneralWidgetConfig _config;

        public Action<bool> OnValueChanged;

        #region UI Ref
        private enum ExtraObj
        {
            Toggle = 0,
        }

        private Toggle _toggle;
        #endregion

        public CommToggleController(GeneralWidgetConfig config)
        {
            _config = config;

            _toggle = _config.ExtraObjectList[(int)ExtraObj.Toggle].GetComponent<Toggle>();

            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        
        public void Dispose()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }

        public void SetValue(bool isOn)
        {
            _toggle.isOn = isOn;
        }

        private void OnToggleValueChanged(bool value)
        {
            OnValueChanged?.Invoke(value);
        }
    }
}