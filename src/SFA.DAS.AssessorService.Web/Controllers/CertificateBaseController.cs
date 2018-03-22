﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates;
using SFA.DAS.AssessorService.Application.Api.Client.Clients;
using SFA.DAS.AssessorService.Domain.JsonData;
using SFA.DAS.AssessorService.Web.ViewModels.Certificate;

namespace SFA.DAS.AssessorService.Web.Controllers
{
    public class CertificateBaseController : Controller
    {
        private readonly ILogger<CertificateController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICertificateApiClient _certificateApiClient;

        public CertificateBaseController(ILogger<CertificateController> logger, IHttpContextAccessor contextAccessor, ICertificateApiClient certificateApiClient)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _certificateApiClient = certificateApiClient;
        }
        protected async Task<IActionResult> LoadViewModel<T>(string view) where T : ICertificateViewModel, new()
        {
            var username = _contextAccessor.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;

            _logger.LogInformation($"Load View Model for {typeof(T).Name} for {username}");

            var query = _contextAccessor.HttpContext.Request.Query;
            if (query.ContainsKey("redirecttocheck") && bool.Parse(query["redirecttocheck"]))
            {
                _logger.LogInformation($"RedirectToCheck for {typeof(T).Name} is true");
                _contextAccessor.HttpContext.Session.SetString("redirecttocheck", "true");
            }
            else
                _contextAccessor.HttpContext.Session.Remove("redirecttocheck");

            var sessionString = _contextAccessor.HttpContext.Session.GetString("CertificateSession");
            if (sessionString == null)
            {
                _logger.LogInformation($"Session for {typeof(T).Name} requested by {username} has been lost. Redirecting to Search Index");
                return RedirectToAction("Index", "Search");
            }
            var certSession = JsonConvert.DeserializeObject<CertificateSession>(sessionString);

            var certificate = await _certificateApiClient.GetCertificate(certSession.CertificateId);

            _logger.LogInformation($"Got Certificate for {typeof(T).Name} requested by {username} with Id {certificate.Id}");

            var viewModel = new T();
            viewModel.FromCertificate(certificate);

            _logger.LogInformation($"Got View Model of type {typeof(T).Name} requested by {username}");

            return View(view, viewModel);
        }

        protected async Task<IActionResult> SaveViewModel<T>(T vm, string returnToIfModelNotValid, RedirectToActionResult nextAction) where T : ICertificateViewModel
        {
            var username = _contextAccessor.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;
            
            _logger.LogInformation($"Save View Model for {typeof(T).Name} for {username} with values: {GetModelValues(vm)}");

            var certificate = await _certificateApiClient.GetCertificate(vm.Id);
            var certData = JsonConvert.DeserializeObject<CertificateData>(certificate.CertificateData);

            if (!ModelState.IsValid)
            {
                vm.FamilyName = certData.LearnerFamilyName;
                vm.GivenNames = certData.LearnerGivenNames;
                _logger.LogInformation($"Model State not valid for {typeof(T).Name} requested by {username} with Id {certificate.Id}. Errors: {ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)}");
                return View(returnToIfModelNotValid, vm);
            }

            var updatedCertificate = vm.GetCertificateFromViewModel(certificate, certData);

            await _certificateApiClient.UpdateCertificate(new UpdateCertificateRequest(updatedCertificate) { Username = username });

            _logger.LogInformation($"Certificate for {typeof(T).Name} requested by {username} with Id {certificate.Id} updated.");

            var session = _contextAccessor.HttpContext.Session;
            if (session.Keys.Any(k => k == "redirecttocheck") && bool.Parse(session.GetString("redirecttocheck")))
            {
                _logger.LogInformation($"Certificate for {typeof(T).Name} requested by {username} with Id {certificate.Id} redirecting back to Certificate Check.");
                return new RedirectToActionResult("Check", "CertificateCheck", null);
            }

            _logger.LogInformation($"Certificate for {typeof(T).Name} requested by {username} with Id {certificate.Id} redirecting to {nextAction.ControllerName} {nextAction.ActionName}");
            return nextAction;
        }

        private string GetModelValues<T>(T viewModel)
        {
            var properties = typeof(T).GetProperties().ToList();

            return properties.Aggregate("", (current, prop) => current + $"{prop.Name}: {prop.GetValue(viewModel)}, ");
        }
    }
}