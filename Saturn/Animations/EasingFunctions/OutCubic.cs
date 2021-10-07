using System;

namespace Saturn.Animations.EasingFunctions
{
    /// <summary>
    /// Class providing an "OutCubic" easing animation.
    /// </summary>
    public class OutCubic : EasingV3
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutCubic"/> class.
        /// </summary>
        /// <param name="duration">The duration of the animation.</param>
        public OutCubic(TimeSpan duration)
            : base(duration)
        {
            // ignored
        }

        /// <inheritdoc/>
        public override void Update()
        {
            var p = this.Progress;
            this.Value = 1 - Math.Pow(1 - p, 3);
        }
    }
}
