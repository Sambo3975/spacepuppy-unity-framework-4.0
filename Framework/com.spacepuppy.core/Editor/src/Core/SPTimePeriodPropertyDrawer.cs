﻿using UnityEngine;
using UnityEditor;
using System.Linq;

using com.spacepuppy;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SPTimePeriod))]
    public class SPTimePeriodPropertyDrawer : PropertyDrawer
    {

        public const string PROP_SECONDS = "_seconds";

        private TimeUnitsSelectorPropertyDrawer _timeDrawer = new TimeUnitsSelectorPropertyDrawer();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            position = SPEditorGUI.SafePrefixLabel(position, label);
            this.DrawTimePeriodSansLabel(position, property);
        }

        protected virtual void DrawTimePeriodSansLabel(Rect position, SerializedProperty property)
        {
            var secondsProp = property.FindPropertyRelative(PROP_SECONDS);
            var w = position.width / 3f;
            var attrib = this.fieldInfo.GetCustomAttributes(typeof(SPTime.Config), false).FirstOrDefault() as SPTime.Config;
            var availNames = (attrib != null) ? attrib.AvailableCustomTimeNames : null;
            _timeDrawer.DefaultUnits = (attrib is SPTimePeriod.Config spta) ? spta.DefaultUnits : null;

            try
            {
                EditorHelper.SuppressIndentLevel();

                if (w > 75f)
                {
                    position = _timeDrawer.DrawDuration(position, secondsProp, Mathf.Min(w, 150f));
                    position = _timeDrawer.DrawUnits(position, secondsProp, 75f);
                    position = SPTimePropertyDrawer.DrawTimeSupplier_SansPrefixLabel(position, property, position.width, availNames); //we mirror the SPTime prop drawer, we can do this because the property names are identical
                }
                else
                {
                    position = _timeDrawer.DrawDuration(position, secondsProp, w);
                    position = _timeDrawer.DrawUnits(position, secondsProp, w);


                    position = SPTimePropertyDrawer.DrawTimeSupplier_SansPrefixLabel(position, property, position.width, availNames); //we mirror the SPTime prop drawer, we can do this because the property names are identical
                }
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }

    }

    [CustomPropertyDrawer(typeof(SPTimeSpan))]
    public class SPTimeSpanPropertyDrawer : PropertyDrawer
    {

        public const string PROP_SECONDS = "_seconds";

        private TimeUnitsSelectorPropertyDrawer _timeDrawer = new TimeUnitsSelectorPropertyDrawer();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            position = SPEditorGUI.SafePrefixLabel(position, label);
            this.DrawTimePeriodSansLabel(position, property);
        }

        protected virtual void DrawTimePeriodSansLabel(Rect position, SerializedProperty property)
        {
            var secondsProp = property.FindPropertyRelative(PROP_SECONDS);
            var w = position.width / 2f;
            var attrib = this.fieldInfo.GetCustomAttributes(typeof(SPTimeSpan.Config), false).FirstOrDefault() as SPTimeSpan.Config;
            _timeDrawer.DefaultUnits = attrib?.DefaultUnits ?? null;

            try
            {
                EditorHelper.SuppressIndentLevel();

                if (w > 75f)
                {
                    position = _timeDrawer.DrawDuration(position, secondsProp, position.width - 75f);
                    position = _timeDrawer.DrawUnits(position, secondsProp, 75f);
                }
                else
                {
                    position = _timeDrawer.DrawDuration(position, secondsProp, w);
                    position = _timeDrawer.DrawUnits(position, secondsProp, w);
                }
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }

    }

}
