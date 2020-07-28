using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace VolunteerConvergence.Infrastructure
{
    public static class JsonError
    {
        public static ContentResult BuildContentResult(object errorModel)
        {
            var result = new ContentResult();
            var content = JsonConvert.SerializeObject(errorModel,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            result.Content = content;
            result.ContentType = "application/json";
            result.StatusCode = 400;

            return result;
        }
    }
}