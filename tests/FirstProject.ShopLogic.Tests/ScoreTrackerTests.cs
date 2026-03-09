using FirstProject.ShopLogic;
using Shouldly;
using Xunit;

namespace FirstProject.ShopLogic.Tests;

public class ScoreTrackerTests
{
    [Fact]
    public void RecordServe_AddsBasePointsPlusTimeBonus()
    {
        var tracker = new ScoreTracker();

        tracker.RecordServe(10f);

        tracker.Score.ShouldBe(ScoreTracker.PointsPerServe + 10 * ScoreTracker.TimeBonusPerSecond);
        tracker.ServedCount.ShouldBe(1);
    }

    [Fact]
    public void RecordServe_AccumulatesAcrossMultipleServes()
    {
        var tracker = new ScoreTracker();

        tracker.RecordServe(5f);
        tracker.RecordServe(3f);

        tracker.ServedCount.ShouldBe(2);
        int expected = (ScoreTracker.PointsPerServe + 5 * ScoreTracker.TimeBonusPerSecond)
                     + (ScoreTracker.PointsPerServe + 3 * ScoreTracker.TimeBonusPerSecond);
        tracker.Score.ShouldBe(expected);
    }

    [Fact]
    public void RecordTimeout_SubtractsPenalty()
    {
        var tracker = new ScoreTracker();
        tracker.RecordServe(10f);
        int scoreBefore = tracker.Score;

        tracker.RecordTimeout();

        tracker.Score.ShouldBe(scoreBefore - ScoreTracker.TimeoutPenalty);
        tracker.FailedCount.ShouldBe(1);
    }

    [Fact]
    public void RecordTimeout_ScoreNeverGoesNegative()
    {
        var tracker = new ScoreTracker();

        tracker.RecordTimeout();
        tracker.RecordTimeout();
        tracker.RecordTimeout();

        tracker.Score.ShouldBe(0);
        tracker.FailedCount.ShouldBe(3);
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var tracker = new ScoreTracker();
        tracker.RecordServe(5f);
        tracker.RecordTimeout();

        tracker.Reset();

        tracker.Score.ShouldBe(0);
        tracker.ServedCount.ShouldBe(0);
        tracker.FailedCount.ShouldBe(0);
    }
}
