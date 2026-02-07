using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using eShopX.Common.Validation;

using Xunit;

namespace eShopX.Common.Test;

public class ValidationTests
{
    private sealed class Sample
    {
        public string? Name { get; init; }
        public string? Email { get; init; }
        public string? Code { get; init; }
        public int Age { get; init; }
        public int ExpectedAge { get; init; }
        public int Rating { get; init; }
        public DayOfWeek? Day { get; init; }
        public List<int>? Items { get; init; }
        public string? Token { get; init; }
    }

    private sealed class EmptyValidator : AbstractValidator<Sample>
    {
        public IRuleBuilder<Sample, TProperty> AddRule<TProperty>(System.Linq.Expressions.Expression<Func<Sample, TProperty>> expr)
            => RuleFor(expr);
    }

    [Fact]
    public void RuleFor_InvalidExpression_Throws()
    {
        var validator = new EmptyValidator();
        Assert.Throws<ArgumentException>(() => validator.AddRule(x => x.Age + 1));
    }

    [Fact]
    public void WithMessage_BeforeRule_UsesCustomMessage()
    {
        var validator = new InlineValidator(v => v.RuleFor(x => x.Name).WithMessage("custom").NotEmpty());
        var result = validator.Validate(new Sample { Name = "" });

        Assert.Single(result.Errors);
        Assert.Equal("custom", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void WithMessage_AfterRule_OverridesLastRuleMessage()
    {
        var validator = new InlineValidator(v => v.RuleFor(x => x.Name).NotEmpty().WithMessage("override"));
        var result = validator.Validate(new Sample { Name = "" });

        Assert.Single(result.Errors);
        Assert.Equal("override", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Or_CombinesRules_AsExpected()
    {
        var validator = new InlineValidator(v => v.RuleFor(x => x.Name).NotEmpty().Or().Equal("ok"));

        var valid = validator.Validate(new Sample { Name = "ok" });
        var invalid = validator.Validate(new Sample { Name = "" });

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
    }

    [Fact]
    public void And_ResetsOrChain()
    {
        var validator = new InlineValidator(v =>
            v.RuleFor(x => x.Name)
                .NotEmpty()
                .Or().Equal("ok")
                .And().MinLength(3));

        var result = validator.Validate(new Sample { Name = "ok" });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void When_SkipsRule_WhenConditionFalse()
    {
        var validator = new InlineValidator(v => v.RuleFor(x => x.Name).NotEmpty().When(x => x.Age > 18));
        var result = validator.Validate(new Sample { Name = "", Age = 10 });
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AsyncRules_And_Comparisons_Work()
    {
        var validator = new InlineValidator(v =>
        {
            v.RuleFor(x => x.Token).MustAsync(async (t, ct) =>
            {
                await Task.Delay(1, ct);
                return t == "ok";
            });
            v.RuleFor(x => x.Age).GreaterThan(0).LessThan(120).Equal(x => x.ExpectedAge);
            v.RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        });

        var result = await validator.ValidateAsync(new Sample
        {
            Token = "bad",
            Age = 10,
            ExpectedAge = 11,
            Rating = 6
        });

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void StringRules_Work()
    {
        var validator = new InlineValidator(v =>
        {
            v.RuleFor(x => x.Name).MinLength(2).MaxLength(5);
            v.RuleFor(x => x.Email).Email();
            v.RuleFor(x => x.Code).Matches(@"^\d{2}$");
        });

        var result = validator.Validate(new Sample
        {
            Name = "too-long",
            Email = "bad@",
            Code = "A1"
        });

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void NotEmpty_HandlesEnumerables()
    {
        var validator = new InlineValidator(v => v.RuleFor(x => x.Items).NotEmpty());

        var emptyResult = validator.Validate(new Sample { Items = new List<int>() });
        var nonEmptyResult = validator.Validate(new Sample { Items = new List<int> { 1 } });

        Assert.False(emptyResult.IsValid);
        Assert.True(nonEmptyResult.IsValid);
    }

    [Fact]
    public void IsInEnum_ValidatesEnumValues()
    {
        var validator = new InlineValidator(v => v.RuleFor(x => x.Day).IsInEnum());

        var valid = validator.Validate(new Sample { Day = DayOfWeek.Monday });
        var invalid = validator.Validate(new Sample { Day = (DayOfWeek)999 });

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
    }

    private sealed class InlineValidator : AbstractValidator<Sample>
    {
        public InlineValidator(Action<InlineValidator> build)
        {
            build(this);
        }
    }
}
