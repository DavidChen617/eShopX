using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;

namespace eShopX.Application.UseCases.Auth;

public record RegisterUserCommand(
    string Name,
    string Email,
    string Password,
    string Otp) : IRequest<Result<RegisterUserResponse>>, IValidatable<RegisterUserCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<RegisterUserCommand> builder)
    {
        builder.For(x => x.Name)
            .NotEmpty("Name is required.")
            .MaxLength(100, "Name must be at most 100 characters.");

        builder.For(x => x.Email)
            .NotEmpty("Email is required.")
            .EmailAddress("Email format is invalid.");

        builder.For(x => x.Password)
            .NotEmpty("Password is required.")
            .MinLength(8, "Password must be at least 8 characters.");

        builder.For(x => x.Otp)
            .NotEmpty("OTP is required.");
    }
}

public record RegisterUserResponse(Guid UserId, string Email, DateTime CreatedAt)
    : IMapFrom<User, RegisterUserResponse>
{
    public RegisterUserResponse() : this(default, null!, default) { }

    public RegisterUserResponse MapFrom(User source) =>
        new(source.Id, source.Email, source.CreatedAt);
}

public class RegisterUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ICacher cacher,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator validator) : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result<RegisterUserResponse>.Invalid(validation.Errors);

        var cachedOtp = await cacher.GetAsync<string>(OtpCacheKeys.Otp(command.Email), cancellationToken);
        if (cachedOtp is null || cachedOtp != command.Otp)
            return Result<RegisterUserResponse>.BadRequest(new Error("otp_invalid", "OTP is invalid or expired."));

        var existing = await userRepository.FindByEmailAsync(command.Email, cancellationToken);
        if (existing is not null)
            return Result<RegisterUserResponse>.BadRequest(
                new Error("email_conflict", $"Email {command.Email} is already registered."));

        var user = User.Create(command.Name, command.Email);
        user.AddAuthProvider(Provider.Local, null, passwordHasher.HashPassword(command.Password));

        await userRepository.AddAsync(user, cancellationToken);
        await cacher.RemoveAsync(OtpCacheKeys.Otp(command.Email), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RegisterUserResponse>.Ok(mapper.Map<User, RegisterUserResponse>(user));
    }
}
