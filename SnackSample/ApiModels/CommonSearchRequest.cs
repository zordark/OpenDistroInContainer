using System;
using System.Collections.Generic;

namespace SnackSample.ApiModels
{
    public class CommonSearchRequest
    {
        public CommonSearchRequest()
        {
            this.PageSize = 20;
        }

        public int Skip { get; set; }
        public int PageSize { get; set; }

        public string Query { get; set; }

        public List<Guid> ActorIds { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}