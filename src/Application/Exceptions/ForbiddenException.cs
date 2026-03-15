using System.Net;
using CoreMesh.Result.Exceptions;

namespace eShopX.Application.Exceptions;

/// <summary>
/// 已驗證身份但無操作權限（HTTP 403），例如：使用者存取他人訂單
/// </summary>
public sealed class ForbiddenException(string message)
    : AppException(message, HttpStatusCode.Forbidden, "forbidden");
