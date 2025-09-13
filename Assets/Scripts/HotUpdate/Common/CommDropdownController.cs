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
            _dropdown.ClearOptions();

            _dropdown.AddOptions(options);

            _dropdown.SetValueWithoutNotify(current);
            _dropdown.RefreshShownValue();
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