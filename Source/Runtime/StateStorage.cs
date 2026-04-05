// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.Unity.Infrastructure.Runtime
{
    /// <summary>
    /// This is a <b>struct</b> to avoid cache-miss and GC pressure.<br/>
    /// <b>DO NOT</b> use <c>readonly</c> for this field.<br/>
    /// <b>DO NOT</b> use automatically implemented property.
    /// </summary>
    /// <remarks>
    /// <b>NOT</b> thread-safe.
    /// </remarks>
    public struct StateStorage<TState> where TState : class, IState
    {
        /// <summary>
        /// Debugging purpose name.
        /// </summary>
        public readonly string Name;

        bool _isLocked;
        TState? _current;

        public StateStorage(string name) : this()
        {
            Name = name;
        }

        public TState? Current
        {
            readonly get => _current;
            set
            {
                if (ReferenceEquals(value, _current))
                {
                    Debug.LogWarning($"=== State '{Name}' trying the same instance.");
                    Debug.LogWarning("    Instance: " + value);
                    return;
                }
                if (_isLocked)
                {
                    Debug.LogWarning($"=== State '{Name}' is locked.");
                    Debug.LogWarning("    From: " + _current + "  To: " + value);
                    return;
                }

                _isLocked = true;
                try
                {
                    _current?.End(value);
                    IState? prev = _current;
                    _current = value;
                    _current?.Begin(prev);
                }
                finally
                {
                    _isLocked = false;
                }
            }
        }
    }
}
