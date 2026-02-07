using System;

using eShopX.Common.Mapping;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace eShopX.Common.Test;

public class MappingTests
{
    private sealed class User
    {
        public string? First { get; init; }
        public string? Last { get; init; }
        public int Age { get; init; }
    }

    private sealed class UserDto
    {
        public string? FullName { get; set; }
        public int Age { get; set; }
    }

    private sealed class CustomCtorDto
    {
        public CustomCtorDto(string name) => Name = name;
        public string Name { get; }
        public int Age { get; set; }
    }

    private sealed class TestProfile : Profile
    {
        public TestProfile()
        {
            Create<User, UserDto>()
                .ForMember(d => d.FullName, s => s.First + " " + s.Last);
        }

        public MappingExpression<TSource, TDestination> Create<TSource, TDestination>()
            => CreateMap<TSource, TDestination>();
    }

    private sealed class ReverseProfile : Profile
    {
        public ReverseProfile()
        {
            Create<User, UserDto>()
                .ForMember(d => d.FullName, s => s.First)
                .ReverseMap();
        }

        public MappingExpression<TSource, TDestination> Create<TSource, TDestination>()
            => CreateMap<TSource, TDestination>();
    }

    [Fact]
    public void Mapper_Maps_WithCustomMember()
    {
        var config = new MapperConfiguration([new TestProfile()]);
        var mapper = new Mapper(config);

        var dto = mapper.Map<User, UserDto>(new User { First = "A", Last = "B", Age = 20 });

        Assert.Equal("A B", dto.FullName);
        Assert.Equal(20, dto.Age);
    }

    [Fact]
    public void Mapper_Maps_FromObject_Overload()
    {
        var config = new MapperConfiguration([new TestProfile()]);
        var mapper = new Mapper(config);

        var dto = mapper.Map<UserDto>(new User { First = "A", Last = "B", Age = 20 });

        Assert.Equal("A B", dto.FullName);
    }

    [Fact]
    public void ConstructUsing_Allows_NoParameterlessCtor()
    {
        var profile = new InlineProfile(p =>
            p.Create<User, CustomCtorDto>().ConstructUsing(u => new CustomCtorDto(u.First ?? "NA")));
        var config = new MapperConfiguration([profile]);
        var mapper = new Mapper(config);

        var dto = mapper.Map<User, CustomCtorDto>(new User { First = "X", Age = 3 });

        Assert.Equal("X", dto.Name);
        Assert.Equal(3, dto.Age);
    }

    [Fact]
    public void ConstructUsing_Missing_Throws()
    {
        var profile = new InlineProfile(p => p.Create<User, CustomCtorDto>());
        Assert.Throws<InvalidOperationException>(() => _ = new MapperConfiguration([profile]));
    }

    [Fact]
    public void AssertConfigurationIsValid_FindsUnmapped()
    {
        var profile = new InlineProfile(p => p.Create<User, UserDto>());
        var config = new MapperConfiguration([profile]);

        var ex = Assert.Throws<InvalidOperationException>(() => config.AssertConfigurationIsValid());
        Assert.Contains("Unmapped properties", ex.Message);
    }

    [Fact]
    public void ReverseMap_CreatesReverseDefinition()
    {
        var config = new MapperConfiguration([new ReverseProfile()]);
        var mapper = new Mapper(config);

        var user = mapper.Map<UserDto, User>(new UserDto { FullName = "OnlyFirst", Age = 9 });

        Assert.Equal("OnlyFirst", user.First);
        Assert.Equal(9, user.Age);
    }

    [Fact]
    public void AddMapping_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddMapping(typeof(TestProfile).Assembly);
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IMapper>();
        var dto = mapper.Map<ServiceSource, ServiceDest>(new ServiceSource { First = "A", Last = "B" });

        Assert.Equal("A B", dto.FullName);
    }

    [Fact]
    public void Mapper_Throws_WhenNoPlan()
    {
        var config = new MapperConfiguration([]);
        var mapper = new Mapper(config);

        Assert.Throws<InvalidOperationException>(() => mapper.Map<User, UserDto>(new User()));
    }

    private sealed class InlineProfile : Profile
    {
        public InlineProfile() { }

        public InlineProfile(Action<InlineProfile> build)
        {
            build(this);
        }

        public MappingExpression<TSource, TDestination> Create<TSource, TDestination>()
            => CreateMap<TSource, TDestination>();
    }

    private sealed class ServiceSource
    {
        public string? First { get; init; }
        public string? Last { get; init; }
    }

    private sealed class ServiceDest
    {
        public string? FullName { get; set; }
    }

    private sealed class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<ServiceSource, ServiceDest>()
                .ForMember(d => d.FullName, s => s.First + " " + s.Last);
        }
    }
}
