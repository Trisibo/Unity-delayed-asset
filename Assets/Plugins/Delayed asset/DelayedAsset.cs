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
using System;

/// <summary>
/// Use on a serialized field of another MonoBehaviour object to allow loading an asset on demand,
/// instead of when the container object is loaded.
/// </summary>

[Serializable]
public class DelayedAsset : ISerializationCallbackReceiver
{
    // The original asset, only available on the editor to be able to set it from the inspector:
    #if UNITY_EDITOR
    [SerializeField] UnityEngine.Object asset;
    #endif


    // The data needed to load the original asset:
    [SerializeField] string assetRelativePath;
    [SerializeField] string assetTypeString;

    Type assetType;








    #region Asset loading methods


    /// <summary>
    /// Loads the original asset.
    /// </summary>
    /// <returns>The loaded asset, or null if it wasn't found.</returns>

    public UnityEngine.Object Load()
    {
        if (string.IsNullOrEmpty(assetRelativePath))
            return null;

        return Resources.Load(assetRelativePath, assetType);
    }


    #endregion








    #region ISerializationCallbackReceiver implementation


    /// <summary>
    /// <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/> implementation.
    /// </summary>

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        #if UNITY_EDITOR
        {
            assetRelativePath = null;
            assetTypeString   = null;

            if (asset != null)
            {
                string errorTextPart = null;

                assetType         = asset.GetType();
                assetRelativePath = GetRelativeAssetPath(UnityEditor.AssetDatabase.GetAssetPath(asset));

                if (assetRelativePath == null)
                {
                    // Error: the asset is not inside a "Resources" folder:
                    errorTextPart = "is not inside a \"Resources\" folder.";
                }
                else
                {
                    UnityEngine.Object otherAsset = FindAssetWithSameTypeAndRelativePath(asset, assetRelativePath, assetType);
                    if (otherAsset != null)
                    {
                        // Error: there's another asset with the same path (could have a different extension, or be in a different "Resources" folder):
                        errorTextPart = "doesn't have a unique type and path relative to a \"Resources\" folder, see the asset \"" + UnityEditor.AssetDatabase.GetAssetPath(otherAsset) + "\".";
                        assetRelativePath = null;
                    }
                }


                // Check if there was some error; if not, set the rest of the data:
                if (assetRelativePath == null)
                {
                    Debug.LogError("The asset \"" + UnityEditor.AssetDatabase.GetAssetPath(asset) + "\", referenced by a DelayedAsset object, " + errorTextPart);
                }
                else
                {
                    assetTypeString = assetType.AssemblyQualifiedName;
                }
            }
        }
        #endif
    }








    /// <summary>
    /// <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/> implementation.
    /// </summary>

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        assetType = string.IsNullOrEmpty(assetTypeString)  ?  null  :  Type.GetType(assetTypeString);
    }


    #endregion








    #region Utility methods, only on the editor
    #if UNITY_EDITOR


    /// <summary>
    /// Retrieves the asset path relative to a resources folder.
    /// </summary>
    /// <param name="assetAbsolutePath">The absolute path of the asset.</param>
    /// <returns>The path relative to a resources folder, null if the asset is not inside a resources folder, either directly or inside a subfolder in the hierarchy.</returns>

    public static string GetRelativeAssetPath(string assetAbsolutePath)
    {
        const string resourcesPathString = "/Resources/";

        int resourcesStringIndex = assetAbsolutePath.IndexOf(resourcesPathString);
        if (resourcesStringIndex == -1)
        {
            return null;
        }

        int start = resourcesStringIndex + resourcesPathString.Length;
        int dot   = assetAbsolutePath.LastIndexOf('.');
        return assetAbsolutePath.Substring(start, (dot >= 0 ? dot : assetAbsolutePath.Length) - start);
    }








    /// <summary>
    /// Searches for any asset with the same path relative to a resources folder and the same type as the supplied asset.
    /// </summary>
    /// <param name="originalAsset">The original asset instance.</param>
    /// <param name="relativePath">The path relative to a resources folder.</param>
    /// <param name="type">The type.</param>
    /// <returns>One of the assets found, null if none.</returns>

    public static UnityEngine.Object FindAssetWithSameTypeAndRelativePath(UnityEngine.Object originalAsset, string relativePath, Type type)
    {
        UnityEngine.Object foundAsset = null;

        UnityEngine.Object[] allAssets = Resources.LoadAll(relativePath, type);
        for (int i = 0;  i < allAssets.Length;  i++)
        {
            if (!allAssets[i].Equals(originalAsset))
            {
                foundAsset = allAssets[i];
                break;
            }
        }

        return foundAsset;
    }


    #endif
    #endregion
}
