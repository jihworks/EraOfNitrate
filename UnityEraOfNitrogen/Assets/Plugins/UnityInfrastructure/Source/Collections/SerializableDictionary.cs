// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Collections
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] List<TKey> _keys = new();
        [SerializeField] List<TValue> _values = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            _keys.Capacity = Count;
            _values.Capacity = Count;

            foreach (var pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            EnsureCapacity(_keys.Count);

            for (int i = 0; i < _keys.Count; i++)
            {
                Add(_keys[i], _values[i]);
            }

            _keys.Clear();
            _values.Clear();
        }
    }
}
