namespace ApplicationCore.UseCases.Auth.Register;

public class RegisterUserHandler(
    IRepository<User> userRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.FirstOrDefaultAsync(
            u => u.Email == command.Email, cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException($"Email {command.Email} already registered");
        }

        User user = new()
        {
            Name = command.Name,
            Email = command.Email,
            Phone = command.Phone,
            PasswordHash = passwordHasher.HashPassword(command.Password)
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(user.Id, user.Email, user.CreatedAt);
    }
}
