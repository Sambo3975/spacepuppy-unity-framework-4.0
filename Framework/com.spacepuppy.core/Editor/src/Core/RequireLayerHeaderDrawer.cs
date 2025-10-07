﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(RequireLayerAttribute))]
    public class RequireLayerHeaderDrawer : ComponentHeaderDrawer
    {

        public override float GetHeight(SerializedObject serializedObject)
        {
            var attrib = this.Attribute as RequireLayerAttribute;
            if(attrib == null) return 0f;

            GUIStyle style = GUI.skin.GetStyle("HelpBox");
            return Mathf.Max(40f, style.CalcHeight(EditorHelper.TempContent("This component requires the current layer to be set to '" + LayerMask.LayerToName(attrib.Layer) + "'."), EditorGUIUtility.currentViewWidth));
        }

        public override void OnGUI(Rect position, SerializedObject serializedObject)
        {
            if (serializedObject.isEditingMultipleObjects) return;

            var attrib = this.Attribute as RequireLayerAttribute;
            if (attrib == null) return;

            foreach (var c in serializedObject.targetObjects)
            {
                var go = (c as Component)?.gameObject;
                if (go && go.layer != attrib.Layer)
                {
                    go.layer = attrib.Layer;
                    EditorHelper.CommitDirectChanges(go, false);
                }
            }

            EditorGUI.HelpBox(position, "This component requires the current layer to be set to '" + LayerMask.LayerToName(attrib.Layer) + "'.", MessageType.Info);
        }

    }
}
