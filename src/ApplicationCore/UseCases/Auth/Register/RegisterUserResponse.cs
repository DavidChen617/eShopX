namespace ApplicationCore.UseCases.Auth.Register;

public record RegisterUserResponse(Guid UserId, string Email, DateTime CreatedAt);