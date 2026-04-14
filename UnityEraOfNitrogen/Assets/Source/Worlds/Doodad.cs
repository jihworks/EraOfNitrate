// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds.Generators;
using Jih.Unity.EraOfNitrogen.Worlds.Runtime;
using Jih.Unity.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public class Doodad
    {
        [JsonProperty] public DoodadType Type { get; private set; }
        [JsonProperty] public int Variant { get; private set; }

        [JsonProperty] public Float3 UnityLocation { get; private set; }
        [JsonProperty] public float UnityRotationY { get; private set; }
        [JsonProperty] public float UnityScale { get; private set; }

        [JsonIgnore, MemberNotNullWhen(true,
            nameof(_tile))]
        public bool IsInitialized { get; private set; }

        [JsonIgnore] Tile? _tile;
        [JsonIgnore] public Tile Tile => _tile.ThrowIfNull(nameof(Tile));

        [JsonIgnore] DoodadElement? _element;
        [JsonIgnore] public DoodadElement Element => _element.ThrowIfNull(nameof(Element));

        [JsonConstructor]
        private Doodad()
        {
        }

        public Doodad(GeneratorDoodad doodad)
        {
            Type = doodad.Type;
            Variant = doodad.Variant;
            UnityLocation = doodad.UnityLocation;
            UnityRotationY = doodad.UnityRotationY;
            UnityScale = doodad.UnityScale;
        }

        public void Initialize(Tile tile)
        {
            if (IsInitialized)
            {
                return;
            }

            _tile = tile;

            IsInitialized = true;
        }

        public void Spawned(DoodadElement element)
        {
            if (_element is not null)
            {
                throw new InvalidOperationException("이미 스폰된 두대드를 다시 스폰함.");
            }

            _element = element;
            element.Doodad = this;
        }

        public DoodadTransform GetTransform()
        {
            return new DoodadTransform(UnityLocation, UnityRotationY, UnityScale);
        }
    }
}
