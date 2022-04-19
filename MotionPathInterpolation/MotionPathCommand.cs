using System;
using CommandSystem;
using Exiled.API.Features;

namespace MotionPathInterpolation {

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MotionPathCommand : ICommand, IUsageProvider {

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            Player p;
            if ((p = Player.Get(sender)) == null) {
                response = "Only a player can perform this command!";
                return false;
            }

            response = "Usage: " + string.Join("\n", Usage);
            if (arguments.Count < 1)
                return false;
            var path = p.GetPath();
            var pos = p.Position;
            var rot = p.Rotation;
            switch (arguments.At(0).ToLower()) {
                case "create": {
                    if (path != null) {
                        response = "You've already created a motion path! To remove it, use 'motionpath delete'";
                        return false;
                    }

                    if (arguments.Count < 2 || !byte.TryParse(arguments.At(1), out var interval))
                        interval = 60;
                    path = p.CreatePath(interval);
                    response = $"Motion path created! Interval: {path.Interval}";
                    return true;
                }
                case "interval": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (arguments.Count < 2) {
                        response =
                            $"Current interval: {path.Interval}\nUse 'motionpath interval [newInterval]' to change it";
                        return true;
                    }

                    if (!int.TryParse(arguments.At(1), out var interval)) {
                        response = "Invalid integer!";
                        return false;
                    }

                    var max = InterpolationPlugin.Singleton.Config.MaxInterval;
                    if (interval > max) {
                        response = $"The maximum interval is {max}, setting it to that value.";
                        interval = max;
                    } else
                        response = $"Interval is now {interval}";

                    path.Interval = interval;
                    return true;
                }
                case "delay": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (arguments.Count < 2) {
                        response = $"Current delay: {path.Delay}s";
                        return true;
                    }

                    if (!byte.TryParse(arguments.At(1), out var delay)) {
                        response = "Invalid byte!";
                        return false;
                    }

                    response = $"Delay is now {path.Delay = delay}s";
                    return true;
                }
                case "delete": {
                    var success = p.RemovePath();
                    response = success ? "Path deleted." : "You don't have a motion path!";
                    return success;
                }
                case "points":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    response = $"Points ({path.SpecifiedPoints.Count}):\n{string.Join(", ", path.SpecifiedPoints)}";
                    return true;
                case "rotations":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    response = $"Rotations ({path.SpecifiedRotations.Count}):\n{string.Join(", ", path.SpecifiedRotations)}";
                    return true;
                case "gotostart": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    var result = path.GoToStart();
                    response = result ? "Teleporting you to the start." : "No points are specified!";
                    return result;
                }
                case "generatedrot":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (!path.Generated) {
                        response = "Path not generated yet! Use 'motionpath generate'";
                        return false;
                    }

                    response =
                        $"Generated rotation ({path.GeneratedRotation.Length}):\n{string.Join(", ", path.GeneratedRotation)}";
                    return true;
                case "generated":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (!path.Generated) {
                        response = "Path not generated yet! Use 'motionpath generate'";
                        return false;
                    }

                    response =
                        $"Generated points ({path.GeneratedPath.Length}):\n{string.Join(", ", path.GeneratedPath)}";
                    return true;
                case "add": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (arguments.Count < 2 || !float.TryParse(arguments.At(1), out var x))
                        x = pos.x;
                    if (arguments.Count < 3 || !float.TryParse(arguments.At(2), out var y))
                        y = pos.y;
                    if (arguments.Count < 4 || !float.TryParse(arguments.At(3), out var z))
                        z = pos.z;

                    var success = path.AddPoint(x, y, z);
                    response = success
                        ? $"Point ({x}, {y}, {z}) added."
                        : $"You've already reached the vector limit ({MotionPath.MaxPoints} points).";
                    return success;
                }
                case "addrot": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (arguments.Count < 2 || !float.TryParse(arguments.At(1), out var x))
                        x = rot.x;
                    if (arguments.Count < 3 || !float.TryParse(arguments.At(2), out var y))
                        y = rot.y;

                    var success = path.AddRotation(x, y);
                    response = success
                        ? $"Rotation ({x}, {y}) added."
                        : $"You've already reached the rotation limit ({MotionPath.MaxPoints} points).";
                    return success;
                }
                case "removefirst": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    var success = path.RemoveFirstPos();
                    response = success ? "First position removed." : "There are no points to remove.";
                    return success;
                }
                case "removelast": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    var success = path.RemoveLastPos();
                    response = success ? "Last position removed." : "There are no points to remove.";
                    return success;
                }
                case "removeat": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (arguments.Count < 2) {
                        response = "Usage: motionpath removeAt <index>";
                        return false;
                    }

                    if (!int.TryParse(arguments.At(1), out var i)) {
                        response = "Invalid integer.";
                        return false;
                    }

                    var success = path.RemovePosAt(i);
                    response = success ? $"Vector at position {i} removed." : "There are no points to remove.";
                    return success;
                }
                case "clear":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    path.ClearPos();
                    response = "Path cleared.";
                    return true;
                case "removefirstrot": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    var success = path.RemoveFirstRot();
                    response = success ? "First rotation removed." : "There are no points to remove.";
                    return success;
                }
                case "removelastrot": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    var success = path.RemoveLastRot();
                    response = success ? "Last rotation removed." : "There are no points to remove.";
                    return success;
                }
                case "removerotat": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (arguments.Count < 2) {
                        response = "Usage: motionpath removeRotAt <index>";
                        return false;
                    }

                    if (!int.TryParse(arguments.At(1), out var i)) {
                        response = "Invalid integer.";
                        return false;
                    }

                    var success = path.RemoveRotAt(i);
                    response = success ? $"Rotation at position {i} removed." : "There are no points to remove.";
                    return success;
                }
                case "clearrot":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    path.ClearRot();
                    response = "Rotations cleared.";
                    return true;
                case "generate": {
                    if (path != null)
                        return path.Generate(out response);
                    response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                    return false;
                }
                case "easing": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    if (arguments.Count < 2) {
                        response = $"Current easing: {path.Easing}\nTo change it, use 'motionpath easing <type>'";
                        return true;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out EasingType easing)) {
                        response = "Valid easing types: Linear, Bezier, CubicSpline";
                        return false;
                    }

                    response = $"Easing set to {path.Easing = easing}";
                    return true;
                }
                case "start": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    var success = path.StartMotion();
                    response = success ? "Started motion." : "Path not generated yet! Use 'motionpath generate'";
                    return success;
                }
                case "restart": {
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    var result = path.Restart();
                    response = result ? "Restarting motion." : "Path not generated yet! Use 'motionpath generate'";
                    return result;
                }
                case "pause":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    path.Pause();
                    response = "Paused motion.";
                    return true;
                case "stop":
                    if (path == null) {
                        response = "You haven't created a motion path yet! Use 'motionpath create [interval]'";
                        return false;
                    }

                    path.Stop();
                    response = "Stopped motion.";
                    return true;
                case "export": {
                    var result = p.ReferenceHub.RamSavePath();
                    response = result ? "MotionPath saved to server memory." : "You haven't created a motion path yet!";
                    return result;
                }
                case "import":
                    return p.ReferenceHub.RamImportPath(out response);
            }

            return false;
        }

        public string Command => "motionpath";
        public string[] Aliases { get; } = {"interpolation", "motionpathinterpolation", "mpi"};
        public string Description => "Manages your own motion path.";

        public string[] Usage { get; } = {
            "motionPath",
            "create [interval]/delete/generate/points/rotations/goToStart/start/pause/stop/restart/interval [newInterval]/delay [newDelay]/easing [newEasing]/",
            "add [x] [y] [z]/addRot [x] [y]/removeFirst/removeLast/removeAt <index>/clear/removeFirstRot/removeLastRot/removeRotAt <index>/clearRot/",
            "generated/generatedRot/export/import/"
        };

    }

}
