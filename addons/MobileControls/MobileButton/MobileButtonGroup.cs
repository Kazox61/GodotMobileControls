using System.Collections.Generic;
using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass]
public partial class MobileButtonGroup : Resource {
	public readonly List<MobileButton> Buttons = [];
	public MobileButton PressedButton;
}