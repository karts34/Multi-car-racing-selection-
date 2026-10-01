using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Afterglow
{
public partial class RaceGame
{
    static void HeroCheck(bool value,string message){if(!value)throw new Exception("Hero validation: "+message);}
    IEnumerator HeroShot(string dir,string name){PlaceHero();ResetHeroCamera();yield return new WaitForSecondsRealtime(1.5f);ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return new WaitForSecondsRealtime(.3f);}
    IEnumerator HeroTest()
    {
        string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Logs"));Directory.CreateDirectory(dir);
        worldTestingSteps=true;yield return new WaitForSecondsRealtime(2);
        ShowMapSelection();SelectMap(0);HeroCheck(!world.Root.gameObject.activeSelf,"map preview changed active environment");
        yield return new WaitForSecondsRealtime(1);ScreenCapture.CaptureScreenshot(Path.Combine(dir,"hero-map-selection.png"));yield return new WaitForSecondsRealtime(.3f);
        BeginSelectedMap();HeroCheck(inWorld&&!player.root.gameObject.activeSelf&&!track.root.gameObject.activeSelf,"city must use human, not car");
        HeroCheck(citizens.Count>=20&&heroTasks.Count==3,"pedestrians and tasks exist");
        var start=worldPosition;var npcStart=citizens[0].Root.position;
        for(int i=0;i<100;i++)StepHero(.02f,Vector2.up,false,false,false,false,false);
        float walk=FlatDistance(start,worldPosition);HeroCheck(walk>5&&walk<9,"walking distance: "+walk);
        start=worldPosition;for(int i=0;i<100;i++)StepHero(.02f,Vector2.up,true,false,false,false,false);
        HeroCheck(FlatDistance(start,worldPosition)>walk*1.8f,"running must be faster");HeroCheck(Vector3.Distance(npcStart,citizens[0].Root.position)>3,"citizens must walk");
        float ground=worldPosition.y;StepHero(.02f,Vector2.zero,false,true,false,false,false);
        for(int i=0;i<30;i++)StepHero(.02f,Vector2.zero,false,false,false,false,false);
        HeroCheck(worldPosition.y>ground+4,"super jump height");
        for(int i=0;i<160;i++)StepHero(.02f,Vector2.zero,false,false,false,false,false);
        HeroCheck(grounded&&Mathf.Abs(worldPosition.y-ground)<.5f,"gravity and ground landing");
        yield return HeroShot(dir,"hero-street");
        cameraYaw=180;yield return HeroShot(dir,"hero-costume");cameraYaw=0;
        var building=world.Buildings[0];
        foreach(var b in world.Buildings)if(b.size.y>20&&Mathf.Abs(b.center.x)<450&&Mathf.Abs(b.center.z)<350){building=b;break;}
        worldPosition=new Vector3(building.min.x-2,world.Height(building.min.x-2,building.center.z)+.18f,building.center.z);cameraYaw=90;heroVelocity=Vector3.zero;grounded=true;
        for(int i=0;i<100;i++)StepHero(.02f,Vector2.up,true,false,false,false,false);
        HeroCheck(worldPosition.x<=building.min.x-.39f,"wall blocks running");
        // Continue holding the real climb input until the controller steps onto the roof.
        for(int i=0;i<650;i++){StepHero(.02f,Vector2.up,false,false,false,false,true);if(grounded&&worldPosition.y>=building.max.y)break;}
        HeroCheck(worldPosition.y>=building.max.y&&grounded,"wall climb and roof step: "+worldPosition+" roof="+building.max.y);
        yield return HeroShot(dir,"hero-rooftop");
        worldPosition=new Vector3(building.min.x-35,world.Height(building.min.x-35,building.center.z)+.18f,building.center.z);heroVelocity=Vector3.zero;cameraYaw=90;grounded=true;
        HeroCheck(AttachWeb(),"web finds actual roof anchor");float initialY=worldPosition.y;
        for(int i=0;i<65;i++)StepHero(.02f,Vector2.up,false,false,true,false,false);
        HeroCheck(worldPosition.y>initialY+2&&swinging,"web swinging lifts hero: y="+worldPosition.y+" initial="+initialY+" anchor="+webAnchor+" grounded="+grounded+" swing="+swinging);HeroCheck(Vector3.Distance(worldPosition+Vector3.up,webAnchor)<=ropeLength+.3f,"rope constrains swing");
        yield return HeroShot(dir,"hero-web-swing");
        var release=heroVelocity;StepHero(.02f,Vector2.zero,false,false,false,false,false);HeroCheck(!swinging&&Vector3.Distance(release,heroVelocity)<2,"release preserves momentum");
        worldPosition=new Vector3(building.min.x-12,world.Height(building.min.x-12,building.center.z)+.18f,building.center.z);heroVelocity=Vector3.zero;heroEnergy=100;cameraYaw=90;
        HeroCheck(ZipWeb()&&heroEnergy==70&&heroVelocity.magnitude>30,"zip and energy cost");heroEnergy=10;HeroCheck(!ZipWeb()&&heroEnergy==10,"zip cannot overspend energy");
        BeginOpenWorld();var pauseAt=worldPosition;ToggleMap();yield return new WaitForSecondsRealtime(.3f);
        HeroCheck(stage==Stage.Paused&&Time.timeScale==0&&worldPosition==pauseAt,"map pauses movement");
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,"hero-city-map.png"));yield return new WaitForSecondsRealtime(.3f);ToggleMap();HeroCheck(stage==Stage.Racing&&Time.timeScale==1,"map resumes");
        beforePause=Stage.Paused;stage=Stage.Paused;Time.timeScale=0;ToggleMap();ToggleMap();HeroCheck(stage==Stage.Paused&&Time.timeScale==0,"map preserves existing pause");stage=Stage.Racing;Time.timeScale=1;
        journal=true;beforePause=Stage.Racing;stage=Stage.Paused;Time.timeScale=0;yield return new WaitForSecondsRealtime(.4f);ScreenCapture.CaptureScreenshot(Path.Combine(dir,"hero-task-journal.png"));yield return new WaitForSecondsRealtime(.3f);
        StartHeroTask(0);worldPosition=world.At(heroTasks[0].Points[0].x,heroTasks[0].Points[0].z);InteractTask();HeroCheck(taskStep==0,"ground below rooftop cannot rescue");
        Vector3 rescue=heroTasks[0].Points[0];Bounds rescueBuilding=new Bounds();foreach(var b in world.Buildings)if(b.Contains(rescue-Vector3.up*.3f)){rescueBuilding=b;break;}
        HeroCheck(rescueBuilding.size.y>0,"rescue has a real building surface");
        worldPosition=new Vector3(rescueBuilding.min.x-1,world.Height(rescueBuilding.min.x-1,rescue.z)+.18f,rescue.z);heroVelocity=Vector3.zero;grounded=true;cameraYaw=90;
        for(int i=0;i<750;i++){StepHero(.02f,Vector2.up,false,false,false,false,true);if(grounded&&worldPosition.y>=rescueBuilding.max.y)break;}
        HeroCheck(grounded&&Vector3.Distance(worldPosition,rescue)<4,"resident reachable through wall climb");InteractTask();HeroCheck(taskStep==1,"climbing can complete real rescue interaction");
        StartHeroTask(1);worldPosition=heroTasks[1].Points[2];StepTasks(.02f);HeroCheck(taskStep==0,"out of order patrol beacon rejected");
        for(int task=0;task<3;task++){
            StartHeroTask(task);int count=heroTasks[task].Points.Length;
            for(int point=0;point<count;point++){
                if(task==2){
                    if(point==0){worldPosition=world.At(14,-345)+Vector3.up*.18f;heroVelocity=Vector3.zero;grounded=true;cameraYaw=0;}
                    Vector3 target=heroTasks[task].Points[point];for(int step=0;step<1500&&Vector3.Distance(worldPosition,target)>2.5f&&activeTask>=0;step++)StepHero(.02f,Vector2.up,true,false,false,false,false);
                    HeroCheck(activeTask==task&&Vector3.Distance(worldPosition,target)<3,"delivery reachable by running before timeout");
                }else{worldPosition=heroTasks[task].Points[point];heroVelocity=Vector3.zero;grounded=true;}
                if(task==1)StepTasks(.02f);else InteractTask();
            }
            HeroCheck(activeTask==-1&&heroTasks[task].Done,"task completion "+task);
        }
        HeroCheck(heroScore==750,"first completion rewards");StartHeroTask(0);foreach(var p in heroTasks[0].Points){worldPosition=p;InteractTask();}HeroCheck(heroScore==750,"replays do not farm score");
        StartHeroTask(2);StepTasks(151);HeroCheck(activeTask==-1,"delivery timeout");
        StartHeroTask(0);worldPosition=heroTasks[0].Points[0]+Vector3.right*1.8f;yield return HeroShot(dir,"hero-rescue");InteractTask();worldPosition=heroTasks[0].Points[1]+Vector3.right*1.8f;InteractTask();yield return HeroShot(dir,"hero-task-complete");
        RecoverHero();HeroCheck(grounded&&!swinging&&!world.HeroBlocked(worldPosition),"street recovery");
        yield return HeroUpgradeChecks(dir);
        BackToGarage();ShowMapSelection();SelectMap(1);BeginSelectedMap();HeroCheck(!inWorld&&player.root.gameObject.activeSelf&&!world.Root.gameObject.activeSelf&&stage==Stage.Countdown,"circuit restores car mode");
        BackToGarage();BeginOpenWorld();HeroCheck(!player.root.gameObject.activeSelf&&hero.Root.gameObject.activeInHierarchy,"hero reentry");
        File.WriteAllText(Path.Combine(dir,"hero-test.txt"),"PASS: human city / car circuit selection, "+citizens.Count+" moving pedestrians, walking, running, super jump, gravity, building collisions, wall climbing and roof landing, roof web anchors, tethered swing, release momentum, web zip energy, map pause/resume, ordered rooftop rescue/patrol/delivery objectives, rewards, replay, timeout, street recovery and mode switching.\nAll tests use the built Windows player. Screenshots: hero street, rooftop, swing, map, journal, rescue, completion.\n");
        Debug.Log("HERO_ALL_TESTS_PASS");Application.Quit();
    }
}
}
