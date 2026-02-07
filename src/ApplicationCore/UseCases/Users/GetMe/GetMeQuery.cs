namespace ApplicationCore.UseCases.Users.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<GetMeResponse>;
