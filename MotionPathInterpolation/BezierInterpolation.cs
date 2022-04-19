using System;
using System.Linq;
using UnityEngine;

namespace MotionPathInterpolation {

    public class BezierInterpolation {

        public float[] ControlVertices { get; }

        public float[] Interpolated { get; private set; }

        public BezierInterpolation(float[] vertices) {
            ControlVertices = vertices;
        }

        public static float BezierPoint(float[] points, float t) {
            while (true) {
                if (points.Length < 2)
                    throw new IndexOutOfRangeException("Must provide at least 2 points!");
                if (points.Length == 2)
                    return Interpolate(points[0], points[1], t);
                var newPoints = new float[points.Length - 1];
                for (var i = 0; i < points.Length - 1; i++)
                    newPoints[i] = Interpolate(points[i], points[i + 1], t);
                points = newPoints;
            }
        }

        public static float Interpolate(float a, float b, float t) {
            return (1f - t) * a + t * b;
        }

        public float[] Eval(int interval) {
            var max = ControlVertices.Length * interval;
            Interpolated = new float[max];
            for (var i = 0; i < max; i++)
                Interpolated[i] = BezierPoint(ControlVertices, i / (float) max);

            return Interpolated;
        }

        public static float[] Evaluate(float[] points, int interval) {
            return new BezierInterpolation(points).Eval(interval);
        }

        public static Vector2[] Evaluate2D(Vector2[] points, int interval) {
            var x = Evaluate(points.Select(e => e.x).ToArray(), interval);
            var y = Evaluate(points.Select(e => e.y).ToArray(), interval);
            var arr = new Vector2[x.Length];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = new Vector2(x[i], y[i]);
            return arr;
        }

        public static Vector3[] Evaluate3D(Vector3[] points, int interval) {
            var x = Evaluate(points.Select(e => e.x).ToArray(), interval);
            var y = Evaluate(points.Select(e => e.y).ToArray(), interval);
            var z = Evaluate(points.Select(e => e.z).ToArray(), interval);
            var arr = new Vector3[x.Length];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = new Vector3(x[i], y[i], z[i]);
            return arr;
        }

    }

}
