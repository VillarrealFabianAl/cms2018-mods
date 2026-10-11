using System;
using Xunit;
using CMS2018CompanionAutosave;
using CMS2018CompanionAutosave.Core;

// Stand-in for the proprietary game boundary; the executor itself is the shipped source.
public class GameLoader
{
    public static GameLoader Instance;
    public int Calls;
    public bool isReady=true;
    public CarLifter[] carLifter=new CarLifter[0];
    public CarLoader[] carLoader=new CarLoader[0];
    public static GameLoader Get() { return Instance; }
    public bool Save() { Calls++; return true; }
}
public class GameSaveExecutorTests
{
    [Fact] public void ExecutorDoesNotBlockItsOwnCoordinator()
    {
        var coordinator = new SaveCoordinator(); GameLoader.Instance = new GameLoader();
        CMS2018CompanionAutosave.Entry.Coordinator=coordinator;
        var adapter=new GameStateAdapter();
        var executor = new GameSaveExecutor(adapter.Read);
        Assert.Equal(AttemptResult.GameReturned,coordinator.TryAttempt(false,executor));
        Assert.Equal(1,GameLoader.Instance.Calls);
    }
    [Fact] public void UnknownAndChangedStatesNeverSave()
    {
        GameLoader.Instance = new GameLoader(); var state=SettingsSchedulerTests.Garage(); state.StateKnown=false;
        Assert.Throws<InvalidOperationException>(()=>new GameSaveExecutor(()=>state).Execute()); Assert.Equal(0,GameLoader.Instance.Calls);
    }
}
