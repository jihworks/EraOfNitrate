// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.Runtime
{
    public interface IState
    {
        void Begin(IState? prev);
        void End(IState? next);
    }

    public abstract class StateBase<TOwner> : IState
    {
        public TOwner Owner { get; }

        public StateBase(TOwner owner)
        {
            Owner = owner;
        }

        public virtual void Begin(IState? prev)
        {
        }
        public virtual void End(IState? next)
        {
        }

        public virtual void Update()
        {
        }
    }
}
