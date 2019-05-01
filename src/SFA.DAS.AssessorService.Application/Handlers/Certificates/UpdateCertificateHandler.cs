﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Application.Logging;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Domain.JsonData;

namespace SFA.DAS.AssessorService.Application.Handlers.Certificates
{
    public class UpdateCertificateHandler : IRequestHandler<UpdateCertificateRequest, Certificate>
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly ILogger<UpdateCertificateHandler> _logger;

        public UpdateCertificateHandler(ICertificateRepository certificateRepository, ILogger<UpdateCertificateHandler> logger)
        {
            _certificateRepository = certificateRepository;
            _logger = logger;
        }

        public async Task<Certificate> Handle(UpdateCertificateRequest request, CancellationToken cancellationToken)
        {
            if (request.Certificate.Status == Domain.Consts.CertificateStatus.Submitted)
            {
                _logger.LogInformation(LoggingConstants.CertificateSubmitted);
                _logger.LogInformation($"Certificate with ID: {request.Certificate.Id} Submitted with reference of {request.Certificate.CertificateReference}");
            }

            // Need to update the EPA record
            var certData = JsonConvert.DeserializeObject<CertificateData>(request.Certificate.CertificateData);
            if(certData != null)
            {
                var epaDetails = certData.EpaDetails ?? new EpaDetails();
                if (epaDetails.Epas is null) epaDetails.Epas = new List<EpaRecord>();

                if (certData.AchievementDate != null && !epaDetails.Epas.Any(rec => rec.EpaDate == certData.AchievementDate.Value && rec.EpaOutcome == certData.OverallGrade))
                {
                    var epaOutcome = certData.OverallGrade == "Fail" ? "fail" : "pass";
                    var record = new EpaRecord { EpaDate = certData.AchievementDate.Value, EpaOutcome = certData.OverallGrade };
                    epaDetails.Epas.Add(record);

                    var latestRecord = epaDetails.Epas.OrderByDescending(epa => epa.EpaDate).First();
                    epaDetails.LatestEpaDate = latestRecord.EpaDate;
                    epaDetails.LatestEpaOutcome = latestRecord.EpaOutcome;
                    epaDetails.EpaReference = request.Certificate.CertificateReference;
                }

                certData.EpaDetails = epaDetails;
                request.Certificate.CertificateData = JsonConvert.SerializeObject(certData);
            }

            // NOTE: In UpdateBatchCertificateHandler we update the status if it's a Fail or Deleted. Here we don't do it as it'll be done in Web app.

            var logs = await _certificateRepository.GetCertificateLogsFor(request.Certificate.Id);
            var latestLogEntry = logs.OrderByDescending(l => l.EventTime).FirstOrDefault();
            if (latestLogEntry != null && latestLogEntry.Action == request.Action && string.IsNullOrWhiteSpace(request.ReasonForChange))
            {
                return await _certificateRepository.Update(request.Certificate, request.Username, request.Action, updateLog:false);
            }
            return await _certificateRepository.Update(request.Certificate, request.Username, request.Action, updateLog: true, reasonForChange: request.ReasonForChange);
        }
    }
}