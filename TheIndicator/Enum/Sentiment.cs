using System;

namespace TheIndicator.Enum
{
    public enum Sentiment
    {
        Buy, Sell, Wait
    }

    public class SwitchSentiment : EventArgs
    {
        public event EventHandler<SwitchSentiment> SentimentChanged;
        public Sentiment NewSentiment { get; set; }
        private Sentiment OldSentimennt = Sentiment.Wait;

        public SwitchSentiment(Sentiment newSentiment)
        {
            this.NewSentiment = newSentiment;

            if(this.NewSentiment != this.OldSentimennt)
            {
                OnSentimentChanged(this);
            }
        }

        public virtual void OnSentimentChanged(SwitchSentiment e)
        {
            SentimentChanged?.Invoke(this, e);
            this.OldSentimennt = this.NewSentiment;
        }

        public void Switch(Sentiment newSentiment)
        {
            this.NewSentiment = newSentiment;
            if (this.NewSentiment != this.OldSentimennt)
                this.OnSentimentChanged(this);
        }
    }
}
