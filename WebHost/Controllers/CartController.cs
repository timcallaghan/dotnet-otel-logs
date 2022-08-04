using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebHost.Models.Responses;

namespace WebHost.Controllers;

[ApiController]
[Route("carts")]
public class CartController : ControllerBase
{
    private static readonly string[] CartItems = 
    {
        "Milk", "Bread", "Apple", "Orange", "Peas", "Rice", "Yoghurt", "Garlic", "Ginger", "Oil"
    };

    private readonly ILogger<CartController> _logger;

    public CartController(ILogger<CartController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("{id:int}")]
    public IActionResult Get(int id)
    {
        if (id is < 1 or > 10)
        {
            return Problem($"Cart {id} does not exist", statusCode: (int)HttpStatusCode.NotFound);
        }
        
        _logger.LogInformation("Getting cart details for {CartId}", id);

        var cartItems = Enumerable.Range(1, id)
            .Select(itemId => new CartItem(itemId, CartItems[Random.Shared.Next(CartItems.Length)]))
            .ToList();
        var cart = new Cart(id, cartItems);
        
        _logger.LogInformation("Responding with cart details: {@Cart}", new { CartId = cart.Id, ItemsCount = cart.CartItems.Count });

        return Ok(cart);
    }
}