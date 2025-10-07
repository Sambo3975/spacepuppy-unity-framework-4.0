﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor
{

    [InitializeOnLoad()]
    public static class SPMenu
    {

        public const string MENU_NAME_ROOT = "Spacepuppy";
        public const string MENU_NAME_SETTINGS = MENU_NAME_ROOT + "/Settings";
        public const string MENU_NAME_MODELS = MENU_NAME_ROOT + "/Models";
        public const string MENU_NAME_TOOLS = MENU_NAME_ROOT + "/Tools";

        public const int MENU_GAP = 1000;

        public const int MENU_PRIORITY_GROUP1 = 0;
        public const int MENU_PRIORITY_GROUP2 = MENU_PRIORITY_GROUP1 + MENU_GAP;
        public const int MENU_PRIORITY_GROUP3 = MENU_PRIORITY_GROUP2 + MENU_GAP;
        public const int MENU_PRIORITY_GROUP4 = MENU_PRIORITY_GROUP3 + MENU_GAP;
        public const int MENU_PRIORITY_SETTINGS = MENU_PRIORITY_GROUP1 - 1;
        public const int MENU_PRIORITY_MODELS = MENU_PRIORITY_GROUP3 - 1;
        public const int MENU_PRIORITY_TOOLS = MENU_PRIORITY_GROUP4 - 1;

        public const int MENU_PRIORITY_SINGLETON = 1000000;

        #region Special Menu Entries

        [MenuItem(SPMenu.MENU_NAME_SETTINGS + "/Sync TagData", priority = SPMenu.MENU_PRIORITY_SETTINGS)]
        public static void SyncTagData()
        {
            var tagData = (TagData)AssetDatabase.LoadAssetAtPath(@"Assets/Resources/TagData.asset", typeof(TagData));
            if (tagData == null)
            {
                if (!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources");
                }
                tagData = ScriptableObjectHelper.CreateAsset<TagData>(@"Assets/Resources/TagData.asset");
            }

            SyncTagData(tagData);
        }

        public static void SyncTagData(TagData tagData)
        {
            if (!tagData.SimilarTo(UnityEditorInternal.InternalEditorUtility.tags))
            {
                var helper = new TagData.EditorHelper(tagData);

                using (var tags = com.spacepuppy.Collections.TempCollection.GetList<string>(UnityEditorInternal.InternalEditorUtility.tags))
                {
                    bool added = false;
                    if (!tags.Contains(SPConstants.TAG_MULTITAG))
                    {
                        tags.Add(SPConstants.TAG_MULTITAG);
                        added = true;
                    }
                    if (!tags.Contains(SPConstants.TAG_ROOT))
                    {
                        tags.Add(SPConstants.TAG_ROOT);
                        added = true;
                    }
                    if (added)
                    {
                        try
                        {
                            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                            SerializedProperty tagsProp = tagManager.FindProperty("tags");

                            var arr = (from st in tags where !string.IsNullOrEmpty(st) && !TagData.IsDefaultUnityTag(st) select st).ToArray();
                            tagsProp.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                tagsProp.GetArrayElementAtIndex(i).stringValue = arr[i];
                            }
                            tagManager.ApplyModifiedProperties();
                        }
                        catch
                        {
                            Debug.LogWarning("Failed to save TagManager when syncing tags.");
                        }
                    }

                    Undo.RecordObject(tagData, "Sync TagData");
                    helper.UpdateTags(tags);
                    EditorHelper.CommitDirectChanges(tagData, true);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        public static void SaveTagData(IEnumerable<string> tags)
        {
            try
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");

                var arr = (from st in tags where !string.IsNullOrEmpty(st) && !TagData.IsDefaultUnityTag(st) select st).ToArray();
                tagsProp.arraySize = arr.Length;
                for (int i = 0; i < arr.Length; i++)
                {
                    tagsProp.GetArrayElementAtIndex(i).stringValue = arr[i];
                }
                tagManager.ApplyModifiedProperties();
            }
            catch
            {
                Debug.LogWarning("Failed to save TagManager when syncing tags.");
            }
        }

#if !SPEDITOR_IGNORE
        [MenuItem("Assets/Copy Asset Guid", priority = 19)]
#endif
        public static void CopyAssetGuid()
        {
            var arr = Selection.assetGUIDs;
            if (arr.Length > 0)
            {
                GUIUtility.systemCopyBuffer = string.Join("\r\n", arr);
            }
        }

        #endregion

    }
}
