namespace SnackSample.ApiModels
{
    public class ReindexResponse
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }

        public ReindexResponse(bool success = true)
        {
            Success = success;
            TotalProcessed = 0;
        }

        public ReindexResponse MergeWith(ReindexResponse other)
        {
            Success &= other.Success;
            TotalProcessed += other.TotalProcessed;

            return this;
        }
    }
}