using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eShopX.Endpoints.FlashSales;

public class FlashSaleWarmUpEndpoint : IGroupedEndpoint<FlashSaleGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("warmup", async (
                EShopContext db,
                IFlashSalePurchaseService flashSaleService) =>
            {
                var items = await db.FlashSaleItems
                    .Where(x => x.FlashSale.IsActive && x.StockRemaining > 0)
                    .Select(x => new { x.Id, x.StockRemaining, x.PurchaseLimit })
                    .ToListAsync();

                foreach (var item in items)
                    await flashSaleService.WarmUpStockAsync(item.Id, item.StockRemaining, item.PurchaseLimit);

                var responseItems = items.Select(x => new FlashSaleWarmUpGroupResponseItem(x.Id, x.StockRemaining, x.PurchaseLimit)).ToList();
                var response = new FlashSaleWarmUpResponse("庫存預熱完成",  items.Count, responseItems);
                
                return Results.Ok(ApiResponse<FlashSaleWarmUpResponse>.Success(response));
            })
            .Produces<FlashSaleWarmUpResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("FlashSale")
            .RequireAuthorization("Admin");
    }
}

public record FlashSaleWarmUpResponse(string Message, int ItemCount, List<FlashSaleWarmUpGroupResponseItem> Items);
public record FlashSaleWarmUpGroupResponseItem(Guid Id, int StockRemaining, int PurchaseLimit);

