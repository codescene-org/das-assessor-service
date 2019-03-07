﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Application.Api.Client.Clients;
using SFA.DAS.AssessorService.Web.Infrastructure;

namespace SFA.DAS.AssessorService.Web.Orchestrators.Login
{
    public class LoginOrchestrator : ILoginOrchestrator
    {
        private readonly ILogger<LoginOrchestrator> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILoginApiClient _loginApiClient;

        public LoginOrchestrator(ILogger<LoginOrchestrator> logger, IHttpContextAccessor contextAccessor, ILoginApiClient loginApiClient)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _loginApiClient = loginApiClient;
        }
        public async Task<LoginResponse> Login()
        {
            var claims = _contextAccessor.HttpContext.User.Claims;
            foreach (var claim in claims)
            {
                _logger.LogInformation($"Claim received: {claim.Type} Value: {claim.Value}");
            }

            _logger.LogInformation("Start of PostSignIn");


            var signinId = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "sub")?.Value;
            var email = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "email")?.Value;
            var givenName = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "given_name")?.Value;
            var familyName = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "family_name")?.Value;

            var loginResult = await _loginApiClient.Login(new LoginRequest()
            {
                DisplayName = givenName + " " + familyName,
                Email = email,
                SignInId = Guid.Parse(signinId)
            });
            return loginResult;
        }
    }
}