﻿using FizzWare.NBuilder;
using Microsoft.Extensions.Localization;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Domain.JsonData;
using SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs;
using System.Collections.Generic;
using Standard = SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs.Types.Standard;
using StandardOrganisationSummary = SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs.Types.StandardOrganisationSummary;

namespace SFA.DAS.AssessorService.Application.Api.UnitTests.Validators.Certificates
{
    public class BatchCertificateRequestValidatorTestBase
    {
        public Mock<ICertificateRepository> CertificateRepositoryMock { get; }
        public Mock<IOrganisationQueryRepository> OrganisationQueryRepositoryMock { get; }
        public Mock<IIlrRepository> IlrRepositoryMock { get; }
        public Mock<IAssessmentOrgsApiClient> AssessmentOrgsApiClientMock { get; }

        public BatchCertificateRequestValidatorTestBase()
        {
            CertificateRepositoryMock = SetupCertificateRepositoryMock();
            OrganisationQueryRepositoryMock = SetupOrganisationQueryRepositoryMock();
            IlrRepositoryMock = SetupIlrRepositoryMock();
            AssessmentOrgsApiClientMock = SetupAssessmentOrgsApiClientMock();
        }

        private static Mock<ICertificateRepository> SetupCertificateRepositoryMock()
        {
            var certificateRepositoryMock = new Mock<ICertificateRepository>();

            certificateRepositoryMock.Setup(q => q.GetCertificate(1234567890, 1)).ReturnsAsync(GenerateCertificate(1234567890, 1, "test", "Draft"));
            certificateRepositoryMock.Setup(q => q.GetCertificate(9999999999, 1)).ReturnsAsync(GenerateCertificate(9999999999, 1, "test", "Printed"));

            return certificateRepositoryMock;
        }

        private static Mock<IOrganisationQueryRepository> SetupOrganisationQueryRepositoryMock()
        {
            var organisationQueryRepositoryMock = new Mock<IOrganisationQueryRepository>();

            organisationQueryRepositoryMock.Setup(r => r.GetByUkPrn(12345678)).ReturnsAsync(GenerateOrganisation(12345678));
            organisationQueryRepositoryMock.Setup(r => r.GetByUkPrn(99999999)).ReturnsAsync(GenerateOrganisation(99999999));

            return organisationQueryRepositoryMock;
        }

        private static Mock<IAssessmentOrgsApiClient> SetupAssessmentOrgsApiClientMock()
        {
            var assessmentOrgsApiClientMock = new Mock<IAssessmentOrgsApiClient>();

            assessmentOrgsApiClientMock.Setup(c => c.GetAllStandards())
                .ReturnsAsync(new List<Standard>
                {
                   GenerateStandard(1),
                   GenerateStandard(99)
                });

            assessmentOrgsApiClientMock.Setup(c => c.FindAllStandardsByOrganisationIdAsync("12345678"))
                .ReturnsAsync(new List<StandardOrganisationSummary>
                {
                    GenerateStandardOrganisationSummary(1),
                    GenerateStandardOrganisationSummary(99)
                });

            assessmentOrgsApiClientMock.Setup(c => c.FindAllStandardsByOrganisationIdAsync("99999999"))
                .ReturnsAsync(new List<StandardOrganisationSummary>
                {
                    GenerateStandardOrganisationSummary(99)
                });

            assessmentOrgsApiClientMock.Setup(c => c.GetStandard(1)).ReturnsAsync(GenerateStandard(1));
            assessmentOrgsApiClientMock.Setup(c => c.GetStandard(13)).ReturnsAsync(GenerateStandard(99));

            return assessmentOrgsApiClientMock;
        }

        private static Mock<IIlrRepository> SetupIlrRepositoryMock()
        {
            var ilrRepositoryMock = new Mock<IIlrRepository>();

            ilrRepositoryMock.Setup(q => q.Get(1234567890, 1)).ReturnsAsync(GenerateIlr(1234567890, 1, "Test", "12345678"));
            ilrRepositoryMock.Setup(q => q.Get(1234567890, 99)).ReturnsAsync(GenerateIlr(1234567890, 1, "Test", "12345678"));
            ilrRepositoryMock.Setup(q => q.Get(9999999999, 1)).ReturnsAsync(GenerateIlr(9999999999, 1, "Test", "99999999"));

            return ilrRepositoryMock;
        }

        private static Certificate GenerateCertificate(long uln, int standardCode, string familyName, string status)
        {
            return Builder<Certificate>.CreateNew()
                .With(i => i.Uln = uln)
                .With(i => i.StandardCode = standardCode)
                .With(i => i.Status = status)
                .With(i => i.CertificateReference = $"{uln}-{standardCode}")
                                .With(i => i.CertificateData = JsonConvert.SerializeObject(Builder<CertificateData>.CreateNew()
                                .With(cd => cd.LearnerFamilyName = familyName)
                                .Build()))
                .Build();
        }

        private static Organisation GenerateOrganisation(int ukprn)
        {
            return Builder<Organisation>.CreateNew()
                .With(i => i.EndPointAssessorOrganisationId = $"{ukprn}")
                .With(i => i.EndPointAssessorUkprn = ukprn)
                .Build();
        }

        private static Standard GenerateStandard(int standardCode)
        {
            return Builder<Standard>.CreateNew()
                .With(i => i.Title = $"{standardCode}")
                .With(i => i.Level = standardCode)
                .With(i => i.Id = standardCode)
                .Build();
        }

        private static StandardOrganisationSummary GenerateStandardOrganisationSummary(int standardCode)
        {
            return Builder<StandardOrganisationSummary>.CreateNew()
                .With(i => i.StandardCode = $"{standardCode}")
                .Build();
        }

        private static Ilr GenerateIlr(long uln, int standardCode, string familyName, string epaOrgId)
        {
            return Builder<Ilr>.CreateNew()
                .With(i => i.Uln = uln)
                .With(i => i.StdCode = standardCode)
                .With(i => i.FamilyName = familyName)
                .With(i => i.EpaOrgId = epaOrgId)
                .Build();
        }
    }
}
