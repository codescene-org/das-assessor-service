﻿using System;
using System.Threading.Tasks;
using SFA.DAS.AssessorService.Domain.Entities;

namespace SFA.DAS.AssessorService.Application.Interfaces
{
    public interface ICertificateRepository
    {
        Task<Certificate> New(Certificate certificate);
        Task<Certificate> GetCertificate(Guid id);
        Task<Certificate> GetCertificate(long uln, int standardCode);
    }
}