﻿using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Settings;
using SFA.DAS.AssessorService.Web.Infrastructure;
using SFA.DAS.AssessorService.Web.ViewModels;

namespace SFA.DAS.AssessorService.Web.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly IHttpClient _httpClient;
        private readonly IWebConfiguration _config;
        private string _remoteServiceBaseUrl;

        public OrganisationService(IHttpClient httpClient, IWebConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            var apiServiceHost = _config.Api.ApiBaseAddress;// "http://localhost:59021";
            _remoteServiceBaseUrl = $"{apiServiceHost}/api/v1/assessment-providers";
        }

        public async Task<Organisation> GetOrganisation(string token, int ukprn)
        {
            var apiUri = ApiUriGenerator.Organisation.GetOrganisation(_remoteServiceBaseUrl, ukprn);

            var dataString = await _httpClient.GetStringAsync(apiUri, token);

            var response = JsonConvert.DeserializeObject<Organisation>(dataString);

            return response;
        }
    }
}