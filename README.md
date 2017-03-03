Unity Delayed Asset
===================

Plugin for Unity that allows to set assets (prefabs, materials, etc.) to fields of objects in the inspector, without Unity automatically loading all the data referenced by the asset in memory when a scene is loaded.


Usage
-----

*   Inside a script, instead of creating a field using the normal type for the asset (GameObject, Transform, etc., or a custom script), create a field of type DelayedAsset. In the inspector it will be possible to directly assign the original asset to the slot. Example:

        [SerializeField] DelayedAsset thing;
        public DelayedAsset AnotherThing;

    It's also possible to use arrays:

        [SerializeField] DelayedAsset[] manyThings;
        public DelayedAsset[] EvenMoreThings;

    In the inspector, the labels for delayed assets will be shown between brackets, to better identify them; from the previous examples, the field "thing" will appear as "[Thing]", and "EvenMoreThings" as "[EvenMoreThings]".




*   The original assets need to be inside a "Resources" folder, directly or inside a subfolder; this is because otherwise Unity would not include the original asset in builds. There can be any number of "Resources" folders in the project.
    
    To avoid mistakes, it will be automatically detected if the asset isn't inside a "Resources" folder and it won't be possible to assign it; however, the check cannot be performed if an asset that was previously assigned is moved outside a "Resources" folder.




*   By default it's possible to assign any type of object derived from "Unity.Object" to the "DelayedAsset". But it's also possible to restrict the type of asset, using the attribute "DelayedAssetType" and supplying the type. For example:

        // Only allows assets of type "GameObject" to be assigned:
        [SerializeField] [DelayedAssetType(typeof(GameObject))] DelayedAsset gameObjectReference;

        // Only allows assets of type "Material" to be assigned:
        [DelayedAssetType(typeof(Material))] public DelayedAsset materialReference;

        // Only allows assets of the custom type "Stuff" to be assigned:
        [SerializeField] [DelayedAssetType(typeof(Stuff))] DelayedAsset stuffReference;




*   To actually load the original asset when needed, "DelayedAsset" contains the method "Load()"; it will return null if the asset cannot be found (or if none was assigned). The type returned is "UnityEngine.Object", so it may be necessary to perform a cast to the actual type of the asset. The asset can be unloaded using Unity's built-in "Resources.Unload()". Example:

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
                Resources.Unload(transf);
            }
        }




Restrictions
------------

*   There cannot be two assets of the same type and path relative to any "Resources" folder. The situation will be detected automatically and won't be allowed when assigning the assets, but it's not possible to detect automatically if an asset was already assigned.

    For example, it would not be possible to assign any of these assets to a DelayedAsset; the full paths are different, but the paths relative to the Resources folders are the same:

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
