using System.Net;
using CoreMesh.Result.Exceptions;

namespace eShopX.Application.Exceptions;

/// <summary>
/// 資源不存在（HTTP 404）
/// </summary>
public sealed class NotFoundException(string message)
    : AppException(message, HttpStatusCode.NotFound, "not_found");
