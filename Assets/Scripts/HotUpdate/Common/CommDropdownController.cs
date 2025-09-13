using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class CommDropdownController
    {
        private GeneralWidgetConfig _config;
        public Action<int> OnValueChanged;

        #region UI Ref
        private enum ExtraObj
        {
            Dropdown = 0,
        }

        private TMP_Dropdown _dropdown;
        #endregion

        public CommDropdownController(GeneralWidgetConfig config)
        {
            _config = config;

            _dropdown = _config.ExtraObjectList[(int)ExtraObj.Dropdown].GetComponent<TMP_Dropdown>();
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        public void Refresh(List<TMP_Dropdown.OptionData> options, int current)
        {
            LogHelper.LogError($"Refresh 1");

            _dropdown.ClearOptions();

            LogHelper.LogError($"Refresh 2");

            _dropdown.AddOptions(options);

            LogHelper.LogError($"Refresh 3");

            _dropdown.SetValueWithoutNotify(current);
            _dropdown.RefreshShownValue();

            LogHelper.LogError($"Refresh 4");
        }

        private void OnDropdownValueChanged(int index)
        {
            OnValueChanged?.Invoke(index);
        }

        public void Dispose()
        {
            _dropdown.onValueChanged.RemoveAllListeners();
        }
    }
}