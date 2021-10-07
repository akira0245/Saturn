//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Text;
//using System.Threading.Tasks;
//using Dalamud.Interface.Animation;

//namespace Saturn.Animations
//{
//	abstract class EasingFloat : Easing<float>
//	{

//		public override float Lerp(float start, float end, double valueInternal)
//		{
//			return AnimUtil.Lerp(start, end, (float)valueInternal);
//		}

//		protected EasingFloat(TimeSpan duration) : base(duration)
//		{
//		}
//	}

//	abstract class EasingV2 : Easing<Vector2>
//	{

//		public override Vector2 Lerp(Vector2 start, Vector2 end, double valueInternal)
//		{
//			return Vector2.Lerp(start, end, (float)valueInternal);
//		}

//		protected EasingV2(TimeSpan duration) : base(duration)
//		{
//		}
//	}

//	abstract class EasingV3 : Easing<Vector3>
//	{

//		public override Vector3 Lerp(Vector3 start, Vector3 end, double valueInternal)
//		{
//			return Vector3.Lerp(start, end, (float)valueInternal);
//		}

//		protected EasingV3(TimeSpan duration) : base(duration)
//		{
//		}
//	}
//}
