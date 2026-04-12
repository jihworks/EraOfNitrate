// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions.Common3D
{
    public interface ICollision
    {
        event Action<ICollision>? WorldBoundsChanged;

        Bounds LocalBounds { get; }
        Bounds WorldBounds { get; }
        Matrix4x4 WorldTransform { get; }

        bool IsEnabled { get; set; }

        uint CollisionChannel { get; set; }

        void UpdateBounds();

        bool IntersectsWith(CollisionShape other);

        bool Raycast(Vector3 rayOrigin, Vector3 rayDirection, out float t);
    }

    public class BoxCollision : BoxShape, ICollision
    {
        public event Action<ICollision>? WorldBoundsChanged;

        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Bit flags. Default value is <see cref="CollisionChannelEx.Default"/>.
        /// </summary>
        public uint CollisionChannel { get; set; } = CollisionChannelEx.Default;

        public BoxCollision(Bounds bounds) : base(bounds)
        {
        }

        protected override void OnWorldBoundsChanged()
        {
            base.OnWorldBoundsChanged();
            WorldBoundsChanged?.Invoke(this);
        }
    }

    public class MeshCollision : MeshShape, ICollision
    {
        public event Action<ICollision>? WorldBoundsChanged;

        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Bit flags. Default value is <see cref="CollisionChannelEx.Default"/>.
        /// </summary>
        public uint CollisionChannel { get; set; } = CollisionChannelEx.Default;

        protected override void OnWorldBoundsChanged()
        {
            base.OnWorldBoundsChanged();
            WorldBoundsChanged?.Invoke(this);
        }
    }
}
