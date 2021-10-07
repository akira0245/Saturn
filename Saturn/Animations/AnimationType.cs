using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Animations
{


	interface IOrbit
	{
		Vector3 CalculateValue(Vector3 p1, Vector3 p2, double valueInternal);
	}

	class Lerp : IOrbit
	{
		public Vector3 CalculateValue(Vector3 p1, Vector3 p2, double valueInternal)
		{
			return Vector3.Lerp(p1, p2, (float)valueInternal);
		}
	}

	static class Arc
	{
		public static Vector3 GetVerticalDir(Vector3 vector3)
		{
			return Vector3.Normalize(new Vector3(vector3.Y, -vector3.X, 0));
		}

		public static Vector3 CalculateValue(Vector3 original, float valueInternal)
		{
			var fromAxisAngle = Quaternion.CreateFromAxisAngle(GetVerticalDir(original), (float)(Math.PI * 2 * valueInternal));
			var calculateValue = Vector3.Transform(original, fromAxisAngle);
			return calculateValue;
		}
	}
}
