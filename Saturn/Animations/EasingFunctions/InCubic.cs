using System;
using System.Numerics;

namespace Saturn.Animations.EasingFunctions
{
	interface IEasingType
	{
		public double Update(double p);
	}

	interface ICameraAnimation<T>
	{
		public Vector3 GetPosition();
	}

	/// <summary>
	/// Class providing an "InCubic" easing animation.
	/// </summary>
	public class InCubic : EasingV3, IEasingType
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InCubic"/> class.
		/// </summary>
		/// <param name="duration">The duration of the animation.</param>
		public InCubic(TimeSpan duration)
			: base(duration)
		{
			// ignored
		}

		/// <inheritdoc/>
		public override void Update()
		{
			var p = this.Progress;
			this.Value = p * p * p;
		}

		public double Update(double p) => p * p * p;
	}
}
