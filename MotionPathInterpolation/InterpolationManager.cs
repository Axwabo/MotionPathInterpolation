using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Object = UnityEngine.Object;

namespace MotionPathInterpolation {

    public static class InterpolationManager {

        internal static readonly HashSet<MotionPath> Paths = new HashSet<MotionPath>();
        private static readonly Dictionary<string, byte[]> SavedPaths = new Dictionary<string, byte[]>();

        public static int SavedPathCount => SavedPaths.Count;

        public static MotionPath GetPath(this ReferenceHub p) {
            return p.gameObject.TryGetComponent<MotionPath>(out var comp)
                ? Paths.FirstOrDefault(e => e == comp)
                : null;
        }

        public static MotionPath GetPath(this Player p) {
            return GetPath(p.ReferenceHub);
        }

        public static bool RemovePath(this Player p) {
            return RemovePath(p.ReferenceHub);
        }

        public static bool RemovePath(this ReferenceHub hub) {
            if (!hub.gameObject.TryGetComponent(out MotionPath path))
                return false;
            Object.Destroy(path);
            return true;
        }

        public static MotionPath CreatePath(this Player player, int interval) {
            return CreatePath(player.ReferenceHub, interval);
        }

        public static MotionPath CreatePath(this ReferenceHub hub, int interval) {
            var path = hub.gameObject.AddComponent<MotionPath>();
            path.Interval = interval;
            return path;
        }

        public static bool RamSavePath(this ReferenceHub hub) {
            if (!hub.gameObject.TryGetComponent(out MotionPath p))
                return false;
            var nick = hub.nicknameSync.MyNick;
            if (!SavedPaths.ContainsKey(nick))
                SavedPaths.Add(nick, null);
            SavedPaths[nick] = p.Export();
            return true;
        }

        public static bool RamImportPath(this ReferenceHub hub, out string result) {
            var nick = hub.nicknameSync.MyNick;
            if (SavedPaths.ContainsKey(nick))
                return MotionPath.Import(hub, SavedPaths[nick], out result);
            result = "No MotionPath has been saved for the target";
            return false;
        }

        public static void ClearSavedPaths() {
            SavedPaths.Clear();
        }

    }

}
