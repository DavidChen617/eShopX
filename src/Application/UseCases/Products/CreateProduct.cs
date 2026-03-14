using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Products;

namespace eShopX.Application.UseCases.Products;

public record CreateProductCommand(
    string Name,
    string? Description,
    Audience? Audience,
    Guid CategoryId,
    IReadOnlyList<Guid>? TagIds) : IRequest<Result<CreateProductResponse>>, IValidatable<CreateProductCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<CreateProductCommand> builder)
    {
        builder.For(x => x.Name)
            .NotEmpty("Product name is required.")
            .MaxLength(200, "Product name must be at most 200 characters.");
    }
}

public record CreateProductResponse(Guid ProductId, string Name, bool IsActive, DateTime CreatedAt)
    : IMapFrom<Product, CreateProductResponse>
{
    public CreateProductResponse MapFrom(Product source) =>
        new(source.Id, source.Name, source.IsActive, source.CreatedAt);
}

public class CreateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IValidator validator,
    IMapper mapper) : IRequestHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async Task<Result<CreateProductResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result<CreateProductResponse>.Invalid(validation.Errors);

        var product = Product.Create(command.Name, command.Description, command.Audience, command.CategoryId);

        foreach (var tagId in command.TagIds ?? [])
            product.AddTag(tagId);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateProductResponse>.Ok(mapper.Map<Product, CreateProductResponse>(product));
    }
}
