using Godot;
using JetBrains.Annotations;

public partial class ServeCounter : StaticBody3D
{
    private Label3D label;

    [UsedImplicitly]
    public override void _Ready()
    {
        label = GetNode<Label3D>("Label3D");
        label.Text = "Serve Here";

        BuildVisuals();
    }

    private void BuildVisuals()
    {
        // Keep existing MeshInstance3D visible (it's the counter)

        // Bell dome
        CreateMesh(
            new SphereMesh { Radius = 0.07f },
            new Color(0.85f, 0.75f, 0.2f),
            new Vector3(0, 0.6f, 0)
        );

        // Bell base
        CreateMesh(
            new CylinderMesh { TopRadius = 0.05f, BottomRadius = 0.05f, Height = 0.03f },
            new Color(0.85f, 0.75f, 0.2f),
            new Vector3(0, 0.53f, 0)
        );

        // Bell ringer
        CreateMesh(
            new SphereMesh { Radius = 0.02f },
            new Color(0.3f, 0.3f, 0.3f),
            new Vector3(0, 0.67f, 0)
        );
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
