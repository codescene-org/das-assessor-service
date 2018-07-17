﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Application.Api.Middleware;
using SFA.DAS.AssessorService.Application.Api.Properties.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SFA.DAS.AssessorService.Application.Api.Controllers
{
    [Authorize]
    [ValidateBadRequest]
    public class AssessmentOrgsImportController: Controller
    {
        private readonly ILogger<AssessmentOrgsImportController> _logger;
        private readonly IMediator _mediator;

        public AssessmentOrgsImportController(ILogger<AssessmentOrgsImportController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("api/ao/update-assessment-orgs", Name = "UpdateAssessmentOrganisations")]
        [SwaggerResponse((int) HttpStatusCode.OK,Type=null)]
        [SwaggerResponse((int) HttpStatusCode.BadRequest, typeof(IDictionary<string, string>))]
        [SwaggerResponse((int) HttpStatusCode.InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> UpdateAssessmentOrganisations()
        {
            _logger.LogInformation("Get Organisation Types");
            var response = await _mediator.Send(new AssessmentOrgsImportRequest());

            return Ok(response);
        }    
    }
}
