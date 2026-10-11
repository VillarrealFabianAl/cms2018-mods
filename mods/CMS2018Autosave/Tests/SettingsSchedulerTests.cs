using System;
using Xunit;
using CMS2018CompanionAutosave.Core;

public class SettingsSchedulerTests
{
    internal static GarageSnapshot Garage() { return new GarageSnapshot { ProfileId="test", InGarage=true, Focused=true, StateKnown=true, LoaderReady=true, GameReady=true, SaveAllowed=true, NormalGarageMode=true }; }
    static GateResult Safe() { return new GateResult(true, "Ready"); }
    [Fact] public void DefaultsAreDryRun() { var s=new SettingsModel(); Assert.True(s.TestMode); Assert.True(s.Enabled); Assert.Equal(60,s.IntervalSeconds); }
    [Theory] [InlineData("15",true,900)] [InlineData("30",true,1800)] [InlineData("30",false,30)] [InlineData("120",true,7200)]
    public void IntervalsValidate(string text,bool mins,int expected) { int seconds; Assert.True(SettingsModel.TryInterval(text,mins,out seconds)); Assert.Equal(expected,seconds); }
    [Theory] [InlineData("")] [InlineData("1.5")] [InlineData("-1")] [InlineData("29")] [InlineData("7201")] [InlineData("999999999999")]
    public void InvalidIntervalsRejected(string text) { int seconds; Assert.False(SettingsModel.TryInterval(text,false,out seconds)); }
    [Theory] [InlineData(30)] [InlineData(60)] [InlineData(120)] [InlineData(300)] [InlineData(600)] [InlineData(900)] [InlineData(1800)]
    public void EachPresetTriggersAtItsInterval(int interval) { var s=new SettingsModel { IntervalSeconds=interval }; var timer=new AutosaveScheduler(); var g=Garage(); Assert.Equal(ScheduleDecision.None,timer.Tick(interval-1,g,s,Safe())); Assert.Equal(ScheduleDecision.Attempt,timer.Tick(1,g,s,Safe())); }
    [Fact] public void PendingIsSingle() { var timer=new AutosaveScheduler(); var g=Garage(); var s=new SettingsModel(); Assert.Equal(ScheduleDecision.None,timer.Tick(60,g,s,new GateResult(false,"Busy"))); Assert.True(timer.Pending); Assert.Equal(ScheduleDecision.None,timer.Tick(1,g,s,Safe())); Assert.Equal(ScheduleDecision.Attempt,timer.Tick(1,g,s,Safe())); Assert.Equal(ScheduleDecision.None,timer.Tick(300,g,s,Safe())); timer.Complete(true); Assert.False(timer.Pending); Assert.Equal(60,timer.RemainingSeconds); }
    [Fact] public void ProfileSwitchCancels() { var t=new AutosaveScheduler(); var g=Garage(); var s=new SettingsModel(); t.Tick(60,g,s,new GateResult(false,"Busy")); g.ProfileId="other"; Assert.Equal(ScheduleDecision.None,t.Tick(0,g,s,Safe())); Assert.False(t.Pending); Assert.Equal(60,t.RemainingSeconds); }
    [Fact] public void PauseAndFocusFreeze() { var t=new AutosaveScheduler(); var g=Garage(); var s=new SettingsModel(); t.Tick(10,g,s,Safe()); g.Paused=true; t.Tick(100,g,s,Safe()); g.Paused=false; g.Focused=false; t.Tick(100,g,s,Safe()); Assert.Equal(50,t.RemainingSeconds); Assert.Equal(0,t.SafeSeconds); }
    [Fact] public void IntervalChangeResets() { var t=new AutosaveScheduler(); var g=Garage(); var s=new SettingsModel(); t.Tick(30,g,s,Safe()); s.IntervalSeconds=900; t.Tick(0,g,s,Safe()); Assert.Equal(900,t.RemainingSeconds); }
    [Fact] public void LeavingGarageAndDisablingCancel() { var t=new AutosaveScheduler(); var g=Garage(); var s=new SettingsModel(); t.Tick(60,g,s,new GateResult(false,"Busy")); g.InGarage=false; t.Tick(0,g,s,Safe()); Assert.False(t.Pending); g.InGarage=true; s.Enabled=false; t.Tick(60,g,s,Safe()); Assert.Equal(60,t.RemainingSeconds); }
    [Fact] public void FailureNeedsReenable() { var t=new AutosaveScheduler(); var g=Garage(); var s=new SettingsModel(); t.Tick(60,g,s,Safe()); t.Complete(false); Assert.Equal(ScheduleDecision.None,t.Tick(60,g,s,Safe())); t.Reenable(); Assert.Equal(ScheduleDecision.Attempt,t.Tick(60,g,s,Safe())); }
    [Theory] [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)] [InlineData(-1)]
    public void InvalidClockDeltaDoesNotTrigger(double delta) { var t=new AutosaveScheduler(); Assert.Equal(ScheduleDecision.None,t.Tick(delta,Garage(),new SettingsModel(),Safe())); Assert.Equal(60,t.RemainingSeconds); }
    [Fact] public void CorruptSettingsReturnSafeDefaults() { string warning; var s=SettingsModel.FromXml("<broken",out warning); Assert.True(s.TestMode); Assert.Equal(60,s.IntervalSeconds); Assert.False(string.IsNullOrEmpty(warning)); }
    [Fact] public void SettingsRoundTrip() { string warning; var s=new SettingsModel { TestMode=false, Enabled=false, IntervalSeconds=1800 }; var restored=SettingsModel.FromXml(s.ToXml(),out warning); Assert.False(restored.TestMode); Assert.False(restored.Enabled); Assert.Equal(1800,restored.IntervalSeconds); Assert.Null(warning); }
    [Fact] public void MissingFieldsAndBadValuesStayDryRun() { string warning; var s=SettingsModel.FromXml("<SettingsModel><TestMode>false</TestMode></SettingsModel>",out warning); Assert.True(s.TestMode); Assert.NotNull(warning); }
    [Theory]
    [InlineData("<Enabled>true</Enabled><TestMode>false</TestMode><TestMode>true</TestMode><IntervalSeconds>60</IntervalSeconds>")]
    [InlineData("<Enabled>true</Enabled><TestMode><Nested>false</Nested></TestMode><IntervalSeconds>60</IntervalSeconds>")]
    [InlineData("<Enabled>true</Enabled><TestMode>false</TestMode><IntervalSeconds>60</IntervalSeconds><Unknown>true</Unknown>")]
    public void AmbiguousSettingsRestoreTestMode(string content) { string warning; var s=SettingsModel.FromXml("<SettingsModel>"+content+"</SettingsModel>",out warning); Assert.True(s.TestMode); Assert.Equal(60,s.IntervalSeconds); Assert.NotNull(warning); }
}
