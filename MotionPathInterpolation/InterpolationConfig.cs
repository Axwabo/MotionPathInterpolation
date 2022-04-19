using Exiled.API.Interfaces;

namespace MotionPathInterpolation {

    public class InterpolationConfig : IConfig {

        public bool IsEnabled { get; set; } = true;

        public int MaxPointsPerUser { get; set; } = 1000;

        public int MaxInterval { get; set; } = 300;

        public EasingType DefaultEasing { get; set; } = EasingType.Bezier;

    }

}
