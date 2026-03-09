using FirstProject.ShopLogic;
using Shouldly;
using Xunit;

namespace FirstProject.ShopLogic.Tests;

public class OrderGeneratorTests
{
    [Fact]
    public void GenerateOrder_AlwaysIncludesFlatbread()
    {
        var generator = new OrderGenerator(new Random(42));

        for (int i = 0; i < 50; i++)
        {
            var order = generator.GenerateOrder();
            order.RequiredIngredients.ShouldContain(IngredientType.Flatbread);
        }
    }

    [Fact]
    public void GenerateOrder_HasTwoToFourIngredients()
    {
        var generator = new OrderGenerator(new Random(42));

        for (int i = 0; i < 50; i++)
        {
            var order = generator.GenerateOrder();
            order.RequiredIngredients.Count.ShouldBeInRange(2, 4);
        }
    }

    [Fact]
    public void GenerateOrder_NoDuplicateIngredients()
    {
        var generator = new OrderGenerator(new Random(42));

        for (int i = 0; i < 50; i++)
        {
            var order = generator.GenerateOrder();
            order.RequiredIngredients.Distinct().Count().ShouldBe(order.RequiredIngredients.Count);
        }
    }

    [Fact]
    public void Order_IsSatisfiedBy_CorrectIngredients()
    {
        var order = new Order(new[] { IngredientType.Flatbread, IngredientType.Chicken, IngredientType.Tomato });

        order.IsSatisfiedBy(new[] { IngredientType.Tomato, IngredientType.Chicken, IngredientType.Flatbread })
            .ShouldBeTrue();
    }

    [Fact]
    public void Order_IsSatisfiedBy_RejectsWrongIngredients()
    {
        var order = new Order(new[] { IngredientType.Flatbread, IngredientType.Chicken });

        order.IsSatisfiedBy(new[] { IngredientType.Flatbread, IngredientType.Tomato })
            .ShouldBeFalse();
    }

    [Fact]
    public void Order_IsSatisfiedBy_RejectsMissingIngredients()
    {
        var order = new Order(new[] { IngredientType.Flatbread, IngredientType.Chicken, IngredientType.Tomato });

        order.IsSatisfiedBy(new[] { IngredientType.Flatbread, IngredientType.Chicken })
            .ShouldBeFalse();
    }

    [Fact]
    public void Order_GetDisplayText_FormatsCorrectly()
    {
        var order = new Order(new[] { IngredientType.Chicken, IngredientType.GarlicSauce, IngredientType.Flatbread });

        var text = order.GetDisplayText();
        text.ShouldContain("Chicken");
        text.ShouldContain("Garlic Sauce");
        text.ShouldContain("Flatbread");
    }
}
