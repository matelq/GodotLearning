namespace FirstProject.ShopLogic;

public class OrderGenerator
{
    private readonly Random random;

    public OrderGenerator(Random? random = null)
    {
        this.random = random ?? new Random();
    }

    /// <summary>
    /// Generates a random order. Always includes Flatbread plus 1-3 other ingredients.
    /// Orders use CookedChicken (not raw Chicken) — player must cook it at the grill.
    /// </summary>
    public Order GenerateOrder(int minToppings = 1)
    {
        var toppings = new[] { IngredientType.CookedChicken, IngredientType.Tomato, IngredientType.GarlicSauce };
        int min = Math.Clamp(minToppings, 1, 3);
        int toppingCount = random.Next(min, 4); // min to 3 toppings

        var ingredients = new List<IngredientType> { IngredientType.Flatbread };

        // Shuffle toppings and pick the first N
        var shuffled = toppings.OrderBy(_ => random.Next()).Take(toppingCount);
        ingredients.AddRange(shuffled);

        return new Order(ingredients);
    }
}

public class Order
{
    public IReadOnlyList<IngredientType> RequiredIngredients { get; }

    public Order(IEnumerable<IngredientType> ingredients)
    {
        RequiredIngredients = ingredients.OrderBy(i => i).ToList().AsReadOnly();
    }

    /// <summary>
    /// Checks whether the provided ingredients satisfy this order (order-independent).
    /// </summary>
    public bool IsSatisfiedBy(IEnumerable<IngredientType> provided)
    {
        var sorted = provided.OrderBy(i => i).ToList();
        return sorted.SequenceEqual(RequiredIngredients);
    }

    public string GetDisplayText()
    {
        return string.Join(" + ", RequiredIngredients.Select(FormatIngredient));
    }

    private static string FormatIngredient(IngredientType type) => type switch
    {
        IngredientType.CookedChicken => "Chicken",
        IngredientType.GarlicSauce => "Garlic Sauce",
        _ => type.ToString()
    };
}
