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

		[Command("/freecam")]
		public void Freecam(string cmd, string args)
		{
			Ui.Instance.freecaming ^= true;
		}




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
