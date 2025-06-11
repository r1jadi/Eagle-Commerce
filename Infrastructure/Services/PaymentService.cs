// using System;
// using System.Linq; // Make sure you have this using statement for .Sum()
// using System.Collections.Generic; // Make sure you have this for List<string>
// using System.Threading.Tasks; // Make sure you have this for Task
// using Core.Entities;
// using Core.Interfaces;
// using Microsoft.Extensions.Configuration;
// using Stripe;

// namespace Infrastructure.Services;

// public class PaymentService(IConfiguration config, ICartService cartService,
//     IUnitOfWork unit) : IPaymentService
// {
//     public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
//     {
//         StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];

//         var cart = await cartService.GetCartAsync(cartId);
//         if (cart == null) return null;

//         var shippingPrice = 0m;
//         if (cart.DeliveryMethodId.HasValue)
//         {
//             var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync((int)cart.DeliveryMethodId);
//             if (deliveryMethod == null) return null; // Or handle more gracefully
//             shippingPrice = deliveryMethod.Price;
//         }

//         foreach (var item in cart.Items)
//         {
//             var productItem = await unit.Repository<Core.Entities.Product>().GetByIdAsync(item.ProductId);
//             if (productItem == null) return null; // Or handle more gracefully
//             if (item.Price != productItem.Price)
//             {
//                 item.Price = productItem.Price;
//             }
//         }

//         var service = new PaymentIntentService();
//         PaymentIntent? intent = null;
//         var amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
//                      + (long)(shippingPrice * 100);

//         if (string.IsNullOrEmpty(cart.PaymentIntentId))
//         {
//             // ---- PATH 1: No Intent exists, create a new one ----
//             var options = new PaymentIntentCreateOptions
//             {
//                 Amount = amount,
//                 Currency = "usd",
//                 PaymentMethodTypes = ["card"]
//             };
//             intent = await service.CreateAsync(options);
//             cart.PaymentIntentId = intent.Id;
//             cart.ClientSecret = intent.ClientSecret;
//         }
//         else
//         {
//             // ---- PATH 2: An Intent ID exists, we need to check its status first ----
//             var existingIntent = await service.GetAsync(cart.PaymentIntentId);

//             // ---- CRITICAL CHECK: Has the previous intent already been paid? ----
//             if (existingIntent.Status == "succeeded")
//             {
//                 // The old payment was successful. We CANNOT update it.
//                 // We must create a NEW PaymentIntent for this new/modified cart.
//                 var options = new PaymentIntentCreateOptions
//                 {
//                     Amount = amount,
//                     Currency = "usd",
//                     PaymentMethodTypes = ["card"]
//                 };
//                 intent = await service.CreateAsync(options);
//                 cart.PaymentIntentId = intent.Id; // Assign the NEW ID
//                 cart.ClientSecret = intent.ClientSecret; // Assign the NEW secret
//             }
//             else
//             {
//                 // The old payment was NOT successful, so it's safe to update.
//                 var options = new PaymentIntentUpdateOptions
//                 {
//                     Amount = amount
//                 };
//                 intent = await service.UpdateAsync(cart.PaymentIntentId, options);
//             }
//         }

//         await cartService.SetCartAsync(cart);

//         return cart;
//     }
// }



using System;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services;

public class PaymentService(IConfiguration config, ICartService cartService,
    IUnitOfWork unit) : IPaymentService
{
    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];

        var cart = await cartService.GetCartAsync(cartId);

        if (cart == null) return null;

        var shippingPrice = 0m;

        if (cart.DeliveryMethodId.HasValue)
        {
            var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync((int)cart.DeliveryMethodId);

            if (deliveryMethod == null) return null;

            shippingPrice = deliveryMethod.Price;
        }

        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Core.Entities.Product>().GetByIdAsync(item.ProductId);

            if (productItem == null) return null;

            if (item.Price != productItem.Price)
            {
                item.Price = productItem.Price;
            }
        }

        var service = new PaymentIntentService();
        PaymentIntent? intent = null;

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
                    + (long)shippingPrice * 100,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };
            intent = await service.CreateAsync(options);
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
                    + (long)shippingPrice * 100
            };
            intent = await service.UpdateAsync(cart.PaymentIntentId, options); //qitu
        }

        await cartService.SetCartAsync(cart);

        return cart;
    }
}
