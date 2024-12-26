using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class State {
    // any shared state properties can be added here
}
public class StateMachine {
    public List<State> states = new List<State>();
    public State CurrentState { get; private set; }

    private Dictionary<(State, State), Func<bool>> _transitions = new Dictionary<(State, State), Func<bool>>();

    private Dictionary<(State, State), Action> _callbacks = new Dictionary<(State, State), Action>();


    public void AddTransition(State from, State to, Func<bool> condition, Action callback = null) {

        if (!states.Contains(from)) {
            states.Add(from);
        }
        if (!states.Contains(to)) {
            states.Add(to);
        }

        _transitions.Add((from, to), condition);
        _callbacks.Add((from, to), callback);
    }
    public void SetState(State state) {
        CurrentState = state;
    }

    public void Update() {

        foreach (var transition in _transitions) {
            if (transition.Key.Item1 != CurrentState)
                continue;
            if (transition.Value()) {
                var callback = _callbacks[transition.Key];
                if (callback != null)
                    callback.Invoke();
                CurrentState = transition.Key.Item2;
                return;
            }
        }
    }
}