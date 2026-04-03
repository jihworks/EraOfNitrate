// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Runtime;

namespace Jih.Unity.EraOfNitrate
{
    public class InputFrame : InputFrame<InputSystem_Actions>
    {
        public bool Player { get; }

        public InputFrame(object holder, bool ui, bool player) : base(holder, ui)
        {
            Player = player;
        }

        protected override void DoApply(InputSystem_Actions inputActions)
        {
            if (Player)
            {
                inputActions.Player.Enable();
            }
            else
            {
                inputActions.Player.Disable();
            }
        }
    }
}
