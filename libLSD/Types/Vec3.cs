using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    public struct Vec3
    {
        public float X;
        public float Y;
        public float Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            Vec3 returnVal = new Vec3
            {
                X = (a.X + b.X),
                Y = (a.Y + b.Y),
                Z = (a.Z + b.Z)
            };
            return returnVal;
        }

        public static Vec3 operator -(Vec3 a)
        {
            Vec3 returnVal = new Vec3
            {
                X = -a.X,
                Y = -a.Y,
                Z = -a.Z
            };
            return returnVal;
        }

        public static Vec3 operator -(Vec3 a, Vec3 b)
        {
            return a + -b;
        }

        public static Vec3 operator *(Vec3 a, float s)
        {
            Vec3 returnVal = new Vec3
            {
                X = (a.X * s),
                Y = (a.Y * s),
                Z = (a.Z * s)
            };
            return returnVal;
        }

        public static Vec3 operator /(Vec3 a, float s)
        {
            Vec3 returnVal = new Vec3
            {
                X = (a.X / s),
                Y = (a.Y / s),
                Z = (a.Z / s)
            };
            return returnVal;
        }

        public static float Dot(Vec3 a, Vec3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            Vec3 returnVal = new Vec3
            {
                X = a.Y * b.Z - a.Z * b.Y,
                Y = a.Z * b.X - a.X * b.Z,
                Z = a.X * b.Y - a.Y * b.X
            };
            return returnVal;
        }

        public static float Magnitude(Vec3 a)
        {
            return (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        }

        public static Vec3 Normalize(Vec3 a)
        {
            return a / Magnitude(a);
        }

        public override string ToString()
        {
            return string.Format("Vec3: ({0}, {1}, {2})", X, Y, Z);
        }
    }
}
