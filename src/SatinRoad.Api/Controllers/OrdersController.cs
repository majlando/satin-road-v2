namespace SatinRoad.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(CurrentUser currentUser, PurchaseService purchases) : ControllerBase
{
    /// <summary>Buy a listing. The answer says whether it completed or was seized.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Buy(OrderRequest request)
    {
        var buyer = await currentUser.RequireActiveAsync();
        var order = await purchases.BuyAsync(buyer, request.ListingId, request.Quantity);
        return Created($"/api/orders/{order.Id}", OrderDto.From(order));
    }
}