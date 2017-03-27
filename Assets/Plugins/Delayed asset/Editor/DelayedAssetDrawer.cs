//
// Copyright (C) 2017 Trinidad Sibajas Bodoque
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(DelayedAsset))]
public class DelayedAssetDrawer : PropertyDrawer
{
    bool isDraggingValidProxy;








    /// <summary>
    /// <see cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/> implementation.
    /// </summary>

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Check if the field has a type attribute, and get the type:
        int dotIndex = property.propertyPath.IndexOf('.');
        string fieldName = dotIndex == -1  ?  property.propertyPath  :  property.propertyPath.Substring(0, dotIndex);

        FieldInfo fieldInfo = property.serializedObject.targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        DelayedAssetTypeAttribute typeAttribute = (DelayedAssetTypeAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DelayedAssetTypeAttribute));

        Type desiredType = typeAttribute != null  ?  typeAttribute.Type  :  typeof(UnityEngine.Object);


        // If a DelayedAssetProxy is being dragged into the slot, and the type of the referenced asset matches the desired type, allow to set it:
        if (Event.current.type == EventType.DragUpdated)
        {
            UnityEngine.Object draggedObject = DragAndDrop.objectReferences.Length == 0  ?  null  :  DragAndDrop.objectReferences[0];
            isDraggingValidProxy = draggedObject is DelayedAssetProxy  &&  ((DelayedAssetProxy)draggedObject).Asset != null  &&  desiredType.IsAssignableFrom(((DelayedAssetProxy)draggedObject).Asset.GetType())  &&  position.Contains(Event.current.mousePosition);
        }
        else if (Event.current.type == EventType.DragExited)
        {
            isDraggingValidProxy = false;
        }

        
        if (isDraggingValidProxy)
            desiredType = typeof(DelayedAssetProxy);
        
        
        // Begin the property:
        EditorGUI.BeginProperty(position, label, property);


        // Check for changes in the property:
        EditorGUI.BeginChangeCheck();


        // Draw the property:
        SerializedProperty assetProperty = property.FindPropertyRelative("asset");
        label.text = "{" + label.text + "}";

        assetProperty.objectReferenceValue = EditorGUI.ObjectField(position, label, assetProperty.objectReferenceValue, desiredType, false);


        // If an object has been assigned, check if there is some problem with it:
        if (EditorGUI.EndChangeCheck()  &&  assetProperty.objectReferenceValue != null)
        {
            string errorString = null;
            string assetRelativePath = DelayedAsset.GetRelativeAssetPath(AssetDatabase.GetAssetPath(assetProperty.objectReferenceValue));

            if (assetRelativePath == null)
            {
                errorString = "The assigned asset is not inside a \"Resources\" folder.";
            }
            else
            {
                UnityEngine.Object otherAsset = DelayedAsset.FindAssetWithSameTypeAndRelativePath(assetProperty.objectReferenceValue, assetRelativePath, desiredType);
                if (otherAsset != null)
                {
                    errorString = "The assigned asset doesn't have a unique type and path relative to a \"Resources\" folder, see the asset \"" + AssetDatabase.GetAssetPath(otherAsset) + "\".";
                }
            }


            if (errorString != null)
            {
                assetProperty.objectReferenceValue = null;
                EditorUtility.DisplayDialog("Delayed asset error", errorString, "OK");
                Debug.LogError("Delayed asset error: " + errorString);
            }
        }


        // End of the property:
        EditorGUI.EndProperty();
    }
}
