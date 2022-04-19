using System;
using Exiled.API.Features;

namespace MotionPathInterpolation {

    public class InterpolationPlugin : Plugin<InterpolationConfig> {

        public static InterpolationPlugin Singleton { get; private set; }

        public InterpolationPlugin() {
            Singleton = this;
        }

        public override string Name => "MotionPathInterpolation";
        public override string Prefix => "MPI";
        public override string Author => "Axwabo";
        public override Version Version { get; } = new Version(1, 0, 0);

    }

}
