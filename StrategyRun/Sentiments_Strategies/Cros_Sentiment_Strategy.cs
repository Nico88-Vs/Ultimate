using StrategyRun.Class_Lybrary;
using System;
using System.Diagnostics;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.Sentiments_Strategies
{
    internal class Cros_Sentiment_Strategy : ISentiment
    {
        public string Name { get; set; } = "Slwo Cross Sentiment Strategy";
        public SwitchSentiment CurrentSent { get; set; }
        public CloudSeries Series { get; set; }

        public TF.TimeFrame CurrentTF { get; set; }
        /// <summary>
        /// Trigger Event when the Cross is detected ____ 
        /// </summary>
        /// <param name="series"></param>
        public Cros_Sentiment_Strategy(CloudSeries series, TF.TimeFrame tf)
        {
            CurrentSent = new SwitchSentiment(Sentiment.Wait);
            this.Series = series;
            series.Cross += this.Series_Cross;
            CurrentTF = tf;
        }

        private void Series_Cross(object sender, CrossEvent e) => EvaluateSentiment(e);
        

        public void SetSentiment()
        {
            
        }

        private void EvaluateSentiment(CrossEvent e)
        {
            if(CurrentTF == TF.TimeFrame.Fast)
            {
                Log("Fast TF not allow", LoggingLevel.Trading);
                return;
            }
            if(CurrentTF == e.CurrentCloud.Time_F.Timeframe)
            {
                switch (e.Args)
                {
                    case EventCrosArg.Gold_slow:
                        CurrentSent.Switch(Sentiment.Buy);
                        break;
                    case EventCrosArg.Dead_slow:
                        CurrentSent.Switch(Sentiment.Sell);
                        break;
                }
            }
        }

        private void Log(string message, LoggingLevel lvl)
        {
            Core.Instance.Loggers.Log(message, lvl);
        }
    }
}
