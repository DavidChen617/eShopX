using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Outbox;

namespace eShopX.Application.UseCases.Auth;

public record SendOtpCommand(string Email) : IRequest<Result>, IValidatable<SendOtpCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<SendOtpCommand> builder)
    {
        builder.For(x => x.Email)
            .NotEmpty("Email is required.")
            .EmailAddress("Email format is invalid.");
    }
}

public class SendOtpHandler(
    IUserRepository userRepository,
    ICacher cacher,
    IOutboxEventRepository outboxEventRepository,
    IUnitOfWork unitOfWork,
    IValidator validator) : IRequestHandler<SendOtpCommand, Result>
{
    public async Task<Result> Handle(SendOtpCommand command, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors);

        var existing = await userRepository.FindByEmailAsync(command.Email, cancellationToken);
        if (existing is not null)
            return Result.BadRequest(new Error("email_conflict", $"Email {command.Email} is already registered."));

        var otp = Random.Shared.Next(100000, 999999).ToString();
        await cacher.SetAsync(OtpCacheKeys.Otp(command.Email), otp, TimeSpan.FromMinutes(10), cancellationToken);

        var outboxEvent = OutboxEventFactory.CreateOtpEmail(command.Email, otp);
        await outboxEventRepository.AddAsync(outboxEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public static class OtpCacheKeys
{
    public static string Otp(string email) => $"otp:{email}";
}
