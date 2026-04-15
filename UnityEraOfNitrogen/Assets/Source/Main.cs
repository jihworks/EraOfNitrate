// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds;
using Jih.Unity.EraOfNitrogen.Worlds.Generators;
using Jih.Unity.EraOfNitrogen.Worlds.Runtime;
using Jih.Unity.Infrastructure;
using Jih.Unity.Infrastructure.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Jih.Unity.EraOfNitrogen
{
    public partial class Main : MonoBehaviour
    {
        static SingletonStorage<Main> _instance;
        public static Main Instance => _instance.Get();

        [SerializeField] InputSystemUIInputModule? _inputSystemUIInputModule;
        InputSystemUIInputModule InputSystemUIInputModule => _inputSystemUIInputModule.ThrowIfNull(nameof(InputSystemUIInputModule));

        InputSystem_Actions? _inputSystemActions;
        public InputSystem_Actions InputSystemActions => _inputSystemActions.ThrowIfNull(nameof(InputSystemActions));

        InputFrameStack<InputSystem_Actions>? _inputFrameStack;
        public InputFrameStack<InputSystem_Actions> InputFrameStack => _inputFrameStack.ThrowIfNull(nameof(InputFrameStack));

        CursorFrameStack? _cursorFrameStack;
        public CursorFrameStack CursorFrameStack => _cursorFrameStack.ThrowIfNull(nameof(CursorFrameStack));

        TimeFrameStack? _timeFrameStack;
        public TimeFrameStack TimeFrameStack => _timeFrameStack.ThrowIfNull(nameof(TimeFrameStack));

        StateStorage<State> _state = new(nameof(Main));
        State? CurrentState { get => _state.Current; set => _state.Current = value; }

        World? _world;

        readonly List<DoodadCluster> _doodadClusters = new();

        public Main()
        {
            _instance = new SingletonStorage<Main>(this);

            CurrentState = new Sleep(this);
        }

        void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _inputSystemActions.Enable();

            _inputSystemUIInputModule.ThrowIfNull(out InputSystemUIInputModule inputSystemUIInputModule, nameof(_inputSystemUIInputModule));
            inputSystemUIInputModule.actionsAsset = _inputSystemActions.asset;

            _inputFrameStack = new InputFrameStack<InputSystem_Actions>(_inputSystemActions, inputSystemUIInputModule);
            _cursorFrameStack = new CursorFrameStack();
            _timeFrameStack = new TimeFrameStack();
        }

        void Start()
        {
            // 프레임 기본값.
            InputFrameStack.Push(new InputFrame(this, ui: false, player: false));
            CursorFrameStack.Push(new CursorFrame(this, lockMode: CursorLockMode.None, cursorVisible: true));
            TimeFrameStack.Push(new TimeFrame(this, timeScale: 1f));

            SaveSession saveSession = new();

            MapGenerator mapGenerator = new();
            mapGenerator.Execute(337296);

            Map? map = mapGenerator.ResultMap;
            if (map is null)
            {
                Debug.LogError("Failed to generate map.");
                return;
            }
            Debug.Log($"Seed: {map.RandomSeed}");

            World? world = new();
            world.Bind(map);
            world.Initialize();

            WorldMeshBuilder worldMeshBuilder = new(world);

            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            {
                string json = Infrastructure.Json.JsonSave.SerializeObject(map, typeof(Map).Namespace);
                Debug.Log($"JSON 길이: " + json.Length);

                saveSession.MapJson = json;
            }
            stopwatch.Stop();
            Debug.Log($"맵 직렬화: {stopwatch.ElapsedMilliseconds}ms");
            {
                GameObject landsRoot = new() { name = "Lands Root", };

                var chunks = worldMeshBuilder.BuildLand();
                _ = worldMeshBuilder.Spawn(chunks, landsRoot.transform);
            }
            stopwatch.Stop();
            Debug.Log($"땅 스폰: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            {
                var groups = worldMeshBuilder.BuildDoodads();
                var clusters = worldMeshBuilder.Spawn(groups);

                _doodadClusters.Clear();
                _doodadClusters.AddRange(clusters);
            }
            stopwatch.Stop();
            Debug.Log($"두대드 스폰: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            {
                GameObject roadsRoot = new() { name = "Roads Root", };
                roadsRoot.transform.localPosition = new Vector3(0f, 0.01f, 0f);

                var blocks = worldMeshBuilder.BuildRoads();
                foreach (var pair in blocks)
                {
                    _ = worldMeshBuilder.Spawn(pair, roadsRoot.transform);
                }
            }
            stopwatch.Stop();
            Debug.Log($"도로 스폰: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            {
                var collisionWorld = world.CollisionWorld;
                List<Infrastructure.Collisions.Common3D.ICollision> buffer = new();
                HashSet<Infrastructure.Collisions.Common3D.ICollision> hided = new();

                foreach (var province in world.Provinces)
                {
                    foreach (var tile in province.Tiles)
                    {
                        if (tile.RoadElement is not null)
                        {
                            buffer.Clear();
                            collisionWorld.Collect(tile.RoadElement.CollisionShape, buffer, ignoredCollisions: hided);

                            foreach (var hit in System.Linq.Enumerable.Cast<IWorldCollision>(buffer))
                            {
                                if (hit.CollisionType is WorldCollisionType.Doodad)
                                {
                                    DoodadCollision doodadCollision = (DoodadCollision)hit;
                                    DoodadElement doodadElement = doodadCollision.Element;
                                    doodadElement.IsVisible = false;

                                    hided.Add(doodadCollision);
                                }
                            }
                        }
                    }
                }
            }
            stopwatch.Stop();
            Debug.Log($"두대드 컬링(도로): {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            {
                GameObject provinceBordersRoot = new() { name = "Province Borders Root", };
                provinceBordersRoot.transform.position = new Vector3(0f, 0.015f, 0f);

                var borders = worldMeshBuilder.BuildProvinceBorders();
                worldMeshBuilder.Spawn(borders, provinceBordersRoot.transform);
            }
            stopwatch.Stop();
            Debug.Log($"프로빈스 보더: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            _world = world;

            {
                string json = Infrastructure.Json.JsonSave.SerializeObject(world, typeof(World).Namespace);
                Debug.Log($"JSON 길이: " + json.Length);

                saveSession.WorldJson = json;
            }
            stopwatch.Stop();
            Debug.Log($"월드 직렬화: {stopwatch.ElapsedMilliseconds}ms");
        }

        void Update()
        {
            CurrentState?.Update();

            foreach (var doodadCluster in _doodadClusters)
            {
                doodadCluster.Update();
            }

            // TODO: Debug draw.
            //if (_world is not null)
            {
                //foreach (var collision in _world.CollisionWorld.Collisions)
                //{
                //    if (collision is DoodadCollision doodadCollision &&
                //        !doodadCollision.Element.IsVisible)
                //    {
                //        foreach (var triangle in doodadCollision.Triangles)
                //        {
                //            Debug.DrawLine(triangle.WorldV0, triangle.WorldV1, Color.green);
                //            Debug.DrawLine(triangle.WorldV1, triangle.WorldV2, Color.green);
                //            Debug.DrawLine(triangle.WorldV2, triangle.WorldV0, Color.green);
                //        }
                //    }
                //}

                //foreach (var province in _world.Provinces)
                //{
                //    foreach (var tile in province.Tiles)
                //    {
                //        if (tile.RoadElement is not null)
                //        {
                //            foreach (var triangle in tile.RoadElement.CollisionShape.Triangles)
                //            {
                //                Debug.DrawLine(triangle.WorldV0, triangle.WorldV1, Color.blue);
                //                Debug.DrawLine(triangle.WorldV1, triangle.WorldV2, Color.blue);
                //                Debug.DrawLine(triangle.WorldV2, triangle.WorldV0, Color.blue);
                //            }
                //        }
                //    }
                //}
            }
        }

        void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        private void OnDestroy()
        {
            DisposableEx.DisposeAll(_doodadClusters);
        }
    }
}
