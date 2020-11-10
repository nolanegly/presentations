using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VolunteerConvergence.Infrastructure
{
    public class ValidatorPageFilter : IPageFilter
    {
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (context.ModelState.IsValid) return;

            var headers = context.HttpContext.Request.Headers;
            if (headers.ContainsKey("x-requested-with") && headers["x-requested-with"].Equals("XMLHttpRequest"))
            {
                HandleAjaxRequest(context);
            }
            else
            {
                HandleFullRequest(context);
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        private void HandleFullRequest(PageHandlerExecutingContext context)
        {
            if (!(context.HandlerInstance is PageModel pm))
            {
                // ideally this should get logged
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Instruct the PageModel to return itself so the validation errors get rendered
            context.Result = pm.Page();
        }

        private void HandleAjaxRequest(PageHandlerExecutingContext context)
        {
            // When the ModelState is invalid, rather than execute the PageHandler this returns serialized errors
            // to be displayed as errors on the page (see doAjaxPost in site.js)

            // An incoming model binding error occurred
            if (context.HttpContext.Request.Method == "GET")
            {
                var getResult = new BadRequestResult();
                context.Result = getResult;
                return;
            }

            // A server side validation error occurred.
            var result = JsonError.BuildContentResult(context.ModelState);
                context.Result = result;
        }
    }
}
