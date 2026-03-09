using Godot;
using JetBrains.Annotations;
using FirstProject.ShopLogic;

public partial class HUD : CanvasLayer
{
    private Label timerLabel;
    private Label scoreLabel;
    private VBoxContainer orderSlots;
    private VBoxContainer heldSlots;
    private Label shawarmaLabel;
    private Label dropHint;
    private Label promptLabel;
    private Label waitingLabel;

    private string[] currentOrderIngredients = [];
    private string[] currentHeldIngredients = [];

    private static readonly Color ColorChicken = new(0.85f, 0.65f, 0.3f);
    private static readonly Color ColorTomato = new(0.9f, 0.2f, 0.15f);
    private static readonly Color ColorGarlic = new(0.95f, 0.95f, 0.85f);
    private static readonly Color ColorFlatbread = new(0.85f, 0.75f, 0.5f);
    private static readonly Color ColorCollected = new(0.3f, 0.9f, 0.3f);

    [UsedImplicitly]
    public override void _Ready()
    {
        timerLabel = GetNode<Label>("TopLeft/VBox/TimerLabel");
        scoreLabel = GetNode<Label>("TopLeft/VBox/ScoreLabel");
        orderSlots = GetNode<VBoxContainer>("OrderPanel/VBox/OrderSlots");
        heldSlots = GetNode<VBoxContainer>("HeldPanel/VBox/HeldSlots");
        shawarmaLabel = GetNode<Label>("HeldPanel/VBox/ShawarmaLabel");
        dropHint = GetNode<Label>("HeldPanel/VBox/DropHint");
        promptLabel = GetNode<Label>("PromptContainer/PromptLabel");
        waitingLabel = GetNode<Label>("WaitingLabel");

        var gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.ScoreChanged += OnScoreChanged;
        gameManager.TimeChanged += OnTimeChanged;
        gameManager.OrderChanged += OnOrderChanged;

        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        player.IngredientsChanged += OnIngredientsChanged;
        player.InteractionPromptChanged += OnInteractionPromptChanged;
        player.ShawarmaReadyChanged += OnShawarmaReadyChanged;
    }

    private void OnShawarmaReadyChanged(bool ready)
    {
        shawarmaLabel.Text = ready ? ">>> SHAWARMA READY <<<" : "";
        if (ready)
        {
            shawarmaLabel.AddThemeColorOverride("font_color", ColorCollected);
        }
    }

    private void OnScoreChanged(int score)
    {
        scoreLabel.Text = $"Score: {score}";
    }

    private void OnTimeChanged(float timeRemaining)
    {
        int minutes = (int)(timeRemaining / 60);
        int seconds = (int)(timeRemaining % 60);
        timerLabel.Text = $"Time: {minutes}:{seconds:D2}";
    }

    private void OnOrderChanged(string orderText)
    {
        if (string.IsNullOrEmpty(orderText))
        {
            waitingLabel.Text = "Waiting for customer...";
            currentOrderIngredients = [];
            RebuildOrderSlots();
            return;
        }

        waitingLabel.Text = "";
        currentOrderIngredients = orderText.Split(" + ");
        RebuildOrderSlots();
    }

    private void OnIngredientsChanged(string[] ingredients)
    {
        currentHeldIngredients = ingredients;
        bool hasAnything = ingredients.Length > 0;
        dropHint.Visible = hasAnything;
        shawarmaLabel.Text = "";
        RebuildHeldSlots();
        RebuildOrderSlots(); // Update checkmarks
    }

    private void OnInteractionPromptChanged(string prompt)
    {
        promptLabel.Text = prompt;
        promptLabel.Visible = !string.IsNullOrEmpty(prompt);
    }

    private void RebuildOrderSlots()
    {
        foreach (var child in orderSlots.GetChildren())
        {
            child.QueueFree();
        }

        foreach (string ingredient in currentOrderIngredients)
        {
            bool collected = IsIngredientHeld(ingredient);
            var slot = CreateSlot(ingredient, GetIngredientColor(ingredient), collected);
            orderSlots.AddChild(slot);
        }
    }

    private void RebuildHeldSlots()
    {
        foreach (var child in heldSlots.GetChildren())
        {
            child.QueueFree();
        }

        if (currentHeldIngredients.Length == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "(empty)";
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            emptyLabel.AddThemeFontSizeOverride("font_size", 16);
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            heldSlots.AddChild(emptyLabel);
            return;
        }

        foreach (string ingredient in currentHeldIngredients)
        {
            var slot = CreateSlot(ingredient, GetIngredientColor(ingredient), false);
            heldSlots.AddChild(slot);
        }
    }

    private bool IsIngredientHeld(string ingredientName)
    {
        foreach (string held in currentHeldIngredients)
        {
            if (held == ingredientName || held == ingredientName.Replace(" ", ""))
            {
                return true;
            }

            // CookedChicken satisfies the "Chicken" requirement in orders
            if (ingredientName == "Chicken" && held == "CookedChicken")
            {
                return true;
            }
        }
        return false;
    }

    private static HBoxContainer CreateSlot(string name, Color color, bool showCheck)
    {
        var hbox = new HBoxContainer();

        var colorRect = new ColorRect();
        colorRect.CustomMinimumSize = new Vector2(20, 20);
        colorRect.Color = color;
        hbox.AddChild(colorRect);

        var label = new Label();
        label.Text = showCheck ? $"  {name}  [OK]" : $"  {name}";
        label.AddThemeFontSizeOverride("font_size", 18);
        if (showCheck)
        {
            label.AddThemeColorOverride("font_color", ColorCollected);
        }
        hbox.AddChild(label);

        return hbox;
    }

    private static readonly Color ColorCookedChicken = new(0.7f, 0.45f, 0.15f);

    private static Color GetIngredientColor(string name) => name switch
    {
        "Chicken" => ColorChicken,
        "CookedChicken" => ColorCookedChicken,
        "Tomato" => ColorTomato,
        "Garlic Sauce" or "GarlicSauce" => ColorGarlic,
        "Flatbread" => ColorFlatbread,
        _ => new Color(0.7f, 0.7f, 0.7f)
    };
}
