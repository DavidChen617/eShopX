namespace ApplicationCore.UseCases.Auth.Register;

public record RegisterUserCommand(
    string Name,
    string Email,
    string Phone,
    string Password) : IRequest<RegisterUserResponse>;
