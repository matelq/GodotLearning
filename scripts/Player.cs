using Godot;
using JetBrains.Annotations;
using FirstProject.ShopLogic;

public partial class Player : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 5.0f;
    [Export] public float MouseSensitivity { get; set; } = 0.003f;
    [Export] public float Gravity { get; set; } = 9.8f;

    [Signal] public delegate void IngredientsChangedEventHandler(string[] ingredients);
    [Signal] public delegate void InteractionPromptChangedEventHandler(string prompt);
    [Signal] public delegate void ShawarmaReadyChangedEventHandler(bool ready);
    [Signal] public delegate void ShawarmaServedEventHandler();

    private Node3D cameraRig;
    private Camera3D camera;
    private RayCast3D interactionRay;
    private readonly List<IngredientType> heldIngredients = new();
    private bool hasShawarma;

    public IReadOnlyList<IngredientType> HeldIngredients => heldIngredients;
    public bool HasShawarma => hasShawarma;

    [UsedImplicitly]
    public override void _Ready()
    {
        cameraRig = GetNode<Node3D>("CameraRig");
        camera = GetNode<Camera3D>("CameraRig/Camera3D");
        interactionRay = GetNode<RayCast3D>("CameraRig/InteractionRay");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    [UsedImplicitly]
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            cameraRig.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            var clampedRotation = cameraRig.Rotation;
            clampedRotation.X = Mathf.Clamp(clampedRotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80));
            cameraRig.Rotation = clampedRotation;
        }

        if (@event.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    [UsedImplicitly]
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("interact"))
        {
            TryInteract();
        }

        if (Input.IsActionJustPressed("drop"))
        {
            DropIngredients();
        }
    }

    [UsedImplicitly]
    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y -= Gravity * (float)delta;
        }

        var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * MoveSpeed;
            velocity.Z = direction.Z * MoveSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, MoveSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();

        UpdateInteractionPrompt();
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionRay.IsColliding())
        {
            var collider = interactionRay.GetCollider();
            string prompt = collider switch
            {
                IngredientStation station => $"[E] Pick up {station.Ingredient}",
                GrillStation grill => GetGrillPrompt(grill),
                WrapStation => hasShawarma ? "" : "[E] Wrap Shawarma",
                ServeCounter => hasShawarma ? "[E] Serve Customer" : "",
                _ => ""
            };
            EmitSignal(SignalName.InteractionPromptChanged, prompt);
        }
        else
        {
            EmitSignal(SignalName.InteractionPromptChanged, "");
        }
    }

    private void TryInteract()
    {
        if (!interactionRay.IsColliding())
        {
            return;
        }

        var collider = interactionRay.GetCollider();

        switch (collider)
        {
            case IngredientStation station:
                PickUpIngredient(station);
                break;
            case GrillStation grill:
                TryGrillInteract(grill);
                break;
            case WrapStation wrapStation:
                TryWrap(wrapStation);
                break;
            case ServeCounter serveCounter:
                TryServe(serveCounter);
                break;
        }
    }

    private void PickUpIngredient(IngredientStation station)
    {
        if (hasShawarma)
        {
            return;
        }

        if (!heldIngredients.Contains(station.Ingredient))
        {
            heldIngredients.Add(station.Ingredient);
            EmitIngredientsChanged();
            station.PlayPickupEffect();
            GD.Print($"Picked up: {station.Ingredient}");
        }
    }

    private void TryGrillInteract(GrillStation grill)
    {
        if (hasShawarma)
        {
            return;
        }

        if (grill.CurrentState == GrillStation.GrillState.Ready ||
            grill.CurrentState == GrillStation.GrillState.Burnt)
        {
            var result = grill.TryTakeChicken();
            if (result.HasValue)
            {
                heldIngredients.Add(result.Value);
                EmitIngredientsChanged();
            }
        }
        else if (heldIngredients.Contains(IngredientType.Chicken))
        {
            if (grill.TryPlaceChicken())
            {
                heldIngredients.Remove(IngredientType.Chicken);
                EmitIngredientsChanged();
            }
        }
    }

    private static string GetGrillPrompt(GrillStation grill)
    {
        return grill.CurrentState switch
        {
            GrillStation.GrillState.Empty => "[E] Place Chicken",
            GrillStation.GrillState.Cooking => "Cooking...",
            GrillStation.GrillState.Ready => "[E] Take Chicken",
            GrillStation.GrillState.Burnt => "[E] Clear Grill",
            _ => ""
        };
    }

    private void TryWrap(WrapStation station)
    {
        if (hasShawarma || heldIngredients.Count == 0)
        {
            return;
        }

        var gameManager = GetNode<GameManager>("/root/GameManager");
        if (gameManager.CurrentOrder?.IsSatisfiedBy(heldIngredients) == true)
        {
            hasShawarma = true;
            heldIngredients.Clear();
            EmitIngredientsChanged();
            EmitSignal(SignalName.ShawarmaReadyChanged, true);
            GD.Print("Shawarma wrapped!");
        }
        else
        {
            GD.Print("Wrong ingredients! Check the order.");
        }
    }

    private void TryServe(ServeCounter counter)
    {
        if (!hasShawarma)
        {
            return;
        }

        hasShawarma = false;
        EmitSignal(SignalName.ShawarmaReadyChanged, false);
        EmitSignal(SignalName.ShawarmaServed);
        GD.Print("Shawarma served!");
    }

    private void EmitIngredientsChanged()
    {
        var names = heldIngredients.Select(i => i.ToString()).ToArray();
        EmitSignal(SignalName.IngredientsChanged, names);
    }

    private void DropIngredients()
    {
        if (heldIngredients.Count == 0 && !hasShawarma)
        {
            return;
        }

        bool wasShawarma = hasShawarma;
        heldIngredients.Clear();
        hasShawarma = false;
        EmitIngredientsChanged();
        if (wasShawarma)
        {
            EmitSignal(SignalName.ShawarmaReadyChanged, false);
        }
        GD.Print("Dropped all ingredients.");
    }

    public void ResetInventory()
    {
        heldIngredients.Clear();
        hasShawarma = false;
        EmitIngredientsChanged();
    }
}
