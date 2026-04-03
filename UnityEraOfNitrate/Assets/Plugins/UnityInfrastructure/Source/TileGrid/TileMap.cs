// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;

namespace Jih.Unity.Infrastructure.TileGrid
{
    /// <summary>
    /// Grid structure of square tiles in 2D euclidean space.
    /// </summary>
    /// <remarks>
    /// +X is right, +Y is down.<br/>
    /// Left-Top corner is origin (0, 0).<br/>
    /// Assuming all of cells are filled in the grid. Hole is not supported.
    /// </remarks>
    public class TileMap
    {
        public int Width { get; }
        public int Height { get; }

        public TileCell this[TileCellCoord coord] => GetCell(coord) ?? throw new ArgumentOutOfRangeException(nameof(coord), $"Invalid cell coordinate: {coord}");
        public TileVertex this[TileVertexCoord coord] => GetVertex(coord) ?? throw new ArgumentOutOfRangeException(nameof(coord), $"Invalid vertex coordinate: {coord}");
        public TileEdge this[TileEdgeCoord coord] => GetEdge(coord) ?? throw new ArgumentOutOfRangeException(nameof(coord), $"Invalid edge coordinate: {coord}");

        readonly TileCell[,] _cells;
        readonly TileVertex[,] _vertices;
        readonly TileEdge[,] _horizontalEdges, _verticalEdges;

        public TileMap(int width, int height, CreateTileCellDelegate? createTileCell, CreateTileVertexDelegate? createTileVertex, CreateTileEdgeDelegate? createTileEdge)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
            }

            Width = width;
            Height = height;

            createTileCell ??= ((map, coord) => new TileCell(map, coord));
            createTileVertex ??= ((map, coord) => new TileVertex(map, coord));
            createTileEdge ??= ((coord, v0, v1, rCell) => new TileEdge(coord, v0, v1, rCell));

