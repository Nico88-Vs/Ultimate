using StrategyRun.Class_Lybrary;
using StrategyRun.Sentiments_Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.Strategie
{
    internal class Test_Cross_Strategy : ITradeTicket
    {
        public double TotalAmmount { get; set; }
        public string CondictionName { get; set; } = "Easy_Cross_Test";
        public CloudSeries Series { get; set; }

        public event EventHandler<TradeTiket> TradeTicketCreated;
        public TF.TimeFrame CurrentTF { get; set; }

        private Sentiment currentSent = Sentiment.Wait;
        private  Cros_Sentiment_Strategy currentSC { get; set; }

        /// <summary>
        /// Generate A Trade Ticket when the Cross is detected ____ 
        /// </summary>
        /// <param name="tot"></param>
        /// <param name="currentSC"></param>
        public Test_Cross_Strategy (Cros_Sentiment_Strategy currentSC)
        {
            this.currentSC = currentSC;
            this.Series = currentSC.Series;
            this.CurrentTF = currentSC.CurrentTF;

            currentSC.CurrentSent.SentimentChanged += this.CurrentSent_SentimentChanged;
        }


        /// <summary>
        /// Ricevo il cross da Sentiment.obj e lo trasformo in un TradeTicket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentSent_SentimentChanged(object sender, SwitchSentiment e)
        {
            TradeTiket tk = GetTiketDetails();

            switch (CurrentTF)
            {
                case TF.TimeFrame.Fast:
                    Log("Invalid TF", LoggingLevel.Trading);
                    break;

                case TF.TimeFrame.Mid:
                    tk.TFrame = Series.MidTF;
                    break;
                case TF.TimeFrame.Slow:
                    tk.TFrame = Series.SlowTF;
                    break;
            }

            tk.TradeSentiment = e.NewSentiment;

            this.OnTicketCreated(tk);
        }

        public void Update()
        {
            Log("Update Not Needed", LoggingLevel.Trading);
        }

        /// <summary>
        /// Invio il TradeTicket al PositionManager
        /// </summary>
        /// <param name="t"></param>
        public virtual void OnTicketCreated(TradeTiket t)
        {
            TradeTicketCreated?.Invoke(this, t);
        }

        private TradeTiket GetTiketDetails()
        {
            TradeTiket output = new TradeTiket();
            int prev_cloudID = 0;


            switch (CurrentTF)
            {
                case TF.TimeFrame.Fast:
                    Log("Invalid TF", LoggingLevel.Trading);
                    break;

                case TF.TimeFrame.Mid:
                    output.ClaudId = Series.CurrentMidCloud.Id;
                    prev_cloudID = GetPrev_CloudID(output.ClaudId);
                    output.Dyctionary = GetDictio(Series.MidCloudDictionary, prev_cloudID);
                    break;

                case TF.TimeFrame.Slow:
                    output.ClaudId = Series.CurrentSlowCloud.Id;
                    prev_cloudID = GetPrev_CloudID(output.ClaudId);
                    output.Dyctionary = GetDictio(Series.MidInSlowDictionary, prev_cloudID);
                    break;
            }

            return output;
        }

        private Dictionary<Cloud, List<Cloud>> GetDictio(Dictionary<Cloud, List<Cloud>> dictio, int prev_cloudID)
        {
            Dictionary<Cloud, List<Cloud>> output = new Dictionary<Cloud, List<Cloud>>();

            foreach (KeyValuePair<Cloud,List<Cloud>> item in dictio)
            {
                if (item.Key.Id >= prev_cloudID)
                    output.Add(item.Key, item.Value);
            }

            return output;
        }

        private int GetPrev_CloudID(int cloudID)
        {
            int output = 0;
            switch (CurrentTF)
            {
                case TF.TimeFrame.Fast:
                    Log("Invalid TF", LoggingLevel.Trading);
                    break;
                case TF.TimeFrame.Mid:
                    output = Series.CloudsMid.Last(x => x.Id == cloudID - 2).Id;
                    break;
                case TF.TimeFrame.Slow:
                    output = Series.CloudSlow.Last(x => x.Id == cloudID -2).Id;
                    break;
            }

            if(output == 0)
                Log("Invalid CloudID", LoggingLevel.Trading);

            return output;
        }

        private void Log(string message ,LoggingLevel lvl)
        {
            Core.Instance.Loggers.Log(message, lvl);
        }
    }
}
