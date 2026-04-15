// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure;
using UnityEngine;

namespace Jih.Unity.EraOfNitrogen.Worlds.Runtime
{
    public struct DoodadTransform
    {
        public Vector3 UnityLocation;
        public float UnityRotationY;
        public float UnityScale;

        public DoodadTransform(Vector3 unityLocation, float unityRotationY, float unityScale)
        {
            UnityLocation = unityLocation;
            UnityRotationY = unityRotationY;
            UnityScale = unityScale;
        }

        public readonly Matrix4x4 GetMatrix()
        {
            return Matrix4x4.TRS(
                UnityLocation,
                Quaternion.AngleAxis(UnityRotationY, Vector3.up),
                Vector3Ex.CreateUniform(UnityScale));
        }
    }
}
