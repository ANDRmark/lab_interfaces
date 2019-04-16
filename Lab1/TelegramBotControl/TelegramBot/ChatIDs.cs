using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotControl
{
    public class ChatIDs
    {
        public Dictionary<string, string> privateChatIdsByUsername { get; set; }
        public Dictionary<string, string> groupChatIdsByGroupName { get; set; }
        public Dictionary<string, string> channelChatIdsByChannelName { get; set; }
        public ChatIDs()
        {
            this.privateChatIdsByUsername = new Dictionary<string, string>();
            this.groupChatIdsByGroupName = new Dictionary<string, string>();
            this.channelChatIdsByChannelName = new Dictionary<string, string>();
        }
    }
}
