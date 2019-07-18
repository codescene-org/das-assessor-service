﻿using Newtonsoft.Json;
using SFA.DAS.AssessorService.Domain.Consts;
using SFA.DAS.AssessorService.Domain.JsonData;

namespace SFA.DAS.AssessorService.Web.ViewModels.Certificate
{
    public class CertificateDeclarationViewModel : CertificateBaseViewModel, ICertificateViewModel
    {
        public Domain.Entities.Certificate GetCertificateFromViewModel(Domain.Entities.Certificate certificate, CertificateData certData)
        {       
            certificate.CertificateData = JsonConvert.SerializeObject(certData);
            return certificate;
        }
    }
}