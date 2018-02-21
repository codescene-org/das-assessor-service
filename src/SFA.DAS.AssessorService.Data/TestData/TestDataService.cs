﻿using System;
using System.Linq;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Domain.Enums;

namespace SFA.DAS.AssessorService.Data.TestData
{
    public static class TestDataService
    {
        public static void AddTestData(AssessorDbContext context)
        {
            var existingOrganisation = context.Organisations.FirstOrDefault();
            if (existingOrganisation == null)
            {
                var organisation = new Organisation
                {
                    Id = Guid.NewGuid(),
                    EndPointAssessorName = "EPAO 1",
                    EndPointAssessorOrganisationId = "1234",
                    EndPointAssessorUKPRN = 10000000,               
                    OrganisationStatus = OrganisationStatus.New
                };

                context.Organisations.Add(organisation);
                context.SaveChanges();

                var firstContact = new Contact
                {
                    Id = Guid.NewGuid(),
                    ContactEmail = "blah@blah.com",
                    ContactName = "Fred Jones",
                    EndPointAssessorContactId = 1,                  
                    ContactStatus = ContactStatus.Live,                
                    OrganisationId = organisation.Id
                };

                context.Contacts.Add(firstContact);

                var secondContact = new Contact
                {
                    Id = Guid.NewGuid(),
                    ContactEmail = "jcoxhead@gmail.com",
                    ContactName = "John Coxhead",
                    EndPointAssessorContactId = 1,             
                    ContactStatus = ContactStatus.Live,                                     
                    OrganisationId = organisation.Id
                };

                context.Contacts.Add(secondContact);
                context.SaveChanges();

                var firstCertificate = new Certificate
                {
                    Id = Guid.NewGuid(),
                    AchievementDate = DateTime.Now.AddDays(-1),
                    AchievementOutcome = "Succesfull",
                    ContactName = "David Gouge",
                    ContactOrganisation = "1234",
                    ContactAddLine1 = "1 Alpha Drive",
                    ContactAddLine2 = "Oakhalls",
                    ContactAddLine3 = "Malvern",
                    ContactAddLine4 = "Worcs",
                    ContactPostCode = "B60 2TY",
                    CourseOption = "French",
                    EndPointAssessorCertificateId = 2222222,
                    EndPointAssessorContactId = firstContact.EndPointAssessorContactId,
                    EndPointAssessorOrganisationId = organisation.EndPointAssessorOrganisationId,
                    LearnerDateofBirth = DateTime.Now.AddYears(-22),
                    LearnerFamilyName = "Gouge",
                    LearnerSex = "Male",
                    LearnerGivenNames = "David",
                    OrganisationId = organisation.Id,
                    OverallGrade = "PASS",
                    ProviderUKPRN = 999999,
                    Registration = "Registered",
                    LearningStartDate = DateTime.Now.AddDays(10),
                    StandardCode = 100,
                    StandardLevel = 1,
                    StandardName = "Test",
                    StandardPublicationDate = DateTime.Now,
                    Status = "Active",
                    ULN = 123456,                  
                    CreatedBy = firstContact.Id,
                    UpdatedBY = firstContact.Id

                };

                context.Certificates.Add(firstCertificate);
                var secondCertificate = new Certificate
                {
                    Id = Guid.NewGuid(),
                    AchievementDate = DateTime.Now.AddDays(-1),
                    AchievementOutcome = "Succesfull",
                    ContactName = "John Coxhead",
                    ContactOrganisation = "1234",
                    ContactAddLine1 = "1 Beta Drive",
                    ContactAddLine2 = "Oakhalls",
                    ContactAddLine3 = "Malvern",
                    ContactAddLine4 = "Worcs",
                    ContactPostCode = "B60 2TY",
                    CourseOption = "French",
                    EndPointAssessorCertificateId = 2222222,
                    EndPointAssessorContactId = firstContact.EndPointAssessorContactId,
                    EndPointAssessorOrganisationId = organisation.EndPointAssessorOrganisationId,
                    LearnerDateofBirth = DateTime.Now.AddYears(-22),
                    LearnerFamilyName = "Coxhead",
                    LearnerSex = "Male",
                    LearnerGivenNames = "David",
                    OrganisationId = organisation.Id,
                    OverallGrade = "PASS",
                    ProviderUKPRN = 999999,
                    Registration = "Registered",
                    LearningStartDate = DateTime.Now.AddDays(10),
                    StandardCode = 100,
                    StandardLevel = 1,
                    StandardName = "Test",
                    StandardPublicationDate = DateTime.Now,
                    Status = "Active",
                    ULN = 123456,                  
                    CreatedBy = firstContact.Id,
                    UpdatedBY = firstContact.Id
                };

                context.Certificates.Add(secondCertificate);
                context.SaveChanges();

                var firstCertificateLog = new CertificateLog
                {
                    Id = Guid.NewGuid(),
                    Action = "Action",
                    CertificateId = firstCertificate.Id,
                    EndPointAssessorCertificateId = 2222222,
                    EventTime = DateTime.Now,
                    Status = "Active",              
                };

                context.CertificateLogs.Add(firstCertificateLog);

                var secondCertificateLog = new CertificateLog
                {
                    Id = Guid.NewGuid(),
                    Action = "Action",
                    CertificateId = secondCertificate.Id,
                    EndPointAssessorCertificateId = 2222222,
                    EventTime = DateTime.Now,
                    Status = "Active",                
                };

                context.CertificateLogs.Add(secondCertificateLog);
                context.SaveChanges();
            }
        }
    }
}