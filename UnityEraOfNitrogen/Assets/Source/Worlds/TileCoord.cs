// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure.HexaGrid;
using Newtonsoft.Json;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public struct TileCoord
    {
        [JsonProperty] public int A;
        [JsonProperty] public int B;
        [JsonProperty] public int C;

        public TileCoord(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public static implicit operator TileCoord(HexaCoord coord)
        {
            return new TileCoord(coord.A, coord.B, coord.C);
        }
        public static implicit operator HexaCoord(TileCoord coord)
        {
            return new HexaCoord(coord.A, coord.B, coord.C);
        }
    }
}
