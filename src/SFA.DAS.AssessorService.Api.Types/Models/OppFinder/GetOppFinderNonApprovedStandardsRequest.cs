﻿using MediatR;

namespace SFA.DAS.AssessorService.Api.Types.Models
{
    public class GetOppFinderNonApprovedStandardsRequest : IRequest<GetOppFinderNonApprovedStandardsResponse>
    {
        public string SearchTerm { get; set; }
        public string SectorFilters { get; set; }
        public string LevelFilters { get; set; }
        public string SortColumn { get; set; }
        public int SortAscending { get; set; }
        public int PageSize { get; set; }
        public int? PageIndex { get; set; }
        public int PageSetSize { get; set; }
        public string NonApprovedType { get; set; }
    }
}
