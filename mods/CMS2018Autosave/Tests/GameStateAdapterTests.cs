using Xunit;
using CMS2018CompanionAutosave;
using CMS2018CompanionAutosave.Core;
[assembly: CollectionBehavior(DisableTestParallelization=true)]
public class GameStateAdapterTests
{
    [Fact] public void MainMenuCannotBeMistakenForGarage() { GameScript.Instance=new GameScript { CurrentSceneType=SceneType.Menu }; Assert.False(SafetyGate.Evaluate(new GameStateAdapter().Read()).Allowed); GameScript.Instance=new GameScript(); }
    [Fact] public void MissingGarageMachinesFailClosed() { GameLoader.Instance=new GameLoader {carLifter=null}; Assert.False(new GameStateAdapter().Read().StateKnown); }
    [Fact] public void PartsLeftOnMachinesDeferSave() { GameScript.Instance=new GameScript { MachineGroup=new object() }; GameLoader.Instance=new GameLoader(); Assert.False(SafetyGate.Evaluate(new GameStateAdapter().Read()).Allowed); GameScript.Instance=new GameScript(); }
    [Fact] public void CarLifterMovementDefersSave() { GameScript.Instance=new GameScript(); GameLoader.Instance=new GameLoader { carLifter=new[]{new CarLifter {isMoving=true}} }; Assert.False(SafetyGate.Evaluate(new GameStateAdapter().Read()).Allowed); }
    [Fact] public void ConnectedToolsDeferSave() { GameScript.Instance=new GameScript(); GameLoader.Instance=new GameLoader { carLoader=new[]{new CarLoader {OilbinIsConnected=true}} }; Assert.False(SafetyGate.Evaluate(new GameStateAdapter().Read()).Allowed); }
    [Fact] public void ExitingGarageDefersSave() { GameScript.Instance=new GameScript {Exiting=true}; GameLoader.Instance=new GameLoader(); Assert.False(SafetyGate.Evaluate(new GameStateAdapter().Read()).Allowed); GameScript.Instance=new GameScript(); }
}
