using System;

namespace TelegramBotControl
{
    public class MainMenuState : State
    {
        public static readonly string StateName = "MainMenuState";
        public MainMenuState(string id = null)
        {
            this.Id = id ?? MainMenuState.StateName;
        }
        public override void Execute(StateContext context)
        {
            Console.Clear();
            Console.WriteLine("Main menu");
            Console.WriteLine("Choose one option :");
            Console.WriteLine("1 : Listen to messages");
            Console.WriteLine("2 : Write new message");
            Console.WriteLine("3 : Answer to last chat");
            var key = Console.ReadKey();

            if (key.KeyChar == '1') context.Signals = Signals.ListenToMessages;
            else
            if (key.KeyChar == '2') context.Signals = Signals.WriteTo;
            else
            if (key.KeyChar == '3') context.Signals = Signals.ResponseLastChat;
            else
                context.Signals = Signals.Exit;
            Console.Clear();
            base.Execute(context);
        }
    }
}

