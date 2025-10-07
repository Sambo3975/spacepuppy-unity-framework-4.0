﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(DisplayFlatAttribute))]
    public class DisplayFlatPropertyDrawer : PropertyDrawer
    {

        private static float TOP_PAD => 2f + EditorGUIUtility.singleLineHeight;
        private const float BOTTOM_PAD = 2f;
        private const float MARGIN = 2f;

        private bool _displayBox;
        private bool _displayHeader;
        private bool _alwaysExpanded;
        private bool _ignoreIfNoChildren;
        private bool _indent;
        private float _trailingSpace;

        #region Properties

        public bool DisplayBox
        {
            get => (this.attribute as DisplayFlatAttribute)?.DisplayBox ?? _displayBox;
            set => _displayBox = value;
        }

        public bool DisplayHeader
        {
            get => (this.attribute as DisplayFlatAttribute)?.DisplayHeader ?? _displayHeader;
            set => _displayHeader = value;
        }

        public bool AlwaysExpanded
        {
            get => (this.attribute as DisplayFlatAttribute)?.AlwaysExpanded ?? _alwaysExpanded;
            set => _alwaysExpanded = value;
        }

        public bool IgnoreIfNoChildren
        {
            get => (this.attribute as DisplayFlatAttribute)?.IgnoreIfNoChildren ?? _ignoreIfNoChildren;
            set => _ignoreIfNoChildren = value;
        }

        public bool Indent
        {
            get => (this.attribute as DisplayFlatAttribute)?.Indent ?? _indent;
            set => _indent = value;
        }

        public float TrailingSpace
        {
            get => (this.attribute as DisplayFlatAttribute)?.TrailingSpace ?? _trailingSpace;
            set => _trailingSpace = value;
        }

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (this.IgnoreIfNoChildren && !property.hasChildren) return 0f;

            bool cache = property.isExpanded;
            if (this.AlwaysExpanded)
            {
                property.isExpanded = true;
            }

            try
            {
                float h = 0f;
                if (property.isExpanded)
                {
                    if (property.hasChildren)
                    {
                        if (this.DisplayBox)
                        {
                            h = SPEditorGUI.GetDefaultPropertyHeight(property, label, true) + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                        }
                        else
                        {
                            h = Mathf.Max(SPEditorGUI.GetDefaultPropertyHeight(property, label, true) - EditorGUIUtility.singleLineHeight, 0f);
                            if (this.DisplayHeader) h += EditorGUIUtility.singleLineHeight;
                        }
                    }
                    else
                    {
                        h = SPEditorGUI.GetDefaultPropertyHeight(property, label);
                    }
                }
                else if (this.DisplayBox)
                {
                    h = EditorGUIUtility.singleLineHeight + BOTTOM_PAD;
                }
                else
                {
                    h = EditorGUIUtility.singleLineHeight;
                }

                if (this.TrailingSpace > 0f) h += this.TrailingSpace;
                return h;
            }
            finally
            {
                property.isExpanded = cache;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (this.IgnoreIfNoChildren && !property.hasChildren) return;

            bool cache = property.isExpanded;
            if (this.AlwaysExpanded)
            {
                property.isExpanded = true;
            }

            try
            {
                if (this.TrailingSpace > 0f) position = new Rect(position.xMin, position.yMin, position.width, position.height - this.TrailingSpace);

                if (!property.hasChildren)
                {
                    SPEditorGUI.DefaultPropertyField(position, property, label);
                    return;
                }

                if (!this.AlwaysExpanded) cache = SPEditorGUI.PrefixFoldoutLabel(position, property.isExpanded, GUIContent.none);

                if (property.isExpanded)
                {
                    if (this.DisplayBox)
                    {
                        //float h = SPEditorGUI.GetDefaultPropertyHeight(property, label, true) + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                        //var area = new Rect(position.xMin, position.yMax - h, position.width, h);
                        var area = position;
                        var drawArea = new Rect(area.xMin, area.yMin + TOP_PAD, area.width - MARGIN, area.height - TOP_PAD);

                        GUI.BeginGroup(area, label, GUI.skin.box);
                        GUI.EndGroup();

                        EditorGUI.indentLevel++;
                        SPEditorGUI.FlatChildPropertyField(drawArea, property);
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        if (this.DisplayHeader)
                        {
                            var rheader = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
                            position = new Rect(position.xMin, rheader.yMax, position.width, position.height - rheader.height);
                            EditorGUI.LabelField(rheader, label, EditorStyles.boldLabel);
                        }

                        if (this.Indent)
                        {
                            EditorGUI.indentLevel++;
                            SPEditorGUI.FlatChildPropertyField(position, property);
                            EditorGUI.indentLevel--;
                        }
                        else
                        {
                            SPEditorGUI.FlatChildPropertyField(position, property);
                        }
                    }
                }
                else if (this.DisplayBox)
                {
                    GUI.BeginGroup(position, label, GUI.skin.box);
                    GUI.EndGroup();
                }
                else
                {
                    SPEditorGUI.SafePrefixLabel(position, label);
                }
            }
            finally
            {
                property.isExpanded = cache;
            }
        }

    }
}
