using System;
using CommandSystem;
using RemoteAdmin;

namespace MotionPathInterpolation {

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SetRot : ICommand {

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            var hub = (sender as PlayerCommandSender)?.ReferenceHub;
            if (hub == null) {
                response = "You can't do that!";
                return false;
            }

            if (arguments.Count < 2 || !float.TryParse(arguments.At(0), out var x) || !float.TryParse(arguments.At(1), out var y)) {
                response = "Usage: setrot <x> <y>";
                return false;
            }

            hub.playerMovementSync.ForceRotation(new PlayerMovementSync.PlayerRotation(x, y));
            response = "Rotation sent.";
            return true;
        }

        public string Command => "setrot";
        public string[] Aliases => null;
        public string Description { get; } = "Look towards a specific angle.";

    }

}
