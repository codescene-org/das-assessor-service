﻿using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation.Results;
using NUnit.Framework;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates.Batch;
using SFA.DAS.AssessorService.Domain.JsonData;
using System;

namespace SFA.DAS.AssessorService.Application.Api.UnitTests.Validators.Certificates.SubmitBatchCertificateRequestValidator
{
    public class WhenValidatorValidatesSuccessfully : SubmitBatchCertificateRequestValidatorTestBase
    {
        private ValidationResult _validationResult;

        [SetUp]
        public void Arrange()
        {
            Setup();

            long uln = 12345;
            int standardCode = 54321;
            string certificateReference = "54321";
            string status = "Draft";

            AddMockCertificate(uln, standardCode, certificateReference, status);

            SubmitBatchCertificateRequest request = Builder<SubmitBatchCertificateRequest>.CreateNew()
                .With(i => i.Uln = uln)
                .With(i => i.StandardCode = standardCode)
                .With(i => i.UkPrn = 10000000)
                .With(i => i.CertificateReference = certificateReference)
                .With(i => i.CertificateData = Builder<CertificateData>.CreateNew()
                                .With(cd => cd.ContactPostCode = "AA11AA")
                                .With(cd => cd.AchievementDate = DateTime.UtcNow)
                                .Build())
                .Build();

            _validationResult = Validator.Validate(request);
        }

        [Test]
        public void ThenValidationResultShouldBeTrue()
        {
            _validationResult.IsValid.Should().BeTrue();
        }
    }
}
