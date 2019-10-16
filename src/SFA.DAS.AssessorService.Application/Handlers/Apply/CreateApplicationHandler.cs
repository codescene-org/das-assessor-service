﻿using MediatR;
using SFA.DAS.AssessorService.Api.Types.Models.AO;
using SFA.DAS.AssessorService.Api.Types.Models.Apply;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.ApplyTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Handlers.Apply
{
    public class CreateApplicationHandler : IRequestHandler<CreateApplicationRequest, Guid>
    {
        private readonly IApplyRepository _applyRepository;
        private readonly IOrganisationQueryRepository _organisationQueryRepository;
        private readonly IRegisterQueryRepository _registerQueryRepository;

        public CreateApplicationHandler(IOrganisationQueryRepository organisationQueryRepository,
            IRegisterQueryRepository registerQueryRepository, IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
            _organisationQueryRepository = organisationQueryRepository;
            _registerQueryRepository = registerQueryRepository;
        }

        public async Task<Guid> Handle(CreateApplicationRequest request, CancellationToken cancellationToken)
        {
            var org = await _organisationQueryRepository.Get(request.OrganisationId);
            var orgTypes = await _registerQueryRepository.GetOrganisationTypes();
            var orgType = orgTypes.FirstOrDefault(x => x.Id == org.OrganisationTypeId);

            var sequences = request.listOfApplySequences;
            DisableSequencesAndSectionsAsAppropriate(sequences, org, orgType);

            var applyData = new ApplyData
            {
                Sequences = sequences,
                Apply = new ApplyTypes.Apply
                {
                    ReferenceNumber = await CreateReferenceNumber(request.ReferenceFormat)
                }
            };

            AddApplyDataWithSubmissionInfo(applyData, request);
            var application = new Domain.Entities.Apply
            {
                ApplyData = applyData,
                ApplicationId = request.ApplicationId,
                StandardCode = request.StandardCode,
                ApplicationStatus = ApplicationStatus.InProgress,
                ReviewStatus = ApplicationReviewStatus.Draft,
                FinancialReviewStatus = IsFinancialExempt(org.OrganisationData?.FHADetails, orgType) ? FinancialReviewStatus.Exempt : FinancialReviewStatus.Required,
                OrganisationId = request.OrganisationId,
                CreatedBy = request.UserId.ToString()
            };

            return await _applyRepository.CreateApplication(application);
        }

        private void AddApplyDataWithSubmissionInfo(ApplyData applyData, CreateApplicationRequest request)
        {
            foreach (var sequence in applyData.Sequences)
            {
                if (sequence.SequenceNo == 1)
                {
                    applyData.Apply.InitSubmissions = new List<Submission>();
                    applyData.Apply.InitSubmissionCount = applyData.Apply.InitSubmissions.Count;

                    applyData.Apply.LatestInitSubmissionDate = null;
                    applyData.Apply.InitSubmissionClosedDate = null;
                    applyData.Apply.InitSubmissionFeedbackAddedDate = null;
                }
                else if (sequence.SequenceNo == 2)
                {
                    applyData.Apply.StandardSubmissions = new List<Submission>();
                    applyData.Apply.StandardSubmissionsCount = applyData.Apply.StandardSubmissions.Count;
                    applyData.Apply.StandardName = request.StandardName;
                    applyData.Apply.StandardCode = request.StandardCode;
                    applyData.Apply.StandardReference = request.StandardReference;

                    applyData.Apply.LatestStandardSubmissionDate = null;
                    applyData.Apply.StandardSubmissionClosedDate = null;
                    applyData.Apply.StandardSubmissionFeedbackAddedDate = null;
                }
            }
        }

        private void DisableSequencesAndSectionsAsAppropriate(List<ApplySequence> sequences, Domain.Entities.Organisation org, OrganisationType orgType)
        {
            bool isEpao = IsOrganisationOnEPAORegister(org);
            if (isEpao)
            {
                RemoveSectionsOneAndTwo(sequences.Single(x => x.SequenceNo == 1));
            }

            bool isFinancialExempt = IsFinancialExempt(org.OrganisationData?.FHADetails, orgType);
            if (isFinancialExempt)
            {
                RemoveSectionThree(sequences.Single(x => x.SequenceNo == 1));
            }

            if (isEpao && isFinancialExempt)
            {
                RemoveSequenceOne(sequences);
            }
        }

        private static bool IsOrganisationOnEPAORegister(Domain.Entities.Organisation org)
        {
            if (org?.OrganisationData == null) return false;

            return org.OrganisationData.RoEPAOApproved;
        }

        private void RemoveSequenceOne(List<ApplySequence> sequences)
        {
            foreach (var sequence1 in sequences.Where(seq => seq.SequenceNo == 1))
            {
                sequence1.IsActive = false;
                sequence1.NotRequired = true;
                sequence1.Status = ApplicationSequenceStatus.Approved;
            }

            foreach (var sequence2 in sequences.Where(seq => seq.SequenceNo == 2))
            {
                if (!sequence2.NotRequired)
                {
                    sequence2.IsActive = true;
                    sequence2.Status = ApplicationSequenceStatus.Draft;
                }
            }
        }

        private void RemoveSectionThree(ApplySequence sequence)
        {
            foreach (var section in sequence.Sections.Where(sec => sec.SectionNo == 3))
            {
                section.NotRequired = true;
                section.Status = ApplicationSectionStatus.Evaluated;
            }
        }

        private void RemoveSectionsOneAndTwo(ApplySequence sequence)
        {
            foreach (var section in sequence.Sections.Where(s => s.SectionNo == 1 || s.SectionNo == 2))
            {
                section.NotRequired = true;
                section.Status = ApplicationSectionStatus.Evaluated;
            }
        }

        private static bool IsFinancialExempt(ApplyTypes.FHADetails financials, OrganisationType orgType)
        {
            if (financials == null) return false;

            bool financialExempt = financials.FinancialExempt ?? false;
            bool orgTypeFinancialExempt = (orgType != null) && orgType.FinancialExempt;

            bool financialIsNotDue = (financials.FinancialDueDate?.Date ?? DateTime.MinValue) > DateTime.Today;

            return financialExempt || financialIsNotDue || orgTypeFinancialExempt;
        }

        private async Task<string> CreateReferenceNumber(string referenceFormat)
        {
            var referenceNumber = string.Empty;

            var seq = await _applyRepository.GetNextAppReferenceSequence();

            if (seq > 0 && !string.IsNullOrEmpty(referenceFormat))
            {
                referenceNumber = $"{referenceFormat}{seq:D6}";
            }

            return referenceNumber;
        }
    }
}