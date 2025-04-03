using Godot;

namespace GodotMobileControls;

[Tool]
public partial class DeviceSetup : Resource {
	[Export] public string DeviceName;
	[Export] public Vector2I DeviceSize;
	[Export] public Texture2D DeviceFrame;
}