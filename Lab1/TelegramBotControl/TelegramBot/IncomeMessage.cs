namespace TelegramBotControl
{
    public class IncomeMessage
    {
        public string UserName { get; set; }
        public string ChannelTitle { get; set; }
        public string GroupTitle { get; set; }
        public string ChatId { get; set; }
        public IncomeMessageSource SourceType { get; set; }
        public string Text { get; set; }
    }
}

