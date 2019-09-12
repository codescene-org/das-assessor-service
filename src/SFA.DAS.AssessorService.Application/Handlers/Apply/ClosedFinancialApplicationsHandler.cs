﻿using MediatR;
using SFA.DAS.AssessorService.Api.Types.Models.Apply;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.ApplyTypes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Handlers.Apply
{
    public class ClosedFinancialApplicationsHandler : IRequestHandler<ClosedFinancialApplicationsRequest, List<FinancialApplicationSummaryItem>>
    {
        private readonly IApplyRepository _repository;

        public ClosedFinancialApplicationsHandler(IApplyRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FinancialApplicationSummaryItem>> Handle(ClosedFinancialApplicationsRequest request, CancellationToken cancellationToken)
        {
            return await _repository.GetClosedFinancialApplications();
        }
    }
}
