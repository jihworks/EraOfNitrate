// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.Unity.Infrastructure.Rendering
{
    public static class IndirectInstancingEx
    {
        public static readonly int InstanceTransformsId = Shader.PropertyToID("_InstanceTransforms");
    }
}
