// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Jih.Unity.Infrastructure
{
    /// <remarks>
    /// <b>NOT</b> thread-safe.
    /// </remarks>
    public class Roulette<T> : IReadOnlyList<RouletteItem<T?>>
    {
        public int TotalWeight { get; private set; } = 0;

        public int Count => items.Count;

        public RouletteItem<T?> this[int index] => items[index];

        readonly List<RouletteItem<T?>> items;

        public Roulette()
        {
            items = new();
        }
        public Roulette(int capacity)
        {
            items = new(capacity);
        }

        public void Add(RouletteItem<T?> item)
        {
            CheckWeight(item.Weight);

            items.Add(item);
            Update();
        }

        public void AddRange(IEnumerable<RouletteItem<T?>> collection)
        {
            foreach (var item in collection)
            {
                CheckWeight(item.Weight);

                items.Add(item);
            }
            Update();
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
            Update();
        }

        public void RemoveAll(Predicate<RouletteItem<T?>> predicate)
        {
            int count = items.RemoveAll(predicate);
            if (count != 0)
            {
                Update();
            }
        }

        public void Clear()
        {
            items.Clear();
            Update();
        }

        void Update()
        {
            TotalWeight = 0;

            for (int i = 0; i < items.Count; i++)
            {
                int w = items[i].Weight;
                TotalWeight = checked(TotalWeight + w);
            }
        }

        public T? Next(IRandomInt32 random)
        {
            return Next(random, out _);
        }
        public T? Next(IRandomInt32 random, out int index)
        {
            if (items.Count <= 0)
            {
                throw new InvalidOperationException("There is no item for Roulette.");
            }

            int r = random.NextInt32(0, TotalWeight);
            return Next(r, out index);
        }

        T? Next(int r, out int index)
        {
            int prevRange = 0;
            for (int i = 0; i < items.Count; i++)
            {
                RouletteItem<T?> item = items[i];

                int nextRange = prevRange + item.Weight;
                if (prevRange <= r && r < nextRange)
                {
                    index = i;
                    return item.Value;
                }

                prevRange = nextRange;
            }

            // This will never trigger.
            throw new InvalidOperationException("Roulette failed.");
        }

        public IEnumerator<RouletteItem<T?>> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckWeight(int weight)
        {
            if (weight > 0)
            {
                return;
            }
            throw new ArgumentException("Weight cannot be 0 or negative value.");
        }
    }

    public readonly struct RouletteItem<T>
    {
        public readonly T? Value;
        public readonly int Weight;

        public RouletteItem(T? value, int weight)
        {
            Value = value;
            Weight = weight;
        }
    }
}
