// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    /// <remarks>
    /// Using Screen Coordinate System<br/>
    /// +X is right, +Y is down.<br/>
    /// Left-Top corner is origin(0, 0).<br/>
    /// Positive rotation is CW.<br/>
    /// <br/>
    /// Assuming all of cells are filled in the grid. Hole is not supported.<br/>
    /// <br/>
    /// Using pointy-topped orientation.<br/>
    /// And using odd-r layout for cell storage.<br/>
    /// <br/>
    /// Position enums prefix legend:<br/>
    /// D = Degree (-Y is 0º)<br/>
    /// C = Clock (-Y is 12 o'clock)<br/>
    /// NEWS = Compass (-Y is North)
    /// </remarks>
    public class HexaMap
    {
        public int Width { get; }
        public int Height { get; }

        public HexaCell this[HexaCoord coord] => GetCell(coord) ?? throw new ArgumentOutOfRangeException(nameof(coord));
        public HexaCell this[HexaIndex index] => GetCell(index) ?? throw new ArgumentOutOfRangeException(nameof(index));
        public HexaVertex this[HexaVertexIndex index] => GetVertex(index) ?? throw new ArgumentOutOfRangeException(nameof(index));
        public HexaEdge this[HexaEdgeIndex index] => GetEdge(index) ?? throw new ArgumentOutOfRangeException(nameof(index));

        readonly HexaCell[,] _cells;
        readonly HexaVertex?[,] _vertices;
        readonly HexaEdge?[,] _horizontalEdges;
        readonly HexaEdge[,] _verticalEdges;

        public HexaMap(int width, int height, CreateHexaCellDelegate? createHexaCell, CreateHexaVertexDelegate? createHexaVertex, CreateHexaEdgeDelegate? createHexaEdge)
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

            createHexaCell ??= ((map, index, coord) => new HexaCell(map, index, coord));
            createHexaVertex ??= ((map, index, coord) => new HexaVertex(map, index, coord));
            createHexaEdge ??= ((vertex0, vertex1, index, rightCell) => new HexaEdge(vertex0, vertex1, index, rightCell));

            _cells = new HexaCell[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexaIndex index = new(x, y);
                    HexaCoord coord = (HexaCoord)index;
                    _cells[y, x] = createHexaCell(this, index, coord);
                }
            }

            static HexaCell? GetCell(HexaCoord coord, HexaCell[,] cells, int width, int height)
            {
                HexaIndex index = (HexaIndex)coord;
                if (index.X < 0 || width <= index.X || index.Y < 0 || height <= index.Y)
                {
                    return null;
                }
                return cells[index.Y, index.X];
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexaCell cell = _cells[y, x];

                    for (int p = 0; p < 6; p++)
                    {
                        HexaNeighborPosition neighborPosition = (HexaNeighborPosition)p;
                        HexaCoord neighborCoord = cell.Coord + neighborPosition.GetOffset();
                        cell.NeighborsInternal[p] = GetCell(neighborCoord, _cells, width, height);

                        HexaDiagonalPosition diagonalPosition = (HexaDiagonalPosition)p;
                        HexaCoord diagonalCoord = cell.Coord + diagonalPosition.GetOffset();
                        cell.DiagonalsInternal[p] = GetCell(diagonalCoord, _cells, width, height);
                    }
                }
            }

            _vertices = new HexaVertex?[height + 1, width * 2 + 2];

            static HexaVertex GetOrCreateVertex(HexaVertexIndex index, HexaCoord cellCoord, HexaVertexPosition vertexPosition, HexaVertex?[,] dest, CreateHexaVertexDelegate create, HexaMap self)
            {
                ref HexaVertex? vertex = ref dest[index.Y, index.X];

                if (vertex is null)
                {
                    HexaCoordF offset = vertexPosition.GetOffset();

                    vertex = create(self, index, cellCoord + offset);
                }

                return vertex;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexaCell cell = _cells[y, x];

                    int parity = y & 1;
                    int baseX = x * 2 + parity;

                    HexaCoord cellCord = cell.Coord;

                    HexaVertex v300 = GetOrCreateVertex(new HexaVertexIndex(baseX, y), cellCord, HexaVertexPosition.D300, _vertices, createHexaVertex, this);
                    HexaVertex v0 = GetOrCreateVertex(new HexaVertexIndex(baseX + 1, y), cellCord, HexaVertexPosition.D0, _vertices, createHexaVertex, this);
                    HexaVertex v60 = GetOrCreateVertex(new HexaVertexIndex(baseX + 2, y), cellCord, HexaVertexPosition.D60, _vertices, createHexaVertex, this);

                    HexaVertex v240 = GetOrCreateVertex(new HexaVertexIndex(baseX, y + 1), cellCord, HexaVertexPosition.D240, _vertices, createHexaVertex, this);
                    HexaVertex v180 = GetOrCreateVertex(new HexaVertexIndex(baseX + 1, y + 1), cellCord, HexaVertexPosition.D180, _vertices, createHexaVertex, this);
                    HexaVertex v120 = GetOrCreateVertex(new HexaVertexIndex(baseX + 2, y + 1), cellCord, HexaVertexPosition.D120, _vertices, createHexaVertex, this);

                    cell.VerticesInternal[(int)HexaVertexPosition.D0] = v0;
                    cell.VerticesInternal[(int)HexaVertexPosition.D60] = v60;
                    cell.VerticesInternal[(int)HexaVertexPosition.D120] = v120;
                    cell.VerticesInternal[(int)HexaVertexPosition.D180] = v180;
                    cell.VerticesInternal[(int)HexaVertexPosition.D240] = v240;
                    cell.VerticesInternal[(int)HexaVertexPosition.D300] = v300;

                    v0.CellsInternal[(int)HexaVertexPosition.D180] = cell;
                    v60.CellsInternal[(int)HexaVertexPosition.D240] = cell;
                    v120.CellsInternal[(int)HexaVertexPosition.D300] = cell;
                    v180.CellsInternal[(int)HexaVertexPosition.D0] = cell;
                    v240.CellsInternal[(int)HexaVertexPosition.D60] = cell;
                    v300.CellsInternal[(int)HexaVertexPosition.D120] = cell;
                }
            }

            _horizontalEdges = new HexaEdge?[height + 1, width * 2 + 1];
            _verticalEdges = new HexaEdge[height, width + 1];

            static HexaEdge GetOrCreateEdge(HexaVertex vertex0, HexaVertex vertex1, HexaEdgePosition edgePosition, HexaEdgeIndex index, HexaCell rightCell, HexaEdge?[,] horizontalDest, HexaEdge[,] verticalDest, CreateHexaEdgeDelegate create)
            {
                HexaEdge?[,] dest = index.Orientation switch
                {
                    HexaEdgeOrientation.Horizontal => horizontalDest,
                    HexaEdgeOrientation.Vertical => verticalDest,
                    _ => throw new NotImplementedException(),
                };

                ref HexaEdge? edge = ref dest[index.Y, index.X];
                if (edge is not null)
                {
                    if (edge.Vertex0 != vertex1 || edge.Vertex1 != vertex0)
                    {
                        throw new InvalidOperationException("Hexa edge already exists but not reversed.");
                    }

                    edge.LeftCell = rightCell;
                    return edge;
                }

                edge = create(vertex0, vertex1, index, rightCell);

                switch (edgePosition)
                {
                    case HexaEdgePosition.D30:
                        vertex0.EdgesInternal[(int)HexaVertexPosition.D120] = edge;
                        vertex1.EdgesInternal[(int)HexaVertexPosition.D300] = edge;
                        break;
                    case HexaEdgePosition.D90:
                        vertex0.EdgesInternal[(int)HexaVertexPosition.D180] = edge;
                        vertex1.EdgesInternal[(int)HexaVertexPosition.D0] = edge;
                        break;
                    case HexaEdgePosition.D150:
                        vertex0.EdgesInternal[(int)HexaVertexPosition.D240] = edge;
                        vertex1.EdgesInternal[(int)HexaVertexPosition.D60] = edge;
                        break;
                    case HexaEdgePosition.D210:
                        vertex0.EdgesInternal[(int)HexaVertexPosition.D300] = edge;
                        vertex1.EdgesInternal[(int)HexaVertexPosition.D120] = edge;
                        break;
                    case HexaEdgePosition.D270:
                        vertex0.EdgesInternal[(int)HexaVertexPosition.D0] = edge;
                        vertex1.EdgesInternal[(int)HexaVertexPosition.D180] = edge;
                        break;
                    case HexaEdgePosition.D330:
                        vertex0.EdgesInternal[(int)HexaVertexPosition.D60] = edge;
                        vertex1.EdgesInternal[(int)HexaVertexPosition.D240] = edge;
                        break;
                    default: throw new NotImplementedException();
                }

                return edge;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexaCell rightCell = _cells[y, x];

                    HexaVertex v0 = rightCell.Vertices[(int)HexaVertexPosition.D0];
                    HexaVertex v60 = rightCell.Vertices[(int)HexaVertexPosition.D60];
                    HexaVertex v120 = rightCell.Vertices[(int)HexaVertexPosition.D120];
                    HexaVertex v180 = rightCell.Vertices[(int)HexaVertexPosition.D180];
                    HexaVertex v240 = rightCell.Vertices[(int)HexaVertexPosition.D240];
                    HexaVertex v300 = rightCell.Vertices[(int)HexaVertexPosition.D300];

                    int parity = y & 1;
                    int baseHorizontalX = x * 2 + parity;

                    HexaEdge h330 = GetOrCreateEdge(v300, v0, HexaEdgePosition.D330, new HexaEdgeIndex(HexaEdgeOrientation.Horizontal, baseHorizontalX, y), rightCell, _horizontalEdges, _verticalEdges, createHexaEdge);
                    HexaEdge h30 = GetOrCreateEdge(v0, v60, HexaEdgePosition.D30, new HexaEdgeIndex(HexaEdgeOrientation.Horizontal, baseHorizontalX + 1, y), rightCell, _horizontalEdges, _verticalEdges, createHexaEdge);
                    HexaEdge h210 = GetOrCreateEdge(v180, v240, HexaEdgePosition.D210, new HexaEdgeIndex(HexaEdgeOrientation.Horizontal, baseHorizontalX, y + 1), rightCell, _horizontalEdges, _verticalEdges, createHexaEdge);
                    HexaEdge h150 = GetOrCreateEdge(v120, v180, HexaEdgePosition.D150, new HexaEdgeIndex(HexaEdgeOrientation.Horizontal, baseHorizontalX + 1, y + 1), rightCell, _horizontalEdges, _verticalEdges, createHexaEdge);

                    HexaEdge v270 = GetOrCreateEdge(v240, v300, HexaEdgePosition.D270, new HexaEdgeIndex(HexaEdgeOrientation.Vertical, x, y), rightCell, _horizontalEdges, _verticalEdges, createHexaEdge);
                    HexaEdge v90 = GetOrCreateEdge(v60, v120, HexaEdgePosition.D90, new HexaEdgeIndex(HexaEdgeOrientation.Vertical, x + 1, y), rightCell, _horizontalEdges, _verticalEdges, createHexaEdge);

                    rightCell.EdgesInternal[(int)HexaEdgePosition.D30] = h30;
                    rightCell.EdgesInternal[(int)HexaEdgePosition.D90] = v90;
                    rightCell.EdgesInternal[(int)HexaEdgePosition.D150] = h150;
                    rightCell.EdgesInternal[(int)HexaEdgePosition.D210] = h210;
                    rightCell.EdgesInternal[(int)HexaEdgePosition.D270] = v270;
                    rightCell.EdgesInternal[(int)HexaEdgePosition.D330] = h330;
                }
            }
        }

        public HexaCell? GetCell(HexaCoord coord)
        {
            return GetCell((HexaIndex)coord);
        }
        public HexaCell? GetCell(HexaIndex index)
        {
            return GetCell(index.X, index.Y);
        }
        public HexaCell? GetCell(int x, int y)
        {
            if (x < 0 || Width <= x || y < 0 || Height <= y)
            {
                return null;
            }
            return _cells[y, x];
        }

        public HexaVertex? GetVertex(HexaVertexIndex index)
        {
            return GetVertex(index.X, index.Y);
        }
        public HexaVertex? GetVertex(int x, int y)
        {
            if (x < 0 || _vertices.GetLength(1) <= x || y < 0 || _vertices.GetLength(0) <= y)
            {
                return null;
            }
            return _vertices[y, x];
        }

        public HexaEdge? GetEdge(HexaEdgeIndex index)
        {
            return GetEdge(index.Orientation, index.X, index.Y);
        }
        public HexaEdge? GetEdge(HexaEdgeOrientation orientation, int x, int y)
        {
            HexaEdge?[,] dest = orientation switch
            {
                HexaEdgeOrientation.Horizontal => _horizontalEdges,
                HexaEdgeOrientation.Vertical => _verticalEdges,
                _ => throw new NotImplementedException(),
            };

            if (x < 0 || dest.GetLength(1) <= x || y < 0 || dest.GetLength(0) <= y)
            {
                return null;
            }
            return dest[y, x];
        }

        public IEnumerable<HexaCell> EnumerateCells()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return _cells[y, x];
                }
            }
        }

        public IEnumerable<HexaVertex> EnumerateVertices()
        {
            int height = _vertices.GetLength(0);
            int width = _vertices.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexaVertex? v = _vertices[y, x];
                    if (v is not null)
                    {
                        yield return v;
                    }
                }
            }
        }

        public IEnumerable<HexaEdge> EnumerateEdges()
        {
            int height = _horizontalEdges.GetLength(0);
            int width = _horizontalEdges.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexaEdge? e = _horizontalEdges[y, x];
                    if (e is not null)
                    {
                        yield return e;
                    }
                }
            }

            height = _verticalEdges.GetLength(0);
            width = _verticalEdges.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    yield return _verticalEdges[y, x];
                }
            }
        }

        public HexaCell? HitTestPoint(Vector2 point, in HexaOrientation orientation)
        {
            return HitTestPoint(point, orientation, out _);
        }
        public HexaCell? HitTestPoint(Vector2 point, in HexaOrientation orientation, out HexaCoordF hitCoord)
        {
            hitCoord = orientation.ScreenToHexa(point);
            return GetCell((HexaCoord)hitCoord);
        }

        /// <remarks>
        /// The method itself is thread-safe. But the <paramref name="result"/> is not. Do not share it between threads.<br/>
        /// Also, do not modify the objects involved with this map while the method is running. It may cause undefined behavior.
        /// </remarks>
        public bool FindPath(HexaCell start, HexaCell goal, HexaPathResult result, HexaPathAccess? access, HexaPathCost? cost, HexaPathHeuristic? heuristic)
        {
            if (start.Map != this)
            {
                throw new InvalidOperationException($"The start cell '{start.Coord}' does not exist in the map.");
            }
            if (goal.Map != this)
            {
                throw new InvalidOperationException($"The goal cell '{goal.Coord}' does not exist in the map.");
            }

            result.Clear();

            Dictionary<HexaCell, HexaCell> connections = result.Connections;

            Dictionary<HexaCell, int> costs = result.Costs;
            costs[start] = 0;

            PriorityQueue<HexaCell, int> frontiers = result.Frontiers;
            frontiers.Enqueue(start, 0);

            HashSet<HexaCell> visited = result.Visited;

            access ??= (current) => current.EnumerateNeighbors();
            cost ??= ((current, next) => 1);
            heuristic ??= ((goal, next) => 0);

            while (frontiers.Count > 0)
            {
                HexaCell current = frontiers.Dequeue();

                if (!visited.Add(current))
                {
                    continue;
                }

                if (current == goal)
                {
                    List<HexaCell> resultPath = result.ResultPath;

                    HexaCell currentTrack = goal;
                    while (currentTrack is not null)
                    {
                        resultPath.Add(currentTrack);

                        if (!connections.TryGetValue(currentTrack, out HexaCell prev))
                        {
                            break;
                        }

                        currentTrack = prev;
                    }

                    if (resultPath[^1] != start)
                    {
                        throw new InvalidOperationException();
                    }

                    resultPath.Reverse();

                    result.IsSucceed = true;
                    return true;
                }

                foreach (var next in access(current))
                {
                    if (next == current)
                    {
                        continue;
                    }

                    if (next.Map != this)
                    {
                        throw new InvalidOperationException($"The next accessing cell '{next.Coord}' does not exist in the map.");
                    }

                    int newCost = costs[current] + Math.Max(1, cost(current, next))/*Must not smaller than 1*/;
                    
                    if (costs.TryGetValue(next, out int prevCost) && newCost >= prevCost)
                    {
                        continue;
                    }

                    costs[next] = newCost;
                    frontiers.Enqueue(next, newCost + Math.Max(0, heuristic(goal, next))/*Must not smaller than 0*/);
                    connections[next] = current;
                }
            }

            result.IsSucceed = false;
            return false;
        }

        public void CollectArea(HexaCell start, HexaAreaResult result, HexaAreaAccess? access, HexaAreaExpand? expand, bool includeStart = true)
        {
            if (start.Map != this)
            {
                throw new InvalidOperationException($"The start cell '{start.Coord}' does not exist in the map.");
            }

            result.Clear();

            Queue<HexaCell> frontiers = result.Frontiers;
            HashSet<HexaCell> visited = result.Visited;
            List<HexaCell> resultCells = result.ResultCells;

            access ??= (start, current) => current.EnumerateNeighbors();
            expand ??= (start, current, next) => true;

            frontiers.Enqueue(start);
            visited.Add(start);
            if (includeStart)
            {
                resultCells.Add(start);
            }

            while (frontiers.Count > 0)
            {
                HexaCell current = frontiers.Dequeue();

                foreach (var next in access(start, current))
                {
                    if (next == current)
                    {
                        continue;
                    }

                    if (next.Map != this)
                    {
                        throw new InvalidOperationException($"The next accessing cell '{next.Coord}' does not exist in the map.");
                    }

                    if (!visited.Add(next))
                    {
                        continue;
                    }

                    if (expand(start, current, next))
                    {
                        resultCells.Add(next);
                        frontiers.Enqueue(next);
                    }
                }
            }
        }

        public void CollectMultiAreas(IEnumerable<HexaCell> startingCells, HexaMultiAreasResult result, HexaAreaAccess? access, HexaAreaExpand? expand)
        {
            result.Clear();

            Queue<MultiAreaFrontier> frontiers = result.Frontiers;
            Dictionary<HexaCell, HexaCell> cellToStartingCells = result.CellToStartingCells;

            access ??= (start, current) => current.EnumerateNeighbors();
            expand ??= (start, current, next) => true;

            foreach (var startingCell in startingCells)
            {
                if (startingCell.Map != this)
                {
                    throw new InvalidOperationException($"The starting cell '{startingCell.Coord}' does not exist in the map.");
                }

                if (cellToStartingCells.TryAdd(startingCell, startingCell))
                {
                    frontiers.Enqueue(new MultiAreaFrontier(startingCell, startingCell));
                }
            }

            while (frontiers.Count > 0)
            {
                MultiAreaFrontier frontier = frontiers.Dequeue();
                HexaCell current = frontier.Cell;
                HexaCell startingCell = frontier.StartingCell;

                foreach (var next in access(startingCell, current))
                {
                    if (next == current)
                    {
                        continue;
                    }

                    if (next.Map != this)
                    {
                        throw new InvalidOperationException($"The next accessing cell '{next.Coord}' does not exist in the map.");
                    }

                    // Check already visited.
                    if (cellToStartingCells.ContainsKey(next))
                    {
                        continue;
                    }

                    if (expand(startingCell, current, next))
                    {
                        cellToStartingCells.Add(next, startingCell);
                        frontiers.Enqueue(new MultiAreaFrontier(next, startingCell));
                    }
                }
            }
        }

        internal readonly struct MultiAreaFrontier
        {
            public readonly HexaCell Cell;
            public readonly HexaCell StartingCell;

            public MultiAreaFrontier(HexaCell cell, HexaCell startingCell)
            {
                Cell = cell;
                StartingCell = startingCell;
            }
        }
    }
}
