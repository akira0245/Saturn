using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Saturn.Infrastructures;

namespace Saturn
{
	unsafe class DOF
	{
		//public delegate void sub_140178990(DOFStruct* a1, long a2, long a3);
		//public Hook<sub_140178990> hook;

		public ref float Near => ref DOFStructPtr->near;
		public ref float Mid => ref DOFStructPtr->mid;
		public ref float Far => ref DOFStructPtr->far;
		public DOFStruct* DOFStructPtr => (DOFStruct*)(*(IntPtr*)Offsets.DOFPtr + Offsets.DOFPtrOffset);
		
		private DOF()
		{
			//hook = new Hook<sub_140178990>(Offsets.DOF, (a1, a2, a3) =>
			//{
			//	DOFStructPtr = a1;

			//	a1->near = Near;
			//	a1->mid = Mid;
			//	a1->far = Far;
			//	a1->unkVector = Vector3.One;
			//	a1->Enabled = 0x40;
			//	hook.Original(a1, a2, a3);
			//});
			//hook.Enable();
		}

		public static DOF Instance { get; } = new DOF();


		[StructLayout(LayoutKind.Explicit)]
		public struct DOFStruct
		{
			[FieldOffset(0x18)] public float near;
			[FieldOffset(0x1C)] public float mid;
			[FieldOffset(0x20)] public float far;
			[FieldOffset(0x24)] public Vector3 unkVector;
			[FieldOffset(0x24)] public float unk1;
			[FieldOffset(0x28)] public float unk2;
			[FieldOffset(0x2C)] public float unk3;
			[FieldOffset(0xC3)] public byte Enabled;

		}
	}
}
