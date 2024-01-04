using Microsoft.AspNetCore.Mvc.ModelBinding;
using SportsStore.Domain.Entities;

namespace SportsStore.WebUI.Binders
{
    public class CartModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(Cart))
            {
                return new CartModelBinder();
            }

            return null;
        }
    }
}
