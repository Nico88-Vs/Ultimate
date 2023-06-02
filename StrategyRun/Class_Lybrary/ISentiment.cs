using TheIndicator.Enum;

namespace StrategyRun.Class_Lybrary
{
    public interface ISentiment
    {
        public string Name { get; set; }
        public SwitchSentiment CurrentSent { get; set; }
        public void SetSentiment();
    }
}
