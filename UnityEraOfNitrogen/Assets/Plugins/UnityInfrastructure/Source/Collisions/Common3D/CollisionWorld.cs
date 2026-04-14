// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions.Common3D
{
    /// <summary>
    /// A collision detecting world based on spatial-hash grid.
    /// </summary>
    /// <remarks>
    /// <b>* Choose a Cell Size</b><br/>
    /// Generally, should consider these conditions:<br/>
    /// <list type="bullet">
    /// <item>
    /// <term>Object Size</term>
    /// <description>
    /// Commonly, 2~3 times greater value about average collision size.<br/>
    /// For example, most collision sizes are about 1~2 in radius, 3~5 are good values.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Too Small Value</term>
    /// <description>
    /// It drops memory and searching efficiency because a collision occupying several cells.<br/>
    /// Additionally, increasing noise from floating-points in raycast calculation. It causes unexpected skipping or failure.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Too Large Value</term>
    /// <description>
    /// It drops broad phase searching because too many collisions are in one cell and also drops narrow phase efficiency.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Objects Distribution</term>
    /// <description>
    /// If the objects are evenly distributed, smaller value may better. If not, bigger value may better.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Raycast Query</term>
    /// <description>
    /// If using raycast many times, bigger value is better because of the DDA algorithm.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Empirical Value</term>
    /// <description>
    /// 5~10 is good starting point in common scenarios. May need several benchmarks to get better value.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public class CollisionWorld
    {
        public float CellSize { get; }

        readonly HashSet<ICollision> _collisions = new();
        public IReadOnlyCollection<ICollision> Collisions => _collisions;

        readonly Dictionary<Vector3Int, List<ICollision>> _cells = new();
        readonly Dictionary<ICollision, List<Vector3Int>> _cellsMap = new();

        readonly ListPool<ICollision> _collisionListPool = new();
        readonly ListPool<Vector3Int> _cellsListPool = new();
        readonly HashSetPool<ICollision> _collisionMapPool = new(isThreadSafe: true);

        readonly HashSet<ICollision> _pendingCellUpdateCollisions = new();

        /// <param name="cellSize">For each cell size of the grid. This value effects to performance and memory efficiency. Check <see cref="CollisionWorld"/> for more details.</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="cellSize"/>is 0 or negative.</exception>
        public CollisionWorld(float cellSize)
        {
            if (cellSize <= 0f)
            {
                throw new InvalidOperationException("Cell size cannot be 0 or negative.");
            }
            CellSize = cellSize;
        }

        public void Register(ICollision collision)
        {
            if (!_collisions.Add(collision))
            {
                return;
            }

            _pendingCellUpdateCollisions.Add(collision);
            
            collision.WorldBoundsChanged += OnCollisionChanged;
        }

        public void Unregister(ICollision collision)
        {
            if (!_collisions.Remove(collision))
            {
                return;
            }

            collision.WorldBoundsChanged -= OnCollisionChanged;

            _pendingCellUpdateCollisions.Remove(collision);

            RemoveFromCells(collision);

            if (_cellsMap.TryGetValue(collision, out List<Vector3Int> list))
            {
                _cellsMap.Remove(collision);

                _cellsListPool.Release(list);
            }
        }

        /// <summary>
        /// Execute pending lazy updates immediately.
        /// </summary>
        public void Flush()
        {
            if (_pendingCellUpdateCollisions.Count > 0)
            {
                foreach (var collision in _pendingCellUpdateCollisions)
                {
                    UpdateCollisionCells(collision);
                }
                _pendingCellUpdateCollisions.Clear();
            }
        }

        public void Clear()
        {
            _pendingCellUpdateCollisions.Clear();

            _collisionListPool.ReleaseMany(_cells.Values);
            _cells.Clear();

            foreach (var collision in _collisions)
            {
                collision.WorldBoundsChanged -= OnCollisionChanged;
            }
            _collisions.Clear();

            _cellsListPool.ReleaseMany(_cellsMap.Values);
            _cellsMap.Clear();
        }

        public int Collect(CollisionShape collision, List<ICollision> buffer, uint collisionChannelMask = CollisionChannelEx.All, HashSet<ICollision>? ignoredCollisions = null)
        {
            collision.UpdateBounds();
            Flush();

            HashSet<ICollision> processed = _collisionMapPool.Get();
            try
            {
                int count = 0;

                foreach (var cellPos in EnumerateCellsForBounds(collision.WorldBounds))
                {
                    if (!_cells.TryGetValue(cellPos, out List<ICollision>? cellCollisions))
                    {
                        continue;
                    }

                    for (int i = 0; i < cellCollisions.Count; i++)
                    {
                        ICollision otherCollision = cellCollisions[i];

                        if (!otherCollision.IsEnabled)
                        {
                            continue;
                        }
                        if (otherCollision == collision)
                        {
                            continue;
                        }
                        // Mask Check.
                        if (!collisionChannelMask.Has(otherCollision.CollisionChannel))
                        {
                            continue;
                        }
                        if (ignoredCollisions is not null && ignoredCollisions.Contains(otherCollision))
                        {
                            continue;
                        }
                        if (processed.Contains(otherCollision))
                        {
                            continue;
                        }

                        processed.Add(otherCollision);
                        if (otherCollision.IntersectsWith(collision))
                        {
                            buffer.Add(otherCollision);
                            count++;
                        }
                    }
                }

                return count;
            }
            finally
            {
                _collisionMapPool.Release(processed);
            }
        }

        public bool Check(CollisionShape collision, [NotNullWhen(true)] out ICollision? hitCollision, uint collisionChannelMask = CollisionChannelEx.All, HashSet<ICollision>? ignoredCollisions = null)
        {
            collision.UpdateBounds();
            Flush();

            HashSet<ICollision> processed = _collisionMapPool.Get();
            try
            {
                foreach (var cellPos in EnumerateCellsForBounds(collision.WorldBounds))
                {
                    if (!_cells.TryGetValue(cellPos, out List<ICollision>? cellCollisions))
                    {
                        continue;
                    }

                    for (int i = 0; i < cellCollisions.Count; i++)
                    {
                        ICollision otherCollision = cellCollisions[i];

                        if (!otherCollision.IsEnabled)
                        {
                            continue;
                        }
                        if (otherCollision == collision)
                        {
                            continue;
                        }
                        // Mask check.
                        if (!collisionChannelMask.Has(otherCollision.CollisionChannel))
                        {
                            continue;
                        }
                        if (ignoredCollisions is not null && ignoredCollisions.Contains(otherCollision))
                        {
                            continue;
                        }
                        if (processed.Contains(otherCollision))
                        {
                            continue;
                        }

                        processed.Add(otherCollision);
                        if (otherCollision.IntersectsWith(collision))
                        {
                            hitCollision = otherCollision;
                            return true;
                        }
                    }
                }

                hitCollision = null;
                return false;
            }
            finally
            {
                _collisionMapPool.Release(processed);
            }
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out ICollision? hitCollision, out float hitDistance, uint collisionChannelMask = CollisionChannelEx.All, HashSet<ICollision>? ignoredCollisions = null)
        {
            Flush();

            hitCollision = null;
            hitDistance = float.MaxValue;

            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                return false;
            }
            Vector3 normalizedDir = direction.normalized;

            // DDA(Digital Differential Analyzer) algorithm to find intersecting cells.
            Vector3 currentPos = origin;
            float traveled = 0;

            // Starting cell position.
            Vector3Int cellPos = GetCellPosition(currentPos);

            // Increament for axis.
            int stepX = normalizedDir.x > 0 ? 1 : (normalizedDir.x < 0 ? -1 : 0);
            int stepY = normalizedDir.y > 0 ? 1 : (normalizedDir.y < 0 ? -1 : 0);
            int stepZ = normalizedDir.z > 0 ? 1 : (normalizedDir.z < 0 ? -1 : 0);

            float tDeltaX = CalculateDeltaT(normalizedDir.x, CellSize);
            float tDeltaY = CalculateDeltaT(normalizedDir.y, CellSize);
            float tDeltaZ = CalculateDeltaT(normalizedDir.z, CellSize);

            float tMaxX = CalculateInitialT(origin.x, normalizedDir.x, cellPos.x, CellSize, stepX);
            float tMaxY = CalculateInitialT(origin.y, normalizedDir.y, cellPos.y, CellSize, stepY);
            float tMaxZ = CalculateInitialT(origin.z, normalizedDir.z, cellPos.z, CellSize, stepZ);

            HashSet<ICollision> processed = _collisionMapPool.Get();
            try
            {
                while (traveled < maxDistance)
                {
                    if (_cells.TryGetValue(cellPos, out List<ICollision>? cellCollisions))
                    {
                        for (int i = 0; i < cellCollisions.Count; i++)
                        {
                            ICollision collision = cellCollisions[i];

                            if (!collision.IsEnabled)
                            {
                                continue;
                            }
                            // Mask check.
                            if (!collisionChannelMask.Has(collision.CollisionChannel))
                            {
                                continue;
                            }
                            if (ignoredCollisions is not null && ignoredCollisions.Contains(collision))
                            {
                                continue;
                            }
                            if (processed.Contains(collision))
                            {
                                continue;
                            }

                            processed.Add(collision);
                            if (collision.Raycast(origin, normalizedDir, out float t) && t < hitDistance && t <= maxDistance)
                            {
                                hitCollision = collision;
                                hitDistance = t;
                            }
                        }
                    }

                    // Abort if found.
                    if (hitCollision is not null &&
                        tMaxX > hitDistance && tMaxY > hitDistance && tMaxZ > hitDistance)
                    {
                        break;
                    }

                    // To nect cell.
                    if (tMaxX < tMaxY && tMaxX < tMaxZ)
                    {
                        // X axis.
                        traveled = tMaxX;
                        cellPos.x += stepX;
                        tMaxX += tDeltaX;
                    }
                    else if (tMaxY < tMaxZ)
                    {
                        // Y axis.
                        traveled = tMaxY;
                        cellPos.y += stepY;
                        tMaxY += tDeltaY;
                    }
                    else
                    {
                        // Z axis.
                        traveled = tMaxZ;
                        cellPos.z += stepZ;
                        tMaxZ += tDeltaZ;
                    }
                }

                return hitCollision is not null;
            }
            finally
            {
                _collisionMapPool.Release(processed);
            }
        }

        // Calculate distance(delta) to next cell boundary in an axis.
        float CalculateDeltaT(float dirComponent, float cellSize)
        {
            if (Mathf.Abs(dirComponent) < Mathf.Epsilon)
            {
                return float.MaxValue;
            }

            return cellSize / Mathf.Abs(dirComponent);
        }

        // Calculate distance to first cell boundary in an axis.
        float CalculateInitialT(float originComponent, float dirComponent, int cellIndex, float cellSize, int step)
        {
            if (Mathf.Abs(dirComponent) < Mathf.Epsilon)
            {
                return float.MaxValue;
            }

            // Next cell boundary.
            float nextBoundary;
            if (step > 0)
            {
                // Positive direction: Top boundary of current cell.
                nextBoundary = (cellIndex + 1) * cellSize;
            }
            else if (step < 0)
            {
                // Negative direction: Bottom boundary of current cell.
                nextBoundary = cellIndex * cellSize;
            }
            else
            {
                return float.MaxValue;
            }

            // Calc distance.
            float t = (nextBoundary - originComponent) / dirComponent;
            return t > 0 ? t : 0; // To 0 for negative distance (Backward from the origin).
        }

        void OnCollisionChanged(ICollision collision)
        {
            _pendingCellUpdateCollisions.Add(collision);
        }

        void UpdateCollisionCells(ICollision collision)
        {
            RemoveFromCells(collision);

            if (_cellsMap.TryGetValue(collision, out List<Vector3Int>? newCells))
            {
                newCells.Clear();
            }
            else
            {
                newCells = _cellsListPool.Get();
                _cellsMap.Add(collision, newCells);
            }
            newCells.AddRange(EnumerateCellsForBounds(collision.WorldBounds));

            foreach (var cellPos in newCells)
            {
                if (!_cells.TryGetValue(cellPos, out var cellCollisions))
                {
                    cellCollisions = _collisionListPool.Get();

                    _cells.Add(cellPos, cellCollisions);
                }
                cellCollisions.Add(collision);
            }
        }

        void RemoveFromCells(ICollision collision)
        {
            if (!_cellsMap.TryGetValue(collision, out List<Vector3Int>? occupiedCells))
            {
                return;
            }

            foreach (var cellPos in occupiedCells)
            {
                if (!_cells.TryGetValue(cellPos, out List<ICollision>? cellCollisions))
                {
                    continue;
                }
                cellCollisions.Remove(collision);

                // Remove empty cell.
                if (cellCollisions.Count <= 0)
                {
                    List<ICollision> list = _cells[cellPos];
                    _cells.Remove(cellPos);

                    _collisionListPool.Release(list);
                }
            }
        }

        IEnumerable<Vector3Int> EnumerateCellsForBounds(Bounds bounds)
        {
            Vector3Int minCell = GetCellPosition(bounds.min);
            Vector3Int maxCell = GetCellPosition(bounds.max);

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    for (int z = minCell.z; z <= maxCell.z; z++)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }

        Vector3Int GetCellPosition(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / CellSize),
                Mathf.FloorToInt(worldPos.y / CellSize),
                Mathf.FloorToInt(worldPos.z / CellSize)
            );
        }
    }
}
