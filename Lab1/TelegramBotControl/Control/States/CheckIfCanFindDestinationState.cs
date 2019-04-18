using System;

namespace TelegramBotControl
{
    public class CheckIfCanFindDestinationState : State
    {
        public static readonly string StateName = "CheckIfCanFindDestinationState";
        public CheckIfCanFindDestinationState(string id = null)
        {
            this.Id = id ?? CheckIfCanFindDestinationState.StateName;
        }
        public override void Execute(StateContext context)
        {
            bool exists = context.Bot.DestinationExists(context.OutcomeMessageType, context.MessageDestination);
            if (exists)
            {
                context.Signals = Signals.TypeInMessage;
            }
            else
            {
                Console.WriteLine($"Can not write to {context.MessageDestination}. It is not exists or Bot did not receive any message from this user/group/channel.");
                Console.ReadKey();
                context.Signals = Signals.Exit;
            }
            base.Execute(context);
        }
    }
}

