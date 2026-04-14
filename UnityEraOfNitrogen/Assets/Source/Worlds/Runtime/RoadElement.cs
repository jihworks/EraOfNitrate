// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure.Collisions.Common3D;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.EraOfNitrogen.Worlds.Runtime
{
    public class RoadElement
    {
        public Tile Tile { get; }

        readonly List<GameObject> _gameObjects;
        public IReadOnlyList<GameObject> GameObjects => _gameObjects;

        public MeshShape CollisionShape { get; }

        public RoadElement(Tile tile, List<GameObject> gameObjects, MeshShape collisionShape)
        {
            Tile = tile;
            _gameObjects = gameObjects;
            CollisionShape = collisionShape;
        }

        public void Destroy()
        {
            foreach (var gameObject in _gameObjects)
            {
                Object.Destroy(gameObject);
            }
            _gameObjects.Clear();
        }
    }
}
