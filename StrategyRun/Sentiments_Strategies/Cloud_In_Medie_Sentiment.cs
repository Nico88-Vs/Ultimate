using StrategyRun.Class_Lybrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;

namespace StrategyRun.Sentiments_Strategies
{
    public class Cloud_In_Medie_Sentiment : ISentiment
    {
        public string Name { get; set; } = "Get Sentiment By Cloud Position";
        public SwitchSentiment CurrentSent { get; set; }

        private CloudSeries serie;

        public Cloud_In_Medie_Sentiment(CloudSeries series)
        {
            serie = series;
            CurrentSent = new SwitchSentiment(Sentiment.Wait);
        }

        public void SetSentiment()
        {
            switch (serie.Scenario)
            {
                case IchimokuCloudScenario.STRONG_BULLISH:
                    CurrentSent.Switch(Sentiment.Sell);
                    break;
                case IchimokuCloudScenario.STRONG_BEARISH:
                    CurrentSent.Switch(Sentiment.Buy);
                    break;
                case IchimokuCloudScenario.MODERATELY_BULLISH:
                    CurrentSent.Switch(Sentiment.Buy);
                    break;
                case IchimokuCloudScenario.MODERATELY_BEARISH:
                    CurrentSent.Switch(Sentiment.Sell);
                    break;
                case IchimokuCloudScenario.CONSOLIDATION_BULLISH:
                    CurrentSent.Switch(Sentiment.Sell);
                    break;
                case IchimokuCloudScenario.CONSOLIDATION_BEARISH:
                    CurrentSent.Switch(Sentiment.Buy);
                    break;
                case IchimokuCloudScenario.UNDEFINED:
                    CurrentSent.Switch(Sentiment.Wait);
                    break;
                default:
                    CurrentSent.Switch(Sentiment.Wait);
                    break;
            }
        }
    }
}
