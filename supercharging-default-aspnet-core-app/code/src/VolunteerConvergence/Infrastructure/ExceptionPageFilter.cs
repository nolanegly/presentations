using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VolunteerConvergence.Infrastructure
{
    public class ExceptionPageFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (!(context.Exception is ValidationException ve))
                return;

            var headers = context.HttpContext.Request.Headers;
            if (headers.ContainsKey("x-requested-with") && headers["x-requested-with"].Equals("XMLHttpRequest"))
            {
                ReturnErrorForAjaxCall(context, ve);
            }
            else
            {
                // Do something for full posts that experience a ValidationException here
                // a) You can display a toast if you're using a toast library of some sort
                // b) You can handle these with try..catches on each individual PageModel so you don't take the user away from their current task
                // c) You can dump the user on the error page, which isn't very helpful
                return;
            }
        }

        private static void ReturnErrorForAjaxCall(ExceptionContext context, ValidationException ve)
        {
            var modelStateMimic = new Dictionary<string, ModelStateEntryMimic>();
            var entry = new ModelStateEntryMimic();
            entry.Errors.Add(ve.Message);
            modelStateMimic.Add("ServerSideValidationError", entry);

            var result = JsonError.BuildContentResult(modelStateMimic);

            context.Result = result;
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// Provides the bare minimum parity with ModelStateEntry needed to return a json representation
        /// of errors that client side code can parse the same as other model validation errors.
        /// </summary>
        class ModelStateEntryMimic
        {
            public ModelErrorCollection Errors { get; } = new ModelErrorCollection();
        }
    }
}