using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MotionPathInterpolation {

    [DisallowMultipleComponent]
    public class MotionPath : MonoBehaviour {

        public static int MaxPoints => InterpolationPlugin.Singleton.Config.MaxPointsPerUser;

        public PlayerMovementSync Sync { get; private set; }

        private readonly List<Vector3> _positions = new List<Vector3>(MaxPoints);
        private readonly List<Vector2> _rotations = new List<Vector2>(MaxPoints);

        private int _frame;

        private int _delay;
        private int _i;
        private EasingType _e;

        public Vector3[] GeneratedPath { get; private set; }
        public Vector2[] GeneratedRotation { get; private set; }

        public bool Running { get; private set; }
        public bool Generated { get; private set; }

        public IList<Vector3> SpecifiedPoints => _positions.AsReadOnly();
        public IList<Vector2> SpecifiedRotations => _rotations.AsReadOnly();

        public EasingType Easing {
            get => _e;
            set {
                _e = value;
                Generated = false;
            }
        }

        private void Awake() {
            Sync = ReferenceHub.GetHub(gameObject)?.playerMovementSync;
            if (Sync == null)
                throw new NullReferenceException("PlayerMovementSync not found!");
            InterpolationManager.Paths.Add(this);
            _e = InterpolationPlugin.Singleton.Config.DefaultEasing;
        }

        private void Update() {
            if (!Running)
                return;
            _frame++;
            if (_frame < 0)
                return;
            if (GeneratedPath.Length <= _frame) {
                Running = false;
                return;
            }

            var vector = GeneratedPath[_frame];
            Sync.ForcePosition(vector);
            if (GeneratedRotation.Length <= _frame)
                return;
            var rot = GeneratedRotation[_frame];
            Sync.ForceRotation(new PlayerMovementSync.PlayerRotation(rot.x, rot.y));
        }

        private void OnDestroy() {
            InterpolationManager.Paths.Remove(this);
        }

        public int Interval {
            get => _i;
            set {
                _i = Mathf.Clamp(value, 1, InterpolationPlugin.Singleton.Config.MaxInterval);
                Generated = false;
            }
        }

        public int Delay {
            get => _delay;
            set {
                _delay = value;
                if (!Running || GeneratedPath == null)
                    DelayToFrame();
            }
        }

        public bool AddPosition(Vector3 vector) {
            if (_positions.Count >= MaxPoints)
                return false;
            Generated = false;
            _positions.Add(vector);
            return true;
        }

        public bool AddPoint(float x, float y, float z) {
            return AddPosition(new Vector3(x, y, z));
        }

        public bool RemoveFirstPos() {
            return RemovePosAt(0);
        }

        public bool RemoveLastPos() {
            return RemovePosAt(_positions.Count - 1);
        }

        public bool RemovePosAt(int index) {
            if (_positions.Count < 1 || index < 0 || _positions.Count <= index)
                return false;
            _positions.RemoveAt(index);
            Generated = false;
            return true;
        }

        public void ClearPos() {
            Generated = false;
            _positions.Clear();
        }

        public bool AddRotation(Vector2 rot) {
            if (_rotations.Count >= MaxPoints)
                return false;
            Generated = false;
            _rotations.Add(rot);
            return true;
        }

        public bool AddRotation(float x, float y) {
            return AddRotation(new Vector2(x, y));
        }

        public bool RemoveFirstRot() {
            return RemoveRotAt(0);
        }

        public bool RemoveLastRot() {
            return RemoveRotAt(_rotations.Count - 1);
        }

        public bool RemoveRotAt(int index) {
            if (_rotations.Count < 1 || index < 0 || _rotations.Count <= index)
                return false;
            _rotations.RemoveAt(index);
            Generated = false;
            return true;
        }

        public void ClearRot() {
            Generated = false;
            _rotations.Clear();
        }

        private void GenerateCubic() {
            float[] px;
            float[] py;
            float[] pz;
            var rx = Array.Empty<float>();
            var ry = Array.Empty<float>();
            lock (_positions) {
                var pointCount = _positions.Count * Interval;
                CubicSpline.FitParametric3D(
                    _positions.Select(e => e.x).ToArray(),
                    _positions.Select(e => e.y).ToArray(),
                    _positions.Select(e => e.z).ToArray(),
                    pointCount, out px, out py, out pz, normalize: false
                );
            }

            if (_rotations.Count > 0)
                lock (_rotations) {
                    var pointCount = _rotations.Count * Interval;
                    CubicSpline.FitParametric(
                        _rotations.Select(e => e.x).ToArray(),
                        _rotations.Select(e => e.y).ToArray(),
                        pointCount, out rx, out ry
                    );
                }

            GeneratedPath = new Vector3[px.Length];
            for (var i = 0; i < px.Length - 1; i++)
                GeneratedPath[i] = new Vector3(px[i], py[i], pz[i]);
            if (_positions.Count > 0)
                GeneratedPath[px.Length - 1] = _positions[_positions.Count - 1];
            GeneratedRotation = new Vector2[rx.Length];
            for (var i = 0; i < rx.Length - 1; i++)
                GeneratedRotation[i] = new Vector2(rx[i], ry[i]);
            if (_rotations.Count > 0)
                GeneratedRotation[rx.Length - 1] = _rotations[_rotations.Count - 1];
        }

        private void GenerateBezier() {
            GeneratedPath = BezierInterpolation.Evaluate3D(_positions.ToArray(), Interval);
            if (_positions.Count > 0)
                GeneratedPath[GeneratedPath.Length - 1] = _positions[_positions.Count - 1];
            GeneratedRotation = BezierInterpolation.Evaluate2D(_rotations.ToArray(), Interval);
            if (_rotations.Count > 0)
                GeneratedRotation[GeneratedRotation.Length - 1] = _rotations[_rotations.Count - 1];
        }

        private void GenerateLinear() {
            GeneratedPath = new Vector3[(_positions.Count - 1) * Interval];
            lock (_positions) {
                for (var i = 0; i < _positions.Count - 1; i++)
                for (var j = 0; j < Interval; j++)
                    GeneratedPath[i * Interval + j] = Vector3.Lerp(_positions[i], _positions[i + 1], j / (float) Interval);
            }

            if (_positions.Count > 0)
                GeneratedPath[GeneratedPath.Length - 1] = _positions[_positions.Count - 1];

            GeneratedRotation = new Vector2[(_rotations.Count - 1) * Interval];
            lock (_rotations) {
                for (var i = 0; i < _rotations.Count - 1; i++)
                for (var j = 0; j < Interval; j++)
                    GeneratedRotation[i * Interval + j] = Vector2.Lerp(_rotations[i], _rotations[i + 1], j / (float) Interval);
            }

            if (_rotations.Count > 0)
                GeneratedRotation[GeneratedRotation.Length - 1] = _rotations[_rotations.Count - 1];
        }

        public bool Generate(out string message) {
            message = "Path already generated.";
            if (Generated)
                return false;
            switch (Easing) {
                case EasingType.Linear:
                    GenerateLinear();
                    break;
                case EasingType.Bezier:
                    GenerateBezier();
                    break;
                case EasingType.CubicSpline:
                    GenerateCubic();
                    break;
                default:
                    message = "Unresolved easing type!";
                    return false;
            }

            message = "Motion path generated.";
            return Generated = true;
        }

        public bool StartMotion() {
            if (GeneratedPath == null)
                return false;
            return Running = true;
        }

        public void Pause() {
            Running = false;
        }

        public void Stop() {
            Running = false;
            DelayToFrame();
        }

        public bool Restart() {
            DelayToFrame();
            GoToStart();
            return StartMotion();
        }

        private void DelayToFrame() {
            _frame = -(_delay * 60);
        }

        public bool GoToStart() {
            if (_positions.Count < 1)
                return false;
            var point = _positions[0];
            if (_rotations.Count > 0) {
                var rot = _rotations[0];
                Sync.ForceRotation(new PlayerMovementSync.PlayerRotation(rot.x, rot.y));
            }

            Sync.ForcePosition(point);
            return true;
        }

        public byte[] Export() {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write((byte) Easing);
            writer.Write(Delay);
            writer.Write(Interval);
            lock (_positions) {
                writer.Write(_positions.Count);
                foreach (var p in _positions) {
                    writer.Write(p.x);
                    writer.Write(p.y);
                    writer.Write(p.z);
                }
            }

            lock (_rotations) {
                writer.Write(_rotations.Count);
                foreach (var r in _rotations) {
                    writer.Write(r.x);
                    writer.Write(r.y);
                }
            }

            var bytes = stream.ToArray();
            writer.Dispose();
            return bytes;
        }

        public static bool Import(ReferenceHub hub, byte[] data, out string result) {
            if (hub == null || data == null) {
                result = "Hub and data are required!";
                return false;
            }

            // header:
            // first 1 byte: easing
            // next 4 bytes: delay
            // next 4 bytes: interval
            //
            // next 4 bytes: number of positions
            // positions, 12 (3 axis * 4) bytes each
            // next 4 bytes: number of rotations
            // rotations, 8 (2 axis * 4) bytes each
            if (data.Length < 17) {
                result = "Invalid data!";
                return false;
            }

            var o = hub.gameObject;
            if (o.TryGetComponent(out MotionPath p))
                DestroyImmediate(p);
            BinaryReader reader = null;
            try {
                var stream = new MemoryStream(data);
                reader = new BinaryReader(stream);
                var c = o.AddComponent<MotionPath>();
                c.Easing = (EasingType) reader.ReadByte();
                c.Delay = reader.ReadInt32();
                c.Interval = reader.ReadInt32();
                var pos = reader.ReadInt32();
                for (var i = 0; i < pos; i++)
                    c.AddPoint(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                var rot = reader.ReadInt32();
                for (var i = 0; i < rot; i++)
                    c.AddRotation(reader.ReadSingle(), reader.ReadSingle());
                c.Generate(out _);
            } catch (Exception e) {
                result = $"Malformed data object:\n{e}";
                return false;
            } finally {
                reader?.Dispose();
            }

            result = "Successfully created MotionPath component";
            return true;
        }

    }

}
