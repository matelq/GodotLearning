using Godot;
using JetBrains.Annotations;

public partial class WrapStation : StaticBody3D
{
    private Label3D label;

    [UsedImplicitly]
    public override void _Ready()
    {
        label = GetNode<Label3D>("Label3D");
        label.Text = "Wrap Station";

        BuildVisuals();
    }

    private void BuildVisuals()
    {
        GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;

        // Table base (green tint)
        CreateMesh(
            new BoxMesh { Size = new Vector3(1.2f, 0.1f, 0.8f) },
            new Color(0.3f, 0.5f, 0.3f),
            new Vector3(0, -0.45f, 0)
        );

        // Table legs
        var legMesh = new CylinderMesh { TopRadius = 0.04f, BottomRadius = 0.04f, Height = 0.4f };
        var legColor = new Color(0.35f, 0.22f, 0.1f);
        CreateMesh(legMesh, legColor, new Vector3(0.5f, -0.7f, 0.3f));
        CreateMesh(legMesh, legColor, new Vector3(-0.5f, -0.7f, 0.3f));
        CreateMesh(legMesh, legColor, new Vector3(0.5f, -0.7f, -0.3f));
        CreateMesh(legMesh, legColor, new Vector3(-0.5f, -0.7f, -0.3f));

        // Cutting board
        CreateMesh(
            new BoxMesh { Size = new Vector3(0.6f, 0.02f, 0.4f) },
            new Color(0.55f, 0.35f, 0.15f),
            new Vector3(0, 0.07f, 0)
        );

        // Rolling pin (rotated 90 degrees on Z)
        var rollingPin = CreateMesh(
            new CylinderMesh { TopRadius = 0.03f, BottomRadius = 0.03f, Height = 0.35f },
            new Color(0.6f, 0.45f, 0.25f),
            new Vector3(0.35f, 0.1f, 0)
        );
        rollingPin.RotateZ(Mathf.DegToRad(90.0f));
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
