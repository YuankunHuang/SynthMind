using System;
using System.Collections.Generic;
using System.Linq;

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
            _langCodeDict = new Dictionary<string, LanguageData>();

            foreach (var data in GetAll())
            {
                if (data != null)
                {
                    _langCodeDict[data.langcode] = data;
                }
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
