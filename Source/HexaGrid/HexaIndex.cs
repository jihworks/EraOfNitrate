// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public struct HexaIndex
    {
        public static HexaIndex ConvertFrom(in HexaCoord hexaCoord)
        {
            int parity = hexaCoord.B & 1;
            int c = hexaCoord.A + (hexaCoord.B - parity) / 2;
            return new HexaIndex(c, hexaCoord.B);
        }
        public static HexaCoord ConvertTo(in HexaIndex hexaIndex)
        {
            int parity = hexaIndex.Y & 1;
            int a = hexaIndex.X - (hexaIndex.Y - parity) / 2;
            int b = hexaIndex.Y;
            return new HexaCoord(a, b, -a - b);
        }

        public int X, Y;

        public HexaIndex(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static explicit operator HexaCoord(HexaIndex hexIndex)
        {
            return ConvertTo(hexIndex);
        }
        public static explicit operator HexaIndex(HexaCoord hexCoord)
        {
            return ConvertFrom(hexCoord);
        }
    }
}
