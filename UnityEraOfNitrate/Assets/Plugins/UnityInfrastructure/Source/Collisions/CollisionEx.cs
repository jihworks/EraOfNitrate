// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions
{
    static class CollisionEx
    {
        public const float BoundsEpsilon = 0.001f;
        public static Vector3 BoundsEpsilon3 => new(BoundsEpsilon, BoundsEpsilon, BoundsEpsilon);
    }
}
