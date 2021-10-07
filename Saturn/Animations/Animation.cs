using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Saturn.Animations.EasingFunctions;

namespace Saturn.Animations
{
	/// <summary>
	/// Base class facilitating the implementation of easing functions.
	/// </summary>
	public abstract class EasingV3
	{
		// TODO: Use game delta time here instead
		private readonly Stopwatch animationTimer = new();

		private double valueInternal;

		/// <summary>
		/// Initializes a new instance of the <see cref="EasingV3"/> class with the specified duration.
		/// </summary>
		/// <param name="duration">The animation duration.</param>
		protected EasingV3(TimeSpan duration)
		{
			this.Duration = duration;
		}

		protected EasingV3()
		{
			this.Duration = new TimeSpan(0, 0, 0, 3);
		}

		/// <summary>
		/// Gets or sets the origin point of the animation.
		/// </summary>
		public Vector3? Point1 { get; set; }

		/// <summary>
		/// Gets or sets the destination point of the animation.
		/// </summary>
		public Vector3? Point2 { get; set; }

		/// <summary>
		/// Gets the resulting point at the current timestep.
		/// </summary>
		public Vector3 EasedPoint { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the result of the easing should be inversed.
		/// </summary>
		public bool IsInverse { get; set; }

		/// <summary>
		/// Gets or sets the current value of the animation, from 0 to 1.
		/// </summary>
		public double Value
		{
			get
			{
				if (this.IsInverse)
					return 1 - this.valueInternal;

				return this.valueInternal;
			}

			protected set
			{
				this.valueInternal = value;

				if (this.Point1.HasValue && this.Point2.HasValue)
					this.EasedPoint = CalculateValue(Point1.Value, Point2.Value, valueInternal);
			}
		}

		private Vector3 CalculateValue(Vector3 p1, Vector3 p2, double value)
		{
			return Vector3.Lerp(p1, p2, (float)value);
		}

		/// <summary>
		/// Gets or sets the duration of the animation.
		/// </summary>
		public TimeSpan Duration { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not the animation is running.
		/// </summary>
		public bool IsRunning => this.animationTimer.IsRunning;

		/// <summary>
		/// Gets a value indicating whether or not the animation is done.
		/// </summary>
		public bool IsDone => this.animationTimer.ElapsedMilliseconds > this.Duration.TotalMilliseconds;

		/// <summary>
		/// Gets the progress of the animation, from 0 to 1.
		/// </summary>
		protected double Progress => this.animationTimer.ElapsedMilliseconds / this.Duration.TotalMilliseconds;

		/// <summary>
		/// Starts the animation from where it was last stopped, or from the start if it was never started before.
		/// </summary>
		public void Start()
		{
			this.animationTimer.Start();
		}

		/// <summary>
		/// Stops the animation at the current point.
		/// </summary>
		public void Stop()
		{
			this.animationTimer.Stop();
		}

		/// <summary>
		/// Restarts the animation.
		/// </summary>
		public void Restart()
		{
			this.animationTimer.Restart();
		}

		/// <summary>
		/// Updates the animation.
		/// </summary>
		public abstract void Update();
	}

	public static class AnimationTypes
	{
		//public static readonly TimeSpan DefaultEasingTime = TimeSpan.FromMilliseconds(3000);

		public static IReadOnlyList<EasingV3> Easings(int easingTime)
		{
			var DefaultEasingTime = TimeSpan.FromMilliseconds(easingTime);
			return new EasingV3[]
			{
				 new InSine(DefaultEasingTime), new OutSine(DefaultEasingTime), new InOutSine(DefaultEasingTime),
				 new InCubic(DefaultEasingTime), new OutCubic(DefaultEasingTime), new InOutCubic(DefaultEasingTime),
				 new InQuint(DefaultEasingTime), new OutQuint(DefaultEasingTime), new InOutQuint(DefaultEasingTime),
				 new InCirc(DefaultEasingTime), new OutCirc(DefaultEasingTime), new InOutCirc(DefaultEasingTime),
				 new InElastic(DefaultEasingTime), new OutElastic(DefaultEasingTime),
				 new InOutElastic(DefaultEasingTime),
			};
		}

		public static EasingV3 GetEasing(AnimationType type, int duration)
		{
			var time = new TimeSpan(0, 0, 0, 0, duration);
			return type switch
			{
				AnimationType.Start => new Start(time),
				AnimationType.End => new End(time),
				AnimationType.Linear => new Linear(time),
				AnimationType.InSine => new InSine(time),
				AnimationType.OutSine => new OutSine(time),
				AnimationType.InOutSine => new InOutSine(time),
				AnimationType.InCubic => new InCubic(time),
				AnimationType.OutCubic => new OutCubic(time),
				AnimationType.InOutCubic => new InOutCubic(time),
				AnimationType.InQuint => new InQuint(time),
				AnimationType.OutQuint => new OutQuint(time),
				AnimationType.InOutQuint => new InOutQuint(time),
				AnimationType.InCirc => new InCirc(time),
				AnimationType.OutCirc => new OutCirc(time),
				AnimationType.InOutCirc => new InOutCirc(time),
				AnimationType.InElastic => new InElastic(time),
				AnimationType.OutElastic => new OutElastic(time),
				AnimationType.InOutElastic => new InOutElastic(time),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
		}

		public enum AnimationType
		{
			Start, End,
			Linear,
			InSine = 100, OutSine, InOutSine,
			InCubic = 200, OutCubic, InOutCubic,
			InQuint = 300, OutQuint, InOutQuint,
			InCirc = 400, OutCirc, InOutCirc,
			InElastic = 500, OutElastic, InOutElastic
		}
	}
}
