// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Rendering;
using UnityEditor;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Editor
{
    /// <summary>
    /// Suppressing Unity editor warnings because of accessing not set shader variables such as instance transforms buffer.
    /// </summary>
    [InitializeOnLoad]
    public class IndirectInstancingEditorBandAid
    {
        static GraphicsBuffer? _dummyBuffer;

        static IndirectInstancingEditorBandAid()
        {
            InitializeDummyBuffer();

            AssemblyReloadEvents.beforeAssemblyReload += ReleaseBuffer;
            EditorApplication.quitting += ReleaseBuffer;
        }

        static void InitializeDummyBuffer()
        {
            if (_dummyBuffer is not null)
            {
                return;
            }

            // Create a dummy buffer with one identity matrix.
            _dummyBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, 64);
            _dummyBuffer.SetData(new Matrix4x4[] { Matrix4x4.identity, });

            // Place the dummy buffer as a global buffer to the ID.
            Shader.SetGlobalBuffer(IndirectInstancingEx.InstanceTransformsId, _dummyBuffer);
        }

        static void ReleaseBuffer()
        {
            if (_dummyBuffer is not null)
            {
                _dummyBuffer.Release();
                _dummyBuffer = null;
            }
        }
    }
}