            _cells = new TileCell[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _cells[y, x] = createTileCell(this, new TileCellCoord(x, y));
                }
            }

            _vertices = new TileVertex[height + 1, width + 1];
            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    _vertices[y, x] = createTileVertex(this, new TileVertexCoord(x, y));
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileCell cell = _cells[y, x];

                    TileCellCoord cellCoord = cell.Coord;

                    TileVertex tl = _vertices[cellCoord.Y, cellCoord.X];
                    TileVertex tr = _vertices[cellCoord.Y, cellCoord.X + 1];
                    TileVertex br = _vertices[cellCoord.Y + 1, cellCoord.X + 1];
                    TileVertex bl = _vertices[cellCoord.Y + 1, cellCoord.X];

                    cell.VerticesInternal[(int)TileDiagonalPosition.TL] = tl;
                    cell.VerticesInternal[(int)TileDiagonalPosition.TR] = tr;
                    cell.VerticesInternal[(int)TileDiagonalPosition.BR] = br;
                    cell.VerticesInternal[(int)TileDiagonalPosition.BL] = bl;

                    tl.CellsInternal[(int)TileDiagonalPosition.BR] = cell;
                    tr.CellsInternal[(int)TileDiagonalPosition.BL] = cell;
                    br.CellsInternal[(int)TileDiagonalPosition.TL] = cell;
                    bl.CellsInternal[(int)TileDiagonalPosition.TR] = cell;
                }
            }

            _horizontalEdges = new TileEdge[height + 1, width];
            _verticalEdges = new TileEdge[height, width + 1];
            // Horizontal edges.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileVertex v0 = _vertices[y, x];
                    TileVertex v1 = _vertices[y, x + 1];
                    TileCell cell = _cells[y, x];
                    TileEdge top = createTileEdge(new TileEdgeCoord(TileEdgeOrientation.TB, x, y), v0, v1, cell);
                    _horizontalEdges[y, x] = top;

                    if (y > 0)
                    {
                        TileCell cellAbove = _cells[y - 1, x];
                        top.LeftCell = cellAbove;
                    }
                }
            }
            for (int x = 0; x < width; x++)
            {
                TileVertex v0 = _vertices[height, x + 1];
                TileVertex v1 = _vertices[height, x];
                TileCell cell = _cells[height - 1, x];
                TileEdge bottom = createTileEdge(new TileEdgeCoord(TileEdgeOrientation.TB, x, height), v0, v1, cell);
                _horizontalEdges[height, x] = bottom;
            }
            // Vertical edges.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileVertex v0 = _vertices[y + 1, x];
                    TileVertex v1 = _vertices[y, x];
                    TileCell cell = _cells[y, x];
                    TileEdge left = createTileEdge(new TileEdgeCoord(TileEdgeOrientation.LR, x, y), v0, v1, cell);
                    _verticalEdges[y, x] = left;

                    if (x > 0)
                    {
                        TileCell cellLeft = _cells[y, x - 1];
                        left.LeftCell = cellLeft;
                    }
                }
            }
            for (int y = 0; y < height; y++)
            {
                TileVertex v0 = _vertices[y, width];
                TileVertex v1 = _vertices[y + 1, width];
                TileCell cell = _cells[y, width - 1];
                TileEdge right = createTileEdge(new TileEdgeCoord(TileEdgeOrientation.LR, width, y), v0, v1, cell);
                _verticalEdges[y, width] = right;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileCell cell = _cells[y, x];

                    TileEdge top = _horizontalEdges[y, x];
                    TileEdge bottom = _horizontalEdges[y + 1, x];
                    cell.EdgesInternal[(int)TileOrthogonalPosition.T] = top;
                    cell.EdgesInternal[(int)TileOrthogonalPosition.B] = bottom;

                    TileEdge left = _verticalEdges[y, x];
                    TileEdge right = _verticalEdges[y, x + 1];
                    cell.EdgesInternal[(int)TileOrthogonalPosition.L] = left;
                    cell.EdgesInternal[(int)TileOrthogonalPosition.R] = right;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileCell cell = _cells[y, x];

                    int l = x - 1, t = y - 1;
                    int r = x + 1, b = y + 1;

                    bool vl = 0 <= l, vr = r < width;
                    bool vt = 0 <= t, vb = b < height;

                    if (vl)
                    {
                        TileCell other = _cells[y, l];
                        cell.OrthogonalsInternal[(int)TileOrthogonalPosition.L] = _cells[y, l];
                    }
                    if (vr)
                    {
                        cell.OrthogonalsInternal[(int)TileOrthogonalPosition.R] = _cells[y, r];
                    }
                    if (vt)
                    {
                        cell.OrthogonalsInternal[(int)TileOrthogonalPosition.T] = _cells[t, x];
                    }
                    if (vb)
                    {
                        cell.OrthogonalsInternal[(int)TileOrthogonalPosition.B] = _cells[b, x];
                    }

                    if (vt && vl)
                    {
                        cell.DiagonalsInternal[(int)TileDiagonalPosition.TL] = _cells[t, l];
                    }
                    if (vt && vr)
                    {
                        cell.DiagonalsInternal[(int)TileDiagonalPosition.TR] = _cells[t, r];
                    }
                    if (vb && vr)
                    {
                        cell.DiagonalsInternal[(int)TileDiagonalPosition.BR] = _cells[b, r];
                    }
                    if (vb && vl)
                    {
                        cell.DiagonalsInternal[(int)TileDiagonalPosition.BL] = _cells[b, l];
                    }
                }
            }
        }

        public TileCell? GetCell(TileCellCoord coord)
        {
            return GetCell(coord.X, coord.Y);
        }
        public TileCell? GetCell(int coordX, int coordY)
        {
            if (0 <= coordX && coordX < Width && 0 <= coordY && coordY < Height)
            {
                return _cells[coordY, coordX];
            }
            else
            {
                return null;
            }
        }

        public TileVertex? GetVertex(TileVertexCoord coord)
        {
            return GetVertex(coord.X, coord.Y);
        }
        public TileVertex? GetVertex(int coordX, int coordY)
        {
            if (0 <= coordX && coordX <= Width && 0 <= coordY && coordY <= Height)
            {
                return _vertices[coordY, coordX];
            }
            else
            {
                return null;
            }
        }

        public TileEdge? GetEdge(TileEdgeCoord coord)
        {
            return GetEdge(coord.Orientation, coord.X, coord.Y);
        }
        public TileEdge? GetEdge(TileEdgeOrientation orientation, int coordX, int coordY)
        {
            switch (orientation)
            {
                case TileEdgeOrientation.TB:
                    if (0 <= coordX && coordX < Width && 0 <= coordY && coordY <= Height)
                    {
                        return _horizontalEdges[coordY, coordX];
                    }
                    return null;

                case TileEdgeOrientation.LR:
                    if (0 <= coordX && coordX <= Width && 0 <= coordY && coordY < Height)
                    {
                        return _verticalEdges[coordY, coordX];
                    }
                    return null;

                default: throw new ArgumentException($"Invalid edge orientation: {orientation}", nameof(orientation));
            }
        }
    }
}
