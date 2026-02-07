using StackExchange.Redis;

namespace Infrastructure.Services;

public class FlashSalePurchaseService(IConnectionMultiplexer redis): IFlashSalePurchaseService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string StockKeyPrefix = "flashsale:stock:";
    private const string LimitKeyPrefix = "flashsale:limit:";
    private const string PurchaseLimitKeyPrefix = "flashsale:purchaselimit:";
    
    // Lua Script: Check purchase limit + check inventory + atomic discount
    private const string DeductStockScript = """
        local stock_key = KEYS[1]
        local user_limit_key = KEYS[2]
        local pruchase_limit_key = KEYS[3]
        local quantity = tonumber(ARGV[1])
        
        -- Get limited purchase quantity
        local pirchase_limit = tonumber(redis.call('GET', pruchase_limit_key) or 0)
        
        -- Check the quantity purchased by the user
        local user_bought = tonumber(redis.call('GET', user_limit_key) or 0)
        
        -- Check whether the purchase limit has been exceeded                                                                                                                            
        if pirchase_limit > 0 and user_bought + quantity > pirchase_limit then                                                                         
            return -2  -- Purchase limit exceeded                                                                                                                     
        end                                                                                                                                            
          
        -- Check inventory
        local stock = tonumber(redis.call('GET', stock_key) or 0)
        if stock < quantity then
            return -1 -- Insufficient stock
        end
        
        -- Deduct inventory
        redis.call('DECRBY', stock_key, quantity)
        -- Increase the number of purchases made by users
        redis.call('INCRBY', user_limit_key, quantity)
        -- Set the user purchase limit key expiration time (24 hours)
        redis.call('EXPIRE', user_limit_key, 86400)
        
        return stock - quantity
     """;
    
    public async Task<long> TryDeductStockAsync(Guid flashSaleItemId, Guid userId, int quantity)
    {
        var stockKey = StockKeyPrefix + flashSaleItemId;
        var userLimitKey = $"{LimitKeyPrefix}{flashSaleItemId}:{userId}";
        var purchaseLimitKey = PurchaseLimitKeyPrefix + flashSaleItemId;

        var result = await _database.ScriptEvaluateAsync(
            DeductStockScript, 
            new RedisKey[] { stockKey, userLimitKey, purchaseLimitKey },
                new RedisValue[] { quantity });

        return (long)result;
    }

    public async Task RollbackStockAsync(Guid flashSaleItemId, Guid userId, int quantity)
    {
        var stockKey = StockKeyPrefix + flashSaleItemId;
        var userLimitKey = $"{LimitKeyPrefix}{flashSaleItemId}:{userId}";
        
        // Rollback: add back inventory and reduce the quantity purchased by the user
        await _database.StringIncrementAsync(stockKey, quantity);                                                                                      
        await _database.StringDecrementAsync(userLimitKey, quantity);
    }

    public async Task WarmUpStockAsync(Guid flashSaleItemId, int stock, int purchaseLimit)
    {
        var stockKey = StockKeyPrefix + flashSaleItemId;
        var purchaseLimitKey = PurchaseLimitKeyPrefix + flashSaleItemId;
        
        // Set inventory and purchase limits, expiration time is 24 hours
        await _database.StringSetAsync(stockKey, stock, TimeSpan.FromHours(24));                                                                     
        await _database.StringSetAsync(purchaseLimitKey, purchaseLimit, TimeSpan.FromHours(24)); 
    }
}
