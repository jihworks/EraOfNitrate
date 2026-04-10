// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure;
using Jih.Unity.Infrastructure.Runtime;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Jih.Unity.EraOfNitrogen
{
    public partial class Main : MonoBehaviour
    {
        static SingletonStorage<Main> _instance;
        public static Main Instance => _instance.Get();

        [SerializeField] InputSystemUIInputModule? _inputSystemUIInputModule;
        InputSystemUIInputModule InputSystemUIInputModule => _inputSystemUIInputModule.ThrowIfNull(nameof(InputSystemUIInputModule));

        InputSystem_Actions? _inputSystemActions;
        public InputSystem_Actions InputSystemActions => _inputSystemActions.ThrowIfNull(nameof(InputSystemActions));

        InputFrameStack<InputSystem_Actions>? _inputFrameStack;
        public InputFrameStack<InputSystem_Actions> InputFrameStack => _inputFrameStack.ThrowIfNull(nameof(InputFrameStack));

        CursorFrameStack? _cursorFrameStack;
        public CursorFrameStack CursorFrameStack => _cursorFrameStack.ThrowIfNull(nameof(CursorFrameStack));

        TimeFrameStack? _timeFrameStack;
        public TimeFrameStack TimeFrameStack => _timeFrameStack.ThrowIfNull(nameof(TimeFrameStack));

        StateStorage<State> _state = new(nameof(Main));
        State? CurrentState { get => _state.Current; set => _state.Current = value; }

        public Main()
        {
            _instance = new SingletonStorage<Main>(this);

            CurrentState = new Sleep(this);
        }

        void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _inputSystemActions.Enable();

            _inputSystemUIInputModule.ThrowIfNull(out InputSystemUIInputModule inputSystemUIInputModule, nameof(_inputSystemUIInputModule));
            inputSystemUIInputModule.actionsAsset = _inputSystemActions.asset;

            _inputFrameStack = new InputFrameStack<InputSystem_Actions>(_inputSystemActions, inputSystemUIInputModule);
            _cursorFrameStack = new CursorFrameStack();
            _timeFrameStack = new TimeFrameStack();
        }

        void Start()
        {
            // 프레임 기본값.
            InputFrameStack.Push(new InputFrame(this, ui: false, player: false));
            CursorFrameStack.Push(new CursorFrame(this, lockMode: CursorLockMode.None, cursorVisible: true));
            TimeFrameStack.Push(new TimeFrame(this, timeScale: 1f));
        }

        void Update()
        {
            CurrentState?.Update();
        }

        void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
    }
}
