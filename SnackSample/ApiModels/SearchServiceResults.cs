using System.Collections.Generic;
using SnackSample.Services.Mappings;

namespace SnackSample.ApiModels
{
    public class SearchServiceResults
    {
        public List<SearchItemDocumentBase> Items { get; set; }

        public int TotalResults { get; set; }
        public string OriginalQuery { get; set; }

        // Usefull while debugging
        public string DebugInformation { get; set; }

        public SearchServiceResults()
        {
            Items = new List<SearchItemDocumentBase>();
        }
    }
}
