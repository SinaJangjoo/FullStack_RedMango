using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using Stripe;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApiResponse _response;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        public PaymentController(IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
            _response = new();
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId) //Based on this userId we will generate the payment intent and we need to find out what is their order total
        {
            //Fist we need to retrieve the ShoppingCart
            ShoppingCart shoppingCart = _db.ShoppingCarts.Include(x => x.CartItems)
                .ThenInclude(u => u.MenuItem).FirstOrDefault(x => x.UserId == userId);
            if (shoppingCart == null || shoppingCart.CartItems == null || shoppingCart.CartItems.Count() == 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            #region create Payment Intent

            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);

            PaymentIntentCreateOptions options = new()
            {
                Amount = (int)(shoppingCart.CartTotal * 100),  //Convert that to integer and mult by 100 because by default it is cent and we want Dollar!
                Currency = "usd",
                PaymentMethodTypes = new List<string>()
                {
                    "card",
                },
            };
            PaymentIntentService service = new();
            PaymentIntent response = service.Create(options);
            shoppingCart.StripePaymentIntentId = response.Id;
            shoppingCart.ClientSecret = response.ClientSecret;

            #endregion

            _response.Result = shoppingCart;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}
