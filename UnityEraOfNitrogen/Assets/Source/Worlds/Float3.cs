// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Newtonsoft.Json;
using UnityEngine;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public struct Float3
    {
        [JsonProperty] public float X;
        [JsonProperty] public float Y;
        [JsonProperty] public float Z;

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator Vector3(Float3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public static implicit operator Float3(Vector3 v)
        {
            return new Float3(v.x, v.y, v.z);
        }
    }
}
