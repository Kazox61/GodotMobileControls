using System;
using System.Reflection;
using Godot;

namespace GodotMobileControls.Debug;

[GlobalClass]
public partial class BindLabelToValue : Node {
	[Export] private Label _label;
	[Export] private Node _target;
	[Export] private string _targetPath;

	public override void _Process(double delta) {
		object value = default;
		try {
			value = GetPropertyValue(_target, _targetPath);
		}
		catch (Exception) {
			// ignored
		}

		try {
			value = GetFieldValue(_target, _targetPath);
		}
		catch (Exception) {
			// ignored
		}

		if (value == null) {
			return;
		}

		_label.Text = $"{_targetPath}: {value}";
	}

	private static object GetPropertyValue(object obj, string propertyName) {
		ArgumentNullException.ThrowIfNull(obj);

		var type = obj.GetType();
		var propertyInfo = type.GetProperty(propertyName);

		if (propertyInfo == null) {
			throw new ArgumentException($"Property '{propertyName}' not found on type '{type.FullName}'");
		}

		return propertyInfo.GetValue(obj);
	}


	private static object GetFieldValue(object obj, string fieldName) {
		ArgumentNullException.ThrowIfNull(obj);

		var type = obj.GetType();
		var fieldInfo =
			type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		if (fieldInfo == null) {
			throw new ArgumentException($"Field '{fieldName}' not found on type '{type.FullName}'");
		}

		return fieldInfo.GetValue(obj);
	}
}