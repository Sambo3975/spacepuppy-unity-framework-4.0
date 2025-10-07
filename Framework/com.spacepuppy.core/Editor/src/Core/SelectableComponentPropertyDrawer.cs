﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Windows;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SelectableComponentAttribute))]
    public class SelectableComponentPropertyDrawer : PropertyDrawer
    {

        #region Fields

        public const float DEFAULT_POPUP_WIDTH_SCALE = 0.4f;

        public bool AllowSceneObjects = true;
        public bool ForceOnlySelf;
        public bool SearchChildren;
        public bool AllowProxy;
        public bool ShowXButton = true;
        public bool XButtonOnRightSide = true;
        public IComponentChoiceSelector ChoiceSelector;

        /// <summary>
        /// SelectableComponentPropertyDrawer will allow drawing an Object field that will only go into 'Component Select' mode if the object is a Component source.
        /// Otherwise it just remains a simple object field.
        /// </summary>
        public bool AllowNonComponents;

        private System.Type _restrictionType;

        public System.Type RestrictionType
        {
            get
            {
                if (_restrictionType == null)
                {
                    //return typeof(Component);
                    //needs to be this so that it works with VariantReference and allows all types
                    return this.AllowNonComponents ? typeof(UnityEngine.Object) : typeof(Component);
                }
                else
                {
                    return _restrictionType;
                }
            }
            set
            {
                _restrictionType = value;
            }
        }

        #endregion

        #region CONSTRUCTOR

        private void Init()
        {
            if (this.fieldInfo != null)
            {
                var tp = this.fieldInfo.FieldType;
                if (tp.IsListType()) tp = tp.GetElementTypeOfListType();
                if (_restrictionType == null)
                    _restrictionType = tp;
            }

            if (this.attribute != null && this.attribute is SelectableComponentAttribute)
            {
                //created as part as a PropertyHandler
                var attrib = (this.attribute as SelectableComponentAttribute);
                this.AllowSceneObjects = attrib.AllowSceneObjects;
                this.ForceOnlySelf = attrib.ForceOnlySelf;
                this.SearchChildren = attrib.SearchChildren;
                this.AllowProxy = attrib.AllowProxy;
                if (attrib.InheritsFromType != null) this.RestrictionType = attrib.InheritsFromType;
            }

            if (this.ChoiceSelector == null)
            {
                this.ChoiceSelector = DefaultComponentChoiceSelector.Default;
            }
        }

        #endregion


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;
            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();
            this.OnGUI(position, property);
            EditorHelper.ResumeIndentLevel();
        }

        public void OnGUI(Rect position, SerializedProperty property)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, GUIContent.none)) return;
            //if (property.propertyType != SerializedPropertyType.ObjectReference || !TypeUtil.IsType(_restrictionType, typeof(Component), typeof(IComponent)))
            if (property.propertyType != SerializedPropertyType.ObjectReference || (!this.AllowNonComponents && !ComponentUtil.IsAcceptableComponentType(this.RestrictionType)))
            {
                this.DrawAsMismatchedAttribute(position, property);
                return;
            }

            this.Init();

            GameObject targGo;
            if (this.ForceOnlySelf)
            {
                targGo = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
                if (targGo == null)
                {
                    this.DrawAsMismatchedAttribute(position, property);
                    return;
                }

                if (property.objectReferenceValue == null)
                {
                    property.objectReferenceValue = this.GetTargetFromSource(targGo);
                }
            }

            targGo = GameObjectUtil.GetGameObjectFromSource(property.objectReferenceValue);
            if (property.objectReferenceValue == null)
            {
                //SPEditorGUI.DefaultPropertyField(position, property, label);
                if (!this.ForceOnlySelf)
                {
                    this.DrawObjectRefField(position, property);
                }
                else
                {
                    EditorHelper.MalformedProperty(position);
                }
            }
            else if (this.AllowNonComponents)
            {
                if (targGo == null)
                {
                    if (this.ShowXButton && SPEditorGUI.XButton(ref position, "Clear Selected Object", this.XButtonOnRightSide))
                    {
                        property.objectReferenceValue = null;
                    }
                    position = this.DrawDotDotButton(position, property);
                    this.DrawObjectRefField(position, property);
                }
                else
                {
                    this.ChoiceSelector.BeforeGUI(this, property, this.RestrictionType, this.AllowProxy);

                    var fullsize = position;
                    if (this.ShowXButton && SPEditorGUI.XButton(ref position, "Clear Selected Object", this.XButtonOnRightSide))
                    {
                        property.objectReferenceValue = null;
                        fullsize = this.DrawDotDotButton(fullsize, property);
                        this.DrawObjectRefField(fullsize, property);

                        this.ChoiceSelector.GUIComplete(property, -1);
                    }
                    else
                    {
                        position = this.DrawDotDotButton(position, property);
                        var names = this.ChoiceSelector.GetPopupEntries();
                        System.Array.Resize(ref names, names.Length + 1);
                        names[names.Length - 1] = EditorHelper.TempContent(targGo.name + " (...GameObject)");

                        int oi = (property.objectReferenceValue is GameObject) ? names.Length - 1 : this.ChoiceSelector.GetPopupIndexOfComponent(property.objectReferenceValue as Component);
                        int ni = EditorGUI.Popup(position, oi, names);

                        if (oi != ni)
                        {
                            if (ni == names.Length - 1)
                                property.objectReferenceValue = this.RestrictionType.IsInstanceOfType(targGo) ? targGo : property.objectReferenceValue;
                            else
                                property.objectReferenceValue = this.ChoiceSelector.GetComponentAtPopupIndex(ni);
                        }

                        this.ChoiceSelector.GUIComplete(property, ni);
                    }
                }
            }
            else
            {
                this.ChoiceSelector.BeforeGUI(this, property, this.RestrictionType, this.AllowProxy);
                var components = this.ChoiceSelector.GetComponents();

                var fullsize = position;
                if (components.Length == 0 ||
                    (this.ShowXButton && SPEditorGUI.XButton(ref position, "Clear Selected Object", this.XButtonOnRightSide)))
                {
                    property.objectReferenceValue = null;
                    fullsize = this.DrawDotDotButton(fullsize, property);
                    this.DrawObjectRefField(fullsize, property);

                    this.ChoiceSelector.GUIComplete(property, -1);
                }
                else
                {
                    position = this.DrawDotDotButton(position, property);
                    var names = this.ChoiceSelector.GetPopupEntries();
                    int oi = this.ChoiceSelector.GetPopupIndexOfComponent(property.objectReferenceValue as Component);
                    int ni = EditorGUI.Popup(position, oi, names);
                    if (oi != ni) property.objectReferenceValue = this.ChoiceSelector.GetComponentAtPopupIndex(ni);

                    this.ChoiceSelector.GUIComplete(property, ni);
                }

            }
        }

        private Rect DrawDotDotButton(Rect position, SerializedProperty property)
        {
            var w = Mathf.Min(SPEditorGUI.X_BTN_WIDTH, position.width);
            Rect r = new Rect(position.xMax - w, position.yMin, w, EditorGUIUtility.singleLineHeight);
            position = new Rect(position.xMin, position.yMin, position.width - w, position.height);

            if (GUI.Button(r, EditorHelper.TempContent("...")))
            {
                EditorGUIUtility.PingObject(property.objectReferenceValue);
            }

            return position;
        }

        private void DrawObjectRefField(Rect position, SerializedProperty property)
        {
            UnityEngine.Object nextobj; //object returned from ObjectField.

            /*
            if (ComponentUtil.IsAcceptableComponentType(this.RestrictionType))
            {
                //var fieldObjType = (!this.SearchChildren && !this.AllowProxy && TypeUtil.IsType(this.RestrictionType, typeof(UnityEngine.Component))) ? this.RestrictionType : typeof(UnityEngine.GameObject);
                System.Type fieldObjType;
                if (this.AllowProxy)
                    fieldObjType = typeof(UnityEngine.Object);
                else if (!this.SearchChildren && TypeUtil.IsType(this.RestrictionType, typeof(UnityEngine.Component)))
                    fieldObjType = this.RestrictionType;
                else
                    fieldObjType = typeof(UnityEngine.GameObject);

                nextobj = EditorGUI.ObjectField(position, property.objectReferenceValue, fieldObjType, this.AllowSceneObjects);
            }
            else if (this.AllowNonComponents)
            {
                var fieldObjType = (!this.AllowProxy && TypeUtil.IsType(this.RestrictionType, typeof(UnityEngine.Object))) ? this.RestrictionType : typeof(UnityEngine.Object);
                nextobj = EditorGUI.ObjectField(position, property.objectReferenceValue, fieldObjType, this.AllowSceneObjects);
            }
            else
            {
                var ogo = GameObjectUtil.GetGameObjectFromSource(property.objectReferenceValue);
                nextobj = EditorGUI.ObjectField(position, ogo, typeof(GameObject), this.AllowSceneObjects) as GameObject;
            }
            */
            if (this.AllowNonComponents || ComponentUtil.IsAcceptableComponentType(this.RestrictionType))
            {
                if (this.AllowProxy || (this.RestrictionType != null && !TypeUtil.IsType(this.RestrictionType, typeof(UnityEngine.Object))))
                {
                    nextobj = SPEditorGUI.AdvancedObjectField(position, GUIContent.none, property.objectReferenceValue, this.RestrictionType, this.AllowSceneObjects, this.AllowProxy);
                }
                else
                {
                    nextobj = EditorGUI.ObjectField(position, property.objectReferenceValue, this.RestrictionType ?? typeof(UnityEngine.Object), this.AllowSceneObjects);
                }
            }
            else
            {
                var ogo = GameObjectUtil.GetGameObjectFromSource(property.objectReferenceValue);
                nextobj = EditorGUI.ObjectField(position, ogo, typeof(GameObject), this.AllowSceneObjects) as GameObject;
            }

            if (this.ForceOnlySelf)
            {
                var targGo = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
                var ngo = GameObjectUtil.GetGameObjectFromSource(nextobj);
                if (targGo == ngo ||
                   (this.SearchChildren && targGo.IsParentOf(ngo)))
                {
                    property.objectReferenceValue = this.GetTargetFromSource(nextobj);
                }
            }
            else
            {
                property.objectReferenceValue = this.GetTargetFromSource(nextobj);
            }
        }

        private UnityEngine.Object GetTargetFromSource(UnityEngine.Object obj)
        {
            if (obj == null) return null;
            if (ObjUtil.IsType(obj, this.RestrictionType)) return obj;
            if (this.AllowProxy && obj is IProxy) return obj;

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            var o = ObjUtil.GetAsFromSource(this.RestrictionType, obj) as UnityEngine.Object;

            if (this.SearchChildren && o == null && go != null)
                o = go.GetComponentInChildren(this.RestrictionType);

            if (this.AllowProxy && o == null && go != null)
            {
                if (this.SearchChildren)
                    o = go.GetComponentInChildren<IProxy>() as Component;
                else
                    o = go.GetComponent<IProxy>() as Component;
            }

            return o;
        }


        private void DrawAsMismatchedAttribute(Rect position, SerializedProperty property)
        {
            EditorGUI.LabelField(position, EditorHelper.TempContent("Mismatched type of PropertyDrawer attribute with field."));
        }

    }

}
