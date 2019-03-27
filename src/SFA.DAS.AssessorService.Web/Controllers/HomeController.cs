﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SFA.DAS.AssessorService.Web.Infrastructure;
using SFA.DAS.AssessorService.Web.Models;

namespace SFA.DAS.AssessorService.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache _cache;
        private readonly ISessionService _sessionService;

        public HomeController(IDistributedCache cache, ISessionService sessionService)
        {
            _cache = cache;
            _sessionService = sessionService;
        }
        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult NotRegistered()
        {
            return View();
        }

        public IActionResult InvalidRole()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult Cookies()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult InvitePending()
        {
            return View((object)_sessionService.Get("OrganisationName"));
        }

        public IActionResult Rejected()
        {
            return View((object)_sessionService.Get("OrganisationName"));
        }
    }
}
