// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Jih.Unity.Infrastructure.Rendering
{
    public static class MaterialEx
    {
        public static Shader GetDefaultShader()
        {
            RenderPipelineAsset currentPipeline = GraphicsSettings.currentRenderPipeline;

            if (currentPipeline != null)
            {
                // SRP.
                return currentPipeline.defaultShader;
            }

            // Maybe Built-in.
            Shader? standardShader = Shader.Find("Standard");
            if (standardShader == null)
            {
                throw new NotSupportedException("Failed to find default material without Render Pipeline asset. Expecting 'Standard' shader but not exists.");
            }
            return standardShader;
        }
    }
}
