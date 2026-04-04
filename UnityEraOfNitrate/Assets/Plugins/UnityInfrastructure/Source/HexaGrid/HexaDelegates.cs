// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public delegate HexaCell CreateHexaCellDelegate(HexaMap map, HexaIndex index, HexaCoord coord);
    public delegate HexaVertex CreateHexaVertexDelegate(HexaMap map, HexaVertexCoord coord);
    public delegate HexaEdge CreateHexaEdgeDelegate(HexaVertex vertex0, HexaVertex vertex1, HexaEdgeCoord coord, HexaCell rightCell);

    /// <remarks>
    /// Default implmentation is calling <see cref="HexaCell.EnumerateNeighbors"/> with <paramref name="current"/>.<br/>
    /// Should return next path candidate cells except not accessable from the <paramref name="current"/>.
    /// </remarks>
    public delegate IEnumerable<HexaCell> HexaPathAccess(HexaCell current);
    /// <remarks>
    /// Default implmentation is <c>1</c>.<br/>
    /// Should be bigger or eqaul than 1. It will be maximized by 1. Lower value is better.
    /// </remarks>
    public delegate int HexaPathCost(HexaCell current, HexaCell next);
    /// <remarks>
    /// Default implmentation is <c>0</c>.<br/>
    /// Should be bigger or eqaul than 0. It will be maximized by 0. Lower value is better.
    /// </remarks>
    public delegate int HexaPathHeuristic(HexaCell goal, HexaCell next);
}
