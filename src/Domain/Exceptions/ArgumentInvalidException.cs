using System.Net;
using CoreMesh.Result.Exceptions;

namespace Domain.Exceptions;

public sealed class ArgumentInvalidException(string message)
    : AppException(message, HttpStatusCode.UnprocessableEntity, "argument_invalid");
