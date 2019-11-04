﻿using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models.Apply.Review;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.ApplyTypes;
using SFA.DAS.AssessorService.Domain.Paging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Handlers.Apply.Review
{
    public class OrganisationApplicationsHandler : IRequestHandler<OrganisationApplicationsRequest, PaginatedList<ApplicationSummaryItem>>
    {
        private readonly IApplyRepository _repository;
        private readonly ILogger<OrganisationApplicationsHandler> _logger;

        public OrganisationApplicationsHandler(IApplyRepository repository, ILogger<OrganisationApplicationsHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PaginatedList<ApplicationSummaryItem>> Handle(OrganisationApplicationsRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving organisation applications");

            var organisationApplicationsResult = await _repository.GetOrganisationApplications(request.ReviewStatus, request.SortColumn, request.SortAscending,
                request.PageSize, request.PageIndex);

            return new PaginatedList<ApplicationSummaryItem>(organisationApplicationsResult.PageOfResults.ToList(),
                    organisationApplicationsResult.TotalCount, request.PageIndex, request.PageSize, request.PageSetSize);
        }
    }
}
