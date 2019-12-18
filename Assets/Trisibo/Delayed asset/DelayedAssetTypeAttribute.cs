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

namespace Trisibo
{
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
}
