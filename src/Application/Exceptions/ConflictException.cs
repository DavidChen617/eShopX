using System.Net;
using CoreMesh.Result.Exceptions;

namespace eShopX.Application.Exceptions;

/// <summary>
/// 資源衝突（HTTP 409），例如：訂單已存在付款記錄、Email 已被註冊
/// </summary>
public sealed class ConflictException(string message)
    : AppException(message, HttpStatusCode.Conflict, "conflict");
