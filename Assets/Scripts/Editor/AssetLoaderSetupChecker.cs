using UnityEngine;
using UnityEditor;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.Editor
{
    /// <summary>
    /// AssetLoader设置检查工具
    /// </summary>
    public class AssetLoaderSetupChecker : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool[] checkResults = new bool[5];

        [MenuItem("SynthMind/Tools/设置检查工具")]
        public static void ShowWindow()
        {
            GetWindow<AssetLoaderSetupChecker>("AssetLoader设置检查");
        }

        private void OnGUI()
        {
            GUILayout.Label("AssetLoader 设置检查", EditorStyles.boldLabel);
            
            if (GUILayout.Button("运行检查", GUILayout.Height(30)))
            {
                RunAllChecks();
            }

            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DisplayCheckResults();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 运行所有检查
        /// </summary>
        private void RunAllChecks()
        {
            checkResults[0] = CheckAddressablesSetup();
            checkResults[1] = CheckSpriteAtlases();
            checkResults[2] = CheckSpriteAssets();
            checkResults[3] = CheckSmartAssetLoader();
            checkResults[4] = CheckBuildSettings();
        }

        /// <summary>
        /// 检查Addressables设置
        /// </summary>
        /// <returns>是否通过</returns>
        private bool CheckAddressablesSetup()
        {
            try
            {
                var groups = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings?.groups;
                if (groups == null || groups.Count == 0)
                {
                    return false;
                }

                // 检查是否有UI相关的Group
                bool hasUIGroup = false;
                foreach (var group in groups)
                {
                    if (group.name.ToLower().Contains("ui") || group.name.ToLower().Contains("sprite"))
                    {
                        hasUIGroup = true;
                        break;
                    }
                }

                return hasUIGroup;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查SpriteAtlas
        /// </summary>
        /// <returns>是否通过</returns>
        private bool CheckSpriteAtlases()
        {
            var atlases = Resources.FindObjectsOfTypeAll<UnityEngine.U2D.SpriteAtlas>();
            return atlases.Length > 0;
        }

        /// <summary>
        /// 检查Sprite资源
        /// </summary>
        /// <returns>是否通过</returns>
        private bool CheckSpriteAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite");
            return guids.Length > 0;
        }

        /// <summary>
        /// 检查SmartAssetLoader
        /// </summary>
        /// <returns>是否通过</returns>
        private bool CheckSmartAssetLoader()
        {
            // 检查SmartAssetLoader类是否存在
            var type = typeof(SmartAssetLoader);
            return type != null;
        }

        /// <summary>
        /// 检查构建设置
        /// </summary>
        /// <returns>是否通过</returns>
        private bool CheckBuildSettings()
        {
            // 检查是否有场景在构建设置中
            var scenes = EditorBuildSettings.scenes;
            return scenes.Length > 0;
        }

        /// <summary>
        /// 显示检查结果
        /// </summary>
        private void DisplayCheckResults()
        {
            string[] checkNames = {
                "Addressables设置",
                "SpriteAtlas配置",
                "Sprite资源",
                "SmartAssetLoader",
                "构建设置"
            };

            string[] descriptions = {
                "检查Addressables Groups是否已创建",
                "检查是否有SpriteAtlas配置",
                "检查项目中是否有Sprite资源",
                "检查SmartAssetLoader类是否可用",
                "检查构建设置是否正确"
            };

            for (int i = 0; i < checkNames.Length; i++)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                // 状态图标
                string statusIcon = checkResults[i] ? "✅" : "❌";
                EditorGUILayout.LabelField(statusIcon, GUILayout.Width(30));
                
                // 检查名称和描述
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(checkNames[i], EditorStyles.boldLabel);
                EditorGUILayout.LabelField(descriptions[i], EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                
                // 修复按钮
                if (!checkResults[i])
                {
                    if (GUILayout.Button("修复", GUILayout.Width(60)))
                    {
                        FixIssue(i);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// 修复问题
        /// </summary>
        /// <param name="issueIndex">问题索引</param>
        private void FixIssue(int issueIndex)
        {
            switch (issueIndex)
            {
                case 0: // Addressables设置
                    FixAddressablesSetup();
                    break;
                case 1: // SpriteAtlas配置
                    FixSpriteAtlases();
                    break;
                case 2: // Sprite资源
                    FixSpriteAssets();
                    break;
                case 3: // SmartAssetLoader
                    FixSmartAssetLoader();
                    break;
                case 4: // 构建设置
                    FixBuildSettings();
                    break;
            }
        }

        /// <summary>
        /// 修复Addressables设置
        /// </summary>
        private void FixAddressablesSetup()
        {
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
            Debug.Log("已打开Addressables窗口，请创建Groups并添加Sprite资源");
        }

        /// <summary>
        /// 修复SpriteAtlas配置
        /// </summary>
        private void FixSpriteAtlases()
        {
            // 创建默认Atlas
            var atlas = new UnityEngine.U2D.SpriteAtlas();
            AssetDatabase.CreateAsset(atlas, "Assets/UIAtlas.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("已创建默认SpriteAtlas: Assets/UIAtlas.asset");
        }

        /// <summary>
        /// 修复Sprite资源
        /// </summary>
        private void FixSpriteAssets()
        {
            Debug.Log("请确保项目中有Sprite资源文件");
        }

        /// <summary>
        /// 修复SmartAssetLoader
        /// </summary>
        private void FixSmartAssetLoader()
        {
            Debug.Log("SmartAssetLoader类应该已经存在，请检查编译错误");
        }

        /// <summary>
        /// 修复构建设置
        /// </summary>
        private void FixBuildSettings()
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings");
            Debug.Log("已打开构建设置窗口，请添加场景");
        }
    }
} 