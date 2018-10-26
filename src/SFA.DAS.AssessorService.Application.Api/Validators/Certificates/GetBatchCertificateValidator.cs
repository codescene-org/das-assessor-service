﻿using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates.Batch;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs;

namespace SFA.DAS.AssessorService.Application.Api.Validators.Certificates
{
    public class GetBatchCertificateRequestValidator : AbstractValidator<GetBatchCertificateRequest>
    {
        public GetBatchCertificateRequestValidator(IStringLocalizer<GetBatchCertificateRequestValidator> localiser, IOrganisationQueryRepository organisationQueryRepository, IIlrRepository ilrRepository, ICertificateRepository certificateRepository, IAssessmentOrgsApiClient assessmentOrgsApiClient)
        {
            RuleFor(m => m.Uln).InclusiveBetween(1000000000, 9999999999).WithMessage("The apprentice's ULN should contain exactly 10 numbers");
            RuleFor(m => m.FamilyName).NotEmpty().WithMessage("Enter the apprentice's last name");
            RuleFor(m => m.StandardCode).GreaterThan(0).WithMessage("A standard should be selected");
            RuleFor(m => m.CertificateReference).NotEmpty().WithMessage("Enter the certificate reference");
            RuleFor(m => m.UkPrn).InclusiveBetween(10000000, 99999999).WithMessage("The UKPRN should contain exactly 8 numbers");
            RuleFor(m => m.Email).NotEmpty();

            RuleFor(m => m)
                .Custom((m, context) =>
                {
                    var existingCertificate = certificateRepository.GetCertificate(m.Uln, m.StandardCode).GetAwaiter().GetResult();

                    if (existingCertificate is null)
                    {
                        context.AddFailure(new ValidationFailure("CertificateReference", $"Certificate not found"));
                    }
                    else if (!existingCertificate.CertificateReference.Equals(m.CertificateReference, StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.AddFailure(new ValidationFailure("CertificateReference", $"Invalid certificate reference"));
                    }
                    else if (!existingCertificate.CertificateData.Contains(m.FamilyName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.AddFailure(new ValidationFailure("FamilyName", $"Invalid family name"));
                    }
                });

            RuleFor(m => m)
                .Custom((m, context) =>
                {
                    var requestedIlr = ilrRepository.Get(m.Uln, m.StandardCode).GetAwaiter().GetResult();
                    var searchingEpao = organisationQueryRepository.GetByUkPrn(m.UkPrn).GetAwaiter().GetResult();

                    if (requestedIlr == null || !string.Equals(requestedIlr.FamilyName, m.FamilyName))
                    {
                        context.AddFailure(new ValidationFailure("Uln", "Cannot find entry for specified Uln, FamilyName & StandardCode"));
                    }
                    else if (searchingEpao == null)
                    {
                        context.AddFailure(new ValidationFailure("UkPrn", "Cannot find EPAO for specified UkPrn"));
                    }
                    else
                    {
                        var providedStandards = assessmentOrgsApiClient.FindAllStandardsByOrganisationIdAsync(searchingEpao.EndPointAssessorOrganisationId).GetAwaiter().GetResult();

                        if (!providedStandards.Any(s => s.StandardCode == m.StandardCode.ToString()))
                        {
                            context.AddFailure(new ValidationFailure("StandardCode", "EPAO does not provide this Standard"));
                        }
                    }
                });
        }
    }
}
