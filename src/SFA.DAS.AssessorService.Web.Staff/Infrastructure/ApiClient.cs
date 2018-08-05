﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates;
using SFA.DAS.AssessorService.Application.Api.Client;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Domain.Paging;

namespace SFA.DAS.AssessorService.Web.Staff.Infrastructure
{
    public class ApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ApiClient> _logger;
        private readonly ITokenService _tokenService;

        public ApiClient(HttpClient client, ILogger<ApiClient> logger, ITokenService tokenService)
        {
            _client = client;
            _logger = logger;
            _tokenService = tokenService;
        }

        private async Task<T> Get<T>(string uri)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());
            var res = await _client.GetAsync(new Uri(uri, UriKind.Relative));
            return await res.Content.ReadAsAsync<T>();
        }

        public async Task<List<CertificateResponse>> GetCertificates()
        {
            return await Get<List<CertificateResponse>>("/api/v1/certificates?statusses=Submitted");
        }

        public async Task<PaginatedList<StaffSearchResult>> Search(string searchString, int page)
        {
            return await Get<PaginatedList<StaffSearchResult>>($"/api/v1/staffsearch?searchQuery={searchString}&page={page}");
        }

        public async Task<LearnerDetail> GetLearner(int stdCode, long uln)
        {
            return await Get<LearnerDetail>($"/api/v1/learnerDetails?stdCode={stdCode}&uln={uln}");
        }

        public async Task<Certificate> GetCertificate(Guid certificateId)
        {
            return await Get<Certificate>($"api/v1/certificates/{certificateId}");
        }

        public async Task<Organisation> GetOrganisation(Guid id)
        {            
            return await Get<Organisation>($"/api/v1/organisations/{id}");         
        }
    }
}