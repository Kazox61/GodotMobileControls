#if TOOLS
using Godot;

[Tool]
public partial class Plugin : EditorPlugin {
	private Control _dock;
	
	public override void _EnterTree() {
		_dock = GD.Load<PackedScene>(GetPluginPath() + "/MobileDeviceSimulator/MobileDeviceSimulator.tscn").Instantiate<Control>();
		AddControlToDock(DockSlot.RightBl, _dock);
	}

	public override void _ExitTree() {
		RemoveControlFromDocks(_dock);
		_dock.Free();
	}

	private string GetPluginPath() {
		var script = (Script)GetScript();
		return script.GetPath().GetBaseDir();
	}
}
#endif