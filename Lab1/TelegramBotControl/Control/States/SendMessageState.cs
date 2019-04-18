using System;

namespace TelegramBotControl
{
    public class SendMessageState : State
    {
        public static readonly string StateName = "SendMessageState";
        public SendMessageState(string id = null)
        {
            this.Id = id ?? SendMessageState.StateName;
        }
        public override void Execute(StateContext context)
        {
            Console.WriteLine("Sending ...");

            SentMessageStatus status = null;
            if (context.OutcomeMessageType == OutcomeMessageType.PrivateMessage)
            {
                status = context.Bot.SendMessageToUser(context.MessageDestination, context.MessageText);
            }
            else if (context.OutcomeMessageType == OutcomeMessageType.GroupMessage)
            {
                status = context.Bot.SendMessageToGroup(context.MessageDestination, context.MessageText);
            }
            else if (context.OutcomeMessageType == OutcomeMessageType.ChannelPost)
            {
                status = context.Bot.SendMessageToChannel(context.MessageDestination, context.MessageText);
            }
            context.MessageText = null;
            context.OutcomeMessageType = OutcomeMessageType.None;
            context.MessageDestination = null;
            if (status.Ok)
            {
                Console.WriteLine("Sent");
            }
            else
            {
                Console.WriteLine(status.Exception);
            }
            if(context.Signals == Signals.SendMessageFromListening)
            {
                context.Signals = Signals.ListenToMessages;
                return;
            }
            Console.ReadKey();
            context.Signals = Signals.Exit;
            base.Execute(context);
        }
    }
}

