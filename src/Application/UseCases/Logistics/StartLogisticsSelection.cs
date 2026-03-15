using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;

namespace eShopX.Application.UseCases.Logistics;

public record StartLogisticsSelectionCommand(Guid UserId) : IRequest<Result<StartLogisticsSelectionResponse>>;

public record StartLogisticsSelectionResponse(string Token);

public class StartLogisticsSelectionHandler(ICacher cacher)
    : IRequestHandler<StartLogisticsSelectionCommand, Result<StartLogisticsSelectionResponse>>
{
    public async Task<Result<StartLogisticsSelectionResponse>> Handle(
        StartLogisticsSelectionCommand command,
        CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString();
        await cacher.SetAsync(
            LogisticsCacheKeys.LogisticsSession(token),
            command.UserId,
            TimeSpan.FromMinutes(15),
            cancellationToken);

        return Result<StartLogisticsSelectionResponse>.Ok(new StartLogisticsSelectionResponse(token));
    }
}
