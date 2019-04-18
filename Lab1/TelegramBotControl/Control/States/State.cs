using System;

namespace TelegramBotControl
{
    public class State
    {
        public virtual string Id { get; set; }
        public virtual bool Completed { get; set; }
        public virtual Action callback { get; set; }

        public virtual void Execute(StateContext context)
        {
            callback?.Invoke();
        }
    }
}

