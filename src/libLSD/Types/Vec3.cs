using System;
using System.IO;
using libLSD.Interfaces;

namespace libLSD.Types
{
    /// <summary>
    /// A 3-dimensional vector.
    /// </summary>
    public struct Vec3 : IWriteable
    {
        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public float Y;

        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        public float Z;

        /// <summary>
        /// Create a new 3D vector.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Create a new 3D vector from a binary stream.
        /// Reads the numbers in as shorts, and is aligned to a 4-byte boundary, so reads an extra 2 bytes as padding.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public Vec3(BinaryReader br)
        {
            X = br.ReadInt16();
            Y = br.ReadInt16();
            Z = br.ReadInt16();
            br.ReadInt16();
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

        public static Vec3 operator -(Vec3 a, Vec3 b) { return a + -b; }

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

        /// <summary>
        /// Compute the dot product of two vectors.
        /// </summary>
        /// <param name="a">Vector A.</param>
        /// <param name="b">Vector B.</param>
        /// <returns>A dot B.</returns>
        public static float Dot(Vec3 a, Vec3 b) { return a.X * b.X + a.Y * b.Y + a.Z * b.Z; }

        /// <summary>
        /// Compute the cross product of two vectors.
        /// </summary>
        /// <param name="a">Vector A.</param>
        /// <param name="b">Vector B.</param>
        /// <returns>A cross B.</returns>
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

        /// <summary>
        /// Get the magnitude of a vector. This calls Math.Sqrt!
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <returns>The magnitude of the vector.</returns>
        public static float Magnitude(Vec3 a) { return (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z); }

        /// <summary>
        /// Normalize a vector.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <returns>The normalized vector.</returns>
        public static Vec3 Normalize(Vec3 a) { return a / Magnitude(a); }

        public override string ToString() { return $"Vec3: ({X}, {Y}, {Z})"; }

        /// <summary>
        /// Write this vector to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write((short)X);
            bw.Write((short)Y);
            bw.Write((short)Z);
            bw.Write((short)0);
        }

        public bool Equals(Vec3 other) { return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z); }

        public override bool Equals(object obj) { return obj is Vec3 other && Equals(other); }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
    }
}
