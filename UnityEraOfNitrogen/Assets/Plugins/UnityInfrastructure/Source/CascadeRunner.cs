// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;

namespace Jih.Unity.Infrastructure
{
    public class CascadeRunner
    {
        public IReadOnlyList<CascadeRun>? Runs
        {
            get;
#if INFRASTRUCTURE_USE_EXTERNAL_INIT || NET5_0_OR_GREATER
            init;
#endif
        }

        public bool IsRunning { get; set; }

        public int CurrentIndex { get; private set; } = 0;
        public float CurrentTime { get; private set; }

        public CascadeRunner()
        {
        }
        public CascadeRunner(params CascadeRun[] runs)
        {
            Runs = runs;
        }
        public CascadeRunner(IReadOnlyList<CascadeRun> runs)
        {
            Runs = runs;
        }

        public void Start()
        {
            IsRunning = true;
        }
        public void Pause()
        {
            IsRunning = false;
        }

        public void Stop()
        {
            Reset();
            Pause();
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        public void Reset()
        {
            CurrentIndex = 0;
            CurrentTime = 0f;
        }

        public void Flush()
        {
            if (!IsRunning)
            {
                return;
            }

            if (Runs is not null)
            {
                for (int i = CurrentIndex; i < Runs.Count; i++)
                {
                    Runs[i].Action();
                }
            }

            Stop();
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning)
            {
                return;
            }

            if (Runs is null || !Runs.TryGetValueAt(CurrentIndex, out CascadeRun curr))
            {
                Stop();
                return;
            }
            if (CurrentTime >= curr.Delay)
            {
                curr.Action();

                if (!Runs.IsValidIndex(++CurrentIndex))
                {
                    Stop();
                    return;
                }

                CurrentTime -= curr.Delay;
            }
            CurrentTime += deltaTime;
        }
    }

    public readonly struct CascadeRun
    {
        public readonly float Delay;
        public readonly Action Action;

        public CascadeRun(float delay, Action action)
        {
            Delay = delay;
            Action = action;
        }
    }
}
