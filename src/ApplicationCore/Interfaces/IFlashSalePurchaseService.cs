namespace ApplicationCore.Interfaces;

public interface IFlashSalePurchaseService
{
    /// <summary>                                                                                                                              
    /// 嘗試預扣庫存                                                                                                                           
    /// </summary>                                                                                                                             
    /// <param name="flashSaleItemId">閃購商品 ID</param>                                                                                      
    /// <param name="userId">用戶 ID</param>                                                                                                   
    /// <param name="quantity">購買數量</param>                                                                                                
    /// <returns>                                                                                                                              
    /// 成功: 剩餘庫存                                                                                                                         
    /// -1: 超過限購                                                                                                                           
    /// -2: 庫存不足                                                                                                                           
    /// </returns>
    Task<long> TryDeductStockAsync(Guid flashSaleItemId, Guid userId, int quantity);
    
    /// <summary>                                                                                                                              
    /// 回滾庫存（訂單失敗時）                                                                                                                 
    /// </summary>
    Task RollbackStockAsync(Guid flashSaleItemId, Guid userId, int quantity);
    
    /// <summary>                                                                                                                              
    /// 預熱庫存到 Redis                                                                                                                       
    /// </summary> 
    Task WarmUpStockAsync(Guid flashSaleItemId, int stock, int purchaseLimit);
}
