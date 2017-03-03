using UnityEngine;
using System;

/// <summary>
/// Allows to specify the wrapped type for a <see cref="DelayedAsset"/>.
/// </summary>

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class DelayedAssetTypeAttribute : PropertyAttribute
{
    public readonly Type Type;

    public DelayedAssetTypeAttribute(Type type)
    {
        if (!typeof(UnityEngine.Object).IsAssignableFrom(type))
            throw new ArgumentException("The type argument for a DelayedAssetTypeAttribute must be derived from Unity.Object");

        this.Type = type;
    }
}