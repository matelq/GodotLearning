namespace FirstProject.ShopLogic;

public class ScoreTracker
{
    public const int PointsPerServe = 100;
    public const int TimeoutPenalty = 50;
    public const int TimeBonusPerSecond = 5;

    public int Score { get; private set; }
    public int ServedCount { get; private set; }
    public int FailedCount { get; private set; }

    /// <summary>
    /// Records a successful serve. Awards base points plus a time bonus.
    /// </summary>
    /// <param name="secondsRemaining">Seconds remaining on the customer's patience timer.</param>
    public void RecordServe(float secondsRemaining)
    {
        int timeBonus = (int)(secondsRemaining * TimeBonusPerSecond);
        Score += PointsPerServe + timeBonus;
        ServedCount++;
    }

    /// <summary>
    /// Records a timeout (customer left angry).
    /// </summary>
    public void RecordTimeout()
    {
        Score = Math.Max(0, Score - TimeoutPenalty);
        FailedCount++;
    }

    public void Reset()
    {
        Score = 0;
        ServedCount = 0;
        FailedCount = 0;
    }
}
