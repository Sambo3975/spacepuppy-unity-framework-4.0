﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Settings
{

    public class SpacepuppySettingsWindow : EditorWindow
    {

        #region Consts

        private const float BTN_WIDTH = 275f;

        public const string MENU_NAME = SPMenu.MENU_NAME_SETTINGS + "/Spacepuppy Settings";
        public const int MENU_PRIORITY = SPMenu.MENU_PRIORITY_SETTINGS;

        #endregion

        #region Menu Entries

        [MenuItem(MENU_NAME, priority = MENU_PRIORITY)]
        public static void OpenWindow()
        {
            if (_openWindow == null)
            {
                EditorWindow.GetWindow<SpacepuppySettingsWindow>();
            }
            else
            {
                GUI.BringWindowToFront(_openWindow.GetInstanceID());
            }
        }

        public static event System.Action DrawExtraEditorSettings;

        #endregion

        #region Window

        private static SpacepuppySettingsWindow _openWindow;

        private CustomTimeLayersData _timeLayersData;

        private Vector2 _totalScrollPosition;
        private Vector2 _scenesScrollBarPosition;

        protected virtual void OnEnable()
        {
            if (_openWindow == null)
                _openWindow = this;
            else
                Object.DestroyImmediate(this);

            this.titleContent = new GUIContent("SP Settings");

            _timeLayersData = AssetDatabase.LoadAssetAtPath(CustomTimeLayersData.PATH_DEFAULTSETTINGS_FULL, typeof(CustomTimeLayersData)) as CustomTimeLayersData;
        }

        protected virtual void OnDisable()
        {
            if (_openWindow == this) _openWindow = null;
        }

        protected virtual void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.box);
            style.stretchHeight = true;
            _totalScrollPosition = EditorGUILayout.BeginScrollView(_totalScrollPosition, style);

            Rect rect;

            var boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.stretchHeight = false;

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            bool storeLocal = EditorGUILayout.ToggleLeft("Store As User Settings", SpacepuppySettings.StoreSettingsLocal);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.StoreSettingsLocal = storeLocal;

            /*
             * Editor Use
             */

            EditorGUILayout.Space();
            GUILayout.BeginVertical("Editor Settings", boxStyle);
            EditorGUILayout.Space();

            DrawEditorSettingsGroup();

            GUILayout.EndVertical();

            /*
             * Material Search Settings
             */

            GUILayout.BeginVertical("Material Settings", boxStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            bool setMaterialSearch = EditorGUILayout.ToggleLeft("Configure Material Settings On Import", SpacepuppySettings.SetMaterialSearchOnImport);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.SetMaterialSearchOnImport = setMaterialSearch;

            EditorGUI.BeginChangeCheck();
            var materialSearch = (ModelImporterMaterialSearch)EditorGUILayout.EnumPopup("Material Search", SpacepuppySettings.MaterialSearch);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.MaterialSearch = materialSearch;

            GUILayout.EndVertical();

            /*
             * Animation Settings
             */

            GUILayout.BeginVertical("Animation Settings", boxStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            bool setAnimSettings = EditorGUILayout.ToggleLeft("Configure Animation Settings On Import", SpacepuppySettings.SetAnimationSettingsOnImport);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.SetAnimationSettingsOnImport = setAnimSettings;

            EditorGUI.BeginChangeCheck();
            var animRigType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("Aimation Rig Type", SpacepuppySettings.ImportAnimRigType);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.ImportAnimRigType = animRigType;

            GUILayout.EndVertical();

            /*
             * Game Settings
             */

            GUILayout.BeginVertical("Game Settings", boxStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.Space();

            if (_timeLayersData == null)
            {
                rect = EditorGUILayout.GetControlRect();
                rect.width = Mathf.Min(rect.width, BTN_WIDTH);
                if (GUI.Button(rect, "Create Custom Time Layers Data Resource"))
                {
                    _timeLayersData = ScriptableObjectHelper.CreateAsset<CustomTimeLayersData>(CustomTimeLayersData.PATH_DEFAULTSETTINGS_FULL);
                }
            }
            else
            {
                EditorGUILayout.ObjectField("Custom Time Layers Data", _timeLayersData, typeof(CustomTimeLayersData), false);
            }

            GUILayout.EndVertical();

            /*
             * Defines
             */

            GUILayout.BeginVertical("Custom Defines", boxStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Sync Spacepuppy Global Define Symbols", GUILayout.Width(BTN_WIDTH)) && EditorUtility.DisplayDialog("Spacepuppy Settings", "This will create/modify the csc.rsp file associated with this project, are you sure?", "Yes", "No"))
            {
                SPPackage.SyncGlobalDefineSymbolsForSpacepuppy();
            }

            EditorGUILayout.Space();

#if UNITY_2021_2_OR_NEWER
            var currentBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            string defines = PlayerSettings.GetScriptingDefineSymbols(currentBuildTarget);
            EditorGUI.BeginChangeCheck();
            defines = EditorGUILayout.DelayedTextField(defines);
            if (EditorGUI.EndChangeCheck())
            {
                PlayerSettings.SetScriptingDefineSymbols(currentBuildTarget, defines);
            }
#else
            var currentBuildTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTarget);
            EditorGUI.BeginChangeCheck();
            defines = EditorGUILayout.DelayedTextField(defines);
            if (EditorGUI.EndChangeCheck())
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTarget, defines);
            }
#endif

            GUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            this.DrawScenes();

            /*
             * Global Editor Use
             */

            EditorGUILayout.Space();
            GUILayout.BeginVertical("Global Settings - these settings are stored across all projects", boxStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawSpecialGlobalSettingsGroup();

            GUILayout.EndVertical();


            EditorGUILayout.EndScrollView();
        }

        #endregion



        protected virtual void DrawEditorSettingsGroup()
        {
            EditorGUI.BeginChangeCheck();
            bool useSPEditor = EditorGUILayout.ToggleLeft(SpacepuppySettings.UseSPEditorAsDefaultEditor ? "Use SPEditor as default editor for MonoBehaviour" : "Use SPEditor as default editor for MonoBehaviour (Optional: place SPEDITOR_IGNORE or DISABLE_GLOBAL_SPEDITOR as a compiler directive in the com.spacepuppyeditor assemblydefinition)", SpacepuppySettings.UseSPEditorAsDefaultEditor);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.UseSPEditorAsDefaultEditor = useSPEditor;


            EditorGUI.BeginChangeCheck();
            bool useAdvancedAnimInspector = EditorGUILayout.ToggleLeft("Use Advanced Animation Inspector", SpacepuppySettings.UseAdvancedAnimationInspector);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.UseAdvancedAnimationInspector = useAdvancedAnimInspector;

            EditorGUI.BeginChangeCheck();
            bool hierarchyDrawerActive = EditorGUILayout.ToggleLeft("Use Hierarchy Drawers", SpacepuppySettings.UseHierarchDrawer);
            if (EditorGUI.EndChangeCheck())
            {
                SpacepuppySettings.UseHierarchDrawer = hierarchyDrawerActive;
                EditorHierarchyDrawerEvents.SetActive(hierarchyDrawerActive);
            }

            EditorGUI.BeginChangeCheck();
            bool hierarchCustomContextMenu = EditorGUILayout.ToggleLeft("Use Alternate Hierarchy Context Menu", SpacepuppySettings.UseHierarchyAlternateContextMenu);
            if (EditorGUI.EndChangeCheck())
            {
                SpacepuppySettings.UseHierarchyAlternateContextMenu = hierarchCustomContextMenu;
                EditorHierarchyAlternateContextMenuEvents.SetActive(hierarchCustomContextMenu);
            }

            EditorGUI.BeginChangeCheck();
            bool signalValidateReceiver = EditorGUILayout.ToggleLeft("Signal IValidateReceiver OnValidate Event", SpacepuppySettings.SignalValidateReceiver);
            if (EditorGUI.EndChangeCheck())
            {
                SpacepuppySettings.SignalValidateReceiver = signalValidateReceiver;
            }

            DrawExtraEditorSettings?.Invoke();
        }

        protected virtual void DrawSpecialGlobalSettingsGroup()
        {
            EditorGUI.BeginChangeCheck();
            bool useAdvancedObjectField = EditorGUILayout.ToggleLeft("Use Advanced Object Field", SpacepuppySettings.UseAdvancedObjectField);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.UseAdvancedObjectField = useAdvancedObjectField;

            EditorGUI.BeginChangeCheck();
            bool autoSyncGlobalDefines = EditorGUILayout.ToggleLeft("Auto Sync Global Defines", SpacepuppySettings.AutoSyncGlobalDefines);
            if (EditorGUI.EndChangeCheck()) SpacepuppySettings.AutoSyncGlobalDefines = autoSyncGlobalDefines;
        }







        #region Debug Build Scenes - OBSOLETE

        private void DrawScenes()
        {
            EditorGUILayout.LabelField("Scenes in Build", EditorStyles.boldLabel);

            bool changed = false;
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                var r = EditorGUILayout.GetControlRect();
                var r0 = new Rect(r.xMin, r.yMin, 25f, r.height);
                var r1 = new Rect(r0.xMax, r.yMin, (r.xMax - r0.xMax) * 0.66f, r.height);
                var r2 = new Rect(r1.xMax, r.yMin, r.xMax - r1.xMax, r.height);

                EditorGUI.BeginChangeCheck();
                scenes[i].enabled = EditorGUI.Toggle(r0, scenes[i].enabled);
                if (EditorGUI.EndChangeCheck()) changed = true;

                EditorGUI.LabelField(r1, EditorHelper.TempContent(scenes[i].path));
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i].path);
                EditorGUI.BeginChangeCheck();
                scene = EditorGUI.ObjectField(r2, GUIContent.none, scene, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    changed = true;
                    scenes[i] = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene), scenes[i].enabled);
                }
            }
            if (changed) EditorBuildSettings.scenes = scenes;


            //DRAG & DROP ON SCROLLVIEW
            var dropArea = GUILayoutUtility.GetLastRect();

            var ev = Event.current;
            switch (ev.type)
            {
                case EventType.DragUpdated:
                    if (dropArea.Contains(ev.mousePosition) && (from o in DragAndDrop.objectReferences where o is SceneAsset select o).Any())
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    break;
                case EventType.DragPerform:
                    if (dropArea.Contains(ev.mousePosition) && (from o in DragAndDrop.objectReferences where o is SceneAsset select o).Any())
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        DragAndDrop.AcceptDrag();

                        var lst = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                        foreach (var o in DragAndDrop.objectReferences)
                        {
                            var scene = o as SceneAsset;
                            if (scene == null) continue;

                            var p = AssetDatabase.GetAssetPath(scene);
                            if (!(from s in lst where s.path == p select s).Any())
                            {
                                lst.Add(new EditorBuildSettingsScene(p, true));
                            }
                        }
                        EditorBuildSettings.scenes = lst.ToArray();
                    }
                    break;

            }



            ////////////////
            //BUTTONS!
            var rect = EditorGUILayout.GetControlRect();

            //DESELECT
            var selectAllPosition = new Rect(rect.xMin, rect.yMin, 100f, rect.height);
            if (GUI.Button(selectAllPosition, new GUIContent("Select All")))
            {
                var arr = EditorBuildSettings.scenes;
                foreach (var s in arr)
                {
                    s.enabled = true;
                }
                EditorBuildSettings.scenes = arr;
            }

            //DESELECT
            var deselectPosition = new Rect(rect.xMin + 105f, rect.yMin, 100f, rect.height);
            if (GUI.Button(deselectPosition, new GUIContent("Deselect All")))
            {
                var arr = EditorBuildSettings.scenes;
                foreach (var s in arr)
                {
                    s.enabled = false;
                }
                EditorBuildSettings.scenes = arr;
            }

            //CLEAR
            var cancelPosition = new Rect(rect.xMax - 110f, rect.yMin, 50f, rect.height);
            if (GUI.Button(cancelPosition, new GUIContent("Clear")))
            {
                EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { };
            }
            //SYNC
            var applyPosition = new Rect(rect.xMax - 55f, rect.yMin, 50f, rect.height);
            var oldScenes = EditorBuildSettings.scenes;
            if (GUI.Button(applyPosition, new GUIContent("Add All")))
            {
                var lst = new List<EditorBuildSettingsScene>();
                var mainFolder = Application.dataPath.EnsureNotEndsWith("Assets");
                foreach (var file in Directory.GetFiles(Application.dataPath + "/Scenes", "*.unity", SearchOption.AllDirectories))
                {
                    var normalizedFile = file.EnsureNotStartWith(mainFolder);
                    bool enabled = (from s in oldScenes where s.enabled && s.path == normalizedFile select s).Any();
                    lst.Add(new EditorBuildSettingsScene(normalizedFile, enabled));
                }
                EditorBuildSettings.scenes = lst.ToArray();
            }

        }

        #endregion

    }

}
