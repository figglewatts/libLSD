using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Interfaces;
using libLSD.Types;

namespace libLSD.Formats.Packets
{
#region Packet interfaces

    /// <summary>
    /// Interface for all TMD packets. They all have vertices.
    /// </summary>
    public interface ITMDPrimitivePacket
    {
        int[] Vertices { get; }
    }

    /// <summary>
    /// A lit TMD packet has normals, for lighting information.
    /// </summary>
    public interface ITMDLitPrimitivePacket
    {
        int[] Normals { get; }
    }

    /// <summary>
    /// A colored TMD packet has colors for each vertex.
    /// </summary>
    public interface ITMDColoredPrimitivePacket
    {
        Vec3[] Colors { get; }
    }

    /// <summary>
    /// A textured TMD packet has texture and color lookup table information, as well as UVs for each vertex.
    /// </summary>
    public interface ITMDTexturedPrimitivePacket
    {
        TMDTexture Texture { get; }
        TMDColorLookup ColorLookup { get; }
        int[] UVs { get; }
    }

#endregion

#region 3 Vertex Poly with No Light Source Calculation

    // Flat, no texture
    // mode=0x21, flag=0x1, ilen=0x3, olen=0x4
    public class TMDTriFlatUnlitPrimitivePacket : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        public byte r, g, b;
        public ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};

        public Vec3[] Colors => new[] {new Vec3(r / 255f, g / 255f, b / 255f)};

        public TMDTriFlatUnlitPrimitivePacket(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte(); // skip
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            br.ReadUInt16(); // skip
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
            bw.Write((byte)0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write((ushort)0);
        }
    }

    // flat, texture
    // mode=0x25, flag=0x1, ilen=0x6, olen=0x7
    public class TMDTriFlatTexUnlitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly byte r, g, b;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public TMDTexture Texture => tsb;
        public TMDColorLookup ColorLookup => cba;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2};
        public Vec3[] Colors => new[] {new Vec3(r / 255f, g / 255f, b / 255f)};

        public TMDTriFlatTexUnlitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
            bw.Write((byte)0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write((ushort)0);
        }
    }

    // gradation, no texture
    // mode=0x31, flag=0x1, ilen=0x5, olen=0x6
    public class TMDTriGradUnlitPrimitivePacket : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
            };

        public TMDTriGradUnlitPrimitivePacket(BinaryReader br)
        {
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0});
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write((ushort)0);
        }
    }

    // gradation, texture
    // mode=0x35, flag=0x1, ilen=0x8, mode=0x9
    public class TMDTriGradTexUnlitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public TMDColorLookup ColorLookup => cba;
        public TMDTexture Texture => tsb;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
            };

        public TMDTriGradTexUnlitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0});
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write((ushort)0);
        }
    }

#endregion

#region 3 Vertex Poly with Light Source Calculation

    // flat, no texture (solid)
    // mode=0x20, flag=0x0, ilen=0x3, olen=0x4
    public class TMDTriFlatLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte r, g, b;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r / 255f, g / 255f, b / 255f)
            };

        public TMDTriFlatLitPrimitivePacket(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
            bw.Write((byte)0);
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
        }
    }

    // flat, no texture (grad)
    // mode=0x20, flag=0x4, ilen=0x5, olen=0x6
    public class TMDTriFlatGradLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
            };

        public TMDTriFlatGradLitPrimitivePacket(BinaryReader br)
        {
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0});
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
        }
    }

    // flat, texture
    // mode=0x24, flag=0x0, ilen=0x5, olen=0x7
    public class TMDTriFlatTexLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0};
        public TMDColorLookup ColorLookup => cba;
        public TMDTexture Texture => tsb;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2};

        public TMDTriFlatTexLitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
        }
    }

    // gouraud, no texture (solid)
    // mode=0x30, flag=0x0, ilen=0x4, olen=0x6
    public class TMDTriShaderLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDLitPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte r, g, b;
        private readonly ushort n0, n1, n2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0, n1, n2};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r / 255f, g / 255f, b / 255f)
            };

        public TMDTriShaderLitPrimitivePacket(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            n1 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            n2 = br.ReadUInt16();
            p2 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r, g, b, 0});
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(n1);
            bw.Write(p1);
            bw.Write(n2);
            bw.Write(p2);
        }
    }

    // gouraud, no texture (grad)
    // mode=0x30, flag=0x4, ilen=0x6, olen=0x6
    public class TMDTriShadedGradLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly ushort n0, n1, n2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0, n1, n2};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
            };

        public TMDTriShadedGradLitPrimitivePacket(BinaryReader br)
        {
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            n1 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            n2 = br.ReadUInt16();
            p2 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0});
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(n1);
            bw.Write(p1);
            bw.Write(n2);
            bw.Write(p2);
        }
    }

    // gouraud, texture
    // mode=0x34, flag=0x0, ilen=0x6, olen=0x9
    public class TMDTriShadedTexLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly ushort n0, n1, n2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0, n1, n2};
        public TMDColorLookup ColorLookup => cba;
        public TMDTexture Texture => tsb;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2};

        public TMDTriShadedTexLitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            n1 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            n2 = br.ReadUInt16();
            p2 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(n1);
            bw.Write(p1);
            bw.Write(n2);
            bw.Write(p2);
        }
    }

