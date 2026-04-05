// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Jih.Unity.Infrastructure.Collections
{
    /// <summary>
    /// Sorting elements by priority in ascending order. The element with the lowest priority will be dequeued first.
    /// </summary>
    public class PriorityQueue<TElement, TPriority> : IReadOnlyList<TElement> where TPriority : IComparable<TPriority>
    {
        readonly List<(TElement Element, TPriority Priority)> _pairs = new();

        public int Count => _pairs.Count;

        /// <summary>
        /// <c>NOT</c> sorted order.
        /// </summary>
        public TElement this[int index] => _pairs[index].Element;

        public void Enqueue(TElement element, TPriority priority)
        {
            _pairs.Add((element, priority));
            HeapifyUp(_pairs.Count - 1);
        }

        public TElement Dequeue()
        {
            if (_pairs.Count <= 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            TElement result = _pairs[0].Element;

            // Move the last element to the root.
            _pairs[0] = _pairs[^1];
            _pairs.RemoveAt(_pairs.Count - 1);

            if (_pairs.Count > 0)
            {
                HeapifyDown(0);
            }

            return result;
        }

        public TElement Peek()
        {
            if (_pairs.Count <= 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }
            return _pairs[0].Element;
        }

        public void Clear()
        {
            _pairs.Clear();
        }

        void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;

                // If the parent's priority is less than or equal to the current element's priority, the sorting is complete.
                if (_pairs[index].Priority.CompareTo(_pairs[parentIndex].Priority) >= 0)
                {
                    break;
                }

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        void HeapifyDown(int index)
        {
            int lastIndex = _pairs.Count - 1;
            while (true)
            {
                int leftChildIndex = index * 2 + 1;
                int rightChildIndex = index * 2 + 2;
                int smallestIndex = index;

                // Compare to the left child.
                if (leftChildIndex <= lastIndex &&
                    _pairs[leftChildIndex].Priority.CompareTo(_pairs[smallestIndex].Priority) < 0)
                {
                    smallestIndex = leftChildIndex;
                }

                // Compare to the right child.
                if (rightChildIndex <= lastIndex &&
                    _pairs[rightChildIndex].Priority.CompareTo(_pairs[smallestIndex].Priority) < 0)
                {
                    smallestIndex = rightChildIndex;
                }

                // If there is no child node with higher priority, the sorting is complete.
                if (smallestIndex == index)
                {
                    break;
                }

                Swap(index, smallestIndex);
                index = smallestIndex;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Swap(int indexA, int indexB)
        {
            (_pairs[indexB], _pairs[indexA]) = (_pairs[indexA], _pairs[indexB]);
        }

        /// <summary>
        /// <c>NOT</c> sorted order.
        /// </summary>
        public IEnumerator<TElement> GetEnumerator()
        {
            return _pairs.Select(pair => pair.Element).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
