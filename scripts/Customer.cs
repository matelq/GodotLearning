using Godot;
using JetBrains.Annotations;

public partial class Customer : CharacterBody3D
{
    [Export] public float WalkSpeed { get; set; } = 2.0f;
    [Export] public float PatienceSeconds { get; set; } = 30.0f;

    [Signal] public delegate void TimedOutEventHandler();
    [Signal] public delegate void ArrivedAtCounterEventHandler();

    private Label3D label;
    private Vector3 targetPosition;
    private Vector3 exitPosition;
    private float patienceRemaining;
    private State currentState = State.WalkingIn;
    private Node3D visualRoot;
    private float animTime;

    public float PatienceRemaining => patienceRemaining;

    private enum State
    {
        WalkingIn,
        Waiting,
        WalkingOut,
        Gone
    }

    [UsedImplicitly]
    public override void _Ready()
    {
        label = GetNode<Label3D>("Label3D");
        patienceRemaining = PatienceSeconds;
        BuildVisuals();
    }

    public void SetTarget(Vector3 counterPos, Vector3 exitPos)
    {
        targetPosition = counterPos;
        exitPosition = exitPos;
        currentState = State.WalkingIn;
    }

    [UsedImplicitly]
    public override void _PhysicsProcess(double delta)
    {
        if (currentState == State.WalkingIn && visualRoot != null)
        {
            animTime += (float)delta;
            visualRoot.Position = new Vector3(0, Mathf.Sin(animTime * 10f) * 0.04f, 0);
        }
        else if (visualRoot != null)
        {
            visualRoot.Position = new Vector3(0, 0, 0);
        }

        switch (currentState)
        {
            case State.WalkingIn:
                WalkToward(targetPosition, delta);
                if (GlobalPosition.DistanceTo(targetPosition) < 0.3f)
                {
                    currentState = State.Waiting;
                    EmitSignal(SignalName.ArrivedAtCounter);
                }
                break;

            case State.Waiting:
                patienceRemaining -= (float)delta;
                label.Text = $"{patienceRemaining:F0}s";
                if (patienceRemaining <= 0)
                {
                    label.Text = "!!!";
                    StartLeaving();
                    EmitSignal(SignalName.TimedOut);
                }
                break;

            case State.WalkingOut:
                // Move directly (no collision) so we don't get stuck on counter/walls
                var dir = (exitPosition - GlobalPosition).Normalized();
                GlobalPosition += dir * WalkSpeed * (float)delta;
                if (GlobalPosition.DistanceTo(exitPosition) < 0.5f)
                {
                    currentState = State.Gone;
                    QueueFree();
                }
                break;

            case State.Gone:
                break;
        }
    }

    private void WalkToward(Vector3 target, double delta)
    {
        var direction = (target - GlobalPosition).Normalized();
        var velocity = direction * WalkSpeed;
        velocity.Y = 0;
        Velocity = velocity;
        MoveAndSlide();

        // Face movement direction
        if (direction.LengthSquared() > 0.01f)
        {
            var lookTarget = GlobalPosition + new Vector3(direction.X, 0, direction.Z);
            LookAt(lookTarget, Vector3.Up);
        }
    }

    public void LeaveHappy()
    {
        label.Text = ":)";
        StartLeaving();
    }

    private void StartLeaving()
    {
        currentState = State.WalkingOut;
        // Disable collision so we don't block other customers or the player
        GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
    }

    private void BuildVisuals()
    {
        GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;

        Color[] shirtColors =
        {
            new Color(0.9f, 0.4f, 0.2f),
            new Color(0.3f, 0.5f, 0.8f),
            new Color(0.4f, 0.7f, 0.3f),
            new Color(0.6f, 0.3f, 0.7f),
            new Color(0.8f, 0.3f, 0.3f)
        };
        var shirtColor = shirtColors[(int)GD.RandRange(0, 4)];
        var skinColor = new Color(0.87f, 0.72f, 0.55f);
        var pantsColor = new Color(0.2f, 0.2f, 0.4f);

        visualRoot = new Node3D();
        AddChild(visualRoot);

        // Body
        var body = new CylinderMesh();
        body.TopRadius = 0.22f;
        body.BottomRadius = 0.22f;
        body.Height = 0.7f;
        visualRoot.AddChild(CreateMesh(body, shirtColor, new Vector3(0, 0.55f, 0)));

        // Head
        var head = new SphereMesh();
        head.Radius = 0.18f;
        head.Height = 0.36f;
        visualRoot.AddChild(CreateMesh(head, skinColor, new Vector3(0, 1.15f, 0)));

        // Left arm
        var leftArm = new BoxMesh();
        leftArm.Size = new Vector3(0.1f, 0.5f, 0.1f);
        visualRoot.AddChild(CreateMesh(leftArm, shirtColor, new Vector3(-0.32f, 0.55f, 0)));

        // Right arm
        var rightArm = new BoxMesh();
        rightArm.Size = new Vector3(0.1f, 0.5f, 0.1f);
        visualRoot.AddChild(CreateMesh(rightArm, shirtColor, new Vector3(0.32f, 0.55f, 0)));

        // Left leg
        var leftLeg = new BoxMesh();
        leftLeg.Size = new Vector3(0.1f, 0.5f, 0.1f);
        visualRoot.AddChild(CreateMesh(leftLeg, pantsColor, new Vector3(-0.1f, 0.05f, 0)));

        // Right leg
        var rightLeg = new BoxMesh();
        rightLeg.Size = new Vector3(0.1f, 0.5f, 0.1f);
        visualRoot.AddChild(CreateMesh(rightLeg, pantsColor, new Vector3(0.1f, 0.05f, 0)));
    }

    private MeshInstance3D CreateMesh(Mesh mesh, Color color, Vector3 position)
    {
        var instance = new MeshInstance3D();
        instance.Mesh = mesh;
        instance.MaterialOverride = new StandardMaterial3D { AlbedoColor = color };
        instance.Position = position;
        return instance;
    }
}
