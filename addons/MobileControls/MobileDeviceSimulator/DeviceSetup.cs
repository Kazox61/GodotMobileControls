using Godot;

namespace GodotMobileControls;

[Tool]
public partial class DeviceSetup : Resource {
	[Export] public string DeviceName;
	[Export] public Vector2I DeviceSize;
	[Export] public Texture2D DeviceFrame;
	[ExportGroup("Safe Area")]
	[ExportSubgroup("Portrait")]
	[Export] public int SafeAreaPortraitTop;
	[Export] public int SafeAreaPortraitBottom;
	[Export] public int SafeAreaPortraitLeft;
	[Export] public int SafeAreaPortraitRight;
	[ExportSubgroup("Landscape")]
	[Export] public int SafeAreaLandscapeTop;
	[Export] public int SafeAreaLandscapeBottom;
	[Export] public int SafeAreaLandscapeLeft;
	[Export] public int SafeAreaLandscapeRight;
}