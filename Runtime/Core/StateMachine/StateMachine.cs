using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class StateMachine
    {
        private State _currentState;
        
        private State _previousState;

        public State CurrentState => _currentState;
        
        public State PreviousState => _previousState;
    
        public void Initialize(State startState)
        {
            _previousState = _currentState = startState;
            _currentState.Enter();
        }

        public void ChangeState(State newState)
        {
            if (_currentState == newState)
            {
                return;
            }
        
            _currentState?.Exit();
            _previousState = _currentState;
            _currentState = newState;
            _currentState.Enter();
        }

        public void Update(float deltaTime)
        {
            _currentState?.LogicUpdate(deltaTime);
        }
    
        public void FixedUpdate(float fixedDeltaTime)
        {
            _currentState?.PhysicsUpdate(fixedDeltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            _currentState?.LateLogicUpdate(deltaTime);
        }
    }
}


