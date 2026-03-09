using Godot;
using JetBrains.Annotations;

public partial class EndScreen : CanvasLayer
{
    private Label scoreLabel;
    private Label statsLabel;
    private Button restartButton;

    [UsedImplicitly]
    public override void _Ready()
    {
        scoreLabel = GetNode<Label>("PanelContainer/VBoxContainer/ScoreLabel");
        statsLabel = GetNode<Label>("PanelContainer/VBoxContainer/StatsLabel");
        restartButton = GetNode<Button>("PanelContainer/VBoxContainer/RestartButton");

        restartButton.Pressed += OnRestartPressed;

        var gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.GameOver += OnGameOver;

        Visible = false;
    }

    private void OnGameOver(int finalScore, int served, int failed)
    {
        scoreLabel.Text = $"Final Score: {finalScore}";
        statsLabel.Text = $"Served: {served} | Failed: {failed}";
        Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void OnRestartPressed()
    {
        Visible = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().ReloadCurrentScene();

        // Re-start the game after scene reload
        var gameManager = GetNode<GameManager>("/root/GameManager");
        CallDeferred(nameof(DeferredStart));
    }

    private void DeferredStart()
    {
        var gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.StartGame();
    }
}
