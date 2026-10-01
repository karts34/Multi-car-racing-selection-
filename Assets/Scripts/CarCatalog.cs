using UnityEngine;

namespace Afterglow
{
public enum CarBody { Sport, Luxury, Muscle, Hyper }

public sealed class CarSpec
{
    public readonly string Id, Name, Class, Description;
    public readonly CarBody Body;
    public readonly float TopSpeed, BoostSpeed, Acceleration, Handling, Grip, Braking;
    public readonly int Power;
    public CarSpec(string id,string name,string category,string description,CarBody body,int power,float top,float boost,float acceleration,float handling,float grip,float braking)
    { Id=id;Name=name;Class=category;Description=description;Body=body;Power=power;TopSpeed=top;BoostSpeed=boost;Acceleration=acceleration;Handling=handling;Grip=grip;Braking=braking; }
}

public static class CarCatalog
{
    public static readonly CarSpec[] All={
        new CarSpec("sovereign","SOVEREIGN V12","LUXURY GRAND TOURER","Command the circuit in a two-tone luxury saloon.\nLong bonnet. Chrome grille. Effortless V12 power.",CarBody.Luxury,610,69,84,13.8f,.82f,1.95f,32),
        new CarSpec("vantage","VANTAGE GT-R","PRECISION SPORTS COUPE","Light on its feet, sharp through every apex.\nThe balanced choice for the mountain circuit.",CarBody.Sport,630,73,88,15.3f,1.08f,1.8f,34),
        new CarSpec("outlaw","OUTLAW V8","AMERICAN MUSCLE","Wide shoulders and a thunderous V8.\nBig acceleration. Playful, flowing drifts.",CarBody.Muscle,710,75,89,17f,.90f,1.3f,30),
        new CarSpec("spectre","SPECTRE X","TRACK HYPERCAR","Low, wide and shaped by the wind.\nMaximum speed with race-bred cornering.",CarBody.Hyper,890,81,96,18.7f,1.18f,2.05f,37)
    };
    public static readonly Color[] Paints={new Color(.10f,.20f,.26f),new Color(.82f,.83f,.78f),new Color(.035f,.045f,.06f),new Color(.66f,.07f,.06f),new Color(.85f,.48f,.12f),new Color(.10f,.27f,.63f)};
    public static readonly string[] PaintNames={"MIDNIGHT TEAL","PEARL SILVER","ONYX BLACK","CRIMSON RED","CHAMPAGNE GOLD","COBALT BLUE"};
}
}
