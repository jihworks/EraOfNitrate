// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.Unity.Infrastructure.UI
{
    /// <remarks>
    /// Add this script to a GameObject to make it a <see cref="IUILayer"/>.<br/>
    /// The GameObject must have a <see cref="CanvasGroup"/> component.<br/>
    /// It will be the RootCanvasGroup of this layer to handle input or interaction states.
    /// </remarks>
    /// <seealso cref="UILayerStack"/>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseUILayerScript : MonoBehaviour, IUILayer
    {
        /// <summary>
        /// Whether this layer is active. It will be set by <see cref="UILayerStack"/>.
        /// </summary>
        /// <remarks>
        /// Do not set this value directly if don't know what effects will occur.
        /// </remarks>
        /// <seealso cref="Initialize"/>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Whether this layer is initialized. <c>true</c> means that the <see cref="Initialize"/> had been called.
        /// </summary>
        public bool IsInitialized { get; private set; }

        CanvasGroup? _rootCanvasGroup;
        public CanvasGroup RootCanvasGroup
        {
            get
            {
                if (_rootCanvasGroup == null)
                {
                    _rootCanvasGroup = GetComponent<CanvasGroup>();
                }
                return _rootCanvasGroup;
            }
        }

        bool IUILayer.GetLayerIsActive()
        {
            return IsActive;
        }
        void IUILayer.SetLayerIsActive(bool active)
        {
            IsActive = active;
        }

        CanvasGroup IUILayer.GetRootCanvasGroup()
        {
            return RootCanvasGroup;
        }

        void Initialize_Internal()
        {
            if (IsInitialized)
            {
                return;
            }

            Initialize();

            IsInitialized = true;
        }

        /// <summary>
        /// Called when this layer is added before to the <see cref="UILayerStack"/> at first time once.
        /// </summary>
        /// <remarks>
        /// Default implementation will <see cref="UILayerStack.ForceDeactivate(IUILayer)"/> this layer.<br/>
        /// Becuase the <see cref="UILayerStack"/> expects the layer had been deactivated when first time added.<br/>
        /// Derived class can override this method to change the default behavior and set initial <see cref="IsActive"/> value when first time added."
        /// </remarks>
        protected virtual void Initialize()
        {
            UILayerStack.ForceDeactivate(this);
        }

        void IUILayer.OnAdding()
        {
            Initialize_Internal();
            OnAdding();
        }
        void IUILayer.OnAdded()
        {
            OnAdded();
        }

        void IUILayer.OnActivating()
        {
            OnActivating();
        }
        void IUILayer.OnActivated()
        {
            OnActivated();
        }

        void IUILayer.OnAttaching()
        {
            OnAttaching();
        }

        void IUILayer.OnAttached()
        {
            OnAttached();
        }

        void IUILayer.OnDeactivating()
        {
            OnDeactivating();
        }
        void IUILayer.OnDeactivated()
        {
            OnDeactivated();
        }

        void IUILayer.Detach()
        {
            Detach();
        }

        void IUILayer.OnObjectFocusing()
        {
            OnObjectFocusing();
        }
        void IUILayer.OnObjectFocused(GameObject? focusedObject)
        {
            OnObjectFocused(focusedObject);
        }

        bool IUILayer.PerformAction(string id, object? args)
        {
            return PerformAction(id, args);
        }

        protected virtual void OnAdding()
        {
        }
        protected virtual void OnAdded()
        {
        }

        protected virtual void OnActivating()
        {
        }
        protected virtual void OnActivated()
        {
        }

        protected virtual void OnAttaching()
        {
        }
        protected virtual void OnAttached()
        {
        }

        protected virtual void OnDeactivating()
        {
        }
        protected virtual void OnDeactivated()
        {
        }

        protected virtual void OnObjectFocusing()
        {
        }
        protected virtual void OnObjectFocused(GameObject? focusedObject)
        {
        }

        protected virtual bool PerformAction(string id, object? args)
        {
            return false;
        }

        protected abstract void Detach();

        public abstract GameObject? GetFocusedObject();
    }
}
