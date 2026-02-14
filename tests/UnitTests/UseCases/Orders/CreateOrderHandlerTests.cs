using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Orders;
using ApplicationCore.UseCases.Orders.CreateOrder;

using eShopX.Common.Exceptions;
using eShopX.Common.Mapping;

using Moq;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Orders;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_NoItems_ThrowsValidationException()
    {
        var orderRepo = new InMemoryRepository<Order>();
        var productRepo = new InMemoryRepository<Product>();
        var userRepo = new InMemoryRepository<User>();
        var unitOfWork = CreateUnitOfWorkMock();
        var mapper = new Mock<IMapper>();
        var handler = new CreateOrderHandler(orderRepo, productRepo, userRepo, unitOfWork.Object, mapper.Object);

        var command = new CreateOrderCommand(Guid.NewGuid(), "Name", "Addr", "Phone", []);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateProductIds_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var orderRepo = new InMemoryRepository<Order>();
        var productRepo = new InMemoryRepository<Product>();
        var userRepo = new InMemoryRepository<User>([new User { Id = userId }]);
        var unitOfWork = CreateUnitOfWorkMock();
        var mapper = new Mock<IMapper>();
        var handler = new CreateOrderHandler(orderRepo, productRepo, userRepo, unitOfWork.Object, mapper.Object);

        var itemId = Guid.NewGuid();
        var command = new CreateOrderCommand(userId, "Name", "Addr", "Phone",
        [
            new OrderItemDto(itemId, 1),
            new OrderItemDto(itemId, 2)
        ]);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InactiveProduct_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), IsActive = false, StockQuantity = 5, Price = 10 };
        var orderRepo = new InMemoryRepository<Order>();
        var productRepo = new InMemoryRepository<Product>([product]);
        var userRepo = new InMemoryRepository<User>([new User { Id = userId }]);
        var unitOfWork = CreateUnitOfWorkMock();
        var mapper = new Mock<IMapper>();
        var handler = new CreateOrderHandler(orderRepo, productRepo, userRepo, unitOfWork.Object, mapper.Object);

        var command = new CreateOrderCommand(userId, "Name", "Addr", "Phone",
            [new OrderItemDto(product.Id, 1)]);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Success_CreatesPaidOrderAndReducesStock()
    {
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), IsActive = true, StockQuantity = 5, Price = 10, Name = "Item" };
        var orderRepo = new InMemoryRepository<Order>();
        var productRepo = new InMemoryRepository<Product>([product]);
        var userRepo = new InMemoryRepository<User>([new User { Id = userId }]);
        var unitOfWork = CreateUnitOfWorkMock();
        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<Order, CreateOrderResponse>(It.IsAny<Order>()))
            .Returns((Order o) => new CreateOrderResponse(o.Id, o.TotalAmount, o.Status, o.CreatedAt, o.PaymentMethod, o.PaidAt));

        var handler = new CreateOrderHandler(orderRepo, productRepo, userRepo, unitOfWork.Object, mapper.Object);

        var response = await handler.Handle(new CreateOrderCommand(userId, "Name", "Addr", "Phone",
            [new OrderItemDto(product.Id, 2)]), CancellationToken.None);

        Assert.Equal(OrderStatus.Paid, response.Status);
        Assert.Equal(20, response.TotalAmount);
        Assert.Equal(3, product.StockQuantity);
        Assert.Single(orderRepo.Data);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWorkMock()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        unitOfWork.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return unitOfWork;
    }
}
