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
    public class TimeFrameStack
    {
        readonly Stack<TimeFrame> _timeFrames = new();

        readonly float _defaultFixedDeltaTime;

        /// <param name="defaultFixedDeltaTime">Check Fixed Timestep value of Time category in project settings.</param>
        public TimeFrameStack(float defaultFixedDeltaTime = 0.02f)
        {
            _defaultFixedDeltaTime = defaultFixedDeltaTime;
        }

        public void Push(TimeFrame timeFrame)
        {
            if (timeFrame.Holder is null)
            {
                throw new InvalidOperationException("Frame's holder must not be a null.");
            }

            _timeFrames.Push(timeFrame);

            timeFrame.Apply(_defaultFixedDeltaTime);
        }

        public void Update(TimeFrame timeFrame)
        {
            if (!_timeFrames.TryPeek(out TimeFrame last) || !ReferenceEquals(last.Holder, timeFrame.Holder))
            {
                throw new InvalidOperationException("Update frame must did by the pushed holder.");
            }

            _timeFrames.Pop();
            _timeFrames.Push(timeFrame);

            timeFrame.Apply(_defaultFixedDeltaTime);
        }

        public void Pop(object holder)
        {
            if (!_timeFrames.TryPeek(out TimeFrame timeFrame) || !ReferenceEquals(timeFrame.Holder, holder))
            {
                throw new InvalidOperationException("Pop frame must did by the pushed holder.");
            }
            _timeFrames.Pop();

            if (!_timeFrames.TryPeek(out timeFrame))
            {
                return;
            }
            timeFrame.Apply(_defaultFixedDeltaTime);
        }
    }

    public readonly struct TimeFrame
    {
        public readonly object Holder;
        public readonly float TimeScale;

        public TimeFrame(object holder, float timeScale)
        {
            Holder = holder;
            TimeScale = timeScale;
        }

        public readonly void Apply(float defaultFixedDeltaTime)
        {
            Time.timeScale = TimeScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * TimeScale;
        }
    }
}
