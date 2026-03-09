using Godot;
using JetBrains.Annotations;
using FirstProject.ShopLogic;

public partial class GameManager : Node
{
    [Export] public float GameDurationSeconds { get; set; } = 180f;
    [Export] public float CustomerPatienceSeconds { get; set; } = 60f;
    [Export] public float CustomerSpawnDelay { get; set; } = 2f;
    [Export] public PackedScene CustomerScene { get; set; }

    [Signal] public delegate void ScoreChangedEventHandler(int score);
    [Signal] public delegate void TimeChangedEventHandler(float timeRemaining);
    [Signal] public delegate void OrderChangedEventHandler(string orderText);
    [Signal] public delegate void GameOverEventHandler(int finalScore, int served, int failed);

    private OrderGenerator orderGenerator;
    private ScoreTracker scoreTracker;
    private float gameTimeRemaining;
    private bool gameRunning;
    private Customer? currentCustomer;
    private Godot.Timer spawnTimer;
    private int totalServed;

    public Order? CurrentOrder { get; private set; }
    public int Score => scoreTracker.Score;
    public bool IsGameRunning => gameRunning;

    [UsedImplicitly]
    public override void _Ready()
    {
        orderGenerator = new OrderGenerator();
        scoreTracker = new ScoreTracker();

        spawnTimer = new Godot.Timer();
        spawnTimer.OneShot = true;
        spawnTimer.Timeout += OnSpawnTimerTimeout;
        AddChild(spawnTimer);
    }

    [UsedImplicitly]
    public override void _Process(double delta)
    {
        if (!gameRunning)
        {
            return;
        }

        gameTimeRemaining -= (float)delta;
        EmitSignal(SignalName.TimeChanged, gameTimeRemaining);

        if (gameTimeRemaining <= 0)
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        scoreTracker.Reset();
        totalServed = 0;
        gameTimeRemaining = GameDurationSeconds;
        gameRunning = true;
        EmitSignal(SignalName.ScoreChanged, 0);
        EmitSignal(SignalName.TimeChanged, gameTimeRemaining);
        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        if (!gameRunning || CustomerScene == null)
        {
            return;
        }

        int minToppings = totalServed >= 5 ? 2 : 1;
        CurrentOrder = orderGenerator.GenerateOrder(minToppings);
        EmitSignal(SignalName.OrderChanged, CurrentOrder.GetDisplayText());

        currentCustomer = CustomerScene.Instantiate<Customer>();
        float patienceReduction = (totalServed / 3) * 5f;
        currentCustomer.PatienceSeconds = Mathf.Max(25f, CustomerPatienceSeconds - patienceReduction);

        var spawnPoint = GetTree().CurrentScene.GetNode<Node3D>("CustomerSpawnPoint");
        var counterPoint = GetTree().CurrentScene.GetNode<Node3D>("CustomerCounterPoint");

        GetTree().CurrentScene.AddChild(currentCustomer);
        currentCustomer.GlobalPosition = spawnPoint.GlobalPosition;
        currentCustomer.SetTarget(counterPoint.GlobalPosition, spawnPoint.GlobalPosition);

        currentCustomer.TimedOut += OnCustomerTimedOut;
        currentCustomer.ArrivedAtCounter += OnCustomerArrived;

        // Connect player's serve signal
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        player.ShawarmaServed += OnShawarmaServed;
    }

    private void OnCustomerArrived()
    {
        GD.Print($"Customer waiting for: {CurrentOrder?.GetDisplayText()}");
    }

    private void OnCustomerTimedOut()
    {
        scoreTracker.RecordTimeout();
        EmitSignal(SignalName.ScoreChanged, scoreTracker.Score);
        GD.Print("Customer timed out!");
        DisconnectPlayerServeSignal();
        ScheduleNextCustomer();
    }

    private void OnShawarmaServed()
    {
        if (currentCustomer == null)
        {
            return;
        }

        int oldScore = scoreTracker.Score;
        scoreTracker.RecordServe(currentCustomer.PatienceRemaining);
        int pointsEarned = scoreTracker.Score - oldScore;
        EmitSignal(SignalName.ScoreChanged, scoreTracker.Score);

        // Floating score popup above the serve counter
        var serveCounter = GetTree().CurrentScene.GetNodeOrNull<Node3D>("ServeCounter");
        if (serveCounter != null)
        {
            ScorePopup.Spawn(GetTree().CurrentScene, serveCounter.GlobalPosition + Vector3.Up * 1.5f, pointsEarned);
        }

        currentCustomer.LeaveHappy();
        totalServed++;
        GD.Print($"Served! Score: {scoreTracker.Score} (Total served: {totalServed})");
        DisconnectPlayerServeSignal();
        ScheduleNextCustomer();

        // Reset player inventory for next order
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        player.ResetInventory();
    }

    private void DisconnectPlayerServeSignal()
    {
        var player = GetTree().CurrentScene.GetNodeOrNull<Player>("Player");
        if (player != null)
        {
            player.ShawarmaServed -= OnShawarmaServed;
        }
    }

    private void ScheduleNextCustomer()
    {
        currentCustomer = null;
        CurrentOrder = null;
        EmitSignal(SignalName.OrderChanged, "");

        if (gameRunning)
        {
            float delayReduction = (totalServed / 2) * 0.3f;
            spawnTimer.WaitTime = Mathf.Max(0.5f, CustomerSpawnDelay - delayReduction);
            spawnTimer.Start();
        }
    }

    private void OnSpawnTimerTimeout()
    {
        if (gameRunning)
        {
            SpawnCustomer();
        }
    }

    private void EndGame()
    {
        gameRunning = false;
        EmitSignal(SignalName.GameOver, scoreTracker.Score, scoreTracker.ServedCount, scoreTracker.FailedCount);
        GD.Print($"Game Over! Final Score: {scoreTracker.Score} | Served: {scoreTracker.ServedCount} | Failed: {scoreTracker.FailedCount}");
    }
}
