using Godot;
using JetBrains.Annotations;
using FirstProject.ShopLogic;

public partial class GrillStation : StaticBody3D
{
    [Export] public float CookTimeSeconds { get; set; } = 5.0f;
    [Export] public float BurnTimeSeconds { get; set; } = 8.0f;

    private Label3D label;
    private MeshInstance3D grillIndicator;
    private float cookTimer;
    private GrillState currentState = GrillState.Empty;

    public GrillState CurrentState => currentState;

    public enum GrillState
    {
        Empty,
        Cooking,
        Ready,
        Burnt
    }

    [UsedImplicitly]
    public override void _Ready()
    {
        label = GetNode<Label3D>("Label3D");
        label.Text = "Grill (Empty)";
        BuildVisuals();
    }

    [UsedImplicitly]
    public override void _Process(double delta)
    {
        if (currentState == GrillState.Cooking)
        {
            cookTimer += (float)delta;
            float remaining = CookTimeSeconds - cookTimer;

            if (remaining > 0)
            {
                label.Text = $"Cooking... {remaining:F0}s";
                UpdateIndicatorColor(LerpColor(RawColor, CookedColor, cookTimer / CookTimeSeconds));
            }
            else if (cookTimer < CookTimeSeconds + BurnTimeSeconds)
            {
                currentState = GrillState.Ready;
                label.Text = "Chicken Ready!";
                UpdateIndicatorColor(CookedColor);
            }
        }
        else if (currentState == GrillState.Ready)
        {
            cookTimer += (float)delta;
            float burnProgress = (cookTimer - CookTimeSeconds) / BurnTimeSeconds;

            if (burnProgress >= 1.0f)
            {
                currentState = GrillState.Burnt;
                label.Text = "BURNT!";
                UpdateIndicatorColor(BurntColor);
            }
            else
            {
                float timeLeft = BurnTimeSeconds - (cookTimer - CookTimeSeconds);
                label.Text = $"Ready! ({timeLeft:F0}s)";
                UpdateIndicatorColor(LerpColor(CookedColor, BurntColor, burnProgress));
            }
        }
    }

    public bool TryPlaceChicken()
    {
        if (currentState != GrillState.Empty)
        {
            return false;
        }

        currentState = GrillState.Cooking;
        cookTimer = 0;
        grillIndicator.Visible = true;
        UpdateIndicatorColor(RawColor);
        GD.Print("Chicken placed on grill!");
        return true;
    }

    public IngredientType? TryTakeChicken()
    {
        switch (currentState)
        {
            case GrillState.Ready:
                Reset();
                GD.Print("Picked up cooked chicken!");
                return IngredientType.CookedChicken;
            case GrillState.Burnt:
                Reset();
                GD.Print("Picked up burnt chicken (useless)!");
                return null;
            default:
                return null;
        }
    }

    private void Reset()
    {
        currentState = GrillState.Empty;
        cookTimer = 0;
        label.Text = "Grill (Empty)";
        grillIndicator.Visible = false;
    }

    private void BuildVisuals()
    {
        GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;

        // Grill base (dark metal table)
        var tableTop = CreateMesh(
            new BoxMesh { Size = new Vector3(1.2f, 0.08f, 0.8f) },
            new Color(0.15f, 0.15f, 0.15f),
            new Vector3(0, -0.42f, 0));
        AddChild(tableTop);

        // Grill grate (slightly raised, dark with red tint)
        var grate = CreateMesh(
            new BoxMesh { Size = new Vector3(1.0f, 0.03f, 0.6f) },
            new Color(0.3f, 0.1f, 0.05f),
            new Vector3(0, -0.35f, 0));
        AddChild(grate);

        // Legs
        float legX = 0.5f;
        float legZ = 0.3f;
        var legMesh = new CylinderMesh { TopRadius = 0.04f, BottomRadius = 0.04f, Height = 0.4f };
        var legColor = new Color(0.2f, 0.2f, 0.2f);
        AddChild(CreateMesh(legMesh, legColor, new Vector3(-legX, -0.25f, -legZ)));
        AddChild(CreateMesh(legMesh, legColor, new Vector3(legX, -0.25f, -legZ)));
        AddChild(CreateMesh(legMesh, legColor, new Vector3(-legX, -0.25f, legZ)));
        AddChild(CreateMesh(legMesh, legColor, new Vector3(legX, -0.25f, legZ)));

        // Chicken indicator (hidden until placed)
        grillIndicator = CreateMesh(
            new CylinderMesh { TopRadius = 0.15f, BottomRadius = 0.15f, Height = 0.1f },
            RawColor,
            new Vector3(0, -0.28f, 0));
        grillIndicator.Visible = false;
        AddChild(grillIndicator);

        // Glow effect - small OmniLight for the grill
        var grillLight = new OmniLight3D();
        grillLight.Position = new Vector3(0, -0.2f, 0);
        grillLight.OmniRange = 1.5f;
        grillLight.LightEnergy = 0.5f;
        grillLight.LightColor = new Color(1f, 0.4f, 0.1f);
        AddChild(grillLight);
    }

    private void UpdateIndicatorColor(Color color)
    {
        if (grillIndicator?.MaterialOverride is StandardMaterial3D mat)
        {
            mat.AlbedoColor = color;
        }
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        t = Mathf.Clamp(t, 0, 1);
        return new Color(
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t);
    }

    private static MeshInstance3D CreateMesh(Mesh mesh, Color color, Vector3 position)
    {
        var instance = new MeshInstance3D();
        instance.Mesh = mesh;
        var mat = new StandardMaterial3D { AlbedoColor = color };
        instance.MaterialOverride = mat;
        instance.Position = position;
        return instance;
    }

    private static readonly Color RawColor = new(0.9f, 0.7f, 0.5f);
    private static readonly Color CookedColor = new(0.7f, 0.45f, 0.15f);
    private static readonly Color BurntColor = new(0.15f, 0.1f, 0.05f);
}
