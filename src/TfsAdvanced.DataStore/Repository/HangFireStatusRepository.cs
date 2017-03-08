namespace TfsAdvanced.DataStore.Repository
{
    public class HangFireStatusRepository
    {
        private bool isLoaded;
        private double percentLoaded;

        public bool IsLoaded()
        {
            return isLoaded;
        }

        public double PercentLoaded()
        {
            return this.percentLoaded;
        }

        public void SetIsLoaded(bool isLoaded)
        {
            this.isLoaded = isLoaded;
        }

        public void SetPercentLoaded(double percentLoaded)
        {
            this.percentLoaded = percentLoaded;
        }
    }
}
