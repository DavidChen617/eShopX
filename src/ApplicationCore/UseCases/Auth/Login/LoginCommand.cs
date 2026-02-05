namespace ApplicationCore.UseCases.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;