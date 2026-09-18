using System.Reflection;
using Oxide.Ext.UiFramework.Animation;
using Oxide.Ext.UiFramework.UiElements;

namespace Rust.UiFramework.UnitTests.Animations;

public class AnimationClockPrecisionTests : BaseAnimationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(30)]
    [InlineData(365)]
    public void ShortAnimation_AdvancesEveryFrame_AfterLongUptime(int days)
    {
        double startTime = days * 86400d;
        AnimationTime time = CreateClock(startTime);
        var owner = CreateElementAnimation<UiPanel>("clock-precision").Animation;
        owner.SetTime(time);
        owner.Duration = AnimationDuration.Create(owner, 0.25f);

        try
        {
            owner.Duration.OnStarted();
            float previousProgress = 0;
            for (int frame = 1; frame <= 17; frame++)
            {
                UpdateClock(time, startTime + frame * 0.015, false);
                float progress = owner.Duration.ElapsedPercentage;
                progress.Should().BeGreaterThan(previousProgress);
                progress.Should().BeApproximately(Math.Min(1f, frame * 0.06f), 0.00001f);
                time.DeltaTime.Should().BeApproximately(0.015f, 0.00001f);
                previousProgress = progress;
            }

            owner.Duration.IsCompleted.Should().BeTrue();
        }
        finally
        {
            owner.Dispose();
        }
    }

    [Theory]
    [InlineData(13)]
    [InlineData(365)]
    public void DelayAndTimeout_KeepSubFrameBoundaries_AfterLongUptime(int days)
    {
        double startTime = days * 86400d;
        AnimationTime time = CreateClock(startTime);
        var owner = CreateElementAnimation<UiPanel>("clock-boundaries").Animation;
        owner.SetTime(time);
        owner.Delay = TimeDelayAnimation.Create(owner.Plugin, owner, 0.03f);
        owner.Timeout = AnimationTimeout.Create(owner, 0.03f, AnimationTimeoutAction.CancelAnimation);

        try
        {
            owner.Delay.OnStarted();
            owner.Timeout.OnStarted();
            UpdateClock(time, startTime + 0.02, false);
            owner.Delay.IsDelayed.Should().BeTrue();
            owner.Timeout.HasTimedOut.Should().BeFalse();

            UpdateClock(time, startTime + 0.04, false);
            owner.Delay.IsDelayed.Should().BeFalse();
            owner.Timeout.HasTimedOut.Should().BeTrue();
        }
        finally
        {
            owner.Dispose();
        }
    }

    [Fact]
    public void RepeatedDuration_PreservesDelayAndProgress_AfterLongUptime()
    {
        double startTime = 365 * 86400d;
        AnimationTime time = CreateClock(startTime);
        var owner = CreateElementAnimation<UiPanel>("clock-repeat").Animation;
        owner.SetTime(time);
        owner.Duration = AnimationDuration.Create(owner, 0.25f);

        try
        {
            owner.Duration.OnStarted();
            owner.Duration.Restart(0.03f);
            UpdateClock(time, startTime + 0.02, false);
            owner.Duration.ElapsedPercentage.Should().Be(0);
            UpdateClock(time, startTime + 0.045, false);
            owner.Duration.ElapsedPercentage.Should().BeApproximately(0.06f, 0.00001f);
            UpdateClock(time, startTime + 0.30, false);
            owner.Duration.IsCompleted.Should().BeTrue();
        }
        finally
        {
            owner.Dispose();
        }
    }

    [Fact]
    public void Clock_ResumesAfterLongPause_WithoutLosingFramePrecision()
    {
        AnimationTime time = CreateClock(0);
        double resumedAt = 365 * 86400d;
        UpdateClock(time, resumedAt, true);
        time.DeltaTime.Should().Be(float.Epsilon);
        UpdateClock(time, resumedAt + 0.015, false);
        time.DeltaTime.Should().BeApproximately(0.015f, 0.00001f);
        time.CurrentFrame.Should().Be(3);
    }

    private static AnimationTime CreateClock(double currentTime)
    {
        var time = (AnimationTime)Activator.CreateInstance(typeof(AnimationTime), true);
        UpdateClock(time, currentTime, true);
        return time;
    }

    private static void UpdateClock(AnimationTime time, double currentTime, bool wasPaused)
    {
        MethodInfo method = typeof(AnimationTime).GetMethod("UpdateTime", BindingFlags.Instance | BindingFlags.NonPublic);
        object timestamp = Convert.ChangeType(currentTime, method.GetParameters()[0].ParameterType);
        method.Invoke(time, [timestamp, wasPaused]);
    }
}
