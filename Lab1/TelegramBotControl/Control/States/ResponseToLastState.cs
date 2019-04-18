using System;

namespace TelegramBotControl
{
    public class ResponseToLastChatState : State
    {
        public static readonly string StateName = "ResponseToLastChatState";
        public ResponseToLastChatState(string id = null)
        {
            this.Id = id ?? ResponseToLastChatState.StateName;
        }
        public override void Execute(StateContext context)
        {
            PrepareDestinationAndOutcomeType(context);

            if(context.OutcomeMessageType == OutcomeMessageType.None || string.IsNullOrEmpty(context.MessageDestination))
            {
                Console.WriteLine("Can not find message destination user/group/channel.");
                if(context.Signals == Signals.ResponseLastChat)
                {
                    context.Signals = Signals.Exit;
                    Console.ReadKey();
                }
                if (context.Signals == Signals.ResponseLastChatFromListening)
                {
                    context.Signals = Signals.ListenToMessages;
                }
                return;
            }

            if (context.Signals == Signals.ResponseLastChat)
            {
                context.Signals = Signals.TypeInMessage;
            }
            if (context.Signals == Signals.ResponseLastChatFromListening)
            {
                context.Signals = Signals.TypeInMessageFromListening;
            }
            base.Execute(context);
        }

        private void PrepareDestinationAndOutcomeType(StateContext context)
        {
            context.MessageDestination = null;
            context.OutcomeMessageType = OutcomeMessageType.None;
            if (context.LastMessage == null) return;

            if (context.LastMessage.SourceType == IncomeMessageSource.PrivateMessage)
            {
                context.MessageDestination = context.LastMessage.UserName;
                context.OutcomeMessageType = OutcomeMessageType.PrivateMessage;
            }
            else if (context.LastMessage.SourceType == IncomeMessageSource.GroupMessage)
            {
                context.MessageDestination = context.LastMessage.GroupTitle;
                context.OutcomeMessageType = OutcomeMessageType.GroupMessage;
            }
            else if (context.LastMessage.SourceType == IncomeMessageSource.ChannelPost)
            {
                context.MessageDestination = context.LastMessage.ChannelTitle;
                context.OutcomeMessageType = OutcomeMessageType.ChannelPost;
            }
        }
    }
}

