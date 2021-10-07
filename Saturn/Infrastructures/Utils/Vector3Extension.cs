using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DigitalRune.Mathematics.Algebra;

namespace Saturn.Infrastructures.Utils
{
	static class Vector3Extension
	{
		public static unsafe Vector3F ToVector3F(this Vector3 vector3) => *(Vector3F*)&vector3;
		public static unsafe Vector3 ToVector3(this Vector3F vector3) => *(Vector3*)&vector3;
	}
}
