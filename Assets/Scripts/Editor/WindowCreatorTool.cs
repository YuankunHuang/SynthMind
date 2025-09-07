using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.Editor
{
    public class WindowCreatorTool : EditorWindow
    {
        // basic
        private string windowName = "";
        private bool hasMask = true;
        private bool useBlurredBackground = false;
        private bool selfDestructOnCovered = false;

        // animation
        private bool usePopupAnimation = true;
        private AnimationType enterAnimation = AnimationType.Scale;
        private AnimationType exitAnimation = AnimationType.Scale;
        private SlideDirection slideDirection = SlideDirection.Up;
        private float enterDuration = 0.3f;
        private float exitDuration = 0.2f;
        private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // foldout states
        private bool showAnimationSettings = true;
        private bool showAdvancedSettings = false;
        private Vector2 scrollPosition;

        private const string STACKABLE_PATH = "Assets/Addressables/Window/Stackable";
        private const string ATTRIBUTE_DATA_PATH = "Assets/Addressables/WindowAttributeData";
        private const string WINDOW_NAMES_PATH = "Assets/Scripts/HotUpdate/WindowNames.cs";
        private const string WINDOW_CONTROLLER_PATH = "Assets/Scripts/HotUpdate/Window";
        private const string UI_GROUP_NAME = "UI";
        private const string STACKABLE_ADDRESSABLE_PATH = "Assets/Addressables/Window/Stackable/{0}/{1}.prefab";
        private const string ATTRIBUTE_DATA_GROUP_NAME = "WindowAttributeData";
        private const string ATTRIBUTE_DATA_ADDRESSABLE_PATH = "Assets/Addressables/WindowAttributeData/WindowAttributeData_{0}.asset";

        [MenuItem("Tools/UI/Window Creator")]
        public static void ShowWindow()
        {
            GetWindow<WindowCreatorTool>("Window Creator");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Create New Window", EditorStyles.boldLabel);
            GUILayout.Space(10);

            DrawWindowNameSection();

            GUILayout.Space(10);

            DrawBasicAttributesSection();

            GUILayout.Space(10);

            DrawAnimationSection();

            GUILayout.Space(10);

            DrawAdvancedSection();

            GUILayout.Space(10);

            DrawCreateButton();

            GUILayout.Space(10);

            DrawPreviewSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawPreviewSection()
        {
            if (!string.IsNullOrEmpty(windowName) && IsValidIdentifier(windowName))
            {
                GUILayout.Label("Preview:", EditorStyles.boldLabel);

                var previewText = $"Prefab: {STACKABLE_PATH}/{windowName}/{windowName}.prefab\n" +
                                $"Controller: {WINDOW_CONTROLLER_PATH}/{windowName}/{windowName}Controller.cs\n" +
                                $"Attribute Data: {ATTRIBUTE_DATA_PATH}/WindowAttributeData_{windowName}.asset\n" +
                                $"Animation: {enterAnimation} → {exitAnimation} ({slideDirection})";

                EditorGUILayout.HelpBox(previewText, MessageType.Info);
            }
        }

        private void DrawCreateButton()
        {
            bool canCreate = !string.IsNullOrEmpty(windowName) && IsValidIdentifier(windowName);

            GUI.enabled = canCreate;
            GUI.backgroundColor = canCreate ? Color.green : Color.gray;

            if (GUILayout.Button("Create Window", GUILayout.Height(35)))
            {
                CreateWindow();
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        private void DrawAdvancedSection()
        {
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings", true, EditorStyles.foldoutHeader);

            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;

                GUILayout.Label("Paths Configuration", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Stackable Path", STACKABLE_PATH);
                EditorGUILayout.TextField("Attribute Data Path", ATTRIBUTE_DATA_PATH);
                EditorGUILayout.TextField("Controller Path", WINDOW_CONTROLLER_PATH);
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
            }
        }

        private void DrawAnimationSection()
        {
            showAnimationSettings = EditorGUILayout.Foldout(showAnimationSettings, "Animation Settings", true, EditorStyles.foldoutHeader);

            if (showAnimationSettings)
            {
                EditorGUI.indentLevel++;

                usePopupAnimation = EditorGUILayout.Toggle("Use Popup Animation", usePopupAnimation);

                if (usePopupAnimation)
                {
                    EditorGUI.indentLevel++;

                    // animations
                    enterAnimation = (AnimationType)EditorGUILayout.EnumPopup("Enter Animation", enterAnimation);
                    exitAnimation = (AnimationType)EditorGUILayout.EnumPopup("Exit Animation", exitAnimation);
                    
                    if (enterAnimation == AnimationType.Slide || exitAnimation == AnimationType.Slide)
                    {
                        slideDirection = (SlideDirection)EditorGUILayout.EnumPopup("Slide Direction", slideDirection);
                    }
                    
                    GUILayout.Space(5);
                    
                    // timing
                    enterDuration = EditorGUILayout.FloatField("Enter Duration", enterDuration);
                    exitDuration = EditorGUILayout.FloatField("Exit Duration", exitDuration);
                    
                    GUILayout.Space(5);
                    
                    // curve
                    curve = EditorGUILayout.CurveField("Animation Curve", curve);

                    GUILayout.Space(5);

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawBasicAttributesSection()
        {
            GUILayout.Label("Basic Attributes", EditorStyles.boldLabel);
            hasMask = EditorGUILayout.Toggle("Has Mask", hasMask);
            useBlurredBackground = EditorGUILayout.Toggle("Use Blurred Background", useBlurredBackground);
            selfDestructOnCovered = EditorGUILayout.Toggle("Self Destruct On Covered", selfDestructOnCovered);
        }

        private void DrawWindowNameSection()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Window Name:", GUILayout.Width(100));
            windowName = EditorGUILayout.TextField(windowName);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(windowName))
            {
                EditorGUILayout.HelpBox("Please enter a window name", MessageType.Warning);
            }
            else if (!IsValidIdentifier(windowName))
            {
                EditorGUILayout.HelpBox("Window name must be a valid C# identifier", MessageType.Warning);
            }
        }

        private bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (!char.IsLetter(name[0]) && name[0] != '_') return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            return true;
        }



        private void CreateWindow()
        {
            try
            {
                CreateFolderStructure();
                CreateWindowController();

                var prefabPath = CreateWindowPrefab();
                var attributeDataPath = CreateWindowAttributeData();

                SetupAddressables(prefabPath, attributeDataPath);

                AddToWindowNames();

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success", $"Window '{windowName}' created successfully!", "OK");

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create window: {e.Message}", "OK");
                Debug.LogError($"Window creation failed: {e}");
            }
        }

        private void CreateFolderStructure()
        {
            var windowFolderPath = Path.Combine(STACKABLE_PATH, windowName);
            if (!Directory.Exists(windowFolderPath))
            {
                Directory.CreateDirectory(windowFolderPath);
            }

            if (!Directory.Exists(ATTRIBUTE_DATA_PATH))
            {
                Directory.CreateDirectory(ATTRIBUTE_DATA_PATH);
            }
        }

        private void CreateWindowController()
        {
            var controllerPath = Path.Combine(WINDOW_CONTROLLER_PATH, windowName, $"{windowName}Controller.cs");
            var code = "using YuankunHuang.Unity.Core;\n" +
                       "using YuankunHuang.Unity.UICore;\n" +
                       "using YuankunHuang.Unity.Util;\n" +
                       "\n" +
                       "namespace YuankunHuang.Unity.HotUpdate\n" +
                       "{\n" +
                      $"    public class {windowName}Controller : WindowControllerBase\n" +
                       "    {\n" +
                       "        #region UI Ref\n" +
                       "        #endregion\n" +
                       "\n" +
                       "        #region Lifecycle\n" +
                       "        protected override void OnInit()\n" +
                       "        {\n" +
                       "        }\n" +
                       "\n" +
                       "        protected override void OnShow(IWindowData data, WindowShowState state)\n" +
                       "        {\n" +
                       "            Config.CanvasGroup.CanvasGroupOn();" +
                       "        }\n" +
                       "\n" +
                       "        protected override void OnHide(WindowHideState state)\n" +
                       "        {\n" +
                       "            Config.CanvasGroup.CanvasGroupOff();" +
                       "        }\n" +
                       "\n" +
                       "        protected override void OnDispose()\n" +
                       "        {\n" +
                       "        }\n" +
                       "        #endregion\n" +
                       "    }\n" +
                       "}\n";
            if (!File.Exists(controllerPath))
            {
                File.WriteAllText(controllerPath, code);
            }
        }

        private string CreateWindowPrefab()
        {
            var prefabPath = Path.Combine(STACKABLE_PATH, windowName, $"{windowName}.prefab");

            var windowGO = new GameObject(windowName);
            windowGO.layer = LayerMask.NameToLayer(LayerNames.UI);
            var rectTransform = windowGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var cg = windowGO.AddComponent<CanvasGroup>();
            var config = windowGO.AddComponent<GeneralWindowConfig>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(windowGO, prefabPath);

            DestroyImmediate(windowGO);

            return prefabPath;
        }

        private string CreateWindowAttributeData()
        {
            var attributeDataPath = Path.Combine(ATTRIBUTE_DATA_PATH, $"WindowAttributeData_{windowName}.asset");

            if (File.Exists(attributeDataPath))
            {
                var choice = EditorUtility.DisplayDialogComplex(
                    "Attribute Data Already Exists",
                    $"Attribute data 'WindowAttributeData_{windowName}.asset' already exists.\n\nWhat would you like to do?",
                    "Overwrite", // 0
                    "Cancel", // 1
                    "Use Existing" // 2
                );

                switch (choice)
                {
                    case 1: // Cancel
                        throw new System.OperationCanceledException("User cancelled attribute data creation.");

                    case 2: // Use Existing
                        Debug.Log($"Using existing attribute data: {attributeDataPath}");
                        return attributeDataPath;

                    case 0: // Overwrite
                    default:
                        Debug.Log($"Overwriting existing attribute data: {attributeDataPath}");
                        break;
                }
            }

            var attributeData = ScriptableObject.CreateInstance<WindowAttributeData>();
            attributeData.hasMask = hasMask;
            attributeData.useBlurredBackground = useBlurredBackground;
            attributeData.selfDestructOnCovered = selfDestructOnCovered;
            attributeData.usePopupAnimation = usePopupAnimation;

            if (usePopupAnimation)
            {
                attributeData.animationSettings = new PopupAnimationSettings()
                {
                    enterAnimation = enterAnimation,
                    exitAnimation = exitAnimation,
                    slideDirection = slideDirection,
                    enterDuration = enterDuration,
                    exitDuration = exitDuration,
                    curve = curve
                };
            }

            AssetDatabase.CreateAsset(attributeData, attributeDataPath);

            return attributeDataPath;
        }

        private void SetupAddressables(string prefabPath, string attributeDataPath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings not found!");
                return;
            }

            var uiGroup = settings.FindGroup(UI_GROUP_NAME);
            if (uiGroup == null)
            {
                LogHelper.LogWarning($"Group '{UI_GROUP_NAME}' not found, creating it...");
                uiGroup = settings.CreateGroup(UI_GROUP_NAME, false, false, true, null);
            }

            var attributeGroup = settings.FindGroup(ATTRIBUTE_DATA_GROUP_NAME);
            if (attributeGroup == null)
            {
                LogHelper.LogWarning($"Group '{ATTRIBUTE_DATA_GROUP_NAME}' not found, creating it...");
                attributeGroup = settings.CreateGroup(ATTRIBUTE_DATA_GROUP_NAME, false, false, true, null);
            }

            // set prefab as addressable
            var prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
            var prefabEntry = settings.CreateOrMoveEntry(prefabGuid, uiGroup);
            if (prefabEntry != null)
            {
                prefabEntry.address = string.Format(STACKABLE_ADDRESSABLE_PATH, windowName, windowName);
                Debug.Log($"Prefab addressable set: {prefabEntry.address}");
            }

            // set attribute data as addressable
            var attributeDataGuid = AssetDatabase.AssetPathToGUID(attributeDataPath);
            var attributeDataEntry = settings.CreateOrMoveEntry(attributeDataGuid, attributeGroup);
            if (attributeDataEntry != null)
            {
                attributeDataEntry.address = string.Format(ATTRIBUTE_DATA_ADDRESSABLE_PATH, windowName);
                Debug.Log($"Attribute data addressable set: {attributeDataEntry.address}");
            }

            EditorUtility.SetDirty(settings);
        }

        private void AddToWindowNames()
        {
            try
            {
                var fileContent = File.ReadAllText(WINDOW_NAMES_PATH);

                if (IsWindowNameAlreadyExists(fileContent, windowName))
                {
                    Debug.Log($"Window name '{windowName}' already exists in WindowNames.cs");
                    return;
                }

                var newContent = InsertWindowNameToFile(fileContent, windowName);

                File.WriteAllText(WINDOW_NAMES_PATH, newContent);
                AssetDatabase.Refresh();

                Debug.Log($"Successfully added '{windowName}' to WindowNames.cs");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to add window name to WindowNames.cs: {e.Message}");
                Debug.Log($"Please manually add 'public static readonly string {windowName} = \"{windowName}\";' to WindowNames.cs");
            }
        }

        private bool IsWindowNameAlreadyExists(string fileContent, string windowName)
        {
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("public static readonly string") &&
                    trimmedLine.Contains($"string {windowName} ="))
                {
                    return true;
                }
            }

            return false;
        }

        private string InsertWindowNameToFile(string fileContent, string windowName)
        {
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var insertIndex = -1;
            var indentation = "        ";

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                var line = lines[i].Trim();

                if (line == "}" && insertIndex == -1)
                {
                    insertIndex = i;
                    continue;
                }

                if (line.StartsWith("public static readonly string") && insertIndex > 0)
                {
                    var originalLine = lines[i];
                    var leadingWhitespace = originalLine.Substring(0, originalLine.Length - originalLine.TrimStart().Length);
                    indentation = leadingWhitespace;
                    break;
                }
            }

            if (insertIndex == -1)
            {
                throw new System.InvalidOperationException("Could not find class closing bracket in WindowNames.cs");
            }

            var newField = $"{indentation}public static readonly string {windowName} = \"{windowName}\";";
            var newLines = new List<string>(lines);
            newLines.Insert(insertIndex, newField);

            return string.Join(System.Environment.NewLine, newLines);
        }
    }
}