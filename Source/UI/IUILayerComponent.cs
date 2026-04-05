// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.UI
{
    public interface IUILayerComponent
    {
        public void OnActivating();
        public void OnActivated();

        public void OnDeactivating();
        public void OnDeactivated();
    }
}
