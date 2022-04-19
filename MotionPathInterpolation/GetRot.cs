using System;
using CommandSystem;
using RemoteAdmin;

namespace MotionPathInterpolation {

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GetRot : ICommand {

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            var hub = (sender as PlayerCommandSender)?.ReferenceHub;
            if (hub == null) {
                response = "You can't do that!";
                return false;
            }

            response = hub.playerMovementSync.Rotations.ToString();
            return true;
        }

        public string Command => "getrot";
        public string[] Aliases => null;
        public string Description { get; } = "Get your rotation vector.";

    }

}
