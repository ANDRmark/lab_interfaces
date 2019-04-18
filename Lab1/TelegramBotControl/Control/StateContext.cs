namespace TelegramBotControl
{
    public class StateContext
    {
        public Signals Signals { get; set; }
        public TelegramBot Bot { get; set; }
        public IncomeMessage LastMessage { get; set; }
        public OutcomeMessageType OutcomeMessageType { get; set; }
        public string MessageDestination { get; set; }
        public string MessageText { get; set; }
    }
}

