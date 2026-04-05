// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Jih.Unity.Infrastructure
{
    public static class GameObjectEx
    {
        /// <summary>
        /// Enumerates child GameObjects of the given GameObject instead of child Transforms.
        /// </summary>
        public static IEnumerable<GameObject> EnumerateChildrenGameObjects(this GameObject gameObject)
        {
            return gameObject.transform.Cast<Transform>().Select(t => t.gameObject);
        }

        /// <summary>
        /// Gets the Component of type <typeparamref name="TComponent"/> from the given GameObject. If the Component not found, throws an exception.
        /// </summary>
        /// <remarks>
        /// This method does not use <see cref="Component.GetComponent{T}()"/> to prevent adding new Component to the GameObject if the Component not found.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Throws if the component not found</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TComponent GetComponentOrThrow<TComponent>(this GameObject gameObject) where TComponent : Component
        {
            if (gameObject.TryGetComponent(out TComponent? component) && component is not null)
            {
                return component;
            }
            throw new System.InvalidOperationException($"Cannot find component '{typeof(TComponent)}' from game object '{gameObject.name}'.");
        }

        /// <param name="includeRoot">Wheather include the <paramref name="root"/> to the returned collection.</param>
        public static IEnumerable<Transform> EnumerateChildrenTree(this Transform root, bool includeRoot)
        {
            if (includeRoot)
            {
                yield return root;
            }

            static IEnumerable<Transform> Populate(Transform root)
            {
                int count = root.childCount;
                for (int i = 0; i < count; i++)
                {
                    Transform child = root.GetChild(i);
                    yield return child;

                    foreach (var grandChild in Populate(child))
                    {
                        yield return grandChild;
                    }
                }
            }

            foreach (var item in Populate(root))
            {
                yield return item;
            }
        }

        /// <param name="includeRoot">Wheather include the <paramref name="root"/> to the returned collection.</param>
        public static IEnumerable<Transform> EnumerateAncestors(this Transform root, bool includeRoot)
        {
            if (includeRoot)
            {
                yield return root;
            }

            Transform? parent = root.parent;
            while (parent != null)
            {
                yield return parent;

                parent = parent.parent;
            }
        }

        /// <summary>
        /// Checks if the <see cref="GameObject.activeSelf"/> state first and sets when the state is different with given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetActiveSelfIfDiff(this GameObject gameObject, bool active)
        {
            if (gameObject.activeSelf == active)
            {
                return;
            }
            gameObject.SetActive(active);
        }
    }
}
