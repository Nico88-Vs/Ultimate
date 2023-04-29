using PlaceOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TheIndicator.Enum;
using TheIndicator.Interfacce;
using StrategyRun.Strategie;
using StrategyRun.Class_Lybrary;

namespace StrategyRun.Headg_Manager
{
    public enum TypeOfPosition
    {
        Main,
        Cover
    }

    public class HeadgeManager
    {
        private enum Status { WaitingCover, WaitingMain, FullFilled }
        public Covers Covers { get; set; }
        public MainTrades MainTrades { get; set; }
        public SwitchSentiment SentimentObj { get; set; }

        private Account account;
        private Symbol symbol;
        private CloudSeries Series;
        private Condic_Gap_Cros_Strategy_V1 condiction;

        private const string MainComment = "Main";
        private const string CoverComment = "Cover";


        public HeadgeManager(Account account, Symbol symbol, Condic_Gap_Cros_Strategy_V1 cond, SwitchSentiment sentObj)
        {
            this.condiction = cond;
            this.Series = cond.Series;
            this.account = account; 
            this.symbol = symbol;
            this.MainTrades = new MainTrades(0, "test");
            this.Covers = new Covers(this.MainTrades);
            this.SentimentObj = sentObj;

            this.SentimentObj.SentimentChanged += this.Sentiment_SentimentChanged; 
        }

        private void Sentiment_SentimentChanged(object sender, SwitchSentiment e)
        {
            if(e.NewSentiment == Sentiment.Wait)
            {
                Covers.Sent = Sentiment.Wait;
                MainTrades.Sent = Sentiment.Wait;
            }

            if(e.NewSentiment == Sentiment.Buy)
            {
                Covers.Sent = Sentiment.Sell;
                MainTrades.Sent = Sentiment.Buy;
            }

            if(e.NewSentiment == Sentiment.Sell)
            {
                Covers.Sent = Sentiment.Buy;
                MainTrades.Sent = Sentiment.Sell;
            }
        }

        private void GetPosition()
        {

        }
       
    }
}
