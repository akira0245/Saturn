using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Structs
{
	[StructLayout(LayoutKind.Explicit)]
	struct GameCamera
	{
		[FieldOffset(0x60)] public Vector3 TargetPosition;
		[FieldOffset(0x90)] public Vector3 EyePosition;
		[FieldOffset(0x108)] public Int32 cameraState;          //108 //seems to be 1 when moving, 0 when not moving?
		[FieldOffset(0x114)] public float zoomCurrent;          //114
		[FieldOffset(0x118)] public float zoomMin;              //118
		[FieldOffset(0x11C)] public float zoomMax;              //11C
		[FieldOffset(0x120)] public float fovCurrent;           //120
		[FieldOffset(0x124)] public float fovMin;               //124
		[FieldOffset(0x128)] public float fovMax;               //128
		[FieldOffset(0x12C)] public float AddedFoV;            
		[FieldOffset(0x130)] public float HRotation;
		[FieldOffset(0x134)] public float CurrentVRotation;    
		[FieldOffset(0x148)] public float MinVRotation;    
		[FieldOffset(0x14C)] public float MaxVRotation; 
		[FieldOffset(0x160)] public float Tilt;   
		[FieldOffset(0x170)] public Int32 Mode;         
		[FieldOffset(0x174)] public Int32 cameraType1;          //174 //I want to watch this one.
		[FieldOffset(0x178)] public Int32 cameraType2;          //178
		[FieldOffset(0x218)] public float CenterHeightOffset;   
		[FieldOffset(0x218)] public float Z2;   
		public static class Offsets
		{
			public const ushort X                  = 0x90;
			public const ushort Z                  = 0x94;
			public const ushort Y                  = 0x98;
			public const ushort CurrentZoom        = 0x114;
			public const ushort MinZoom            = 0x118;
			public const ushort MaxZoom            = 0x11C;
			public const ushort CurrentFoV         = 0x120;
			public const ushort MinFoV             = 0x124;
			public const ushort MaxFoV             = 0x128;
			public const ushort AddedFoV           = 0x12C;
			public const ushort HRotation          = 0x130;
			public const ushort CurrentVRotation   = 0x134;
			public const ushort MinVRotation       = 0x148;
			public const ushort MaxVRotation       = 0x14C;
			public const ushort Tilt               = 0x160;
			public const ushort Mode               = 0x170;
			public const ushort CenterHeightOffset = 0x218;
			public const ushort Z2                 = 0x2B4;
		}
	}
}
