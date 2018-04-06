//
// Copyright (C) 2018 Trinidad Sibajas Bodoque
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

namespace Trisibo
{
    /// <summary>
    /// Use on a serialized field of another MonoBehaviour object to allow loading an asset on demand,
    /// instead of when the container object is loaded.
    /// </summary>

    [Serializable]
    public class DelayedAsset : ISerializationCallbackReceiver
    {
        #region Data


        // The asset, only available on the editor to be able to set it from the inspector:
        #if UNITY_EDITOR
        [SerializeField] UnityEngine.Object asset;
        #endif


        // The data needed to load the asset:
        [SerializeField] string assetRelativePath;
        [SerializeField] string assetTypeString;

        Type assetType;


        // The asset once loaded at runtime:
        UnityEngine.Object _loadedAsset;
    
        UnityEngine.Object loadedAsset
        {
            get
            {
                if (_loadedAsset == null  &&  asyncLoadedAssetGetter != null)
                    _loadedAsset = asyncLoadedAssetGetter();
                return _loadedAsset;
            }

            set
            {
                _loadedAsset = value;
            }
        }


        // The async load request, if any:
        AsyncLoadRequest asyncLoadRequest;
        Func<UnityEngine.Object> asyncLoadedAssetGetter;


        #endregion








        #region Data types


        /// <summary>
        /// Represents an async request to load the original asset.
        /// Behaves in a similar way to Unity's <see cref="ResourceRequest"/>.
        /// </summary>

        public class AsyncLoadRequest : CustomYieldInstruction
        {
            // The original resource request. Can be null if no resource request was done:
            readonly ResourceRequest resourceRequest;


            // The already loaded asset if there was no resource request, null otherwise:
            readonly UnityEngine.Object assetIfNoResourceRequest;




            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="resourceRequest">The resource request used to load the asset. Cannot be null; if no resource request was done, use the constructor that accepts an already loaded asset.</param>
            /// <param name="actualLoadedAssetGetter">Receives a method that returns the loaded asset (not the original asset).</param>

            public AsyncLoadRequest(ResourceRequest resourceRequest, out Func<UnityEngine.Object> actualLoadedAssetGetter)
            {
                this.resourceRequest = resourceRequest;
                actualLoadedAssetGetter = GetActualLoadedAsset;
            }




            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="alreadyLoadedAsset">This must contain an already loaded asset. Use this constructor only if there was no resource request done.</param>
            /// <param name="actualLoadedAssetGetter">Receives a method that returns the loaded asset (not the original asset).</param>

            public AsyncLoadRequest(UnityEngine.Object alreadyLoadedAsset, out Func<UnityEngine.Object> actualLoadedAssetGetter)
            {
                assetIfNoResourceRequest = alreadyLoadedAsset;
                actualLoadedAssetGetter = GetActualLoadedAsset;
            }




            /// <summary>
            /// <para>Has the asset loading finished?</para>
            /// <para>See <see cref="AsyncOperation.isDone"/> documentation for more details.</para>
            /// </summary>

            public bool IsDone
            {
                get
                {
                    return resourceRequest != null  ?  resourceRequest.isDone  :  true; 
                }
            }




            /// <summary>
            /// <para>Allows tweaking the order in which async operations will be performed.</para>
            /// <para>See <see cref="AsyncOperation.priority"/> documentation for more details.</para>
            /// <para>If no actual loading was necessary, assigning a value will have no effect, and will always return -1.</para>
            /// </summary>

            public int Priority
            {
                get
                {
                    return resourceRequest != null  ?  resourceRequest.priority  :  -1;
                }

                set
                {
                    if (resourceRequest != null)
                        resourceRequest.priority = value;
                }
            }




            /// <summary>
            /// <para>The progress of the operation.</para>
            /// <para>See <see cref="AsyncOperation.progress"/> documentation for more details.</para>
            /// </summary>

            public float Progress
            {
                get
                {
                    return resourceRequest != null  ?  resourceRequest.progress  :  1;
                }
            }




            /// <summary>
            /// <para>Asset object being loaded.</para>
            /// <para>See <see cref="ResourceRequest.asset"/> documentation for more details. The difference is that trying to get the value before <see cref="IsDone"/> is true won't stall the loading process, but will cause an <see cref="InvalidOperationException"/>.</para>
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown if <see cref="IsDone"/> is false when trying to access the asset.</exception>

            public UnityEngine.Object Asset
            {
                get
                {
                    if (!IsDone)
                        throw new InvalidOperationException("Tried to access the asset when IsDone was false");

                    return GetOriginalAsset(GetActualLoadedAsset());
                }
            }




