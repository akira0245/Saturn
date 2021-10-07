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
			_ = MainCameraHook.Instance;
			_ = Ui.Instance;
		}

		public string Name => nameof(Saturn);

		private void ReleaseUnmanagedResources()
		{
			Ui.Instance.Dispose();
			MainCameraHook.Instance.Dispose();
			Config.Save();
			api.Dispose();
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
