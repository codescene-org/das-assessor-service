﻿using MediatR;
using SFA.DAS.AssessorService.Api.Types.Consts;
using SFA.DAS.AssessorService.Api.Types.Models;
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
    public class SubmitApplicationSequenceHandler : IRequestHandler<SubmitApplicationSequenceRequest, bool>
    {
        private readonly IApplyRepository _applyRepository;
        private readonly IContactQueryRepository _contactQueryRepository;     
        private readonly IEMailTemplateQueryRepository _eMailTemplateQueryRepository;
        private readonly IMediator _mediator;

        public SubmitApplicationSequenceHandler(IApplyRepository applyRepository, IContactQueryRepository contactQueryRepository, IEMailTemplateQueryRepository eMailTemplateQueryRepository, IMediator mediator)
        {
            _applyRepository = applyRepository;
            _contactQueryRepository = contactQueryRepository;
            _eMailTemplateQueryRepository = eMailTemplateQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(SubmitApplicationSequenceRequest request, CancellationToken cancellationToken)
        {
            // CanSubmitApplication was migrated over from Apply Service. If this causes issues then remove it
            if (await _applyRepository.CanSubmitApplication(request.ApplicationId))
            {
                var application = await _applyRepository.GetApplication(request.ApplicationId);
                var submittingContact = await _contactQueryRepository.GetContactById(request.SubmittingContactId);

                if (application?.ApplyData != null && submittingContact != null)
                {
                    if (application.ApplyData.Apply == null)
                    {
                        application.ApplyData.Apply = new ApplyTypes.Apply();
                    }

                    if (string.IsNullOrWhiteSpace(application.ApplyData.Apply.ReferenceNumber))
                    {
                        application.ApplyData.Apply.ReferenceNumber = await CreateReferenceNumber(request.ApplicationReferenceFormat);
                    }

                    AddSubmissionInfoToApplyData(application.ApplyData, request.SequenceNo, submittingContact);
                    UpdateSequenceInformation(application.ApplyData, request.SequenceNo, request.RequestedFeedbackAnswered);
                    UpdateApplicationStatus(application, request.SequenceNo);

                    application.UpdatedBy = submittingContact.Id.ToString();
                    application.UpdatedAt = DateTime.UtcNow;

                    await _applyRepository.SubmitApplicationSequence(application);

                    await NotifyContact(submittingContact, application.ApplyData, request.SequenceNo, cancellationToken);

                    return true;
                }
            }

            return false;
        }

        private void UpdateSequenceInformation(ApplyData applyData, int sequenceNo, Dictionary<int,bool?> dictOfRequestedFeedbackAnswered)
        {
            if (applyData.Sequences != null)
            {
                foreach (var sequence in applyData.Sequences.Where(seq => seq.SequenceNo == sequenceNo && !seq.NotRequired))
                {
                    var resubmittedStatues = new string[] { ApplicationSequenceStatus.Submitted, ApplicationSequenceStatus.FeedbackAdded };

                    sequence.Status = resubmittedStatues.Contains(sequence.Status) ? ApplicationSequenceStatus.Resubmitted : ApplicationSequenceStatus.Submitted;

                    // NOTE: Must update Required Sections within too!
                    if (sequence.Sections != null)
                    {
                        foreach (var section in sequence.Sections.Where(sec => !sec.NotRequired))
                        {
                            if (section.Status == ApplicationSectionStatus.Draft)
                            {
                                section.Status = ApplicationSectionStatus.Submitted;
                            }
                            else if (dictOfRequestedFeedbackAnswered[section.SectionNo] == true)
                            {
                                // TODO: This is dependant on QnA notifying us that the RequestedFeedbackAnswered has been answered
                                // otherwise section.Status will remain as is (most likely Evaluated)
                                section.Status = ApplicationSectionStatus.Submitted;
                            }
                        }
                    }
                }
            }
        }

        private void AddSubmissionInfoToApplyData(ApplyData applyData, int sequenceNo, Domain.Entities.Contact submittingContact)
        {
            var submission = new Submission
            {
                SubmittedAt = DateTime.UtcNow,
                SubmittedBy = submittingContact.Id,
                SubmittedByEmail = submittingContact.Email
            };

            if (sequenceNo == 1)
            {
                if (applyData.Apply.InitSubmissions == null)
                {
                    applyData.Apply.InitSubmissions = new List<Submission>();
                }

                applyData.Apply.InitSubmissions.Add(submission);
                applyData.Apply.InitSubmissionsCount = applyData.Apply.InitSubmissions.Count;
                applyData.Apply.LatestInitSubmissionDate = submission.SubmittedAt;
            }
            else if (sequenceNo == 2)
            {
                if (applyData.Apply.StandardSubmissions == null)
                {
                    applyData.Apply.StandardSubmissions = new List<Submission>();
                }

                applyData.Apply.StandardSubmissions.Add(submission);
                applyData.Apply.StandardSubmissionsCount = applyData.Apply.StandardSubmissions.Count;
                applyData.Apply.LatestStandardSubmissionDate = submission.SubmittedAt;
            }
        }

        private string CheckFinancialStatus(ApplyData applyData)
        {
            if (applyData.Sequences != null)
            {
                foreach (var sequence in applyData.Sequences.Where(seq => seq.SequenceNo == 1 && !seq.NotRequired))
                {
                    // NOTE: Get Status for a required Section 3 - Financial
                    if (sequence.Sections != null)
                    {
                        foreach (var section in sequence.Sections.Where(sec => sec.SectionNo == 3 && !sec.NotRequired))
                        {
                            return section.Status;
                        }
                    }
                }
            }
            return null;
        }

        private void UpdateApplicationStatus(Domain.Entities.Apply application, int sequenceNo)
        {
            // Always default it to submitted
            application.ApplicationStatus = ApplicationStatus.Submitted;
            application.ReviewStatus = ApplicationReviewStatus.New;

            var applyData = application.ApplyData;

            if (sequenceNo == 1)
            {
                application.ApplicationStatus = (applyData.Apply.InitSubmissions.Count == 1) ? ApplicationStatus.Submitted : ApplicationStatus.Resubmitted;

                var closedFinanicalStatuses = new List<string> { FinancialReviewStatus.Approved, FinancialReviewStatus.Exempt };

                if (!closedFinanicalStatuses.Contains(application.FinancialReviewStatus))
                {
                    if (CheckFinancialStatus(applyData) != ApplicationSectionStatus.Evaluated)
                    {
                        application.FinancialReviewStatus = FinancialReviewStatus.New;
                    }
                }
            }
            else if (sequenceNo == 2)
            {
                application.ApplicationStatus = (applyData.Apply.StandardSubmissions.Count == 1) ? ApplicationStatus.Submitted : ApplicationStatus.Resubmitted;
            }
        }

        private async Task<string> CreateReferenceNumber(string referenceFormat)
        {
            var referenceNumber = string.Empty;

            var seq = await _applyRepository.GetNextAppReferenceSequence();

            if (seq > 0 && !string.IsNullOrEmpty(referenceFormat))
            {
                referenceNumber = string.Format($"{referenceFormat}{seq:D6}");
            }

            return referenceNumber;
        }

        private async Task NotifyContact(Domain.Entities.Contact contactToNotify, ApplyData applyData, int sequenceNo, CancellationToken cancellationToken)
        {
            var email = contactToNotify.Email;
            var contactname = contactToNotify.DisplayName;
            var reference = applyData.Apply.ReferenceNumber;
            var standard = applyData.Apply.StandardName;

            if (sequenceNo == 1)
            {
                var emailTemplate = await _eMailTemplateQueryRepository.GetEmailTemplate(EmailTemplateNames.ApplyEPAOInitialSubmission);
                await _mediator.Send(new SendEmailRequest(email, emailTemplate, new { contactname, reference }), cancellationToken);
            }
            else if (sequenceNo == 2)
            {
                var emailTemplate = await _eMailTemplateQueryRepository.GetEmailTemplate(EmailTemplateNames.ApplyEPAOStandardSubmission);
                await _mediator.Send(new SendEmailRequest(email, emailTemplate, new { contactname, reference, standard }), cancellationToken);
            }
        }

    }
}