#endregion

#region 4 Vertex Poly with No Light Source Calculation

    // flat, no texture
    // mode=0x29, flag=0x1, ilen=0x3, olen=0x5
    public class TMDQuadFlatUnlitPrimitivePacket : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte r, g, b;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r / 255f, g / 255f, b / 255f)
            };

        public TMDQuadFlatUnlitPrimitivePacket(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            p3 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r, g, b, 0});
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write(p3);
        }
    }

    // flat, texture
    // mode=0x2D, flag=0x1, ilen=0x7, olen=0x9
    public class TMDQuadFlatTexUnlitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly byte u3, v3;
        private readonly byte r, g, b;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public TMDColorLookup ColorLookup => cba;
        public TMDTexture Texture => tsb;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2, u3, v3};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r / 255f, g / 255f, b / 255f)
            };

        public TMDQuadFlatTexUnlitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            u3 = br.ReadByte();
            v3 = br.ReadByte();
            br.ReadUInt16();
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            p3 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(u3);
            bw.Write(v3);
            bw.Write((ushort)0);

            bw.Write(new byte[] {r, g, b, 0});
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write(p3);
        }
    }

    // grad, no texture
    // mode=0x39, flag=0x1, ilen=0x6, olen=0x8
    public class TMDQuadGradUnlitPrimitivePacket : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly byte r3, g3, b3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
                new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
            };

        public TMDQuadGradUnlitPrimitivePacket(BinaryReader br)
        {
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            r3 = br.ReadByte();
            g3 = br.ReadByte();
            b3 = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            p3 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0, r3, g3, b3, 0});
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write(p3);
        }
    }

    // grad, texture
    // mode=0x3D, flag=0x1, ilen=0xA, olen=0xC
    public class TMDQuadGradTexUnlitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly byte u3, v3;
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly byte r3, g3, b3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public TMDColorLookup ColorLookup => cba;
        public TMDTexture Texture => tsb;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2, u3, v3};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
                new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
            };

        public TMDQuadGradTexUnlitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            u3 = br.ReadByte();
            v3 = br.ReadByte();
            br.ReadUInt16();
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            r3 = br.ReadByte();
            g3 = br.ReadByte();
            b3 = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            p3 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(u3);
            bw.Write(v3);
            bw.Write((ushort)0);

            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0, r3, g3, b3, 0});
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write(p3);
        }
    }

#endregion

