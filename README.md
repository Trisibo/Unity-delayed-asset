**Important update note**

New versions of the plugin added the `DelayedAssetProxy` class, which makes it possible to use assets that aren't inside a Resources folder. To make things as transparent and easy as possible, a breaking change was required: **to unload the original assets from memory, do NOT use `Resources.Unload`, use `DelayedAsset.Unload` instead**.


Unity Delayed Asset
===================

Plugin for Unity that allows to set assets (prefabs, materials, etc.) to fields of objects in the inspector, without Unity automatically loading all the data referenced by the asset in memory when a scene is loaded.


Usage
-----

*   Inside a script, instead of creating a field using the normal type for the asset (`GameObject`, `Transform`, etc., or a custom component), create a field of type `DelayedAsset`. In the inspector it will be possible to directly assign the original asset to the slot. Example:

        [SerializeField] DelayedAsset thing;
        public DelayedAsset AnotherThing;

    It's also possible to use arrays:

        [SerializeField] DelayedAsset[] manyThings;
        public DelayedAsset[] EvenMoreThings;

    In the inspector, the labels for delayed assets will be shown between curly braces, to better identify them; from the previous examples, the field "thing" will appear as "{Thing}", and "EvenMoreThings" as "{EvenMoreThings}".




*   By default it's possible to assign any type of object derived from `Unity.Object` to the `DelayedAsset`. But it's also possible to restrict the type of asset, using the attribute `DelayedAssetType` and supplying the type. For example:

        // Only allows assets of type "GameObject" to be assigned:
        [SerializeField] [DelayedAssetType(typeof(GameObject))] DelayedAsset gameObjectReference;

        // Only allows assets of type "Material" to be assigned:
        [DelayedAssetType(typeof(Material))] public DelayedAsset materialReference;

        // Only allows assets of the custom type "Stuff" to be assigned:
        [SerializeField] [DelayedAssetType(typeof(Stuff))] DelayedAsset stuffReference;




*   The original assets need to be inside a "Resources" folder, directly or inside a subfolder; this is because otherwise Unity would not include the original asset in builds. There can be any number of "Resources" folders in the project.

    If it's not possible or desired to have the original asset inside a "Resources" folder, a `DelayedAssetProxy` can be used:

    *   Right-click in the project window inside a "Resources" folder, and select "Create -> Delayed Asset Proxy"; this will create a `DelayedAssetProxy` instance, which must be inside a "Resources" folder. Give it the desired name.
    *   Select it, and, in the inspector window, drag the desired original asset to the `Asset` slot; the original asset can be inside any folder of the project.
    *   Drag the `DelayedAssetProxy` file to the desired `DelayedAsset` slot, just as if it was the original asset. The slot will accept the `DelayedAssetProxy`, but only if its asset type coincides with the original asset (see previous section about `DelayedAssetType`). For example, this would accept both assets of type `Texture2D` inside a "Resources" folder, and assets of type `DelayedAssetProxy` inside a "Resources" folder with a `Texture2D` assigned which is inside any project folder:

        public [DelayedAssetType(typeof(Texture2D))] DelayedAsset texture;
    
    To avoid mistakes, it will be automatically detected if the asset isn't inside a "Resources" folder and it won't be possible to assign it; however, the check cannot be performed if an asset that was previously assigned is moved outside a "Resources" folder.




*   To actually load the original asset when needed, `DelayedAsset` contains the methods `Load` and `LoadAsync`; they will return null if the asset cannot be found (or if none was assigned). The type returned by `Load` is `UnityEngine.Object`, so it may be necessary to perform a cast to the actual type of the asset. The asset can be unloaded using `Unload`. The way to load the original asset is the same whether the slot has a normal asset or a `DelayedAssetProxy` assigned, the original asset will always be returned, it's transparent for the caller. Example:

        [SerializeField] [DelayedAssetType(typeof(Transform))] DelayedAsset transformReference;

        void Awake()
        {
            // Load the original asset:
            Transform transf = (Transform)transformReference.Load();

            if (transf != null)
            {
                // If existing, add an instance to the scene:
                Instantiate(transf.gameObject);
            
                // Unload the original asset if desired:
                transformReference.Unload();
            }
        }




Restrictions
------------

*   There cannot be two assets of the same type and path relative to any "Resources" folder. The situation will be detected automatically and won't be allowed when assigning the assets, but it's not possible to detect automatically when an asset was already assigned.

    For example, it would not be possible to assign any of these assets to a `DelayedAsset`; the full paths are different, but the paths relative to the Resources folders are the same:

    >Assets/Resources/Materials/Metal.mat  
    >Assets/Mobile/Resources/Materials/Metal.mat

    But these would be possible, since the types of the assets are different:
    
    >Assets/Resources/Metal.mat  
    >Assets/Resources/Metal.prefab




*   It may not be possible to load the original asset if the reference is assigned using the inspector in debug mode. It's highly recommended to always assign the assets in normal inspector mode.




License
-------

Copyright (C) 2017 Trinidad Sibajas Bodoque

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
