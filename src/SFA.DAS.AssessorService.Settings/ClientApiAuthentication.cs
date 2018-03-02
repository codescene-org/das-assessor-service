﻿using Newtonsoft.Json;
using SFA.DAS.AssessorService.EpaoImporter.Settings;

namespace SFA.DAS.AssessorService.Settings
{
    public class ClientApiAuthentication : IClientApiAuthentication
    {
        [JsonRequired]
        public string Instance { get; set; }
        [JsonRequired]
        public string TenantId { get; set; }
        [JsonRequired]
        public string ClientId { get; set; }
        [JsonRequired]
        public string ClientSecret { get; set; }
        [JsonRequired]
        public string ResourceId { get; set; }
        [JsonRequired]
        public string ApiBaseAddress { get; set; }
    }
}