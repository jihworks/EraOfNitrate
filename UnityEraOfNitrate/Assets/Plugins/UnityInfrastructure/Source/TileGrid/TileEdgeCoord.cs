// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.TileGrid
{
    public struct TileEdgeCoord
    {
        public TileEdgeOrientation Orientation;
        public int X, Y;

        public TileEdgeCoord(TileEdgeOrientation orientation, int x, int y)
        {
            Orientation = orientation;
            X = x;
            Y = y;
        }

        public readonly override string ToString()
        {
            char orientationChar = Orientation switch
            {
                TileEdgeOrientation.Horizontal => 'H',
                TileEdgeOrientation.Vertical => 'V',
                _ => '?',
            };
            return $"<{orientationChar}, {X}, {Y}>";
        }
    }
}
