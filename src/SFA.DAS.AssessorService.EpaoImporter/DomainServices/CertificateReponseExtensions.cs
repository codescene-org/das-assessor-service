﻿using System.Collections.Generic;
using System.Linq;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates;
using SFA.DAS.AssessorService.EpaoImporter.Logger;

namespace SFA.DAS.AssessorService.EpaoImporter.DomainServices
{
    public static class CertificateReponseExtensions
    {
        public static List<CertificateResponse> Sanitise(this List<CertificateResponse> certificateResponses, IAggregateLogger logger)
        {
            var sanitisedCertificateResponse = new List<CertificateResponse>();

            foreach (var certificateResponse in certificateResponses)
            {
                var errorFlag = false;

                logger.LogInfo($"Sanitising Certificate - {certificateResponse.CertificateReference} ...");

                var certificateData = certificateResponse.CertificateData;      
                if (string.IsNullOrEmpty(certificateData.ContactAddLine1))
                {
                   errorFlag = true;
                }

                if (string.IsNullOrEmpty(certificateData.ContactPostCode))
                {
                    errorFlag = true;                   
                }

                if (errorFlag)
                {
                    if (!string.IsNullOrEmpty(certificateData.LearnerGivenNames)
                        && !string.IsNullOrEmpty(certificateData.LearnerFamilyName))
                    {
                        logger.LogInfo(
                            $"Unprintable Certificate -> Given Names = {certificateData.LearnerGivenNames} Family Name = {certificateData.LearnerFamilyName} - Cannot be processed");
                    }
                    else
                    {
                        logger.LogInfo($"Unprintable Certificate - Cannot be processed");
                    }
                }
                else
                {
                    sanitisedCertificateResponse.Add(certificateResponse);
                }
            }
            return sanitisedCertificateResponse;
        }


        public static Dictionary<string, List<CertificateResponse>> Group(this IEnumerable<CertificateResponse> certificateResponses, IAggregateLogger logger)
        {
            logger.LogInfo($"Grouping Certificates ...");
            return certificateResponses.GroupBy(item => item.BatchNumber)
                                             .ToDictionary(g => g.Key??"", g => g.ToList());
        }
    }
}
