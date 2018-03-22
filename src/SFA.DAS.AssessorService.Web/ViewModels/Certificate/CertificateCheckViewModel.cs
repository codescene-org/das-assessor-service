﻿using System;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Domain.JsonData;

namespace SFA.DAS.AssessorService.Web.ViewModels.Certificate
{
    public class CertificateCheckViewModel : CertificateBaseViewModel, ICertificateViewModel
    {
        public long Uln { get; set; }
        public int Level { get; set; }
        public string Option { get; set; }
        public string SelectedGrade { get; set; }
        public DateTime AchievementDate { get; set; }

        public string Name { get; set; }
        public string Dept { get; set; }
        public string Employer { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }

        public void FromCertificate(Domain.Entities.Certificate cert)
        {
            BaseFromCertificate(cert);

            Uln = cert.Uln;
            Level = CertificateData.StandardLevel;
            Option = CertificateData.CourseOption;
            SelectedGrade = CertificateData.OverallGrade;
            AchievementDate = CertificateData.AchievementDate.Value;

            Name = CertificateData.ContactName;
            Dept = CertificateData.Department;
            Employer = CertificateData.ContactOrganisation;
            AddressLine1 = CertificateData.ContactAddLine1;
            AddressLine2 = CertificateData.ContactAddLine2;
            AddressLine3 = CertificateData.ContactAddLine3;
            City = CertificateData.ContactAddLine4;
            Postcode = CertificateData.ContactPostCode;
        }

        public Domain.Entities.Certificate GetCertificateFromViewModel(Domain.Entities.Certificate certificate, CertificateData data)
        {
            certificate.CertificateData = JsonConvert.SerializeObject(data);
            return certificate;
        }
    }
}