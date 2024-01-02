using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace VolunteerConvergence
{
    public static class PageModelExtensions
    {
        public static ActionResult RedirectToPageJson<TPage>(this TPage page, string pageName)
            where TPage : PageModel
        {
            return page.JsonNet(new
                {
                    redirect = page.Url.Page(pageName)
                }
            );
        }

        public static ActionResult RedirectToPageAndIdJson<TPage>(this TPage page, string pageName, Guid id)
            where TPage : PageModel
        {
            return page.JsonNet(new
                {
                    redirect = page.Url.Page(pageName, new{id})
                }
            );
        }

        public static ContentResult JsonNet(this PageModel page, object model)
        {
            var serialized = JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return new ContentResult
            {
                Content = serialized,
                ContentType = "text/html"
            };
        }
    }
}