using Godot;
using JetBrains.Annotations;

public partial class ShopLevel : Node3D
{
    [UsedImplicitly]
    public override void _Ready()
    {
        // Give the scene one frame to fully initialize before starting the game
        CallDeferred(nameof(StartGame));
        BuildEnvironment();
    }

    private void StartGame()
    {
        var gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.CustomerScene = ResourceLoader.Load<PackedScene>("res://scenes/Customer.tscn");
        gameManager.StartGame();
    }

    private void BuildEnvironment()
    {
        // Ceiling
        CreateBox(new Vector3(12, 0.2f, 14), new Vector3(0, 3.1f, -1), new Color(0.45f, 0.4f, 0.35f));

        // Serving window frame
        var frameColor = new Color(0.5f, 0.35f, 0.2f);
        CreateBox(new Vector3(0.15f, 3, 0.3f), new Vector3(-0.9f, 1.5f, -5), frameColor);   // Left pillar
        CreateBox(new Vector3(0.15f, 3, 0.3f), new Vector3(0.9f, 1.5f, -5), frameColor);    // Right pillar
        CreateBox(new Vector3(2, 0.3f, 0.3f), new Vector3(0, 2.85f, -5), frameColor);       // Top lintel
        CreateBox(new Vector3(2, 0.15f, 0.5f), new Vector3(0, 1.0f, -5), frameColor);       // Serving shelf

        // Interior warm lighting
        var light = new OmniLight3D();
        light.Position = new Vector3(0, 2.8f, 0);
        light.OmniRange = 12f;
        light.LightEnergy = 1.5f;
        light.LightColor = new Color(1, 0.95f, 0.85f);
        AddChild(light);
    }

    private void CreateBox(Vector3 size, Vector3 position, Color color)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = color },
            Position = position
        };
        AddChild(mesh);
    }
}
