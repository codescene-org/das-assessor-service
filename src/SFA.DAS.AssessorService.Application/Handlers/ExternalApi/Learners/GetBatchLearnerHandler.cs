﻿using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.ExternalApi.Certificates;
using SFA.DAS.AssessorService.Api.Types.Models.ExternalApi.Learners;
using SFA.DAS.AssessorService.Api.Types.Models.Standards;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Handlers.ExternalApi.Learners
{
    public class GetBatchLearnerHandler : IRequestHandler<GetBatchLearnerRequest, GetBatchLearnerResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetBatchLearnerHandler> _logger;
        private readonly IIlrRepository _ilrRepository;
        private readonly IOrganisationQueryRepository _organisationRepository;
        private readonly IContactQueryRepository _contactRepository;

        public GetBatchLearnerHandler(IMediator mediator, ILogger<GetBatchLearnerHandler> logger,
                                IIlrRepository ilrRepository, IOrganisationQueryRepository organisationRepository, IContactQueryRepository contactRepository)
        {
            _mediator = mediator;
            _logger = logger;
            _ilrRepository = ilrRepository;
            _organisationRepository = organisationRepository;
            _contactRepository = contactRepository;
        }

        public async Task<GetBatchLearnerResponse> Handle(GetBatchLearnerRequest request, CancellationToken cancellationToken)
        {
            var standard = await GetStandard(request);
            var learner = await GetLearnerDetails(request, standard);
            var certificate = await GetCertificate(request, standard);

            return new GetBatchLearnerResponse { Learner = learner, Certificate = certificate };
        }

        private async Task<StandardCollation> GetStandard(GetBatchLearnerRequest request)
        {
            StandardCollation standard = null;

            if (int.TryParse(request.Standard, out var standardCode))
            {
                standard = await _mediator.Send(new GetCollatedStandardRequest { StandardId = standardCode });
            }
            else
            {
                standard = await _mediator.Send(new GetCollatedStandardRequest { ReferenceNumber = request.Standard });
            }

            return standard;
        }

        private async Task<LearnerDetailForExternalApi> GetLearnerDetails(GetBatchLearnerRequest request, StandardCollation standard)
        {
            LearnerDetailForExternalApi learnerDetail = null;

            if (standard != null)
            {
                var learner = await _ilrRepository.Get(request.Uln, standard.StandardId.GetValueOrDefault());

                if (learner != null)
                {
                    var epao = await _organisationRepository.GetByUkPrn(learner.UkPrn);
                    var primaryContact = await _contactRepository.GetContact(epao.PrimaryContact);

                    if (primaryContact is null)
                    {
                        var contacts = await _contactRepository.GetAllContacts(epao.EndPointAssessorOrganisationId);
                        primaryContact = contacts.FirstOrDefault();
                    }

                    learnerDetail = new LearnerDetailForExternalApi()
                    {
                        Uln = learner.Uln,
                        GivenNames = learner.GivenNames,
                        FamilyName = learner.FamilyName,
                        LearnerStartDate = learner.LearnStartDate,
                        CompletionStatus = learner.CompletionStatus,
                        Standard = standard,
                        EndPointAssessorOrganisationId = epao?.EndPointAssessorOrganisationId ?? learner.EpaOrgId,
                        OrganisationPrimaryContactEmail = primaryContact?.Email,
                        UkPrn = epao?.EndPointAssessorUkprn ?? learner.UkPrn,
                        OrganisationName = epao.EndPointAssessorName
                    };
                }
            }

            return learnerDetail;
        }

        private async Task<Certificate> GetCertificate(GetBatchLearnerRequest request, StandardCollation standard)
        {
            Certificate certificate = null;

            if (standard != null && request.IncludeCertificate)
            {
                var certificateRequest = new GetBatchCertificateRequest
                {
                    Uln = request.Uln,
                    FamilyName = request.FamilyName,
                    StandardCode = standard.StandardId.GetValueOrDefault(),
                    StandardReference = standard.ReferenceNumber,
                    UkPrn = request.UkPrn,
                    Email = request.Email
                };

                certificate = await _mediator.Send(certificateRequest);
            }

            return certificate;
        }
    }
}
