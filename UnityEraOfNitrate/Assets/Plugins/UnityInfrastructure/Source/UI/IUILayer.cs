// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.Infrastructure.UI
{
    /// <seealso cref="UILayerStack"/>
    /// <seealso cref="BaseUILayerScript"/>
    public interface IUILayer
    {
        static readonly ListPool<Component> _componentsListPool = new(isThreadSafe: true);

        public static IEnumerable<IUILayerComponent> EnumerateLayerComponents(IUILayer layer)
        {
            CanvasGroup canvasGroup = layer.GetRootCanvasGroup();

            List<Component> buffer = _componentsListPool.Get();
            try
            {
                foreach (var item in Populate(canvasGroup.transform))
                {
                    yield return item;
                }

                IEnumerable<IUILayerComponent> Populate(Transform root)
                {
                    for (int i = 0; i < root.childCount; i++)
                    {
                        Transform child = root.GetChild(i);

                        buffer.Clear();
                        child.GetComponents(buffer);

                        for (int j = 0; j < buffer.Count; j++)
                        {
                            Component component = buffer[j];
                            if (component is IUILayerComponent layerItem)
                            {
                                yield return layerItem;
                            }
                        }

                        foreach (var item in Populate(child))
                        {
                            yield return item;
                        }
                    }
                }
            }
            finally
            {
                _componentsListPool.Release(buffer);
            }
        }

        /// <summary>
        /// This method is used to track activation state of this layer.
        /// </summary>
        /// <remarks>
        /// <b>DO NOT</b> manipulate the activation state of this layer by itself. The <see cref="UILayerStack"/> will call this method when it needs to change the activation state of this layer.
        /// </remarks>
        public bool GetLayerIsActive();
        /// <summary>
        /// This method is used to track activation state of this layer.
        /// </summary>
        /// <remarks>
        /// <b>DO NOT</b> manipulate the activation state of this layer by itself. The <see cref="UILayerStack"/> will call this method when it needs to change the activation state of this layer.
        /// </remarks>
        public void SetLayerIsActive(bool active);

        /// <summary>
        /// It requires to disable or enable the inputs.
        /// </summary>
        /// <remarks>
        /// If this layer is top, the input will be enabled.<br/>
        /// Otherwise, disabled.
        /// </remarks>
        public CanvasGroup GetRootCanvasGroup();

        /// <summary>
        /// If this layer been top, this object will get focus.<br/>
        /// If <c>null</c> returned, not ignored and pass to the engine. It may occur focus-lost.
        /// </summary>
        public GameObject? GetFocusedObject();

        public void OnAdding();
        public void OnAdded();

        public void OnActivating();
        public void OnActivated();

        public void OnDeactivating();
        public void OnDeactivated();

        public void OnObjectFocusing();
        public void OnObjectFocused(GameObject? focusedObject);

        /// <returns>Whether the action has been handled or not.</returns>
        public bool PerformAction(string id, object? args);

        public void OnAttaching();
        public void OnAttached();

        /// <summary>
        /// Called when this layer is being removed from the stack structure.
        /// </summary>
        /// <remarks>
        /// The <see cref="UILayerStack"/> does nothing in this situation. The layer should destroy or hide itself.
        /// </remarks>
        public void Detach();
    }
}
