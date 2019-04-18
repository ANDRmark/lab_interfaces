using System;
using System.Collections.Generic;

namespace TelegramBotControl
{
    public class TransitionTable
    {
        Dictionary<string, Dictionary<string, Func<Signals, bool>>> table { get; set; }
        public TransitionTable()
        {
            this.table = new Dictionary<string, Dictionary<string, Func<Signals, bool>>>();
        }
        public void AddTransition(string currentState, string nextState, Func<Signals, bool> condition)
        {
            if (!this.table.ContainsKey(currentState))
            {
                this.table[currentState] = new Dictionary<string, Func<Signals, bool>>();
            }
            this.table[currentState][nextState] = condition;
        }
        public string GetNextState(string currentState, Signals currentSignals)
        {
            if (this.table.ContainsKey(currentState))
            {
                var transitionsForCurrentState = this.table[currentState];
                if (transitionsForCurrentState != null)
                {
                    foreach (var possibleState in transitionsForCurrentState.Keys)
                    {
                        if (transitionsForCurrentState[possibleState](currentSignals))
                        {
                            return possibleState;
                        }
                    }
                }
            }
            throw new Exception($"Transition for {currentState} not defined when signals {currentSignals}.");
        }
    }
}

