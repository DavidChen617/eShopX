namespace ApplicationCore.UseCases.Auth.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;