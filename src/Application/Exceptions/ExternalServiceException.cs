using System.Net;
using CoreMesh.Result.Exceptions;

namespace Application.Exceptions;

/// <summary>
/// 外部服務呼叫失敗（HTTP 502），例如：LinePay、PayPal、ECPay、Google、LINE 回傳錯誤
/// </summary>
public sealed class ExternalServiceException(string service, string detail)
    : AppException($"{service} 回傳錯誤：{detail}", HttpStatusCode.BadGateway, "external_service_error")
{
    /// <summary>外部服務名稱（如 LinePay、Google）</summary>
    public string Service { get; } = service;
}
