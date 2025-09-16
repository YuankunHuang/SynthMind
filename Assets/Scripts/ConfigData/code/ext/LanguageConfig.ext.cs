using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YuankunHuang.Unity.GameDataConfig
{
    /// <summary>
    /// LanguageConfig extension class
    /// Add your custom logic and methods here
    /// This file is NOT overwritten during build
    /// </summary>
    public partial class LanguageConfig : BaseConfigData<LanguageData>
    {
        // Add your custom methods here
        private static Dictionary<string, LanguageData> _langCodeDict;

        static partial void PostInitialize()
        {
            Debug.Log("[LanguageConfig] PostInitialize ENTER");

            try
            {
                _langCodeDict = new Dictionary<string, LanguageData>();
                Debug.Log("[LanguageConfig] Created _langCodeDict");

                var allData = GetAll();
                Debug.Log($"[LanguageConfig] GetAll() returned {allData?.Count() ?? 0} items");

                foreach (var data in allData)
                {
                    if (data != null)
                    {
                        Debug.Log($"[LanguageConfig] Adding language: {data.LangCode}");
                        _langCodeDict[data.LangCode] = data;
                    }
                    else
                    {
                        Debug.LogWarning("[LanguageConfig] Found null data item");
                    }
                }

                Debug.Log($"[LanguageConfig] PostInitialize COMPLETE - Added {_langCodeDict.Count} languages");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LanguageConfig] PostInitialize FAILED: {e.Message}");
                Debug.LogException(e);
            }
        }

        public static LanguageData GetByLangCode(string langCode)
        {
            if (_langCodeDict.TryGetValue(langCode, out var data))
            {
                return data;
            }

            return null;
        }
    }
}
