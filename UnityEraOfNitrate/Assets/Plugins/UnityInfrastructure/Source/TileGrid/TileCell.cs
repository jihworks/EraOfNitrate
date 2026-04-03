// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.TileGrid
{
    public class TileCell
    {
        public TileMap Map { get; }

        public TileCellCoord Coord { get; }

        internal TileVertex[] VerticesInternal { get; } = new TileVertex[4];
        public IReadOnlyList<TileVertex> Vertices => VerticesInternal;

        public TileVertex TopLeftVertex => VerticesInternal[(int)TileDiagonalPosition.TL];
        public TileVertex TopRightVertex => VerticesInternal[(int)TileDiagonalPosition.TR];
        public TileVertex BottomRightVertex => VerticesInternal[(int)TileDiagonalPosition.BR];
        public TileVertex BottomLeftVertex => VerticesInternal[(int)TileDiagonalPosition.BL];

        internal TileEdge[] EdgesInternal { get; } = new TileEdge[4];
        public IReadOnlyList<TileEdge> Edges => EdgesInternal;

        public TileEdge TopEdge => EdgesInternal[(int)TileOrthogonalPosition.T];
        public TileEdge RightEdge => EdgesInternal[(int)TileOrthogonalPosition.R];
        public TileEdge BottomEdge => EdgesInternal[(int)TileOrthogonalPosition.B];
        public TileEdge LeftEdge => EdgesInternal[(int)TileOrthogonalPosition.L];

        internal TileCell?[] OrthogonalsInternal { get; } = new TileCell?[4];
        public IReadOnlyList<TileCell?> Orthogonals => OrthogonalsInternal;

        public TileCell? TopCell => OrthogonalsInternal[(int)TileOrthogonalPosition.T];
        public TileCell? RightCell => OrthogonalsInternal[(int)TileOrthogonalPosition.R];
        public TileCell? BottomCell => OrthogonalsInternal[(int)TileOrthogonalPosition.B];
        public TileCell? LeftCell => OrthogonalsInternal[(int)TileOrthogonalPosition.L];

        internal TileCell?[] DiagonalsInternal { get; } = new TileCell?[4];
        public IReadOnlyList<TileCell?> Diagonals => DiagonalsInternal;

        public TileCell? TopLeftCell => DiagonalsInternal[(int)TileDiagonalPosition.TL];
        public TileCell? TopRightCell => DiagonalsInternal[(int)TileDiagonalPosition.TR];
        public TileCell? BottomRightCell => DiagonalsInternal[(int)TileDiagonalPosition.BR];
        public TileCell? BottomLeftCell => DiagonalsInternal[(int)TileDiagonalPosition.BL];

        public TileCell(TileMap map, TileCellCoord coord)
        {
            Map = map;
            Coord = coord;
        }

        public IEnumerable<TileCell> EnumerateOrthogonals()
        {
            foreach (var cell in OrthogonalsInternal)
            {
                if (cell is not null)
                {
                    yield return cell;
                }
            }
        }
        public IEnumerable<TileCell> EnumerateDiagonals()
        {
            foreach (var cell in DiagonalsInternal)
            {
                if (cell is not null)
                {
                    yield return cell;
                }
            }
        }

        public IEnumerable<TileCell> EnumerateAllAdjacents()
        {
            for (int i = 0; i < 4; i++)
            {
                TileCell? cell = DiagonalsInternal[i];
                if (cell is not null)
                {
                    yield return cell;
                }

                cell = OrthogonalsInternal[i];
                if (cell is not null)
                {
                    yield return cell;
                }
            }
        }
    }
}
