// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jih.Unity.Infrastructure.UI
{
    /// <summary>
    /// Manages <see cref="IUILayer"/>s with stack-structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Stack-Structure is consists with three parts:
    /// <list type="number">
    /// <item>
    /// <term>Top-Most Layer</term>
    /// <description>Always on the top of the stack-structure. Only one layer can be set as top-most layer.</description>
    /// </item>
    /// <item>
    /// <term>Layers-Stack</term>
    /// <description>Can have multiple layers with push and pop operations. The layers will be ordered in stack(LIFO).</description>
    /// </item>
    /// <item>
    /// <term>Bottom-Least Layer</term>
    /// <description>Always on the bottom of the stack-structure. Only one layer can be set as bottom-least layer.</description>
    /// </item>
    /// </list>
    /// The Current Layer(or Very-Top Layer) is determined by the top-most layer if it exists, otherwise the top layer in layers-stack if it exists, otherwise the bottom-least layer if it exists, otherwise null.<br/>
    /// The current layer will be activated, and the others will be deactivated. When pushing or popping layers, the activation and deactivation will be handled automatically.<br/>
    /// Also supports focusing object in the layer.
    /// </para>
    /// <para>
    /// This class also handles order(sibling index) of the layer's GameObjects in the <see cref="ContainerTransform"/>.<br/>
    /// This class will attach GameObject of the layer to directly under the <see cref="ContainerTransform"/>.<br/>
    /// This class getting the layer's GameObject from the <see cref="IUILayer.GetRootCanvasGroup"/>.<br/>
    /// <br/>
    /// But when the layer removed from the stack-structure, this class will call <see cref="IUILayer.Detach"/> and <b>do nothing</b>.<br/>
    /// Because if detach the GameObject, RectTransform's information will be lost. So <b>the layer should handle the detach mechanism by itself</b>.<br/>
    /// Set GameObject.activeSelf or move the GameObject to other backing Canvas may be solution.<br/>
    /// </para>
    /// <para>
    /// This class does not handle the lifecycle of the layers. You need to create and destroy the layers by yourself.<br/>
    /// Also does not handle active state of the layer's GameObject.
    /// </para>
    /// <para>
    /// <see cref="IUILayerComponent"/>s in <see cref="IUILayer"/> can receive the activation and deactivation events of the layer.
    /// </para>
    /// <para>
    /// You can get a <see cref="UnityEngine.EventSystems.EventSystem"/> by adding a Canvas to the scene. Unity will create one to the scene.
    /// </para>
    /// </remarks>
    public class UILayerStack
    {
        public Transform ContainerTransform { get; }

        /// <summary>
        /// It requires to handle focus.
        /// </summary>
        public EventSystem EventSystem { get; }

        IUILayer? topMostLayer, bottomLeastLayer;

        readonly Stack<IUILayer> layers = new();

        public UILayerStack(Transform containerTransform, EventSystem eventSystem)
        {
            ContainerTransform = containerTransform;
            EventSystem = eventSystem;
        }

        /// <summary>
        /// Set or clear the top-most layer.
        /// </summary>
        /// <param name="layer">If <c>null</c>, clear the top-most layer. The layer must be deactivated.</param>
        public void SetTopMost(IUILayer? layer)
        {
            if (topMostLayer == layer)
            {
                return;
            }
            CheckDuplication(layer);

            layer?.OnAdding();

            CheckDeactivated(layer);

            if (topMostLayer is not null)
            {
                Deactivate(topMostLayer);
                Detach(topMostLayer);

                topMostLayer = null;
            }

            if (layer is not null)
            {
                topMostLayer = layer;

                Attach(topMostLayer);
            }

            UpdateItemsActive();
            Sort();

            layer?.OnAdded();
        }

        /// <summary>
        /// Set or clear the bottom-least layer.
        /// </summary>
        /// <param name="layer">If <c>null</c>, clear the bottom-least layer. The layer must be deactivated.</param>
        public void SetBottomLeast(IUILayer? layer)
        {
            if (bottomLeastLayer == layer)
            {
                return;
            }
            CheckDuplication(layer);

            layer?.OnAdding();

            CheckDeactivated(layer);

            if (bottomLeastLayer is not null)
            {
                Deactivate(bottomLeastLayer);
                Detach(bottomLeastLayer);

                bottomLeastLayer = null;
            }

            if (layer is not null)
            {
                bottomLeastLayer = layer;

                Attach(bottomLeastLayer);
            }

            UpdateItemsActive();
            Sort();

            layer?.OnAdded();
        }

        /// <summary>
        /// Push an layer to the layers-stack.
        /// </summary>
        /// <param name="layer">Layer to push. The layer must be deactivated.</param>
        public void Push(IUILayer layer)
        {
            CheckDuplication(layer);

            layer.OnAdding();

            CheckDeactivated(layer);

            layers.Push(layer);
            Attach(layer);

            UpdateItemsActive();
            Sort();

            layer.OnAdded();
        }

        /// <summary>
        /// Pop an layer in the layers-stack.
        /// </summary>
        /// <param name="layer">Layer to pop.</param>
        /// <exception cref="InvalidOperationException">Throws if current top layer in stack is not the <paramref name="layer"/>.</exception>
        public void Pop(IUILayer layer)
        {
            if (!layers.TryPeek(out IUILayer lastLayer) || lastLayer != layer)
            {
                throw new InvalidOperationException("Popping UI invalid order.");
            }

            layers.Pop();
            Deactivate(layer);
            Detach(layer);

            UpdateItemsActive();
            Sort();
        }

        /// <summary>
        /// Pop layers until the target layer is popped from layers-stack. If the target layer is <c>null</c>, pops all layers in the layers-stack.
        /// </summary>
        /// <remarks>
        /// This method does not effect the top-most and the bottom-least layers.<br/>
        /// If the target layer is not in layers-stack, pops all layers.
        /// </remarks>
        public void PopAll(IUILayer? layer)
        {
            while (layers.TryPop(out IUILayer lastLayer))
            {
                Deactivate(lastLayer);
                Detach(lastLayer);

                if (lastLayer == layer)
                {
                    break;
                }
            }

            UpdateItemsActive();
            Sort();
        }

        /// <summary>
        /// Pop all layers until the target layer is on top in layers-stack. If the target layer is <c>null</c>, pops all layers in the layers-stack.
        /// </summary>
        /// <remarks>
        /// This method does not effect the top-most and the bottom-least layers.<br/>
        /// If the target layer is not in layers-stack, pops all layers.
        /// </remarks>
        public void PopTo(IUILayer? targetLayer)
        {
            while (layers.TryPeek(out IUILayer lastLayer))
            {
                if (lastLayer == targetLayer)
                {
                    break;
                }

                layers.Pop();
                Deactivate(lastLayer);
                Detach(lastLayer);
            }

            UpdateItemsActive();
            Sort();
        }

        /// <summary>
        /// Propagate the action to all layers.
        /// </summary>
        /// <remarks>
        /// The layers will be enumerated in stack-structure order, and the action will be handled by the first layer that returns <c>true</c>.<br/>
        /// If no layer handled the action, returns <c>false</c>.<br/>
        /// If any layer handled the action, the propagation will be stopped, and the rest layers will not receive the action.
        /// </remarks>
        /// <param name="id">User custom action ID string. It will pass to for each layers.</param>
        /// <param name="args">User custom action arguments object. It will pass to for each layers.</param>
        /// <returns>Whether any layer handled the action or not.</returns>
        public bool PerformAction(string id, object? args)
        {
            foreach (var layer in EnumerateAllLayers())
            {
                if (layer.PerformAction(id, args))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the very-top layer. If there is no layer, returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This method considers the top-most layer, the layers-stack and the bottom-least layer.
        /// </remarks>
        public IUILayer? GetCurrent()
        {
            return EnumerateAllLayers().FirstOrDefault();
        }

        /// <summary>
        /// Clear or pop layers until the target layer is being current layer.
        /// </summary>
        /// <remarks>
        /// If the target layer is <c>null</c> or not exists in stack-structure, clear all layers in the stack-structure.
        /// </remarks>
        public void MakeCurrent(IUILayer? targetLayer)
        {
            if (GetCurrent() == targetLayer)
            {
                return;
            }

            SetTopMost(null);
            if (GetCurrent() == targetLayer)
            {
                return;
            }

            PopTo(targetLayer);
            if (GetCurrent() == targetLayer)
            {
                return;
            }

            SetBottomLeast(null);
        }

        /// <summary>
        /// Enumerate all layers in stack-structure order.
        /// </summary>
        /// <remarks>
        /// The top-most layer will be enumerated first, and the bottom-least layer will be enumerated last.<br/>
        /// The layers in the layers-stack will be enumerated in the order of the stack.
        /// </remarks>
        public IEnumerable<IUILayer> EnumerateAllLayers()
        {
            if (topMostLayer is not null)
            {
                yield return topMostLayer;
            }
            foreach (var layer in layers)
            {
                yield return layer;
            }
            if (bottomLeastLayer is not null)
            {
                yield return bottomLeastLayer;
            }
        }

        void UpdateItemsActive()
        {
            bool active = true;
            foreach (var layer in EnumerateAllLayers())
            {
                if (active)
                {
                    Activate(layer);
                    active = false;
                }
                else
                {
                    Deactivate(layer);
                }
            }
        }
        void Activate(IUILayer? layer)
        {
            if (layer is null)
            {
                return;
            }
            if (layer.GetLayerIsActive())
            {
                return;
            }

            layer.OnActivating();

            foreach (var component in IUILayer.EnumerateLayerComponents(layer))
            {
                component.OnActivating();
            }

            SetInputActive(layer, true);

            foreach (var component in IUILayer.EnumerateLayerComponents(layer))
            {
                component.OnActivated();
            }

            layer.OnActivated();

            layer.OnObjectFocusing();

            GameObject? focusObject = layer.GetFocusedObject();
            EventSystem.SetSelectedGameObject(focusObject);

            layer.OnObjectFocused(focusObject);

            layer.SetLayerIsActive(true);
        }
        void Deactivate(IUILayer? layer)
        {
            if (layer is null)
            {
                return;
            }
            if (!layer.GetLayerIsActive())
            {
                return;
            }

            ForceDeactivate(layer);
        }

        void Attach(IUILayer layer)
        {
            layer.OnAttaching();

            Transform layerTransform = layer.GetRootCanvasGroup().transform;
            if (layerTransform.parent != ContainerTransform)
            {
                layerTransform.SetParent(ContainerTransform, worldPositionStays: false);
            }

            layer.OnAttached();
        }
        void Detach(IUILayer layer)
        {
            layer.Detach();
        }

        void Sort()
        {
            static void Consume(ref int i, IUILayer? layer)
            {
                if (layer is null)
                {
                    return;
                }

                int targetIndex = --i;

                Transform transform = layer.GetRootCanvasGroup().transform;
                if (transform.GetSiblingIndex() != targetIndex)
                {
                    transform.SetSiblingIndex(targetIndex);
                }
            }

            int index = layers.Count;
            if (topMostLayer is not null)
            {
                index++;
            }
            if (bottomLeastLayer is not null)
            {
                index++;
            }

            Consume(ref index, topMostLayer);
            foreach (var item in layers)
            {
                Consume(ref index, item);
            }
            Consume(ref index, bottomLeastLayer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckDuplication(IUILayer? layer)
        {
            if (layer is null)
            {
                return;
            }
            if (EnumerateAllLayers().Any(x => x == layer))
            {
                throw new InvalidOperationException("Layer already exists in stack-structure.");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckDeactivated(IUILayer? layer, [CallerMemberName] string? context = "Add")
        {
            if (layer is null)
            {
                return;
            }
            if (layer.GetLayerIsActive())
            {
                throw new InvalidOperationException($"Layer must be deactivated before '{context}'.");
            }
        }

        /// <summary>
        /// Call the deactivation steps to the layer forcibly. This is useful for initial state of the layers.
        /// </summary>
        public static void ForceDeactivate(IUILayer layer)
        {
            layer.OnDeactivating();

            foreach (var component in IUILayer.EnumerateLayerComponents(layer))
            {
                component.OnDeactivating();
            }

            SetInputActive(layer, false);

            foreach (var component in IUILayer.EnumerateLayerComponents(layer))
            {
                component.OnDeactivated();
            }

            layer.OnDeactivated();

            layer.SetLayerIsActive(false);
        }

        static void SetInputActive(IUILayer layer, bool active)
        {
            CanvasGroup canvasGroup = layer.GetRootCanvasGroup();
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
    }
}
