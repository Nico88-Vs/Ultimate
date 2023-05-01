using StrategyRun.Class_Lybrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;

namespace StrategyRun.Strategie
{
    public class Cloud_In_Medie_Sentiment : ISentiment
    {
        public string Name { get; set; } = "Get Sentiment By Cloud Position";
        public SwitchSentiment CurrentSent { get; set; }

        private CloudSeries serie;

        public Cloud_In_Medie_Sentiment(CloudSeries series)
        {
            this.serie = series;
        }

        public void SetSentiment()
        {
            switch (serie.Scenario)
            {
                case IchimokuCloudScenario.STRONG_BULLISH:
                    this.CurrentSent.Switch(Sentiment.Sell);
                    break;
                case IchimokuCloudScenario.STRONG_BEARISH:
                    this.CurrentSent.Switch(Sentiment.Buy);
                    break;
                case IchimokuCloudScenario.MODERATELY_BULLISH:
                    this.CurrentSent.Switch(Sentiment.Buy);
                    break;
                case IchimokuCloudScenario.MODERATELY_BEARISH:
                    this.CurrentSent.Switch(Sentiment.Sell);
                    break;
                case IchimokuCloudScenario.CONSOLIDATION_BULLISH:
                    this.CurrentSent.Switch(Sentiment.Sell);
                    break;
                case IchimokuCloudScenario.CONSOLIDATION_BEARISH:
                    this.CurrentSent.Switch(Sentiment.Buy);
                    break;
                case IchimokuCloudScenario.UNDEFINED:
                    this.CurrentSent.Switch(Sentiment.Wait);
                    break;
                default:
                    this.CurrentSent.Switch(Sentiment.Wait);
                    break;
            }
        }
    }
}
