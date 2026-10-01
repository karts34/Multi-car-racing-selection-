using UnityEngine;

namespace Afterglow
{
public partial class RaceGame
{
    bool choosingMap;
    int selectedMap;
    void ShowMapSelection()
    {
        if(stage!=Stage.Menu)return;
        if(world==null){world=new OpenWorld();world.Root.gameObject.SetActive(false);}
        choosingMap=true;help=false;fullMap=false;
    }
    void SelectMap(int index){selectedMap=Mathf.Clamp(index,0,1);SaveSettings();}
    void BeginSelectedMap(){if(selectedMap==0)BeginOpenWorld();else BeginRace();}
    void DrawMapSelection()
    {
        Panel(0,0,1600,900,new Color(.025f,.037f,.048f,.99f));Panel(0,0,1600,4,accent);
        Label(65,40,1000,28,"A P E X   /   D E S T I N A T I O N S",15,accent);
        Label(60,88,1200,70,"CHOOSE YOUR ADVENTURE",46,Color.white);
        Label(65,165,1200,32,Selected.Name+"   /   "+CarCatalog.PaintNames[paintIndex]+"   /   "+DifficultyName+" RIVALS",16,faint);
        for(int i=0;i<2;i++){
            float x=65+i*757;bool chosen=selectedMap==i;
            Panel(x,225,708,485,chosen?new Color(.09f,.17f,.20f):ink);Panel(x,225,708,4,chosen?accent:faint);
            Label(x+26,246,650,27,"0"+(i+1)+" / "+(i==0?"OPEN WORLD":"RACE TRACK")+"     "+(chosen?"SELECTED":""),14,chosen?accent:faint);
            GUI.color=Color.white;GUI.DrawTexture(new Rect(x+24,294,310,282),i==0?world.Map:map,ScaleMode.ScaleToFit);
            Label(x+360,303,324,51,i==0?"SOLSTICE CITY":"GRAND TOUR",26,Color.white);
            Label(x+360,365,324,125,i==0?"Play as the human Web Sentinel.\nWalk, run, swing and walk on walls.\nLift cars or enter them and drive.":"A dedicated mountain circuit.\nPaddock, canyon, forest and lake.\nRace four rivals across three laps.",16,faint);
            Label(x+360,509,324,50,i==0?"2.7 x 2.7 KM REGION\n"+(world.RoadLength/1000).ToString("F1")+" KM OF ROADS":(track.Length/1000).ToString("F2")+" KM CIRCUIT\n3 LAPS / 5 DRIVERS",15,Color.white);
            Label(x+28,610,645,25,i==0?"HUMAN HERO  /  SUPER STRENGTH  /  DRIVE CARS":"CIRCUIT RACING  /  PERSONAL BEST LAP & TIME",14,chosen?accent:faint);
            if(Button(new Rect(x+26,652,656,40),chosen?"SELECTED DESTINATION":"SELECT THIS MAP",chosen))SelectMap(i);
        }
        if(Button(new Rect(65,767,330,59),"BACK TO CAR SELECTION",false))choosingMap=false;
        if(Button(new Rect(822,767,708,59),selectedMap==0?"ENTER SOLSTICE CITY     >":"START GRAND TOUR RACE     >",true))BeginSelectedMap();
        Label(65,853,1300,25,"LEFT / RIGHT  Choose map    /    ENTER  Play    /    ESC  Back    /    City: human hero    /    Circuit: selected car",14,faint);
    }
}
}
