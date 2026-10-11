using System;
using Xunit;
using CMS2018CompanionAutosave.Core;
public class SaveCoordinatorTests
{
    class Executor : ISaveExecutor { public int Calls; public bool Result=true; public Action Action; public bool Execute() { Calls++; if(Action!=null) Action(); return Result; } }
    [Fact] public void DryRunNeverExecutes() { var e=new Executor(); Assert.Equal(AttemptResult.Simulated,new SaveCoordinator().TryAttempt(true,e)); Assert.Equal(0,e.Calls); }
    [Fact] public void RealAttemptExecutesOnce() { var e=new Executor(); Assert.Equal(AttemptResult.GameReturned,new SaveCoordinator().TryAttempt(false,e)); Assert.Equal(1,e.Calls); }
    [Fact] public void ReentrantAttemptRejected() { var c=new SaveCoordinator(); var e=new Executor(); e.Action=()=>Assert.Equal(AttemptResult.AlreadyRunning,c.TryAttempt(false,e)); Assert.Equal(AttemptResult.GameReturned,c.TryAttempt(false,e)); Assert.Equal(1,e.Calls); }
    [Fact] public void FalseReturnFails() { var c=new SaveCoordinator(); Assert.Equal(AttemptResult.Failed,c.TryAttempt(false,new Executor{Result=false})); Assert.NotNull(c.Error); Assert.False(c.Running); }
    [Fact] public void ExceptionFails() { var c=new SaveCoordinator(); Assert.Equal(AttemptResult.Failed,c.TryAttempt(false,new Executor{Action=()=>throw new InvalidOperationException("disk error")})); Assert.Contains("disk error",c.Error); Assert.False(c.Running); }
}
