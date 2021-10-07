using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Logging;
using Saturn.Infrastructures;
using static Saturn.Infrastructures.Offsets;

namespace Saturn
{
	unsafe class MainCameraHook : IDisposable
	{





		private MainCameraHook()
		{
			api.Framework.Update += Framework_Update;

			//GetMatrixSingletonHook = new Hook<Apricot__Engine__Core__GetSingleton>(
			//	GetMatrixSingleton,
			//	() =>
			//	{
			//		//PluginLog.Information($"[{count1++}]");
			//		var intPtr = GetMatrixSingletonHook.Original();
			//		var matrix = (MatrixSingleton*)intPtr;
			//		matrix->m_ProjectionMatrix = Matrix4x4.Identity;
			//		matrix->m_ProjectionMatrix2 = Matrix4x4.Identity;
			//		return intPtr;
			//	});
			//GetMatrixSingletonHook.Enable();

			LookAtRHHook = new Hook<LootAtRHPrototype>(LookAtRH, detour: (viewmatrix, eye, target, unk) =>
			{
				if (count++ == 0)
				{
					MatrixPtr = new IntPtr(viewmatrix);
					this.ViewMatrix = *viewmatrix;
					this.Eye = *eye;
					this.Target = *target;
					this.Unk = *unk;

					//if (DoCamControl)
					//{
					//	PluginLog.Information("docamcotrol null");
					//	return LookAtRH.Original(viewmatrix, eye, target, unk);
					//}
					//else
					//{
					//	PluginLog.Information("docamcotrol");

					//}
					PluginLog.Information($"{DoCamControl?.Method} {DoCamControl?.Target} {DoCamControl}");

					//if (DoCamControl..)
					//{
					//	DoCamControl?.Invoke(viewmatrix, eye, target, unk);
					//	PluginLog.Information($"doing cam control");
					//	return viewmatrix;
					//}
				}

				return LookAtRHHook.Original(viewmatrix, eye, target, unk);

				//var matrix4X4 = LookAtRH.Original(viewmatrix, eye, target, unk);
				//PluginLog.Debug($"[{count++}]{(long)matrix4X4:X} {(long)viewmatrix:X} {*eye} {*target} {*unk}");
				return viewmatrix;
			});

			LookAtRHHook.Enable();
		}

		private void Framework_Update(Dalamud.Game.Framework framework)
		{
			count = count1 = 0;
		}

		public static MainCameraHook Instance { get; } = new MainCameraHook();

		public int count;
		public int count1;

		public IntPtr MatrixPtr { get; private set; }
		public Matrix4x4 ViewMatrix { get; private set; }
		public Vector3 Target { get; private set; }
		public Vector3 Eye { get; private set; }
		public Vector3 Unk { get; private set; }

		public event OnCamControl DoCamControl;

		public delegate void OnCamControl(Matrix4x4* matrix, Vector3* eye, Vector3* target, Vector3* unk);

		public void Dispose()
		{
			ClearControls();
			api.Framework.Update -= Framework_Update;
		}

		public void ClearControls() => DoCamControl = delegate { };
	}
}
