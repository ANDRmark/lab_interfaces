using System;
using System.Collections.Generic;

namespace TelegramBotControl
{
    public class Updates
    {
        public List<IncomeMessage> IncomeMessages { get; set; } = new List<IncomeMessage>();
        public Exception Exception { get; set; }
    }
}

