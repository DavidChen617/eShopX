namespace ApplicationCore.Enums;

/// <summary>
/// 賣家申請狀態
/// </summary>
public enum SellerStatus
{
    /// <summary>
    /// 申請中（待審核）
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已通過
    /// </summary>
    Approved = 1,

    /// <summary>
    /// 已拒絕
    /// </summary>
    Rejected = 2
}