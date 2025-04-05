using System.Threading.Tasks;
using Godot;

namespace GodotMobileControls.Extensions;

public static class TweenExtensions {
	public static Task PlayAsync(this Tween tween) {
		var tcs = new TaskCompletionSource();

		tween.Play();
		tween.Finished += () => tcs.SetResult();

		return tcs.Task;
	}
}