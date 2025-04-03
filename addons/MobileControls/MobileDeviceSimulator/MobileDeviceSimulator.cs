#if TOOLS
using Godot;

namespace GodotMobileControls;

[Tool]
public partial class MobileDeviceSimulator : Control {
	private const string SettingViewportWidthPath = "display/window/size/viewport_width";
	private const string SettingViewportHeightPath = "display/window/size/viewport_height";
	
	[Export] public DeviceSetup[] DeviceSetups;
	
	[Export] private PackedScene _deviceFrameContainerPrefab;
	
	[Export] private OptionButton _deviceSelector;
	[Export] private CheckBox _landscapeOrientation;
	[Export] private CheckBox _showFrame;

	private DeviceSetup _activeDeviceSetup;
	private DeviceFrameContainer _deviceFrameContainer;

	private Node _editingSceneRoot;
	private Node EditingSceneRoot {
		get => _editingSceneRoot;
		set {
			if (_editingSceneRoot != null) {
				_editingSceneRoot.RemoveChild(_deviceFrameContainer);
				_editingSceneRoot.TreeExited -= OnEditingSceneRootChanged;
			}
			
			_editingSceneRoot = value;

			if (_editingSceneRoot == null) {
				return;
			}
			
			_editingSceneRoot.AddChild(_deviceFrameContainer);
			_editingSceneRoot.TreeExited += OnEditingSceneRootChanged;
		}
	}
	
	private int ViewportWidth {
		get => ProjectSettings.GetSetting(SettingViewportWidthPath).AsInt32();
		set => ProjectSettings.SetSetting(SettingViewportWidthPath, value);
	}
	
	private int ViewportHeight {
		get => ProjectSettings.GetSetting(SettingViewportHeightPath).AsInt32();
		set => ProjectSettings.SetSetting(SettingViewportHeightPath, value);
	}

	public override void _EnterTree() {
		_deviceSelector.Clear();
		foreach (var deviceSetup in DeviceSetups) {
			_deviceSelector.AddItem(deviceSetup.DeviceName);
		}
		
		_activeDeviceSetup = DeviceSetups[_deviceSelector.GetSelected()];
		
		_deviceSelector.ItemSelected += OnDeviceSelected;
		_landscapeOrientation.Toggled += OnLandscapeOrientationToggled;
		_showFrame.Toggled += OnShowFrameToggled;

		_deviceFrameContainer = _deviceFrameContainerPrefab.Instantiate<DeviceFrameContainer>();
		
		EditingSceneRoot = EditorInterface.Singleton.GetEditedSceneRoot();
	}

	public override void _ExitTree() {
		if (_deviceFrameContainer != null) {
			_deviceFrameContainer.GetParent().RemoveChild(_deviceFrameContainer);
			_deviceFrameContainer.Free();
			_deviceFrameContainer = null;
		}
		
		_deviceSelector.ItemSelected -= OnDeviceSelected;
		_landscapeOrientation.Toggled -= OnLandscapeOrientationToggled;
		_showFrame.Toggled -= OnShowFrameToggled;
	}

	private void UpdateDevice(DeviceSetup deviceSetup) {
		_activeDeviceSetup = deviceSetup;
		
		ViewportWidth = deviceSetup.DeviceSize.X;
		ViewportHeight = deviceSetup.DeviceSize.Y;
		
		_deviceFrameContainer.UpdateDevice(deviceSetup);
	}

	private void OnDeviceSelected(long index) {
		var deviceSetup = DeviceSetups[index];
		UpdateDevice(deviceSetup);
	}
	
	private void OnShowFrameToggled(bool isChecked) {
		if (_deviceFrameContainer != null) {
			_deviceFrameContainer.Visible = isChecked;
		}
	}

	private void OnLandscapeOrientationToggled(bool isChecked) {
		if (_deviceFrameContainer == null) {
			return;
		}
		
		if (isChecked) {
			ViewportWidth = _activeDeviceSetup.DeviceSize.Y;
			ViewportHeight = _activeDeviceSetup.DeviceSize.X;
			_deviceFrameContainer.RotateToLandscape();
			return;
		}

		ViewportWidth = _activeDeviceSetup.DeviceSize.X;
		ViewportHeight = _activeDeviceSetup.DeviceSize.Y;
		_deviceFrameContainer.RotateToPortrait();
	}
	
	private void OnEditingSceneRootChanged() {
		Callable.From(() => EditingSceneRoot = EditorInterface.Singleton.GetEditedSceneRoot()).CallDeferred();
	}
}
#endif