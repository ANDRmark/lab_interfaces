using System;

namespace TelegramBotControl
{
    public class WriteToState : State
    {
        public static readonly string StateName = "WriteToState";
        public WriteToState(string id = null)
        {
            this.Id = id ?? WriteToState.StateName;
        }
        public override void Execute(StateContext context)
        {
            var destType = AskDestinationType();
            var destination = AskDestination(destType);

            if (string.IsNullOrEmpty(destination))
            {
                context.OutcomeMessageType = OutcomeMessageType.None;
                context.MessageDestination = null;
                context.Signals = Signals.Exit;
                return;
            }
            context.OutcomeMessageType = destType;
            context.MessageDestination = destination;
            context.Signals = Signals.CheckIfCanFindDestination;
            base.Execute(context);
        }
        private OutcomeMessageType AskDestinationType()
        {
            Console.Clear();
            Console.WriteLine("Send Message menu");
            Console.WriteLine("Choose one option :");
            Console.WriteLine("1 : Send message to user");
            Console.WriteLine("2 : Send message to group");
            Console.WriteLine("3 : Send message to channel");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.KeyChar == '1')
            {
                return OutcomeMessageType.PrivateMessage;
            }
            if (key.KeyChar == '2')
            {
                return OutcomeMessageType.GroupMessage;
            }
            if (key.KeyChar == '3')
            {
                return OutcomeMessageType.ChannelPost;
            }
            return OutcomeMessageType.None;
        }

        private string AskDestination(OutcomeMessageType destinationType)
        {
            Console.Clear();
            if (destinationType == OutcomeMessageType.None) return null;

            if (destinationType == OutcomeMessageType.PrivateMessage)
            {
                Console.WriteLine("Enter user name");
            }
            if (destinationType == OutcomeMessageType.GroupMessage)
            {
                Console.WriteLine("Enter group name");
            }
            if (destinationType == OutcomeMessageType.ChannelPost)
            {
                Console.WriteLine("Enter channel name");
            }
            return Console.ReadLine();
        }
    }
}

