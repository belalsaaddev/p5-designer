using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PowerfistTools
{
    public static class PMath
    {
        //XZ
        public static float GetAngleXZ(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            return angle;
        }
        public static float GetAngleXZ(Vector3 current, Vector3 target)
        {
            Vector3 diff = (target - current).normalized;
            float angle = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;
            return angle;
        }
        //XY
        public static float GetAngleXY(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return angle;
        }
        public static float GetAngleXY(Vector3 current, Vector3 target)
        {
            Vector3 diff = (target - current).normalized;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            return angle;
        }
        //YZ
        public static float GetAngleYZ(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
            return angle;
        }
        public static float GetAngleYZ(Vector3 current, Vector3 target)
        {
            Vector3 diff = (target - current).normalized;
            float angle = Mathf.Atan2(diff.y, diff.z) * Mathf.Rad2Deg;
            return angle;
        }

        //Mapping
        public static float Map(float x, float min, float max, float newMin, float newMax)
        {
            return (x - min) / (max - min) * (newMax - newMin) + newMin;
        }
        public static int MapToInt(float x, float min, float max, int newMin, int newMax)
        {
            return Mathf.RoundToInt((x - min) / (max - min) * (newMax - newMin) + newMin);
        }

        //Vector Functions
        public static Vector3 RotateVectorAroundAxis(Vector3 vector, Vector3 axis, float angle)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            return rotation * vector;
        }
        public static Vector3 RotateVectorAroundPoint(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
        {
            Vector3 dir = point - pivot;
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Vector3 rotatedDir = rotation * dir;
            return pivot + rotatedDir;
        }
        public static Vector2 RotateVectorAroundPoint(Vector2 point, Vector2 pivot, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            Vector2 dir = point - pivot;
            Vector2 rotatedDir = new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
            return pivot + rotatedDir;
        }

        //Other Mathematical Functions
        public static int Factorial(int n)
        {
            if (n <= 1) return 1;
            int num = n;
            for (int i = 1; i < n; i++) num *= i;
            return num;
        }
        public static float nCr(int n, int r)
        {
            if(n <= 1)
            {
                Debug.LogError("MATH ERROR: Cannot nCr when n is negative");
                return 0;
            }
            return (float)Factorial(n) / (Factorial(r) * Factorial(n - r));
        }
        public static float Mean(float[] nums)
        {
            float total = 0;
            for (int i = 0; i < nums.Length; i++) total += nums[i];
            return total / nums.Length;
        }
    }
    public static class QuickAnimations
    {
        public static async Task FadeIn(SpriteRenderer renderer, float alpha, float time)
        {
            float startTime = Time.time;
            float endTime = startTime + time;
            float elapsed = 0;
            float refVel = 0;
            while (elapsed <= 1)
            {
                elapsed = PMath.Map(Time.time, startTime, endTime, 0f, 1f);
                Color colour = renderer.color;
                colour.a = Mathf.SmoothDamp(colour.a, elapsed * alpha, ref refVel, 0.01f);
                renderer.color = colour;
                await Task.Delay((int)(Time.deltaTime * 1000));
            }
            Color finalColour = renderer.color;
            finalColour.a = alpha;
            renderer.color = finalColour;
        }
        public static async Task FadeIn(Image renderer, float alpha, float time)
        {
            float startTime = Time.time;
            float endTime = startTime + time;
            float elapsed = 0;
            float refVel = 0;
            while (elapsed <= 1)
            {
                elapsed = PMath.Map(Time.time, startTime, endTime, 0f, 1f);
                Color colour = renderer.color;
                colour.a = Mathf.SmoothDamp(colour.a, elapsed * alpha, ref refVel, 0.01f);
                renderer.color = colour;
                await Task.Delay((int)(Time.deltaTime * 1000));
            }
            Color finalColour = renderer.color;
            finalColour.a = alpha;
            renderer.color = finalColour;
        }

        public static async Task FadeOut(SpriteRenderer renderer, float time)
        {
            float alpha = renderer.color.a;
            float startTime = Time.time;
            float endTime = startTime + time;
            float elapsed = 1f;
            float refVel = 0;
            while (elapsed >= 0)
            {
                elapsed = 1f - PMath.Map(Time.time, startTime, endTime, 0f, 1f);
                Color colour = renderer.color;
                colour.a = Mathf.SmoothDamp(colour.a, elapsed * alpha, ref refVel, 0.01f);
                renderer.color = colour;
                await Task.Delay((int)(Time.deltaTime * 1000));
            }
            Color finalColour = renderer.color;
            finalColour.a = 0f;
            renderer.color = finalColour;
        }
        public static async Task FadeOut(Image renderer, float time)
        {
            float alpha = renderer.color.a;
            float startTime = Time.time;
            float endTime = startTime + time;
            float elapsed = 1;
            float refVel = 0;
            while (elapsed >= 0)
            {
                elapsed = 1f - PMath.Map(Time.time, startTime, endTime, 0f, 1f);
                Color colour = renderer.color;
                colour.a = Mathf.SmoothDamp(colour.a, elapsed * alpha, ref refVel, 0.01f); ;
                renderer.color = colour;
                await Task.Delay((int)(Time.deltaTime * 1000));
            }
            Color finalColour = renderer.color;
            finalColour.a = 0f;
            renderer.color = finalColour;
        }
    }
    public static class ExtentionMethods
    {
        public static int FindIndexOfType<T>(this List<T> list, T element) where T : class
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == element) index = i;
            }
            return index;
        }
        public static int FindIndexOfType<T>(this T[] list, T element) where T : class
        {
            int index = -1;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == element) index = i;
            }
            return index;
        }

        public static Vector2 SnapToGrid(this Vector2 position, float gridSize = 1f)
        {
            return SnapToGrid(position, new Vector2(gridSize, gridSize));
        }
        public static Vector2 SnapToGrid(this Vector2 position, Vector2 gridSize)
        {
            float x = Mathf.Round(position.x / gridSize.x) * gridSize.x;
            float y = Mathf.Round(position.y / gridSize.y) * gridSize.y;
            return new Vector2(x, y);
        }
        public static Vector2 SnapToIsometricGrid(this Vector2 position, float gridSize = 1f, float ySize = 0.5f)
        {
            float x = Mathf.Round(position.x / gridSize) * gridSize;
            float y = Mathf.Round(position.y / (gridSize * ySize)) * (gridSize * ySize);
            return new Vector2(x, y);
        }
    }
}
