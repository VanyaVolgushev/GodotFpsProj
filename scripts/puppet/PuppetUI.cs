using Godot;
using System;

public partial class PuppetUI : Control
{
	[Export] HBoxContainer ItemPreviewContainer { get; set; }
	[Export] PackedScene ItemPreviewScene { get; set; }
	public override void _Ready()
	{
		
	}
	public override void _Process(double delta)
	{
	}
	public void ClearPreviews()
	{
		foreach(Node ItemPreview in ItemPreviewContainer.GetChildren())
		{
			ItemPreview.QueueFree();
		}
	}
	public void AddItemPreview(CompressedTexture2D preview, bool primary, bool mirror)
	{
		var ItemPreview = ItemPreviewScene.Instantiate() as TextureRect;
		ItemPreviewContainer.AddChild(ItemPreview); 
		ItemPreview.Texture = preview;
		ItemPreview.FlipH = mirror;
		if(primary)
		{
			ItemPreview.Modulate = new Color(2.0f, 0.0f, 0.0f, 1f);
		}
		else
		{
			ItemPreview.Modulate = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		}
	}
}
