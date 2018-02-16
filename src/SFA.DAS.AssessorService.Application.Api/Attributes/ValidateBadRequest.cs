﻿namespace SFA.DAS.AssessorService.Application.Api.Attributes
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class ValidateBadRequest : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }

            //if (!context.ModelState.IsValid)
            //{
            //    context.Result = new BadRequestObjectResult(new ApiBadRequestResponse(context.ModelState)));
            //}

            //base.OnActionExecuting(context);
        }
    }
}