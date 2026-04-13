// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions.Common3D
{
    public abstract class CollisionShape
    {
        bool _isLocalBoundsDirty = false;
        Bounds _localBounds = BoundsEx.Empty;
        public Bounds LocalBounds
        {
            get => _localBounds;
            protected set
            {
                if (_localBounds == value)
                {
                    return;
                }
                _localBounds = value;
                _isLocalBoundsDirty = true;
            }
        }

        Bounds _worldBounds = BoundsEx.Empty;
        public Bounds WorldBounds
        {
            get
            {
                UpdateBounds();
                return _worldBounds;
            }
            protected set
            {
                if (_worldBounds == value)
                {
                    return;
                }
                _worldBounds = value;
                OnWorldBoundsChanged();
            }
        }

        bool _isWorldTransformDirty = false;
        Matrix4x4 _worldTransform = Matrix4x4.identity;
        public Matrix4x4 WorldTransform
        {
            get => _worldTransform;
            set
            {
                if (_worldTransform == value)
                {
                    return;
                }
                _worldTransform = value;
                _isWorldTransformDirty = true;
                OnWorldBoundsChanged();
            }
        }

        protected void ClearCollisionShapeBoundsAndTransform()
        {
            _localBounds = _worldBounds = BoundsEx.Empty;
            _isLocalBoundsDirty = false;

            _worldTransform = Matrix4x4.identity;
            _isWorldTransformDirty = false;
        }

        public void UpdateBounds()
        {
            if (!NeedUpdateBounds_Impl())
            {
                return;
            }
            UpdateBounds_Impl(ref _isLocalBoundsDirty, ref _isWorldTransformDirty);
            if (_isLocalBoundsDirty || _isWorldTransformDirty)
            {
                throw new InvalidOperationException($"Derived {nameof(CollisionShape)} {GetType().FullName} must clear dirty flags after bounds updated.");
            }
        }
        protected virtual bool NeedUpdateBounds_Impl()
        {
            return _isLocalBoundsDirty || _isWorldTransformDirty;
        }
        protected abstract void UpdateBounds_Impl(ref bool isLocalBoundsDirty, ref bool isWorldTransformDirty);

        public bool IntersectsWith(CollisionShape other)
        {
            return other switch
            {
                BoxShape box => IntersectsWith(box),
                MeshShape mesh => IntersectsWith(mesh),
                _ => throw new NotImplementedException(),
            };
        }

        public bool IntersectsWith(BoxShape other)
        {
            UpdateBounds();
            other.UpdateBounds();

            return IntersectsWith_Impl(other);
        }
        protected abstract bool IntersectsWith_Impl(BoxShape other);

        public bool IntersectsWith(MeshShape other)
        {
            UpdateBounds();
            other.UpdateBounds();

            return IntersectsWith_Impl(other);
        }
        protected abstract bool IntersectsWith_Impl(MeshShape other);

        public bool Raycast(Vector3 rayOrigin, Vector3 rayDirection, out float t)
        {
            UpdateBounds();

            return Raycast_Impl(rayOrigin, rayDirection, out t);
        }
        protected abstract bool Raycast_Impl(Vector3 rayOrigin, Vector3 rayDirection, out float t);

        protected virtual void OnWorldBoundsChanged()
        {
        }
    }
}
