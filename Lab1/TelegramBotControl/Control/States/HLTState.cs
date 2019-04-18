namespace TelegramBotControl
{
    public class HLTState : State
    {
        public static readonly string StateName = "HLTState";
        public HLTState(string id = null)
        {
            this.Id = id ?? HLTState.StateName;
        }
        public override void Execute(StateContext context)
        {
            context.Signals = Signals.None;
            base.Execute(context);
        }
    }
}

