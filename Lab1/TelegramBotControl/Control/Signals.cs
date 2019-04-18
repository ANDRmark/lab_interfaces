using System;

namespace TelegramBotControl
{
    [Flags]
    public enum Signals
    {
        None = 0,
        ListenToMessages = 1 << 0,
        WriteTo = 1 << 1,
        ResponseLastChat = 1 << 2,
        Exit = 1 << 3,
        CheckIfCanFindDestination = 1 << 4,
        TypeInMessage = 1 << 5,
        SendMessage = 1 << 6,
        SendMessageFromListening = 1 << 7,
        ResponseLastChatFromListening = 1 << 8,
        TypeInMessageFromListening = 1 << 9,
    }
}

