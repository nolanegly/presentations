using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.Infrastructure.Tags.SelectBuilders
{
    public class EntityModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            return typeof(IDomainEntity).IsAssignableFrom(context.Metadata.ModelType) ? new EntityModelBinder() : null;
        }
    }

    public class EntityModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var original = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (original != ValueProviderResult.None)
            {
                var originalValue = original.FirstValue;
                if (Guid.TryParse(originalValue, out var id))
                {
                    var dbContext = bindingContext.HttpContext.RequestServices.GetService<VolConContext>();
                    IDomainEntity domainEntity = null;

                    if (bindingContext.ModelType == typeof(Organization))
                        domainEntity = await dbContext.Organizations.FindAsync(id);

                    if (bindingContext.ModelType == typeof(OrganizationType))
                        domainEntity = await dbContext.OrganizationTypes.FindAsync(id);

                    bindingContext.Result = domainEntity != null ? ModelBindingResult.Success(domainEntity) : bindingContext.Result;
                }
            }
        }
    }
}