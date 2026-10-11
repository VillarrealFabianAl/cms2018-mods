using System;
using Xunit;
using CMS2018CompanionBridge;
public class CapturePolicyTests
{
    internal static BridgeSnapshot Sample() { return new BridgeSnapshot { session_id="s", profile=0, status="ready", cars=new[]{ new BridgeCar { car_id="c", valid=true, requirements=new[]{ new BridgeRequirement { key="k", found=true, done=false, instance_path="p", part_id="bolt", name="Bolt", kind="part" } } } } }; }
    [Fact] public void RateLimited() { var p=new CapturePolicy(); Assert.NotNull(p.Observe(0,Sample(),false)); Assert.Null(p.Observe(.5,Sample(),false)); Assert.NotNull(p.Observe(1,Sample(),false)); }
    [Fact] public void NoCatchupBurst() { var p=new CapturePolicy(); p.Observe(0,Sample(),false); Assert.NotNull(p.Observe(10,Sample(),false)); Assert.Null(p.Observe(10,Sample(),false)); }
    [Fact] public void RequiresTwoStableCaptures() { var p=new CapturePolicy(); Assert.Equal("suspended",p.Observe(0,Sample(),true).status); Assert.Equal("suspended",p.Observe(1,Sample(),false).status); Assert.Equal("ready",p.Observe(2,Sample(),false).status); }
    [Fact] public void RedactsHiddenTasks() { var s=Sample(); s.cars[0].requirements[0].found=false; var r=new CapturePolicy().Observe(0,s,false).cars[0].requirements[0]; Assert.Null(r.name); Assert.Null(r.part_id); Assert.Null(r.instance_path); Assert.Null(r.done); }
    [Fact] public void RejectsDuplicatePaths() { var s=Sample(); s.cars[0].requirements=new[]{new BridgeRequirement{key="a",found=true,instance_path="p"},new BridgeRequirement{key="b",found=true,instance_path="p"}}; Assert.False(new CapturePolicy().Observe(0,s,false).cars[0].valid); }
    [Fact] public void ReorderPreservesKeys() { Assert.Equal(TaskKeys.ForPart("c",1,"p"),TaskKeys.ForPart("c",1,"p")); Assert.NotEqual(TaskKeys.ForPart("c",1,"p"),TaskKeys.ForPart("c",1,"q")); }
    [Fact] public void ChangedPlacementRestartsDwell() { var p=new CapturePolicy(); p.Observe(0,Sample(),false); var s=Sample();s.cars[0].location="2"; Assert.Equal("suspended",p.Observe(1,s,false).status);s=Sample();s.cars[0].location="2";Assert.Equal("ready",p.Observe(2,s,false).status); }
    [Fact] public void UnknownCompletionStaysUnknown() { var s=Sample();s.cars[0].requirements[0].done=null;Assert.Null(new CapturePolicy().Observe(0,s,false).cars[0].requirements[0].done); }
    [Fact] public void NonPartOccurrencesHaveDistinctKeys() { Assert.NotEqual(TaskKeys.ForService("c",1,"Oil","Change oil",0),TaskKeys.ForService("c",1,"Oil","Change oil",1)); }
    [Fact] public void InvalidCarCannotPoisonWholeEnvelopeWithDuplicateTasks() { var s=Sample();s.cars[0].requirements=new[]{new BridgeRequirement{key="a",found=true,instance_path="p"},new BridgeRequirement{key="a",found=true,instance_path="p"}};var c=new CapturePolicy().Observe(0,s,false).cars[0];Assert.False(c.valid);Assert.Empty(c.requirements); }
}