            /// <summary>
            /// <see cref="CustomYieldInstruction.keepWaiting"/> implementation.
            /// </summary>

            public override bool keepWaiting
            {
                get
                {
                    return !IsDone;
                }
            }




            /// <summary>
            /// Returns the loaded asset (which will be different to the original asset in case the loaded asset is a <see cref="DelayedAssetProxy"/>).
            /// Will return null if <see cref="IsDone"/> is false.
            /// </summary>
            /// <returns>The loaded asset.</returns>

            UnityEngine.Object GetActualLoadedAsset()
            {
                if (IsDone)
                    return resourceRequest != null  ?  resourceRequest.asset  :  assetIfNoResourceRequest;
                else
                    return null;
            }
        }


        #endregion








        #region Asset loading methods


        /// <summary>
        /// Loads the original asset. Does not attempt to load it again if it was already loaded.
        /// To unload it do not use <see cref="Resources.UnloadAsset(UnityEngine.Object)"/>, use <see cref="Unload"/> instead.
        /// </summary>
        /// <returns>The loaded asset, or null if it wasn't found or hadn't been assigned.</returns>

        public UnityEngine.Object Load()
        {
            if (loadedAsset == null  &&  !string.IsNullOrEmpty(assetRelativePath))
            {
                loadedAsset = Resources.Load(assetRelativePath, assetType);
            }

            return GetOriginalAsset(loadedAsset);
        }








        /// <summary>
        /// Asynchronously loads the original asset. Does not attempt to load it again if it was already loaded.
        /// To unload it do not use <see cref="Resources.UnloadAsset(UnityEngine.Object)"/>, use <see cref="Unload"/> instead.
        /// </summary>
        /// <returns>An <see cref="AsyncLoadRequest"/> object, from which the asset can be retrieved once the operation is completed. Will be null if the original asset couldn't be found, or hadn't been assigned.</returns>

        public AsyncLoadRequest LoadAsync()
        {
            if (asyncLoadRequest == null)
            {
                if (loadedAsset != null)
                {
                    asyncLoadRequest = new AsyncLoadRequest(loadedAsset, out asyncLoadedAssetGetter);
                }
                else if (!string.IsNullOrEmpty(assetRelativePath))
                {
                    ResourceRequest resourceRequest = Resources.LoadAsync(assetRelativePath, assetType);
                    asyncLoadRequest = new AsyncLoadRequest(resourceRequest, out asyncLoadedAssetGetter);
                }
            }

            return asyncLoadRequest;
        }








        /// <summary>
        /// Returns the original asset from the specified loaded asset, checking if the loaded asset is a <see cref="DelayedAssetProxy"/> instance.
        /// </summary>
        /// <param name="loadedAsset">The loaded asset.</param>
        /// <returns>The original asset.</returns>

        static UnityEngine.Object GetOriginalAsset(UnityEngine.Object loadedAsset)
        {
            return (loadedAsset is DelayedAssetProxy)  ?  ((DelayedAssetProxy)loadedAsset).Asset  :  loadedAsset;
        }


        #endregion








        #region Asset unloading methods


        /// <summary>
        /// <para>Unloads the original asset if it's currently loaded.</para>
        /// <para>Must not be called if there's an unfinished <see cref="AsyncLoadRequest"/> operation (generated from a previous call to <see cref="LoadAsync"/>).</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there's an unfinished <see cref="AsyncLoadRequest"/> operation when this method is called.</exception>

        public void Unload()
        {
            if (asyncLoadRequest != null  &&  !asyncLoadRequest.IsDone)
                throw new InvalidOperationException("Called DelayedAsset.Unload() when there was an AsyncLoadRequest operation in progress");


            if (loadedAsset != null)
            {
                // If the original asset is inside a proxy, unload it. Only if not in the editor, since otherwise it won't allow to load the asset again:
                #if !UNITY_EDITOR
                {
                    var originalAsset = GetOriginalAsset(loadedAsset);
                    if (originalAsset != null  &&  originalAsset != loadedAsset)
                        Resources.UnloadAsset(originalAsset);
                }
                #endif


                // Unload the loaded asset:
                Resources.UnloadAsset(loadedAsset);
            }
        
            loadedAsset            = null;
            asyncLoadRequest       = null;
            asyncLoadedAssetGetter = null;
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








        #region Editor members
        #if UNITY_EDITOR


        /// <summary>
        /// **ONLY IN THE EDITOR** The original asset object.
        /// </summary>

        public UnityEngine.Object Editor_OriginalAsset
        {
            get
            {
                return asset;
            }
        }








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
}
