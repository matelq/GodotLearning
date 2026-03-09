using Godot;
using JetBrains.Annotations;

public partial class ScorePopup : Node3D
{
    private Label3D label;
    private float lifetime;
    private const float MaxLifetime = 1.5f;
    private const float RiseSpeed = 1.5f;

    [UsedImplicitly]
    public override void _Ready()
    {
        label = new Label3D();
        label.FontSize = 48;
        label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        label.NoDepthTest = true;
        label.Modulate = new Color(0.3f, 1f, 0.3f);
        AddChild(label);
    }

    [UsedImplicitly]
    public override void _Process(double delta)
    {
        lifetime += (float)delta;

        Position += Vector3.Up * RiseSpeed * (float)delta;

        float alpha = 1.0f - (lifetime / MaxLifetime);
        float scale = 1.0f + lifetime * 0.3f;
        label.Modulate = new Color(0.3f, 1f, 0.3f, alpha);
        label.Scale = Vector3.One * scale;

        if (lifetime >= MaxLifetime)
        {
            QueueFree();
        }
    }

    public static ScorePopup Spawn(Node parent, Vector3 position, int points)
    {
        var popup = new ScorePopup();
        parent.AddChild(popup);
        popup.GlobalPosition = position;
        popup.label.Text = $"+{points}";
        return popup;
    }
}
