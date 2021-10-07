using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Vector2 = System.Numerics.Vector2;

namespace Saturn
{
	[StructLayout(LayoutKind.Explicit)]
	unsafe struct MatrixSingleton
	{
		[FieldOffset(0)] public void* vtbl;
		[FieldOffset(0x144)] public RawMatrix4x3 m_ViewMatrix;
		[FieldOffset(0x174)] public Matrix4x4 m_ProjectionMatrix;
		[FieldOffset(0x1b4)] public Matrix4x4 m_ViewProjectionMatrix;
		[FieldOffset(0x1f4)] public Vector2 ViewportSize;
		[FieldOffset(0x770)] public RawMatrix4x3 m_ViewMatrix2;
		[FieldOffset(0x7A0)] public Matrix4x4 m_ProjectionMatrix2;
		[FieldOffset(0x7E0)] public Matrix4x4 m_ViewProjectionMatrix2;
		[FieldOffset(0x820)] public Vector2 ViewportSize2;
	}
}
