using System;

namespace TelegramBotControl
{
    public class TypeInMessageState : State
    {
        public static readonly string StateName = "TypeInMessageState";
        public TypeInMessageState(string id = null)
        {
            this.Id = id ?? TypeInMessageState.StateName;
        }
        public override void Execute(StateContext context)
        {
            Console.WriteLine($"Enter the message you want to send to {context.MessageDestination} (empty string to abort)");
            string message = Console.ReadLine();

            if (string.IsNullOrEmpty(message))
            {
                context.MessageText = null;
                context.OutcomeMessageType = OutcomeMessageType.None;
                context.MessageDestination = null;
                if(context.Signals == Signals.TypeInMessageFromListening)
                {
                    context.Signals = Signals.ListenToMessages;
                }
                else if(context.Signals == Signals.TypeInMessage)
                {
                    context.Signals = Signals.Exit;
                }
            }
            context.MessageText = message;
            if (context.Signals == Signals.TypeInMessageFromListening)
            {
                context.Signals = Signals.SendMessageFromListening;
            }
            else if (context.Signals == Signals.TypeInMessage)
            {
                context.Signals = Signals.SendMessage;
            }
            base.Execute(context);
        }
    }
}

