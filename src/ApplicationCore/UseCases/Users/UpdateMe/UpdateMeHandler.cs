namespace ApplicationCore.UseCases.Users.UpdateMe;

public class UpdateMeHandler(IRepository<User> userRepository)
    : IRequestHandler<UpdateMeCommand>
{
    public async Task Handle(UpdateMeCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), command.UserId);

        user.Name = command.Name;
        user.Phone = command.Phone;
        user.Address = command.Address;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
