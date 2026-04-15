// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Geometries;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Editor.Geometries
{
    public abstract class BaseConvexHullGeneratorWindow : EditorWindow
    {
        Mesh? _sourceMesh;

        string _savePath = "Assets/ConvexHull.asset";

        bool _previewConvexHull = true;
        GameObject? _previewObject;
        Material? _previewMaterial;

        int _originalVertexCount, _originalTriangleCount;
        int _hullVertexCount, _hullTriangleCount;
        float _vertexReductionPercent, _triangleReductionPercent;

        bool _showEfficiencyStats = false;

        protected abstract Material CreatePreviewMaterial();
        protected abstract void GenerateHull(Vector3[] points, out List<Vector3> hullVertices, out List<int> hullTriangles);

        protected virtual void OnEnable()
        {
            _previewMaterial = CreatePreviewMaterial();
            ResetStats();
        }

        protected virtual void OnDisable()
        {
            DestroyPreview();
        }

        protected virtual void OnGUI()
        {
            EditorGUILayout.LabelField("Convex Hull Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Mesh? prevMesh = _sourceMesh;

            _sourceMesh = (Mesh)EditorGUILayout.ObjectField("Source Mesh", _sourceMesh, typeof(Mesh), false);

            if (prevMesh != _sourceMesh)
            {
                ResetStats();
                DestroyPreview();
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Save Path", GUILayout.Width(80));
            _savePath = EditorGUILayout.TextField(_savePath, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                BrowseSavePath();
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_savePath) && !_savePath.EndsWith(".asset"))
            {
                _savePath += ".asset";
            }

            _previewConvexHull = EditorGUILayout.Toggle("Preview Convex Hull", _previewConvexHull);

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(_sourceMesh == null);

            if (GUILayout.Button("Save Convex Hull"))
            {
                SaveConvexHull();
            }

            EditorGUI.EndDisabledGroup();

            if (_sourceMesh != null && _previewConvexHull)
            {
                if (GUILayout.Button("Update Preview"))
                {
                    UpdatePreview();
                }
            }

            if (_previewObject != null)
            {
                if (GUILayout.Button("Clear Preview"))
                {
                    DestroyPreview();
                }
            }

            if (_showEfficiencyStats)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mesh Efficiency Statistics", EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Original Mesh:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Vertices: {_originalVertexCount}");
                EditorGUILayout.LabelField($"Triangles: {_originalTriangleCount}");

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Convex Hull:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Vertices: {_hullVertexCount}");
                EditorGUILayout.LabelField($"Triangles: {_hullTriangleCount}");

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Efficiency:", EditorStyles.boldLabel);

                GUIStyle vertexStyle = new(EditorStyles.label);
                vertexStyle.normal.textColor = GetEfficiencyColor(_vertexReductionPercent);
                GUIStyle triangleStyle = new(EditorStyles.label);
                triangleStyle.normal.textColor = GetEfficiencyColor(_triangleReductionPercent);

                EditorGUILayout.LabelField($"Vertex Reduction: {_vertexReductionPercent:F2}%", vertexStyle);
                EditorGUILayout.LabelField($"Triangle Reduction: {_triangleReductionPercent:F2}%", triangleStyle);

                string efficiencySummary = GetEfficiencySummary();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Summary:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(efficiencySummary, EditorStyles.wordWrappedLabel);

                EditorGUILayout.EndVertical();
            }
        }

        void BrowseSavePath()
        {
            string initialDir = System.IO.Path.GetDirectoryName(_savePath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(_savePath);

            if (string.IsNullOrEmpty(initialDir) || !initialDir.StartsWith("Assets"))
            {
                initialDir = "Assets";
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "ConvexHull";
            }

            string path = EditorUtility.SaveFilePanelInProject("Save Convex Hull", fileName, "asset", "Select location to save the convex hull asset", initialDir);

            if (!string.IsNullOrEmpty(path))
            {
                _savePath = path;
            }
        }

        void SaveConvexHull()
        {
            if (_sourceMesh == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_savePath) || !_savePath.StartsWith("Assets"))
            {
                BrowseSavePath();

                if (string.IsNullOrEmpty(_savePath) || !_savePath.StartsWith("Assets"))
                {
                    Debug.LogWarning("Save canceled: Invalid save path");
                    return;
                }
            }

            _originalVertexCount = _sourceMesh.vertexCount;
            _originalTriangleCount = _sourceMesh.triangles.Length / 3;

            Vector3[] vertices = _sourceMesh.vertices;
            GenerateHull(vertices, out List<Vector3> hullVertices, out List<int> hullTriangles);

            _hullVertexCount = hullVertices.Count;
            _hullTriangleCount = hullTriangles.Count / 3;

            _vertexReductionPercent = 100f * (1f - (float)_hullVertexCount / _originalVertexCount);
            _triangleReductionPercent = 100f * (1f - (float)_hullTriangleCount / _originalTriangleCount);

            _showEfficiencyStats = true;

            SerializableMesh serializableMesh = CreateInstance<SerializableMesh>();
            SetSerializableMeshData(serializableMesh, hullVertices, hullTriangles);

            string directory = System.IO.Path.GetDirectoryName(_savePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            if (System.IO.File.Exists(_savePath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "File Already Exists",
                    "The file already exists. Do you want to overwrite it?",
                    "Overwrite",
                    "Cancel"
                );

                if (!overwrite)
                {
                    Debug.Log("Save canceled: File already exists");
                    return;
                }
            }

            AssetDatabase.CreateAsset(serializableMesh, _savePath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = serializableMesh;

            Debug.Log($"Convex hull generated and saved to {_savePath}\n" +
                      $"Original Mesh: {_originalVertexCount} vertices, {_originalTriangleCount} triangles\n" +
                      $"Convex Hull: {_hullVertexCount} vertices, {_hullTriangleCount} triangles\n" +
                      $"Vertex Reduction: {_vertexReductionPercent:F2}%, Triangle Reduction: {_triangleReductionPercent:F2}%");

            if (_previewConvexHull)
            {
                UpdatePreview(hullVertices, hullTriangles);
            }
        }

        void SetSerializableMeshData(SerializableMesh mesh, List<Vector3> vertices, List<int> triangles)
        {
            mesh.SetVertices(vertices);

            SerializableSubMesh subMesh = new();
            subMesh.SetIndices(triangles);

            mesh.SetSubMeshes(subMesh);
        }

        void UpdatePreview()
        {
            if (_sourceMesh == null)
            {
                return;
            }

            _originalVertexCount = _sourceMesh.vertexCount;
            _originalTriangleCount = _sourceMesh.triangles.Length / 3;

            Vector3[] vertices = _sourceMesh.vertices;
            GenerateHull(vertices, out List<Vector3> hullVertices, out List<int> hullTriangles);

            _hullVertexCount = hullVertices.Count;
            _hullTriangleCount = hullTriangles.Count / 3;

            _vertexReductionPercent = 100f * (1f - (float)_hullVertexCount / _originalVertexCount);
            _triangleReductionPercent = 100f * (1f - (float)_hullTriangleCount / _originalTriangleCount);

            _showEfficiencyStats = true;

            UpdatePreview(hullVertices, hullTriangles);
        }

        void UpdatePreview(List<Vector3> vertices, List<int> triangles)
        {
            DestroyPreview();

            _previewObject = new GameObject("ConvexHull_Preview")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            Undo.RegisterCreatedObjectUndo(_previewObject, "Convex Hull Preview");

            MeshFilter meshFilter = _previewObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = _previewObject.AddComponent<MeshRenderer>();

            Mesh previewMesh = new();
            previewMesh.SetVertices(vertices);
            previewMesh.SetTriangles(triangles, 0);
            previewMesh.RecalculateNormals();

            meshFilter.sharedMesh = previewMesh;
            meshRenderer.sharedMaterial = _previewMaterial;

            SceneView.lastActiveSceneView.Repaint();
        }

        static Color GetEfficiencyColor(float reductionPercent)
        {
            if (reductionPercent >= 90f)
            {
                return Color.green;
            }
            else if (reductionPercent >= 70f)
            {
                return Color.Lerp(Color.yellow, Color.green, (reductionPercent - 70f) / 20f);
            }
            else if (reductionPercent >= 30f)
            {
                return Color.Lerp(Color.red, Color.yellow, (reductionPercent - 30f) / 40f);
            }
            else
            {
                return Color.red;
            }
        }

        string GetEfficiencySummary()
        {
            float avgReduction = (_vertexReductionPercent + _triangleReductionPercent) / 2;

            if (avgReduction >= 90f)
            {
                return "Excellent optimization! The convex hull provides a very efficient simplification of the original mesh.";
            }
            else if (avgReduction >= 70f)
            {
                return "Good optimization. The convex hull significantly reduces complexity while maintaining shape.";
            }
            else if (avgReduction >= 50f)
            {
                return "Moderate optimization. The convex hull provides a reasonable simplification.";
            }
            else if (avgReduction >= 30f)
            {
                return "Low optimization. The convex hull provides minimal simplification.";
            }
            else
            {
                return "Minimal optimization. The original mesh may already be close to convex or has unique features that prevent significant simplification.";
            }
        }

        void ResetStats()
        {
            _originalVertexCount = _originalTriangleCount = 0;
            _hullVertexCount = _hullTriangleCount = 0;
            _vertexReductionPercent = _triangleReductionPercent = 0;
            _showEfficiencyStats = false;
        }

        void DestroyPreview()
        {
            if (_previewObject != null)
            {
                DestroyImmediate(_previewObject);
                _previewObject = null;
            }
        }
    }
}
