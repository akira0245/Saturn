using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Animations.EasingFunctions
{
	/// <summary>
	/// Class providing a value equals to start point.
	/// </summary>
	public class Start : EasingV3
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Start"/> class.
		/// </summary>
		/// <param name="duration">The duration of the animation.</param>
		public Start(TimeSpan duration)
			: base(duration)
		{
			// ignored
		}

		/// <inheritdoc/>
		public override void Update()
		{
			this.Value = 0;
		}
	}

	/// <summary>
	/// Class providing a value equals to destination.
	/// </summary>
	public class End : EasingV3
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="End"/> class.
		/// </summary>
		/// <param name="duration">The duration of the animation.</param>
		public End(TimeSpan duration)
			: base(duration)
		{
			// ignored
		}

		/// <inheritdoc/>
		public override void Update()
		{
			this.Value = 1;
		}
	}

	/// <summary>
	/// Class providing an easing animation does absolutely nothing.
	/// </summary>
	public class Linear : EasingV3
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="None"/> class.
		/// </summary>
		/// <param name="duration">The duration of the animation.</param>
		public Linear(TimeSpan duration)
			: base(duration)
		{
			// ignored
		}

		/// <inheritdoc/>
		public override void Update()
		{
			this.Value = Progress;
		}
	}
}
