using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Afterglow
{
public partial class RaceGame
{
    bool keyboardTest;
    int keyboardFrames;
    Vector2 lastHeroInput;
    string lastKeyboardState;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    // This opt-in player test sends keyboard events only to its own focused window.
    // Gameplay still reads Unity Input and runs the normal Update loop.
    [StructLayout(LayoutKind.Sequential)] struct TestKeyboardEvent {public ushort key,scan;public uint flags,time;public UIntPtr extra;}
    [StructLayout(LayoutKind.Explicit,Size=32)] struct TestInputData {[FieldOffset(0)] public TestKeyboardEvent keyboard;}
    [StructLayout(LayoutKind.Sequential)] struct TestNativeInput {public uint type;public TestInputData data;}
    [DllImport("user32.dll")] static extern uint SendInput(uint count,TestNativeInput[] input,int size);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
    [DllImport("user32.dll")] static extern uint MapVirtualKey(uint key,uint type);
    bool OwnTestFocus(){GetWindowThreadProcessId(GetForegroundWindow(),out uint process);return process==Process.GetCurrentProcess().Id;}
    void TestKey(ushort key,bool down)
    {
        if(down&&!OwnTestFocus())throw new Exception("Keyboard test lost focus; refusing to send input to another app");
        uint scan=MapVirtualKey(key,0),extended=key>=0x21&&key<=0x28?1u:0u;
        var packet=new TestNativeInput{type=1,data=new TestInputData{keyboard=new TestKeyboardEvent{scan=(ushort)scan,flags=8u|extended|(down?0u:2u)}}};
        if(SendInput(1,new[]{packet},Marshal.SizeOf(typeof(TestNativeInput)))!=1)throw new Exception("Keyboard event could not be sent");
    }
#else
    bool OwnTestFocus()=>false;
    void TestKey(ushort key,bool down){throw new Exception("Keyboard integration test requires the Windows player");}
#endif
    IEnumerator HoldTestKeys(float seconds,params ushort[] keys)
    {
        foreach(ushort key in keys)TestKey(key,true);
        try{float until=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<until){if(!OwnTestFocus())throw new Exception("Keyboard test lost game focus");yield return null;}}
        finally{foreach(ushort key in keys)TestKey(key,false);}
        yield return null;yield return null;
    }
    IEnumerator KeyboardTest()
    {
        string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Logs"));Directory.CreateDirectory(dir);
        var routine=KeyboardTestSteps(dir);Exception failure=null;
        // Flatten the coroutine so input is released and failures produce a result file.
        var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(routine);
        while(stack.Count>0){object next=null;bool moved=false;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}
            catch(Exception ex){failure=ex;break;}
            if(!moved){(stack.Pop() as IDisposable)?.Dispose();continue;}
            if(next is IEnumerator nested){stack.Push(nested);continue;}
            yield return next;
        }
        while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();
        if(failure!=null){UnityEngine.Debug.LogException(failure);File.WriteAllText(Path.Combine(dir,"keyboard-test.txt"),"FAIL: "+failure);Application.Quit(1);}
        else{UnityEngine.Debug.Log("KEYBOARD_GAMEPLAY_PASS");Application.Quit();}
    }
    IEnumerator KeyboardTestSteps(string dir)
    {
        yield return new WaitForSecondsRealtime(3);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
#endif
        yield return new WaitForSecondsRealtime(.5f);HeroCheck(OwnTestFocus(),"game must have keyboard focus");
        yield return HoldTestKeys(.15f,0x4f); // O: enter city through the real menu input.
        HeroCheck(inWorld&&stage==Stage.Racing&&!worldTestingSteps,"keyboard city entry must enable live control / "+lastKeyboardState);
        yield return HoldTestKeys(.15f,0x45);HeroCheck(!journal&&stage==Stage.Racing,"E away from a car must not pause movement");
        Vector3 start=worldPosition;int frames=keyboardFrames;
        yield return HoldTestKeys(2,0x57); // W
        float walk=FlatDistance(start,worldPosition);UnityEngine.Debug.Log("KEYBOARD_WALK distance="+walk+" inputFrames="+(keyboardFrames-frames)+" input="+lastHeroInput+" pos="+worldPosition);
        HeroCheck(walk>4&&keyboardFrames>frames,"held W must walk through Update");
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,"keyboard-walking.png"));yield return new WaitForSecondsRealtime(.2f);
        start=worldPosition;yield return HoldTestKeys(2,0x10,0x53); // Shift + S
        float run=FlatDistance(start,worldPosition);UnityEngine.Debug.Log("KEYBOARD_RUN distance="+run);HeroCheck(run>walk*1.65f,"held Shift must run faster");
        start=worldPosition;yield return HoldTestKeys(2,0x26); // Up
        float arrows=FlatDistance(start,worldPosition);UnityEngine.Debug.Log("KEYBOARD_ARROW distance="+arrows+" input="+lastHeroInput);
        HeroCheck(arrows>4,"Up arrow must walk rather than leave hero stationary");
        start=worldPosition;yield return HoldTestKeys(.4f,0x27);HeroCheck(worldPosition.x>start.x+.7f,"Right arrow must walk sideways");
        float until=Time.realtimeSinceStartup+12;
        while(NearbyCar()<0&&Time.realtimeSinceStartup<until){
            Vector3 delta=cityCars[0].root.position-worldPosition;
            ushort horizontal=delta.x>1? (ushort)0x44:delta.x< -1?(ushort)0x41:(ushort)0;
            ushort vertical=delta.z>1?(ushort)0x57:delta.z< -1?(ushort)0x53:(ushort)0;
            if(horizontal!=0&&vertical!=0)yield return HoldTestKeys(.15f,horizontal,vertical);
            else if(horizontal!=0)yield return HoldTestKeys(.15f,horizontal);
            else if(vertical!=0)yield return HoldTestKeys(.15f,vertical);
            else break;
        }
        yield return new WaitForSecondsRealtime(.2f);HeroCheck(NearbyCar()>=0,"walk to nearby parked car using actual keys");
        yield return HoldTestKeys(.15f,0x47);HeroCheck(Driving&&!hero.Root.gameObject.activeSelf,"G enters the car through keyboard input");
        start=worldPosition;yield return HoldTestKeys(1,0x57);HeroCheck(FlatDistance(start,worldPosition)>3&&speed>5,"W drives entered car");
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,"keyboard-driving.png"));yield return new WaitForSecondsRealtime(.2f);
        yield return HoldTestKeys(.15f,0x47);HeroCheck(Driving,"G does not exit at speed");
        yield return HoldTestKeys(1.2f,0x20);HeroCheck(Mathf.Abs(speed)<1,"handbrake stops car");
        yield return HoldTestKeys(.15f,0x47);HeroCheck(!Driving&&hero.Root.gameObject.activeSelf,"G exits stopped car");
        yield return HoldTestKeys(.15f,0x45);HeroCheck(Driving&&!journal,"E enters nearby car without opening journal");
        yield return HoldTestKeys(.15f,0x45);HeroCheck(!Driving&&stage==Stage.Racing,"E exits a stopped car");
        start=worldPosition;yield return HoldTestKeys(1,0x53);HeroCheck(FlatDistance(start,worldPosition)>2,"walking resumes after exiting");
        yield return HoldTestKeys(.15f,0x4a);HeroCheck(journal&&stage==Stage.Paused,"J opens journal");start=worldPosition;
        yield return HoldTestKeys(.4f,0x57);HeroCheck(worldPosition==start,"journal pauses movement");
        yield return HoldTestKeys(.15f,0x4a);HeroCheck(!journal&&stage==Stage.Racing&&Time.timeScale==1,"J resumes gameplay");
        yield return HoldTestKeys(.15f,0x09);HeroCheck(fullMap&&stage==Stage.Paused,"Tab opens map");
        yield return HoldTestKeys(.15f,0x1b);HeroCheck(!fullMap&&stage==Stage.Racing&&Time.timeScale==1,"Escape closes map and resumes");
        yield return HoldTestKeys(.15f,0x1b);HeroCheck(stage==Stage.Paused,"Escape pauses");
        yield return HoldTestKeys(.15f,0x0d);HeroCheck(stage==Stage.Racing&&Time.timeScale==1,"Enter resumes");
        File.WriteAllText(Path.Combine(dir,"keyboard-test.txt"),"PASS: Windows scan-code keyboard events read by Unity Input through normal Update: O city entry, E away from car does not pause, W walking ("+walk.ToString("F2")+" m), Shift+S running ("+run.ToString("F2")+" m), arrow walking ("+arrows.ToString("F2")+" m), real-key approach to car, G enter, W drive, moving exit lock, Space handbrake, G exit, E reenter and exit, walking after exit, J journal pause/resume, Tab/Escape map and Escape/Enter pause/resume. Test never calls StepHero or StepCityCar directly.\n");
    }
}
}