#region 4 Vertex Poly with Light Source Calculation

    // flat, no texture (solid)
    // mode=0x28, flag=0x0, ilen=0x4, olen=0x5
    public class TMDQuadFlatLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte r, g, b;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r / 255f, g / 255f, b / 255f)
            };

        public TMDQuadFlatLitPrimitivePacket(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            p3 = br.ReadUInt16();
            br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
            bw.Write((byte)0);
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write(p3);
            bw.Write((ushort)0);
        }
    }

    // flat, no texture (grad)
    // mode=0x28, flag=0x4, ilen=0x7, olen=0x8
    public class TMDQuadFlatGradLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly byte r3, g3, b3;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
                new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
            };

        public TMDQuadFlatGradLitPrimitivePacket(BinaryReader br)
        {
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            r3 = br.ReadByte();
            g3 = br.ReadByte();
            b3 = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            p3 = br.ReadUInt16();
            br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0, r3, g3, b3, 0});
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write(p3);
            bw.Write((ushort)0);
        }
    }

    // flat, texture
    // mode=0x2C, flag=0x0, ilen=0x7, olen=0x9
    public class TMDQuadFlatTexLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly byte u3, v3;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0};
        public TMDColorLookup ColorLookup => cba;
        public TMDTexture Texture => tsb;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2, u3, v3};

        public TMDQuadFlatTexLitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            u3 = br.ReadByte();
            v3 = br.ReadByte();
            br.ReadUInt16();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            p3 = br.ReadUInt16();
            br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(u3);
            bw.Write(v3);
            bw.Write((ushort)0);
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(p1);
            bw.Write(p2);
            bw.Write(p3);
            bw.Write((ushort)0);
        }
    }

    // gouraud, no texture (solid)
    // mode=0x38, flag=0x0, ilen=0x5, olen=0x8
    public class TMDQuadShadedLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte r, g, b;
        private readonly ushort n0, n1, n2, n3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0, n1, n2, n3};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r / 255f, g / 255f, b / 255f)
            };

        public TMDQuadShadedLitPrimitivePacket(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            n1 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            n2 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            n3 = br.ReadUInt16();
            p3 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r, g, b, 0});
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(n1);
            bw.Write(p1);
            bw.Write(n2);
            bw.Write(p2);
            bw.Write(n3);
            bw.Write(p3);
        }
    }

    // gouraud, no texture (grad)
    // mode=0x38, flag=0x4, ilen=0x8, olen=0x8
    public class TMDQuadShadedGradLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly byte r3, g3, b3;
        private readonly ushort n0, n1, n2, n3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0, n1, n2, n3};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
                new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
                new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
            };

        public TMDQuadShadedGradLitPrimitivePacket(BinaryReader br)
        {
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            r2 = br.ReadByte();
            g2 = br.ReadByte();
            b2 = br.ReadByte();
            br.ReadByte();
            r3 = br.ReadByte();
            g3 = br.ReadByte();
            b3 = br.ReadByte();
            br.ReadByte();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            n1 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            n2 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            n3 = br.ReadUInt16();
            p3 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0, r3, g3, b3, 0});
            bw.Write(n0);
            bw.Write(p0);
            bw.Write(n1);
            bw.Write(p1);
            bw.Write(n2);
            bw.Write(p2);
            bw.Write(n3);
            bw.Write(p3);
        }
    }

    // gouraud, texture
    // mode=0x3C, flag=0x0, ilen=0x8, olen=0xC
    public class TMDQuadShadedTexLitPrimitivePacket
        : ITMDPrimitivePacket, ITMDTexturedPrimitivePacket, ITMDLitPrimitivePacket, IWriteable
    {
        private readonly byte u0, v0;
        private readonly TMDColorLookup cba;
        private readonly byte u1, v1;
        private readonly TMDTexture tsb;
        private readonly byte u2, v2;
        private readonly byte u3, v3;
        private readonly ushort n0, n1, n2, n3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0, n1, n2, n3};
        public TMDColorLookup ColorLookup => cba;
        public TMDTexture Texture => tsb;
        public int[] UVs => new int[] {u0, v0, u1, v1, u2, v2, u3, v3};

        public TMDQuadShadedTexLitPrimitivePacket(BinaryReader br)
        {
            u0 = br.ReadByte();
            v0 = br.ReadByte();
            cba = br.ReadUInt16();
            u1 = br.ReadByte();
            v1 = br.ReadByte();
            tsb = br.ReadUInt16();
            u2 = br.ReadByte();
            v2 = br.ReadByte();
            br.ReadUInt16();
            u3 = br.ReadByte();
            v3 = br.ReadByte();
            br.ReadUInt16();
            n0 = br.ReadUInt16();
            p0 = br.ReadUInt16();
            n1 = br.ReadUInt16();
            p1 = br.ReadUInt16();
            n2 = br.ReadUInt16();
            p2 = br.ReadUInt16();
            n3 = br.ReadUInt16();
            p3 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(u0);
            bw.Write(v0);
            cba.Write(bw);
            bw.Write(u1);
            bw.Write(v1);
            tsb.Write(bw);
            bw.Write(u2);
            bw.Write(v2);
            bw.Write((ushort)0);
            bw.Write(u3);
            bw.Write(v3);
            bw.Write((ushort)0);

            bw.Write(n0);
            bw.Write(p0);
            bw.Write(n1);
            bw.Write(p1);
            bw.Write(n2);
            bw.Write(p2);
            bw.Write(n3);
            bw.Write(p3);
        }
    }

#endregion

#region Straight Line

    // mode=0x40, flag=0x1, ilen=0x2, olen=0x3
    public class TMDLineFlatPrimitivePacket : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte r, g, b;
        private readonly ushort p0, p1;

        public int[] Vertices => new int[] {p0, p1};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r / 255f, g / 255f, b / 255f)
            };

        public TMDLineFlatPrimitivePacket(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r, g, b, 0});
            bw.Write(p0);
            bw.Write(p1);
        }
    }

    // mode=0x50, flag=0x1, ilen=0x3, olen=0x4
    public class TMDLineGradPrimitivePacket : ITMDPrimitivePacket, ITMDColoredPrimitivePacket, IWriteable
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly ushort p0, p1;

        public int[] Vertices => new int[] {p0, p1};

        public Vec3[] Colors =>
            new[]
            {
                new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
                new Vec3(r1 / 255f, g1 / 255f, b1 / 255f)
            };

        public TMDLineGradPrimitivePacket(BinaryReader br)
        {
            r0 = br.ReadByte();
            g0 = br.ReadByte();
            b0 = br.ReadByte();
            br.ReadByte();
            r1 = br.ReadByte();
            g1 = br.ReadByte();
            b1 = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(new byte[] {r0, g0, b0, 0, r1, g1, b1, 0});
            bw.Write(p0);
            bw.Write(p1);
        }
    }

#endregion
}
