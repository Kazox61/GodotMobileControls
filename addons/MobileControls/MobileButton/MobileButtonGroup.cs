using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotMobileControls;

[Tool, GlobalClass]
public partial class MobileButtonGroup : Resource {
	private readonly HashSet<MobileButton> _buttons = [];
	private bool _allowUnpress;

	[Export]
	public bool AllowUnpress {
		get => _allowUnpress;
		set => _allowUnpress = value;
	}

	[Signal]
	public delegate void PressedEventHandler(MobileButton button);

	public MobileButtonGroup() {
		SetLocalToScene(true);
	}

	public MobileButton GetPressedButton() {
		return _buttons.FirstOrDefault(button => button.ButtonPressed);
	}

	public Godot.Collections.Array<MobileButton> GetButtons() {
		var buttonArray = new Godot.Collections.Array<MobileButton>();
		foreach (var button in _buttons) {
			buttonArray.Add(button);
		}
		return buttonArray;
	}

	public bool IsAllowUnpress() {
		return _allowUnpress;
	}

	internal void AddButton(MobileButton button) {
		_buttons.Add(button);
	}

	internal void RemoveButton(MobileButton button) {
		_buttons.Remove(button);
	}

	internal HashSet<MobileButton> GetButtonsInternal() {
		return _buttons;
	}
}