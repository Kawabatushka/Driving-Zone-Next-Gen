using System;
using UnityEngine;
using UnityEditor;

// Usage Example 1 (float): [RangeEx(0f, 10f, 0.25f)]
// Usage Example 2 (int): [RangeEx(1, 100, 25)]
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeExAttribute : PropertyAttribute
{
    internal readonly float m_min = 0f;
    internal readonly float m_max = 100f;
    internal readonly float m_step = 1;
    // Whether a increase that is not the step is allowed (Occurs when we are reaching the end)
    internal readonly bool m_allowNonStepReach = true;
    internal readonly bool m_IsInt = false;

    /// <summary>
    /// Allow you to increase a float value in step, make sure the type of the variable matches the the parameters
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="step"></param>
    /// <param name="allowNonStepReach">Whether a increase that is not the step is allowed (Occurs when we are reaching the end)</param>
    public RangeExAttribute(float min, float max, float step = 1f, bool allowNonStepReach = true)
    {
        m_min = min;
        m_max = max;
        m_step = step;
        m_allowNonStepReach = allowNonStepReach;
        m_IsInt = false;
    }

    /// <summary>
    /// Allow you to increase a int value in step, make sure the type of the variable matches the the parameters
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="step"></param>
    /// <param name="allowNonStepReach"></param>
    public RangeExAttribute(int min, int max, int step = 1, bool allowNonStepReach = true)
    {
        m_min = min;
        m_max = max;
        m_step = step;
        m_allowNonStepReach = allowNonStepReach;
        m_IsInt = true;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RangeExAttribute))]
internal sealed class RangeStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rangeAttribute = (RangeExAttribute)base.attribute;

        if (!rangeAttribute.m_IsInt)
        {
            float rawFloat = EditorGUI.Slider(position, label, property.floatValue, rangeAttribute.m_min, rangeAttribute.m_max);
            property.floatValue = Step(rawFloat, rangeAttribute);
        }
        else
        {
            int rawInt = EditorGUI.IntSlider(position, label, property.intValue, (int)rangeAttribute.m_min, (int)rangeAttribute.m_max);
            property.intValue = Step(rawInt, rangeAttribute);
        }
       
    }

    // This is time tested and bug free!
    // I stayed up late until 2:50 AM in September 23 2022 trying to get this right, relentless curiocity paid off
    internal float Step(float rawValue, RangeExAttribute range)
    {
        float f = rawValue;

        if (range.m_allowNonStepReach)
        {
            // In order to ensure a reach, where the difference between rawValue and the max allowed value is less than step
            float topCap = Mathf.Floor(range.m_max / range.m_step) * range.m_step;
            float topRemaining = range.m_max - topCap;

            // If this is the special case near the top maximum
            if (topRemaining < range.m_step && f > topCap)
            {
                f = range.m_max;
            }
            else
            {
                // Otherwise we do a regular snap
                f = Snap(f, range.m_step);
            }
        }
        else if(!range.m_allowNonStepReach)
        {
            f = Snap(f, range.m_step);
            // Make sure the value doesn't exceed the maximum allowed range
            if (f > range.m_max)
            {
                f -= range.m_step;
            }
        }

        return f;
    }

    internal int Step(int rawValue, RangeExAttribute range)
    {
        int f = rawValue;

        if (range.m_allowNonStepReach)
        {
            // In order to ensure a reach, where the difference between rawValue and the max allowed value is less than step
            int topCap = (int)range.m_max / (int)range.m_step * (int)range.m_step;
            int topRemaining = (int)range.m_max - topCap;

            // If this is the special case near the top maximum
            if (topRemaining < range.m_step && f > topCap)
            {
                f = (int)range.m_max;
            }
            else
            {
                // Otherwise we do a regular snap
                f = (int)Snap(f, range.m_step);
            }
        }
        else if (!range.m_allowNonStepReach)
        {
            f = (int)Snap(f, range.m_step);
            // Make sure the value doesn't exceed the maximum allowed range
            if (f > range.m_max)
            {
                f -= (int)range.m_step;
            }
        }

        return f;
    }

    /// <summary>
    /// Snap a value to a interval
    /// </summary>
    /// <param name="value"></param>
    /// <param name="snapInterval"></param>
    /// <returns></returns>
    internal static float Snap(float value, float snapInterval)
    {
        return Mathf.Round(value / snapInterval) * snapInterval;
    }
}
#endif