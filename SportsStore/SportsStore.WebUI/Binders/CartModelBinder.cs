using Microsoft.AspNetCore.Mvc.ModelBinding;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Extensions;

namespace SportsStore.WebUI.Binders
{
    public class CartModelBinder : IModelBinder
    {
        private const string sessionKey = "Cart";

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var cart = bindingContext.ActionContext.HttpContext.Session.Get<Cart>(sessionKey);

            if (cart == null)
            {
                cart = new Cart();
                bindingContext.ActionContext.HttpContext.Session.Set(sessionKey, cart);
            }

            bindingContext.Result = ModelBindingResult.Success(cart);

            return Task.CompletedTask;
        }
    }
}
