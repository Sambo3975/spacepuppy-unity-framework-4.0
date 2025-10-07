﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppyeditor.Internal
{
    public static class ScriptAttributeUtility
    {

        public static readonly IPropertyHandler SharedNullPropertyHandler = new DefaultPropertyHandler();
        public static readonly IPropertyHandler SharedNullInternalPropertyHandler = new UnityInternalPropertyHandler();

        #region Fields

        private static PropertyHandlerCache _handlerCache = new PropertyHandlerCache();



        //Internal Wrapper Fields

        private static TypeAccessWrapper _accessWrapper;

        private delegate System.Reflection.FieldInfo GetFieldInfoFromPropertyDelegate(SerializedProperty property, out System.Type type);
        private static GetFieldInfoFromPropertyDelegate _imp_getFieldInfoFromProperty;

        private static System.Func<SerializedProperty, System.Type> _imp_getScriptTypeFromProperty;

        private static System.Func<SerializedProperty, object> _imp_getHandler;

        #endregion

        #region CONSTRUCTOR

        static ScriptAttributeUtility()
        {
            var klass = InternalTypeUtil.UnityEditorAssembly.GetType("UnityEditor.ScriptAttributeUtility");
            _accessWrapper = new TypeAccessWrapper(klass, true);
        }

        #endregion

        #region Properties

        internal static object InternalPropertyHandlerCache
        {
            get => _accessWrapper.GetStaticProperty("propertyHandlerCache");
        }

        #endregion

        #region Methods

        public static void ResetPropertyHandler(SerializedProperty property, bool includeChildren)
        {
            if (property == null) return;

            _handlerCache.SetHandler(property, null);
            if (includeChildren)
            {
                foreach (var child in property.GetChildren())
                {
                    _handlerCache.SetHandler(child, null);
                }
            }
        }

        //#######################
        // GetDrawerTypeForType

#if UNITY_2023_3_OR_NEWER
        private static System.Func<System.Type, System.Type[], bool, System.Type> _imp_getDrawerTypeForType;
        public static System.Type GetDrawerTypeForType(System.Type tp)
        {
            if (_imp_getDrawerTypeForType == null) _imp_getDrawerTypeForType = _accessWrapper.GetStaticMethod("GetDrawerTypeForType", typeof(System.Func<System.Type, System.Type[], bool, System.Type>)) as System.Func<System.Type, System.Type[], bool, System.Type>;
            return _imp_getDrawerTypeForType(tp, null, false);
        }
#elif UNITY_2022_3_OR_NEWER
        private static System.Func<System.Type, bool, System.Type> _imp_getDrawerTypeForType;
        public static System.Type GetDrawerTypeForType(System.Type tp)
        {
            if (_imp_getDrawerTypeForType == null)
            {
                if (!_accessWrapper.TryGetStaticMethod<System.Func<System.Type, bool, System.Type>>("GetDrawerTypeForType", out _imp_getDrawerTypeForType))
                {
                    if (_accessWrapper.TryGetStaticMethod<System.Func<System.Type, System.Type>>("GetDrawerTypeForType", out System.Func<System.Type, System.Type> olddel)) //depending how old the version of 2022_3 you're in, it might still be the older version
                    {
                        _imp_getDrawerTypeForType = (t, b) => olddel(t);
                    }
                    else
                    {
                        _imp_getDrawerTypeForType = (t, b) => null;
                    }
                }
            }
            return _imp_getDrawerTypeForType(tp, false);
        }
#else
        private static System.Func<System.Type, System.Type> _imp_getDrawerTypeForType;
        public static System.Type GetDrawerTypeForType(System.Type tp)
        {
            if (_imp_getDrawerTypeForType == null) _imp_getDrawerTypeForType = _accessWrapper.GetStaticMethod("GetDrawerTypeForType", typeof(System.Func<System.Type, System.Type>)) as System.Func<System.Type, System.Type>;
            return _imp_getDrawerTypeForType(tp);
        }
#endif

        //#######################
        // GetHandler

        public static IPropertyHandler GetHandler(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            IPropertyHandler result = _handlerCache.GetHandler(property);
            if (result != null)
            {
                return result;
            }

            //TEST FOR SPECIAL CASE HANDLER
            var fieldInfo = ScriptAttributeUtility.GetFieldInfoFromProperty(property);
            //if (fieldInfo != null)
            //{
            //    var attribs = fieldInfo.GetCustomAttributes(typeof(PropertyAttribute), false) as PropertyAttribute[];
            //    var isListType = property.isArray && !(property.propertyType == SerializedPropertyType.String);
            //    result = new MultiPropertyAttributePropertyHandler(property, fieldInfo, isListType, attribs);
            //    _handlerCache.SetHandler(property, result);
            //    return result;
            //}
            if (fieldInfo != null && System.Attribute.IsDefined(fieldInfo, typeof(SPPropertyAttribute)))
            {
                var attribs = fieldInfo.GetCustomAttributes(typeof(PropertyAttribute), false) as PropertyAttribute[];
                if (attribs.Any((a) => a is SPPropertyAttribute))
                {
                    var isListType = property.isArray && !(property.propertyType == SerializedPropertyType.String);
                    result = new MultiPropertyAttributePropertyHandler(property, fieldInfo, isListType, attribs);
                    _handlerCache.SetHandler(property, result);
                    return result;
                }
            }

            //USE STANDARD HANDLER if none was found
            var handler = GetInternalHandler(property);
            //_handlerCache.SetHandler(property, handler);
            return handler;
        }

        public static IPropertyHandler GetInternalHandler(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            if (_imp_getHandler == null) _imp_getHandler = _accessWrapper.GetStaticMethod("GetHandler", typeof(System.Func<SerializedProperty, object>)) as System.Func<SerializedProperty, object>;
            var ihandler = _imp_getHandler(property);
            var handler = ihandler != null ? new UnityInternalPropertyHandler(ihandler) : ScriptAttributeUtility.SharedNullPropertyHandler;
            return handler;
        }

        public static bool TryGetInternalPropertyDrawer(SerializedProperty property, out PropertyDrawer drawer)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var handler = GetHandler(property);
            if (handler is UnityInternalPropertyHandler uiph)
            {
                drawer = uiph.InternalDrawer;
                return true;
            }
            else
            {
                drawer = null;
                return false;
            }
        }

        //#######################
        // GetFieldInfoFromProperty

        public static System.Reflection.FieldInfo GetFieldInfoFromProperty(SerializedProperty property)
        {
            if (_imp_getFieldInfoFromProperty == null) _imp_getFieldInfoFromProperty = _accessWrapper.GetStaticMethod("GetFieldInfoFromProperty", typeof(GetFieldInfoFromPropertyDelegate)) as GetFieldInfoFromPropertyDelegate;
            System.Type type;
            return _imp_getFieldInfoFromProperty(property, out type);
        }

        public static System.Reflection.FieldInfo GetFieldInfoFromProperty(SerializedProperty property, out System.Type type)
        {
            if (_imp_getFieldInfoFromProperty == null) _imp_getFieldInfoFromProperty = _accessWrapper.GetStaticMethod("GetFieldInfoFromProperty", typeof(GetFieldInfoFromPropertyDelegate)) as GetFieldInfoFromPropertyDelegate;
            return _imp_getFieldInfoFromProperty(property, out type);
        }

        /*
        /// <summary>
        /// Returns the fieldInfo of the property. If the property is an Array/List, the fieldInfo for the Array is returned.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static FieldInfo GetFieldInfoFromProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var scriptType = GetScriptTypeFromProperty(prop);
            var elements = path.Split('.');

            FieldInfo result = null;
            System.Type tp = scriptType;
            foreach (var element in elements)
            {
                if (element.Contains('['))
                {
                    var name = element.Substring(0, element.IndexOf('['));
                    FieldInfo info = null;
                    for (var tp2 = tp; info == null && tp2 != null; tp2 = tp2.BaseType)
                        info = tp2.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (info == null)
                    {
                        return null;
                    }
                    else
                    {
                        result = info;
                        tp = info.FieldType;
                    }

                    if (ObjUtil.IsListType(tp))
                    {
                        tp = ObjUtil.GetElementTypeOfListType(tp);
                    }
                }
                else
                {
                    FieldInfo info = null;
                    for (var tp2 = tp; info == null && tp2 != null; tp2 = tp2.BaseType)
                        info = tp2.GetField(element, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (info == null)
                    {
                        return null;
                    }
                    else
                    {
                        result = info;
                        tp = info.FieldType;
                    }
                }
            }

            return result;
        }
         */

        //#######################
        // GetScriptTypeFromProperty

        public static System.Type GetScriptTypeFromProperty(SerializedProperty property)
        {
            if (_imp_getScriptTypeFromProperty == null) _imp_getScriptTypeFromProperty = _accessWrapper.GetStaticMethod("GetScriptTypeFromProperty", typeof(System.Func<SerializedProperty, System.Type>)) as System.Func<SerializedProperty, System.Type>;
            return _imp_getScriptTypeFromProperty(property);
        }

        /*
        public static System.Type GetScriptTypeFromProperty(SerializedProperty prop)
        {
            SerializedProperty scriptProp = prop.serializedObject.FindProperty(PROP_SCRIPT);
            if (scriptProp == null)
                return null;
            MonoScript monoScript = scriptProp.objectReferenceValue as MonoScript;
            if ((UnityEngine.Object)monoScript == (UnityEngine.Object)null)
                return null;
            else
                return monoScript.GetClass();
        }
         */

        #endregion

    }
}
