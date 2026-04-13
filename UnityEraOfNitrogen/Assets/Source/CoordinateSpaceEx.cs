// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure.HexaGrid;
using UnityEngine;

namespace Jih.Unity.EraOfNitrogen
{
    public static class CoordinateSpaceEx
    {
        public static Vector3 ScreenToUnity(Vector2 screen, float unityY = 0f)
        {
            return new Vector3(screen.x, unityY, -screen.y);
        }
        public static Vector2 UnityToScreen(Vector3 unity)
        {
            return new Vector2(unity.x, -unity.z);
        }

        public static Vector2 HexaToScreen(HexaCoordF h)
        {
            return _hexaOrientation.HexaToScreen(h);
        }
        public static HexaCoordF ScreenToHexa(Vector2 p)
        {
            return _hexaOrientation.ScreenToHexa(p);
        }

        public static Vector2 GetScreenVertexOffset(HexaVertexPosition position)
        {
            return _hexaOrientation.GetScreenVertexOffset(position);
        }

        static readonly HexaOrientation _hexaOrientation = new(Vector2.zero, Vector2.one);
    }
}
