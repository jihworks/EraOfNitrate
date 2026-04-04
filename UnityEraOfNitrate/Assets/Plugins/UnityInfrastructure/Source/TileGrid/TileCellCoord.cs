// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.TileGrid
{
    public struct TileCellCoord
    {
        public int X, Y;

        public TileCellCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly override string ToString()
        {
            return $"<{X}, {Y}>";
        }
    }
}
