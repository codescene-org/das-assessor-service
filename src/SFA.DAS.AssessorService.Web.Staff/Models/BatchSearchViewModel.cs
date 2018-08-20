﻿using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Domain.Paging;

namespace SFA.DAS.AssessorService.Web.Staff.Models
{
    public class BatchSearchViewModel
    {
        public int BatchNumber { get; set; }
        public int Page { get; set; }
        public PaginatedList<StaffBatchSearchResult> PaginatedList { get; set; }
    }
}
