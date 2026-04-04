// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.TileGrid
{
    /// <summary>
    /// CW order from Top-Left.
    /// </summary>
    public enum TileDiagonalPosition
    {
        TopLeft = 0,
        TopRight,
        BottomRight,
        BottomLeft,

        TL = TopLeft,
        TR = TopRight,
        BR = BottomRight,
        BL = BottomLeft,
    }

    /// <summary>
    /// CW order from Top.
    /// </summary>
    public enum TileOrthogonalPosition
    {
        Top = 0,
        Right,
        Bottom,
        Left,

        T = Top,
        R = Right,
        B = Bottom,
        L = Left,
    }

    public enum TileEdgeOrientation
    {
        Horizontal = 0,
        Vertical,

        TB = Horizontal,
        LR = Vertical,
    }
}
