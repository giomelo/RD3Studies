//
// Deferred AO - SSAO image effect for deferred shading
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(DeferredAO))]
public class DeferredAOEditor : Editor
{
    SerializedProperty _intensity;
    SerializedProperty _sampleRadius;
    SerializedProperty _rangeCheck;
    SerializedProperty _fallOffDistance;
    SerializedProperty _sampleCount;
    private DeferredAO _deferredAO;
    void OnEnable()
    {
        _intensity = serializedObject.FindProperty("_intensity");
        _sampleRadius = serializedObject.FindProperty("_sampleRadius");
        _rangeCheck = serializedObject.FindProperty("_rangeCheck");
        _fallOffDistance = serializedObject.FindProperty("_fallOffDistance");
        _sampleCount = serializedObject.FindProperty("_sampleCount");
    }

    bool CheckDisabled()
    {
        var cam = ((DeferredAO)target).GetComponent<Camera>();
        return cam.actualRenderingPath != RenderingPath.DeferredShading;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (CheckDisabled())
        {
            var text = "To enable the effect, change Rendering Path to Deferred.";
            EditorGUILayout.HelpBox(text, MessageType.Warning);
        }
        else
        {
            SerializedProperty shader = serializedObject.FindProperty("_shader");
            EditorGUILayout.PropertyField(_intensity);
            EditorGUILayout.PropertyField(_sampleRadius);
            EditorGUILayout.PropertyField(_rangeCheck);
            EditorGUILayout.PropertyField(_fallOffDistance);
            EditorGUILayout.PropertyField(_sampleCount);
            EditorGUILayout.PropertyField(shader);
            
        }

        serializedObject.ApplyModifiedProperties();
    }
}
