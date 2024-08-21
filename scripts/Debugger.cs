using System.Collections.Generic;
using System;
using Godot;
public partial class Debugger : Node
{
    public static Debugger Instance { get; private set; }
    
    private RichTextLabel Label;
    private PackedScene SphereMarkerScene {get; set;}
	private PackedScene ArrowMarkerScene {get; set;}
    private Dictionary<string, string> PropertyDict = new Dictionary<string, string> ();

    public override void _Ready()
    {
        Instance = this;
        Label = ((PackedScene)GD.Load("res://scenes/debug/DebugLabel.tscn")).Instantiate() as RichTextLabel;
        AddChild(Label);
        ArrowMarkerScene = (PackedScene)GD.Load("res://scenes/debug/MarkerArrow.tscn");
		SphereMarkerScene = (PackedScene)GD.Load("res://scenes/debug/MarkerSphere.tscn");
    }
    public override void _Process(double delta)
    {
        SetProperty("FPS", (float)Engine.GetFramesPerSecond(), "", 3);
        UpdateDebugLabel();
    }
    void UpdateDebugLabel()
    {
        Label.Text = "[font_size=15][b][outline_size=6][outline_color=black]";
        foreach(var property in PropertyDict)
        {
            Label.Text += property.Key + ": " + property.Value + "\n";
        }
        Label.Text += "[/outline_color][/outline_size][/b][/font_size]";
    }
    public void CreateDebugArrow(Vector3 pos, Vector3 dir, float scale, float timer, Color color)
    {
        var arrow = CreateDebugGismo(ArrowMarkerScene, pos, scale, timer, color);
        if(dir.Normalized().Dot(Vector3.Up) > 1 - 0.0001f)
        {
            arrow.Rotate(Vector3.Right, Mathf.Pi / 2);
        }
        else if(dir.Normalized().Dot(Vector3.Down) > 1 - 0.0001f)
        {
            arrow.Rotate(Vector3.Right, -Mathf.Pi / 2);
        }
        else
        {
            arrow.LookAt(arrow.GlobalPosition + dir, Vector3.Up);
        }
    }
    public void CreateDebugSphere(Vector3 pos, float scale, float timer, Color color)
    {
        CreateDebugGismo(SphereMarkerScene, pos, scale, timer, color);
    }
    public DebugGismo CreateDebugGismo(PackedScene scene, Vector3 pos, float scale, float timer, Color color)
    {
        var gismo = (DebugGismo)scene.Instantiate();
        AddChild(gismo);

        gismo.GlobalPosition = pos;
        gismo.SetDuration(timer);
        gismo.SetColor(color);
        gismo.MultiplyScale(scale);
        return gismo;
    }
    public void SetProperty(string name, string value)
    {
        if(PropertyDict.ContainsKey(name))
        {
            PropertyDict[name] = value;
        }
        else
        {
            PropertyDict.Add(name, value);
        }
    }
    public void SetProperty(string name, float value, string unit = "", int decimals = 4)
    {
        float pow = (float)Math.Pow(10, decimals);
        var String = (Mathf.Round(value * pow)/pow).ToString();
		SetProperty(name, String[..Math.Min(String.Length, 3)] + unit);
		//int depth = 0;
    }
}