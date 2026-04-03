// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Jih.Unity.Infrastructure.Runtime
{
    /// <remarks>
    /// You have to set <see cref="InputSystemUIInputModule.actionsAsset"/> as provided to this class constructor's argument.<br/>
    /// You can get a <see cref="InputSystemUIInputModule"/> by adding a Canvas to the scene. Unity will create one to the scene.<br/>
    /// You can get a <see cref="IInputActionCollection2"/> by enabling the generated C# class of your Input Action asset. The asset is usually placed in Assets directory.<br/>
    /// And then, set the property to the generated C# class _instance manually.
    /// </remarks>
    /// <typeparam name="TInputActions">Generated C# class type of Input Action asset.</typeparam>
    public class InputFrameStack<TInputActions> where TInputActions : IInputActionCollection2
    {
        readonly Stack<InputFrame<TInputActions>> _inputFrames;

        readonly TInputActions _inputActions;
        readonly InputSystemUIInputModule _inputSystemUIInputModule;

        public InputFrameStack(TInputActions inputActions, InputSystemUIInputModule inputSystemUIInputModule)
        {
            _inputFrames = new Stack<InputFrame<TInputActions>>();

            _inputActions = inputActions;
            _inputSystemUIInputModule = inputSystemUIInputModule;
        }

        public void Push(InputFrame<TInputActions> inputFrame)
        {
            if (_inputActions is null)
            {
                throw new InvalidOperationException("Input frame pushing but input actions is null.");
            }
            if (inputFrame.Holder is null)
            {
                throw new InvalidOperationException("Frame's holder must not be a null.");
            }
            _inputFrames.Push(inputFrame);

            inputFrame.Apply(_inputActions, _inputSystemUIInputModule);
        }

        public void Update(InputFrame<TInputActions> inputFrame)
        {
            if (!_inputFrames.TryPeek(out InputFrame<TInputActions> last) || !ReferenceEquals(last.Holder, inputFrame.Holder))
            {
                throw new InvalidOperationException("Update frame must did by the pushed holder.");
            }

            _inputFrames.Pop();
            _inputFrames.Push(inputFrame);

            inputFrame.Apply(_inputActions, _inputSystemUIInputModule);
        }

        public void Pop(object holder)
        {
            if (_inputActions is null)
            {
                throw new InvalidOperationException("Input frame popping but input actions is null.");
            }
            if (!_inputFrames.TryPeek(out InputFrame<TInputActions> inputFrame) || !ReferenceEquals(inputFrame.Holder, holder))
            {
                throw new InvalidOperationException("Pop frame must did by the pushed holder.");
            }
            _inputFrames.Pop();

            if (!_inputFrames.TryPeek(out inputFrame))
            {
                return;
            }
            inputFrame.Apply(_inputActions, _inputSystemUIInputModule);
        }
    }

    /// <summary>
    /// Derived class must implement own constructor and <see cref="DoApply(TInputActions)"/> to support other Action Maps such as Player.
    /// </summary>
    /// <typeparam name="TInputActions">Generated C# class type of Input Action asset.</typeparam>
    public abstract class InputFrame<TInputActions> where TInputActions : IInputActionCollection2
    {
        public object Holder { get; }
        public bool UI { get; }

        protected InputFrame(object holder, bool ui)
        {
            Holder = holder;
            UI = ui;
        }

        public void Apply(TInputActions inputActions, InputSystemUIInputModule inputSystemUIInputModule)
        {
            // Have to set input module instead of input actions.
            // Because the input module controls enabled states of the input actions.
            inputSystemUIInputModule.enabled = UI;

            DoApply(inputActions);
        }

        protected abstract void DoApply(TInputActions inputActions);
    }
}
