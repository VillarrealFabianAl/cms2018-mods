using System;
using Xunit;
using CMS2018CompanionAutosave.Core;

public class SafetyGateTests
{
    [Fact] public void IdleGarageAllowed() { Assert.True(SafetyGate.Evaluate(SettingsSchedulerTests.Garage()).Allowed); }
    [Theory]
    [InlineData("StateKnown",false)] [InlineData("InGarage",false)] [InlineData("Focused",false)]
    [InlineData("LoaderReady",false)] [InlineData("GameReady",false)] [InlineData("SaveAllowed",false)]
    [InlineData("NormalGarageMode",false)] [InlineData("Paused",true)] [InlineData("Transitioning",true)]
    [InlineData("ModMenuOpen",true)] [InlineData("TransactionBusy",true)] [InlineData("SaveBusy",true)] [InlineData("BenchmarkActive",true)]
    public void EveryBlockerRejects(string flag,bool value) { var g=SettingsSchedulerTests.Garage(); typeof(GarageSnapshot).GetField(flag).SetValue(g,value); var result=SafetyGate.Evaluate(g); Assert.False(result.Allowed); Assert.False(string.IsNullOrEmpty(result.Reason)); }
    [Theory] [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void MissingProfileRejects(string id) { var g=SettingsSchedulerTests.Garage(); g.ProfileId=id; Assert.False(SafetyGate.Evaluate(g).Allowed); }
    [Fact] public void UnknownStateRejects() { Assert.False(SafetyGate.Evaluate(new GarageSnapshot()).Allowed); Assert.False(SafetyGate.Evaluate(null).Allowed); }
    [Fact] public void UnsafeActivityRestartsDwell() { var t=new AutosaveScheduler(); var g=SettingsSchedulerTests.Garage(); var s=new SettingsModel(); g.TransactionBusy=true; t.Tick(60,g,s,SafetyGate.Evaluate(g)); g.TransactionBusy=false; Assert.Equal(ScheduleDecision.None,t.Tick(1,g,s,SafetyGate.Evaluate(g))); g.Transitioning=true; t.Tick(1,g,s,SafetyGate.Evaluate(g)); g.Transitioning=false; Assert.Equal(ScheduleDecision.None,t.Tick(1,g,s,SafetyGate.Evaluate(g))); Assert.Equal(ScheduleDecision.Attempt,t.Tick(1,g,s,SafetyGate.Evaluate(g))); }
}
