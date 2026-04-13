// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using UnityEngine;

namespace Jih.Unity.EraOfNitrogen.Worlds.Generators
{
    public class GeneratorDoodad
    {
        public DoodadType Type { get; }
        /// <summary>
        /// 동일 타입 내에서 어떤 배리에이션을 사용할지를 의미.
        /// </summary>
        /// <remarks>
        /// 아무 양수값이나 할당. 모듈로 연산으로 존재하는 배리에이션 개수에 맞춰 사용.
        /// </remarks>
        public int Variant { get; }

        public Vector3 UnityLocation { get; }
        /// <summary>
        /// 육십분법 각도.
        /// </summary>
        public float UnityRotationY { get; }
        public float UnityScale { get; }

        public GeneratorDoodad(DoodadType type, int variant, Vector3 unityLocation, float unityRotationY, float unityScale)
        {
            Type = type;
            Variant = variant;
            UnityLocation = unityLocation;
            UnityRotationY = unityRotationY;
            UnityScale = unityScale;
        }
    }
}
