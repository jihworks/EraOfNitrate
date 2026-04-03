// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.TileGrid
{
    public delegate TileCell CreateTileCellDelegate(TileMap map, TileCellCoord coord);
    public delegate TileVertex CreateTileVertexDelegate(TileMap map, TileVertexCoord coord);
    public delegate TileEdge CreateTileEdgeDelegate(TileEdgeCoord coord, TileVertex vertex0, TileVertex vertex1, TileCell rightCell);
}
