using System;
using Xunit;
using CMS2018CompanionBridge;
[assembly:CollectionBehavior(DisableTestParallelization=true)]
public class GameSnapshotAdapterTests
{
    CarLoader car;
    public GameSnapshotAdapterTests(){GameScript.Instance=new GameScript();GameLoader.Instance=new GameLoader();ProfileManager.Instance=new ProfileManager();ScreenFader.Instance=new ScreenFader();OrderGenerator.Instance=new OrderGenerator();SceneLoader.BlockProgress=false;GameManager.IsGameReady=true;UnityEngine.Time.timeScale=1;car=new CarLoader();GameLoader.Instance.carLoader=new[]{car};}
    BridgeSnapshot Read(GameSnapshotAdapter adapter=null){return (adapter??new GameSnapshotAdapter()).Read("s",1,DateTime.UtcNow);}
    Job Job(){car.Customer=true;var j=new Job{jobParts=new[]{new JobPart{partList=new System.Collections.Generic.List<string>{"engine/bolt"},_partfound=new[]{true},_partdone=new[]{false},_name=new[]{"Bolt"}}}};OrderGenerator.Instance.Jobs.Add(j);car.Parts.Add(new carPart{name="engine/bolt"});return j;}
    [Fact]public void CopiedArraysStayAligned(){var j=Job();var s=Read();j.jobParts[0]._partdone[0]=true;Assert.False(s.cars[0].requirements[0].done);Assert.Equal("engine/bolt",s.cars[0].requirements[0].instance_path);}
    [Fact]public void MismatchedArraysFailClosed(){Job().jobParts[0]._partfound=new bool[0];Assert.False(Read().cars[0].valid);}
    [Fact]public void TwoMatchingJobsAreAmbiguous(){Job();OrderGenerator.Instance.Jobs.Add(new Job());Assert.False(Read().cars[0].valid);}
    [Fact]public void UnloadedCarsExcluded(){car.Loaded=false;Assert.Empty(Read().cars);}
    [Fact]public void RecreatedPersonalCarGetsNewId(){var a=new GameSnapshotAdapter();var id=Read(a).cars[0].car_id;GameLoader.Instance.carLoader=new[]{new CarLoader()};Assert.NotEqual(id,Read(a).cars[0].car_id);}
    [Fact]public void CustomerIdentitySurvivesSlotSwap(){var j=Job();var a=new GameSnapshotAdapter();var id=Read(a).cars[0].car_id;car.Place=2;j.carLoaderID=2;Assert.Equal(id,Read(a).cars[0].car_id);}
    [Fact]public void NoFocusedCarIsNull(){Assert.Null(Read().focused_car_id);}
    [Fact]public void TransitionSuppressesReady(){ScreenFader.Instance.Running=true;Assert.Equal("suspended",Read().status);}
    [Fact]public void InspectionModeCanExport(){Job();Assert.Equal("ready",Read().status);}
    [Fact]public void HiddenRequirementIsRedacted(){Job().jobParts[0]._partfound[0]=false;var r=Read().cars[0].requirements[0];Assert.Null(r.instance_path);Assert.Null(r.name);Assert.Null(r.done);}
    [Fact]public void PersonalIdentityChangesOnProfileSwitch(){var a=new GameSnapshotAdapter();var first=Read(a).cars[0].car_id;ProfileManager.Instance.Profile=1;Assert.NotEqual(first,Read(a).cars[0].car_id);}
    [Fact]public void RepeatedPartAcrossObjectiveGroupsExportsOnce(){var j=Job();j.jobParts=new[]{j.jobParts[0],new JobPart{type="Gearbox",subtype="Clutch",partList=new System.Collections.Generic.List<string>{"engine/bolt"},_partfound=new[]{true},_partdone=new[]{false},_name=new[]{"Bolt"}}};var c=Read().cars[0];Assert.True(c.valid);Assert.Single(c.requirements);Assert.False(c.requirements[0].done);}
    [Fact]public void RepeatedPartWithConflictingCompletionFailsClosed(){var j=Job();j.jobParts=new[]{j.jobParts[0],new JobPart{partList=new System.Collections.Generic.List<string>{"engine/bolt"},_partfound=new[]{true},_partdone=new[]{true},_name=new[]{"Bolt"}}};var c=Read().cars[0];Assert.False(c.valid);Assert.Empty(c.requirements);}
    [Fact]public void RepeatedHiddenPartStaysRedactedAndExportsOnce(){var j=Job();j.jobParts[0]._partfound[0]=false;j.jobParts=new[]{j.jobParts[0],new JobPart{partList=new System.Collections.Generic.List<string>{"engine/bolt"},_partfound=new[]{false},_partdone=new[]{false},_name=new[]{"Bolt"}}};var c=Read().cars[0];Assert.True(c.valid);Assert.Single(c.requirements);Assert.Null(c.requirements[0].name);Assert.Null(c.requirements[0].instance_path);}
}
