namespace TelegramBotControl
{
    public class ConsoleBotControl
    {
        private FSM FSM { get; set; }
        public ConsoleBotControl(string botToken)
        {
            this.FSM = new FSM(botToken);
        }

        public void Start()
        {
            this.FSM.Reset();
            this.FSM.Execute();
        }

        public void BackupBotChats()
        {
            this.FSM.BackupBotChats();
        }
    }
}

