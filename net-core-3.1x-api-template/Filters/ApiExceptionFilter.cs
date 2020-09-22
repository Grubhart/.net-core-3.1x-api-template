using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InnovaFunding.AuctionService.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
           
            if (context.Exception is UnauthorizedAccessException)
            {
                context.Result = new JsonResult(context.Exception.Message);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            
            


            base.OnException(context);
        }
    }
}