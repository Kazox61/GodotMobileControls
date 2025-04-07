using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace GodotMobileControls.Extensions;

public static class TweenExtensions {
	public static Task PlayAsync(this Tween tween) {
		var tcs = new TaskCompletionSource();

		tween.Finished += FinishedHandler;

		tween.Play();

		return tcs.Task;

		void FinishedHandler() {
			tween.Finished -= FinishedHandler;
			tcs.TrySetResult();
		}
	}

	public static Task PlayAsync(this Tween tween, CancellationToken cancellationToken) {
		if (cancellationToken.IsCancellationRequested) {
			return Task.FromCanceled(cancellationToken);
		}

		var tcs = new TaskCompletionSource();

		tween.Finished += FinishedHandler;

		CancellationTokenRegistration registration = default;
		if (cancellationToken.CanBeCanceled) {
			registration = cancellationToken.Register(() => {
				tween.Finished -= FinishedHandler;
				tween.Stop();
				tcs.TrySetCanceled(cancellationToken);
			});
		}

		tween.Play();

		return tcs.Task.ContinueWith(task => {
			registration.Dispose();
			return task;
		}, TaskContinuationOptions.ExecuteSynchronously).Unwrap();

		void FinishedHandler() {
			tween.Finished -= FinishedHandler;
			tcs.TrySetResult();
		}
	}
}