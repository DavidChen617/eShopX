namespace ApplicationCore.UseCases.Users.GetMe;

public class GetMeHandler(
    IRepository<User> userRepository,
    IMapper mapper)
    : IRequestHandler<GetMeQuery, GetMeResponse>
{
    public async Task<GetMeResponse> Handle(GetMeQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), query.UserId);

        return mapper.Map<User, GetMeResponse>(user);
    }
}
