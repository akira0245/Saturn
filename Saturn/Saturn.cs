using System;
using Dalamud.Plugin;
using Saturn.Infrastructures;

namespace Saturn
{
	public class Saturn : IDalamudPlugin
	{
		public Saturn(DalamudPluginInterface pluginInterface)
		{
			api.Initialize(this, pluginInterface);
			OffsetManager.Setup(api.SigScanner);
			_ = ViewMatrixHook.Instance;
			_ = Ui.Instance;
		}

		public string Name => nameof(Saturn);

		private void ReleaseUnmanagedResources()
		{
			Ui.Instance.Dispose();
			ViewMatrixHook.Instance.Dispose();
			Config.Save();
			api.Dispose();
		}

		[Command("/freecam")] public void freecam(string cmd, string args) => Ui.Instance.freecaming ^= true;
		[Command("/saturn")] public void saturn(string cmd, string args) => Ui.Instance.visible ^= true;
		[Command("/pathadd")] public void pathAdd(string cmd, string args) => Ui.Instance.AddCurrentCamPositionToPath();
		[Command("/pathclear")] public void pathClear(string cmd, string args) => Ui.Instance.CameraPathClear();
		[Command("/animbegin")] public void animBegin(string cmd, string args) =>  Ui.Instance.CameraBeginControl();
		[Command("/animstop")] public void animStop(string cmd, string args) => ViewMatrixHook.Instance.ClearControls();


		public void Dispose()
		{
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}
		~Saturn()
		{
			ReleaseUnmanagedResources();
		}
	}
}
