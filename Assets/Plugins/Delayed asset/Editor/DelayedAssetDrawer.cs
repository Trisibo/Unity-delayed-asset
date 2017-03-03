using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(DelayedAsset))]
public class DelayedAssetDrawer : PropertyDrawer
{
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


        // Begin the property:
        EditorGUI.BeginProperty(position, label, property);


        // Check for changes in the property:
        EditorGUI.BeginChangeCheck();


        // Draw the property:
        SerializedProperty assetProperty = property.FindPropertyRelative("asset");
        label.text = "[" + label.text + "]";

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
