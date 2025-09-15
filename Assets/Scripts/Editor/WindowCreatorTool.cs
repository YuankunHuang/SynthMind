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

                UpdateWindowControllerFactory();

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
            EnsureFolder($"{STACKABLE_PATH}/{windowName}");
            EnsureFolder(ATTRIBUTE_DATA_PATH);
            EnsureFolder($"{WINDOW_CONTROLLER_PATH}/{windowName}");
        }

        private void EnsureFolder(string assetFolderPath)
        {
            var parts = assetFolderPath.Split('/');
            var current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
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
                       "            Config.CanvasGroup.CanvasGroupOn();\n" +
                       "        }\n" +
                       "\n" +
                       "        protected override void OnHide(WindowHideState state)\n" +
                       "        {\n" +
                       "            Config.CanvasGroup.CanvasGroupOff();\n" +
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

            var rootGO = new GameObject("Root");
            var rootRT = rootGO.AddComponent<RectTransform>();
            rootRT.SetParent(rectTransform);
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

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
                if (!File.Exists(WINDOW_NAMES_PATH))
                {
                    Debug.LogError($"WindowNames.cs not found at path: {WINDOW_NAMES_PATH}");
                    Debug.Log($"Please manually add 'public static readonly string {windowName} = \"{windowName}\";' to WindowNames.cs");
                    return;
                }

                var fileContent = File.ReadAllText(WINDOW_NAMES_PATH);
                Debug.Log($"Original WindowNames.cs content length: {fileContent.Length} characters");

                if (IsWindowNameAlreadyExists(fileContent, windowName))
                {
                    Debug.Log($"Window name '{windowName}' already exists in WindowNames.cs");
                    return;
                }

                var newContent = InsertWindowNameToFile(fileContent, windowName);
                Debug.Log($"Generated new WindowNames.cs content length: {newContent.Length} characters");

                // Backup original file
                var backupPath = WINDOW_NAMES_PATH + ".backup";
                File.WriteAllText(backupPath, fileContent);
                Debug.Log($"Created backup at: {backupPath}");

                // Write new content
                File.WriteAllText(WINDOW_NAMES_PATH, newContent);
                AssetDatabase.Refresh();

                Debug.Log($"Successfully added '{windowName}' to WindowNames.cs");
                
                // Clean up backup after successful write
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to add window name to WindowNames.cs: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
                Debug.Log($"Please manually add 'public static readonly string {windowName} = \"{windowName}\";' to WindowNames.cs");
                
                // Try to restore from backup if it exists
                var backupPath = WINDOW_NAMES_PATH + ".backup";
                if (File.Exists(backupPath))
                {
                    try
                    {
                        File.Copy(backupPath, WINDOW_NAMES_PATH, true);
                        File.Delete(backupPath);
                        Debug.Log("Restored WindowNames.cs from backup due to error");
                    }
                    catch
                    {
                        Debug.LogError("Failed to restore from backup!");
                    }
                }
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
            // First check if the window name already exists
            if (IsWindowNameAlreadyExists(fileContent, windowName))
            {
                Debug.Log($"Window name '{windowName}' already exists in WindowNames.cs, skipping insertion");
                return fileContent; // Return original content unchanged
            }

            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();
            bool fieldAdded = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // If we hit the class closing bracket and haven't added the field yet, add it before the bracket
                if (line.Trim() == "}" && !fieldAdded)
                {
                    // Use standard indentation (8 spaces)
                    var newField = $"        public static readonly string {windowName} = \"{windowName}\";";
                    newLines.Add(newField);
                    fieldAdded = true;
                }
                
                newLines.Add(line);
            }

            return string.Join(System.Environment.NewLine, newLines);
        }

        private void UpdateWindowControllerFactory()
        {
            try
            {
                Debug.Log($"[WindowCreatorTool] Starting WindowControllerFactory update for window: {windowName}");
                var factoryPath = "Assets/Scripts/Core/UI/WindowControllerFactory.cs";

                if (!File.Exists(factoryPath))
                {
                    Debug.LogWarning($"[WindowCreatorTool] WindowControllerFactory.cs not found at {factoryPath}");
                    return;
                }

                var content = File.ReadAllText(factoryPath);
                Debug.Log($"[WindowCreatorTool] Read factory file, content length: {content.Length}");

                // Check if the window already exists in factory
                var entryPattern = $"\"{windowName}\"";
                if (content.Contains(entryPattern))
                {
                    Debug.Log($"[WindowCreatorTool] {windowName} already exists in WindowControllerFactory");
                    return;
                }

                // Find the dictionary and add new entry
                var newEntry = $"                {{ \"{windowName}\", () => new {windowName}Controller() }},";
                Debug.Log($"[WindowCreatorTool] Preparing to add entry: {newEntry}");

                var newContent = AddEntryToFactoryDictionary(content, newEntry);

                if (newContent != content)
                {
                    Debug.Log($"[WindowCreatorTool] Content changed, writing to file...");
                    File.WriteAllText(factoryPath, newContent);
                    Debug.Log($"[WindowCreatorTool] Successfully added {windowName} to WindowControllerFactory");
                }
                else
                {
                    Debug.LogWarning($"[WindowCreatorTool] Content unchanged - entry was not added");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WindowCreatorTool] Failed to update WindowControllerFactory: {e.Message}");
                Debug.LogError($"[WindowCreatorTool] Stack trace: {e.StackTrace}");
                Debug.Log($"Please manually add '{{ \"{windowName}\", () => new {windowName}Controller() }},' to WindowControllerFactory.cs");
            }
        }

        private string AddEntryToFactoryDictionary(string content, string newEntry)
        {
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();
            bool entryAdded = false;
            bool inFactoryDictionary = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.Trim();

                // Detect if we're starting the _controllerFactories dictionary declaration
                if (!inFactoryDictionary && line.Contains("_controllerFactories"))
                {
                    inFactoryDictionary = true;
                    Debug.Log("[WindowCreatorTool] Found _controllerFactories dictionary start");
                }

                // Look for the opening brace of the dictionary initialization
                if (inFactoryDictionary && trimmedLine == "{")
                {
                    // We're now inside the dictionary
                    Debug.Log("[WindowCreatorTool] Inside dictionary initialization");
                }

                // If we're in the dictionary and found the closing brace with semicolon
                if (inFactoryDictionary && !entryAdded && trimmedLine == "};")
                {
                    // Insert the new entry before the closing brace
                    newLines.Add(newEntry);
                    entryAdded = true;
                    inFactoryDictionary = false; // Reset for potential future dictionaries
                    Debug.Log($"[WindowCreatorTool] Added entry: {newEntry.Trim()}");
                }

                newLines.Add(line);
            }

            if (!entryAdded)
            {
                Debug.LogWarning("[WindowCreatorTool] Could not find the right place to add the factory entry");
                Debug.LogWarning("[WindowCreatorTool] Please check the WindowControllerFactory.cs structure");
            }

            return string.Join(System.Environment.NewLine, newLines);
        }
    }
}