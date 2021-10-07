using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Hooking;
using static Saturn.Infrastructures.OffsetManager;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Saturn.Infrastructures
{
	public static unsafe partial class Offsets
	{
		[Function("89 54 24 10 53 55 57 41 54 41 55 41 56 48 83 EC 48 8B C2 45 8B E0 44 8B D2 45 32 F6 44 8B C2 45 32 ED")]
		public static IntPtr SetOption { get; private set; }

		[Offset("48 81 F9 ?? ?? ?? ?? 7C ?? B8 ?? ?? ?? ??", 3)]
		public static int AgentCountOffset { get; private set; }

		[StaticAddress("48 8D 0D ?? ?? ?? ?? 45 33 C0 33 D2 C6 40 09 01")]
		public static IntPtr CamPtr { get; private set; }

		public delegate IntPtr Apricot__Engine__Core__GetSingleton();
		[Function("E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 89 4c 24 ?? 4C 8D 4D ?? 4C 8D 44 24 ??")]
		public static IntPtr GetMatrixSingleton { get; private set; }
		public static Hook<Apricot__Engine__Core__GetSingleton> GetMatrixSingletonHook { get; set; }

		[Function("E8 ?? ?? ?? ?? 4C 8D 45 A0 48 8D 53 10")]
		public static IntPtr LookAtRH { get; private set; }


		[StaticAddress("48 8D 35 ?? ?? ?? ?? 48 8B 34 C6 F3")]
		public static IntPtr CameraManager { get; private set; }

		[Function("E8 ?? ?? ?? ?? 41 FF C6 49 83 C7 04 49 83 C4 08 ")]
		public static IntPtr WriteProjMatrix { get; private set; }

		public delegate byte sub_140A58350(void* a1, long a2, long a3, long a4);

		[Function("E8 ?? ?? ?? ?? 48 8B CB E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 33 D2 ")]
		public static IntPtr RefreshGroupPose { get; private set; }

		[Function(" E8 ?? ?? ?? ?? 49 8D 57 10 48 8D 4D 00 ")]
		public static IntPtr DOF { get; private set; }

		[StaticAddress("48 8B 05 ? ? ? ? C6 40 2C ?")]
		public static IntPtr DOFPtr { get; private set; }

		[Offset("48 8D ? ? ? ? ? E8 ? ? ? ? 49 8D ? ? 48 8D ? ? E8", 3)]
		public static int DOFPtrOffset { get; private set; }

	}
}
