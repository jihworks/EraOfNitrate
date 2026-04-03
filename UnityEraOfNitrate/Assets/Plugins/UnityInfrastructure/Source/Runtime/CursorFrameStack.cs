// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Runtime
{
    public class CursorFrameStack
    {
        readonly Stack<CursorFrame> _cursorFrames = new();

        public void Push(CursorFrame cursorFrame)
        {
            if (cursorFrame.Holder is null)
            {
                throw new InvalidOperationException("Frame's holder must not be a null.");
            }

            _cursorFrames.Push(cursorFrame);

            cursorFrame.Apply();
        }

        public void Update(CursorFrame cursorFrame)
        {
            if (!_cursorFrames.TryPeek(out CursorFrame last) || !ReferenceEquals(last.Holder, cursorFrame.Holder))
            {
                throw new InvalidOperationException("Update frame must did by the pushed holder.");
            }

            _cursorFrames.Pop();
            _cursorFrames.Push(cursorFrame);

            cursorFrame.Apply();
        }

        public void Pop(object holder)
        {
            if (!_cursorFrames.TryPeek(out CursorFrame cursorFrame) || !ReferenceEquals(cursorFrame.Holder, holder))
            {
                throw new InvalidOperationException("Pop frame must did by the pushed holder.");
            }
            _cursorFrames.Pop();

            if (!_cursorFrames.TryPeek(out cursorFrame))
            {
                return;
            }
            cursorFrame.Apply();
        }
    }

    public readonly struct CursorFrame
    {
        public readonly object Holder;
        public readonly CursorLockMode LockMode;
        public readonly bool CursorVisible;

        public CursorFrame(object holder, CursorLockMode lockMode, bool cursorVisible)
        {
            Holder = holder;
            LockMode = lockMode;
            CursorVisible = cursorVisible;
        }

        public readonly void Apply()
        {
            Cursor.lockState = LockMode;
            Cursor.visible = CursorVisible;
        }
    }
}
