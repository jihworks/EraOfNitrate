// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public struct HexaEdgeCoord
    {
        public HexaEdgeOrientation Orientation;
        public int X, Y;

        public HexaEdgeCoord(HexaEdgeOrientation orientation, int x, int y)
        {
            Orientation = orientation;
            X = x;
            Y = y;
        }
    }
}
