using System;
using System.Collections.Generic;

namespace TelegramBotControl
{
    public class FSM
    {
        string startStateId { get { return MainMenuState.StateName; } }
        string endStateId { get { return HLTState.StateName; } }
        Dictionary<string, State> States { get; set; }
        State CurrentState { get; set; }
        TransitionTable transitionTable { get; set; }
        StateContext context { get; set; }

        private static Dictionary<string, Func<State>> AllRegisteredStates { get; set; } = new Dictionary<string, Func<State>>();
        private static void RegisterState(string stateId, Func<State> stateCreator)
        {
            FSM.AllRegisteredStates[stateId] = stateCreator;
        }
        static void RegisterAllStates()
        {
            FSM.RegisterState(CheckIfCanFindDestinationState.StateName, () => new CheckIfCanFindDestinationState());
            FSM.RegisterState(HLTState.StateName, () => new HLTState());
            FSM.RegisterState(ListenState.StateName, () => new ListenState());
            FSM.RegisterState(MainMenuState.StateName, () => new MainMenuState());
            FSM.RegisterState(ResponseToLastChatState.StateName, () => new ResponseToLastChatState());
            FSM.RegisterState(SendMessageState.StateName, () => new SendMessageState());
            FSM.RegisterState(TypeInMessageState.StateName, () => new TypeInMessageState());
            FSM.RegisterState(WriteToState.StateName, () => new WriteToState());
        }
        static FSM()
        {
            RegisterAllStates();
        }

        public FSM(string botToken)
        {
            this.States = new Dictionary<string, State>();
            foreach (var stateCreator in FSM.AllRegisteredStates.Values)
            {
                State s = stateCreator();
                this.States[s.Id] = s;
            }
            this.transitionTable = new TransitionTable();

            this.transitionTable.AddTransition(CheckIfCanFindDestinationState.StateName, TypeInMessageState.StateName, s => s == Signals.TypeInMessage);
            this.transitionTable.AddTransition(CheckIfCanFindDestinationState.StateName, MainMenuState.StateName, s => s == Signals.Exit);

            this.transitionTable.AddTransition(HLTState.StateName, MainMenuState.StateName, s => true);

            this.transitionTable.AddTransition(ListenState.StateName, ResponseToLastChatState.StateName, s => s == Signals.ResponseLastChatFromListening);
            this.transitionTable.AddTransition(ListenState.StateName, MainMenuState.StateName, s => s == Signals.Exit);

            this.transitionTable.AddTransition(MainMenuState.StateName, ListenState.StateName, s => s == Signals.ListenToMessages);
            this.transitionTable.AddTransition(MainMenuState.StateName, HLTState.StateName, s => s == Signals.Exit);
            this.transitionTable.AddTransition(MainMenuState.StateName, ResponseToLastChatState.StateName, s => s == Signals.ResponseLastChat);
            this.transitionTable.AddTransition(MainMenuState.StateName, WriteToState.StateName, s => s == Signals.WriteTo);

            this.transitionTable.AddTransition(ResponseToLastChatState.StateName, TypeInMessageState.StateName, s => s == Signals.TypeInMessage || s == Signals.TypeInMessageFromListening);
            this.transitionTable.AddTransition(ResponseToLastChatState.StateName, MainMenuState.StateName, s => s == Signals.Exit);
            this.transitionTable.AddTransition(ResponseToLastChatState.StateName, ListenState.StateName, s => s == Signals.ListenToMessages);

            this.transitionTable.AddTransition(SendMessageState.StateName, ListenState.StateName, s => s == Signals.ListenToMessages);
            this.transitionTable.AddTransition(SendMessageState.StateName, MainMenuState.StateName, s => s == Signals.Exit);

            this.transitionTable.AddTransition(TypeInMessageState.StateName, SendMessageState.StateName, s => s == Signals.SendMessage || s == Signals.SendMessageFromListening);
            this.transitionTable.AddTransition(TypeInMessageState.StateName, ListenState.StateName, s => s == Signals.ListenToMessages);
            this.transitionTable.AddTransition(TypeInMessageState.StateName, MainMenuState.StateName, s => s == Signals.Exit);

            this.transitionTable.AddTransition(WriteToState.StateName, CheckIfCanFindDestinationState.StateName, s => s == Signals.CheckIfCanFindDestination);
            this.transitionTable.AddTransition(WriteToState.StateName, MainMenuState.StateName, s => s == Signals.Exit);

            this.Reset();

            this.context = new StateContext();
            this.context.Bot = new TelegramBot(botToken, "chatIDs.json");
            this.context.Signals = Signals.None;
        }

        public void Reset()
        {
            this.CurrentState = this.States[startStateId];
        }

        public void Execute()
        {
            while (this.CurrentState.Id != this.endStateId)
            {
                this.CurrentState.Execute(context);
                this.TransitToNextState();
            }
        }

        public void BackupBotChats()
        {
            this.context.Bot.BackupChatIDs();
        }
        private void TransitToNextState()
        {
            string nextStateId = this.transitionTable.GetNextState(this.CurrentState.Id, this.context.Signals);
            this.CurrentState = this.States[nextStateId];
        }
    }
}

