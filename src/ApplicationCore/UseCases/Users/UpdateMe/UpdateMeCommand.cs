namespace ApplicationCore.UseCases.Users.UpdateMe;

public record UpdateMeCommand(
    Guid UserId,
    string Name,
    string Phone,
    string? Address) : IRequest;
