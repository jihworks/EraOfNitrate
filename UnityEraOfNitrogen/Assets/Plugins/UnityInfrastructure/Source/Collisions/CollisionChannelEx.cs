// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.Collisions
{
    public static class CollisionChannelEx
    {
        public const uint All = 0xffffffffu;
        public const uint None = 0u;

        public const uint Default = 1u;

        public static bool Has(this uint mask, uint flag)
        {
            return (mask & flag) != 0u;
        }
    }
}
