﻿using UnityEngine;
using UnityEditor;

using com.spacepuppy.Serialization;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Serialization
{
    [CustomEditor(typeof(PersistentGameObject), true)]
    public class PersistentGameObjectInspector : SPEditor
    {
        
        public const string PROP_ASSETID = "_assetId";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            //TODO - refactor this to account for Unity's new nested-prefabs
            var assetIdProp = this.serializedObject.FindProperty(PROP_ASSETID);
            var go = GameObjectUtil.GetGameObjectFromSource(this.serializedObject.targetObject);
            if (go != null)
            {
                switch (PrefabUtility.GetPrefabType(go))
                {
                    case PrefabType.None:
                        SPEditorGUILayout.PropertyField(assetIdProp);
                        break;
                    case PrefabType.Prefab:
                        {
                            if(string.IsNullOrEmpty(assetIdProp.stringValue))
                            {
                                var path = AssetHelper.GetRelativeResourcePath(AssetDatabase.GetAssetPath(go));
                                assetIdProp.stringValue = (string.IsNullOrEmpty(path)) ? go.name : path;
                            }
                            SPEditorGUILayout.PropertyField(assetIdProp);
                            break;
                        }
                    case PrefabType.PrefabInstance:
                        {
                            if (string.IsNullOrEmpty(assetIdProp.stringValue))
                            {
                                go = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
                                if(go != null)
                                {
                                    var path = AssetHelper.GetRelativeResourcePath(AssetDatabase.GetAssetPath(go));
                                    assetIdProp.stringValue = (string.IsNullOrEmpty(path)) ? go.name : path;
                                }
                            }
                            SPEditorGUILayout.PropertyField(assetIdProp);
                            break;
                        }
                    default:
                        //do nothing
                        break;
                }
            }



            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_ASSETID);

            this.serializedObject.ApplyModifiedProperties();
        }

    }
}
