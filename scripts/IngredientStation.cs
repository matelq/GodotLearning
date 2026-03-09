using Godot;
using JetBrains.Annotations;
using FirstProject.ShopLogic;

public partial class IngredientStation : StaticBody3D
{
    public IngredientType Ingredient { get; private set; }

    private Label3D label;
    private MeshInstance3D indicator;
    private float baseIndicatorY;
    private float animTime;
    private float pulseTimer;

    [UsedImplicitly]
    public override void _Ready()
    {
        label = GetNode<Label3D>("Label3D");

        // Derive ingredient type from node name (e.g. "ChickenStation" -> Chicken)
        Ingredient = Name.ToString() switch
        {
            "ChickenStation" => IngredientType.Chicken,
            "TomatoStation" => IngredientType.Tomato,
            "GarlicSauceStation" => IngredientType.GarlicSauce,
            "FlatbreadStation" => IngredientType.Flatbread,
            _ => IngredientType.Chicken
        };

        string displayName = Ingredient switch
        {
            IngredientType.GarlicSauce => "Garlic Sauce",
            _ => Ingredient.ToString()
        };
        label.Text = displayName;

        BuildVisuals();
    }

    [UsedImplicitly]
    public override void _Process(double delta)
    {
        animTime += (float)delta;

        indicator.Position = new Vector3(0, baseIndicatorY + Mathf.Sin(animTime * 2.0f) * 0.08f, 0);
        indicator.RotateY((float)delta * 1.5f);

        if (pulseTimer > 0)
        {
            pulseTimer -= (float)delta;
            indicator.Scale = Vector3.One * (1.0f + pulseTimer * 1.5f);
        }
        else
        {
            indicator.Scale = Vector3.One;
        }
    }

    public void PlayPickupEffect()
    {
        pulseTimer = 0.3f;
    }

    private void BuildVisuals()
    {
        GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;

        // Table base
        CreateMesh(
            new BoxMesh { Size = new Vector3(1.2f, 0.1f, 0.8f) },
            new Color(0.35f, 0.22f, 0.1f),
            new Vector3(0, -0.45f, 0)
        );

        // Table legs
        var legMesh = new CylinderMesh { TopRadius = 0.04f, BottomRadius = 0.04f, Height = 0.4f };
        var legColor = new Color(0.35f, 0.22f, 0.1f);
        CreateMesh(legMesh, legColor, new Vector3(0.5f, -0.7f, 0.3f));
        CreateMesh(legMesh, legColor, new Vector3(-0.5f, -0.7f, 0.3f));
        CreateMesh(legMesh, legColor, new Vector3(0.5f, -0.7f, -0.3f));
        CreateMesh(legMesh, legColor, new Vector3(-0.5f, -0.7f, -0.3f));

        // Ingredient indicator
        switch (Ingredient)
        {
            case IngredientType.Chicken:
                indicator = CreateMesh(
                    new CylinderMesh { TopRadius = 0.15f, BottomRadius = 0.15f, Height = 0.12f },
                    new Color(0.85f, 0.65f, 0.3f),
                    new Vector3(0, 0.15f, 0)
                );
                baseIndicatorY = 0.15f;
                break;

            case IngredientType.Tomato:
                indicator = CreateMesh(
                    new SphereMesh { Radius = 0.15f },
                    new Color(0.9f, 0.2f, 0.15f),
                    new Vector3(0, 0.2f, 0)
                );
                baseIndicatorY = 0.2f;
                break;

            case IngredientType.GarlicSauce:
                indicator = CreateMesh(
                    new CylinderMesh { TopRadius = 0.06f, BottomRadius = 0.06f, Height = 0.25f },
                    new Color(0.95f, 0.95f, 0.85f),
                    new Vector3(0, 0.2f, 0)
                );
                baseIndicatorY = 0.2f;
                break;

            case IngredientType.Flatbread:
                indicator = CreateMesh(
                    new CylinderMesh { TopRadius = 0.2f, BottomRadius = 0.2f, Height = 0.03f },
                    new Color(0.85f, 0.75f, 0.5f),
                    new Vector3(0, 0.1f, 0)
                );
                baseIndicatorY = 0.1f;
                break;
        }
    }

    private MeshInstance3D CreateMesh(Mesh mesh, Color color, Vector3 position)
    {
        var meshInstance = new MeshInstance3D
        {
            Mesh = mesh,
            MaterialOverride = new StandardMaterial3D { AlbedoColor = color },
            Position = position
        };
        AddChild(meshInstance);
        return meshInstance;
    }
}
