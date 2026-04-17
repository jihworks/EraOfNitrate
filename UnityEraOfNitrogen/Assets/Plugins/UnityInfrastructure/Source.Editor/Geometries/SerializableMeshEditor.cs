// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Geometries;
using Jih.Unity.Infrastructure.Rendering;
using System;
using UnityEditor;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Editor.Geometries
{
    [CustomEditor(typeof(SerializableMesh))]
    public sealed class SerializableMeshEditor : UnityEditor.Editor
    {
        PreviewRenderUtility? _previewRenderUtility;

        Vector2 _cameraOrbitAngles = new(120f, -20f);
        float _cameraDistance = 5f;
        Vector3 _cameraPivotPoint = Vector3.zero;

        Mesh? _previewMesh;
        Material? _previewMaterial;

        void OnEnable()
        {
            UpdatePreviewMesh();
            CreatePreviewMaterial();

            _previewRenderUtility ??= new PreviewRenderUtility();

            ResetPreviewCamera();
        }

        void OnDisable()
        {
            if (_previewRenderUtility is not null)
            {
                _previewRenderUtility.Cleanup();
                _previewRenderUtility = null;
            }

            DestroyPreviewMesh();
            DestroyPreviewMaterial();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreviewMesh();
                ResetPreviewCamera();
                Repaint();
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (_previewRenderUtility is null)
            {
                return;
            }

            if (_previewMesh == null || _previewMaterial == null)
            {
                EditorGUI.DropShadowLabel(r, "There is no preview to show.");
                return;
            }

            Event e = Event.current;

            HandleInputs(r, e);

            if (e.type is EventType.Repaint)
            {
                _previewRenderUtility.BeginPreview(r, background);

                Quaternion rotation = Quaternion.Euler(_cameraOrbitAngles.y, _cameraOrbitAngles.x, 0f);
                Vector3 position = _cameraPivotPoint - (rotation * Vector3.forward * _cameraDistance);

                Camera previewCamera = _previewRenderUtility.camera;
                previewCamera.transform.SetPositionAndRotation(position, rotation);

                int subMeshCount = _previewMesh.subMeshCount;
                for (int s = 0; s < subMeshCount; s++)
                {
                    _previewRenderUtility.DrawMesh(_previewMesh, Matrix4x4.identity, _previewMaterial, s);
                }

                previewCamera.Render();

                Texture renderResult = _previewRenderUtility.EndPreview();
                GUI.DrawTexture(r, renderResult, ScaleMode.StretchToFill, false);
            }
        }

        void HandleInputs(Rect r, Event e)
        {
            if (_previewRenderUtility is null)
            {
                return;
            }

            if (!r.Contains(e.mousePosition))
            {
                return;
            }

            // Orbit
            if (e.type is EventType.MouseDrag && e.button == 0)
            {
                _cameraOrbitAngles.x += e.delta.x;
                _cameraOrbitAngles.y += e.delta.y;
                e.Use();
            }
            // Zoom
            else if (e.type is EventType.ScrollWheel)
            {
                _cameraDistance += e.delta.y * (_cameraDistance * 0.05f);
                _cameraDistance = Math.Max(0.01f, _cameraDistance);
                e.Use();
            }
            // Panning
            else if (e.type is EventType.MouseDrag && e.button == 2)
            {
                Vector3 panDelta = new Vector3(-e.delta.x, e.delta.y, 0f) * (_cameraDistance * 0.001f);
                _cameraPivotPoint += _previewRenderUtility.camera.transform.rotation * panDelta;
                e.Use();
            }

            if (e.type is not EventType.Layout && e.type is not EventType.Used)
            {
                Repaint();
            }
        }

        void ResetPreviewCamera()
        {
            if (_previewMesh == null || _previewRenderUtility is null)
            {
                return;
            }

            Camera previewCamera = _previewRenderUtility.camera;

            Bounds bounds = _previewMesh.bounds;
            _cameraPivotPoint = bounds.center;

            float radius = bounds.extents.magnitude;
            MathEx.Max(ref radius, 0.001f);

            float fov = previewCamera.fieldOfView;
            _cameraDistance = radius / Mathf.Sin((fov * 0.5f).ToRadians()) * 1.1f;

            previewCamera.nearClipPlane = Math.Max(0.001f, radius * 0.05f);
            previewCamera.farClipPlane = Math.Max(100f, _cameraDistance + radius * 10f);
        }

        void UpdatePreviewMesh()
        {
            DestroyPreviewMesh();

            SerializableMesh source = (SerializableMesh)target;

            MeshCollector collector = new(AdditionalAttributes.None);
            collector.Append(source);

            _previewMesh = collector.ToTrianglesMesh(false, false);
            _previewMesh.hideFlags = HideFlags.HideAndDontSave;
        }

        void DestroyPreviewMesh()
        {
            if (_previewMesh != null)
            {
                DestroyImmediate(_previewMesh);
                _previewMesh = null;
            }
        }

        void CreatePreviewMaterial()
        {
            _previewMaterial = new Material(MaterialEx.GetDefaultShader())
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
        }

        void DestroyPreviewMaterial()
        {
            if (_previewMaterial != null)
            {
                DestroyImmediate(_previewMaterial);
                _previewMaterial = null;
            }
        }
    }
}
