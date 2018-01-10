using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Types;

namespace libLSD.Formats
{
    #region Packet interfaces

    public interface IPacket
    {
        int[] Vertices { get; }
    }

    public interface ILitPacket
    {
        int[] Normals { get; }
    }

    public interface IColouredPacket
    {
        Vec3[] Colours { get; }
    }

    public interface ITexturedPacket
    {
        TMDTexture Texture { get; }
        TMDColorLookup ColorLookup { get; }
        int[] UVs { get; }
    }

    #endregion
    
    public abstract class TMDPrimitivePacketData { }

    #region 3 Vertex Poly with No Light Source Calculation
    // Flat, no texture
    // mode=0x21, flag=0x1, ilen=0x3, olen=0x4
    public class TriFlatUnlit : TMDPrimitivePacketData, IPacket, IColouredPacket
    {
        public byte r, g, b;
        public ushort p0, p1, p2;

        public int[] Vertices => new int[] { p0, p1, p2 };

        public Vec3[] Colours => new [] { new Vec3(r / 255f, g / 255f, b / 255f) };

        public TriFlatUnlit(BinaryReader br)
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
    }

    // flat, texture
    // mode=0x25, flag=0x1, ilen=0x6, olen=0x7
    public class TriFlatTexUnlit : TMDPrimitivePacketData, IPacket, ITexturedPacket, IColouredPacket
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
        public Vec3[] Colours => new[] {new Vec3(r / 255f, g / 255f, b / 255f)};

        public TriFlatTexUnlit(BinaryReader br)
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
    }

    // gradation, no texture
    // mode=0x31, flag=0x1, ilen=0x5, olen=0x6
    public class TriGradUnlit : TMDPrimitivePacketData, IPacket, IColouredPacket
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};

        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
        };

        public TriGradUnlit(BinaryReader br)
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
    }

    // gradation, texture
    // mode=0x35, flag=0x1, ilen=0x8, mode=0x9
    public class TriGradTexUnlit : TMDPrimitivePacketData, IPacket, ITexturedPacket, IColouredPacket
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
        public int[] UVs => new int[] { u0, v0, u1, v1, u2, v2};
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
        };

        public TriGradTexUnlit(BinaryReader br)
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
    }
    #endregion

    #region 3 Vertex Poly with Light Source Calculation

    // flat, no texture (solid)
    // mode=0x20, flag=0x0, ilen=0x3, olen=0x4
    public class TriFlatLit : TMDPrimitivePacketData, IPacket, IColouredPacket, ILitPacket
    {
        private readonly byte r, g, b;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0};
        public Vec3[] Colours => new[]
        {
            new Vec3(r / 255f, g / 255f, b / 255f)
        };

        public TriFlatLit(BinaryReader br)
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
    }

    // flat, no texture (grad)
    // mode=0x20, flag=0x4, ilen=0x5, olen=0x6
    public class TriFlatGradLit : TMDPrimitivePacketData, IPacket, IColouredPacket, ILitPacket
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0};
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
        };

        public TriFlatGradLit(BinaryReader br)
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
    }

    // flat, texture
    // mode=0x24, flag=0x0, ilen=0x5, olen=0x7
    public class TriFlatTexLit : TMDPrimitivePacketData, IPacket, ITexturedPacket, ILitPacket
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

        public TriFlatTexLit(BinaryReader br)
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
    }

    // gouraud, no texture (solid)
    // mode=0x30, flag=0x0, ilen=0x4, olen=0x6
    public class TriShadedLit : TMDPrimitivePacketData, IPacket, ILitPacket, IColouredPacket
    {
        private readonly byte r, g, b;
        private readonly ushort n0, n1, n2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0, n1, n2};
        public Vec3[] Colours => new[]
        {
            new Vec3(r / 255f, g / 255f, b / 255f)
        };

        public TriShadedLit(BinaryReader br)
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
    }

    // gouraud, no texture (grad)
    // mode=0x30, flag=0x4, ilen=0x6, olen=0x6
    public class TriShadedGradLit : TMDPrimitivePacketData, IPacket, IColouredPacket, ILitPacket
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly ushort n0, n1, n2;
        private readonly ushort p0, p1, p2;

        public int[] Vertices => new int[] {p0, p1, p2};
        public int[] Normals => new int[] {n0, n1, n2};
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f)
        };

        public TriShadedGradLit(BinaryReader br)
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
    }

    // gouraud, texture
    // mode=0x34, flag=0x0, ilen=0x6, olen=0x9
    public class TriShadedTexLit : TMDPrimitivePacketData, IPacket, ITexturedPacket, ILitPacket
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

        public TriShadedTexLit(BinaryReader br)
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
    }

    #endregion

    #region 4 Vertex Poly with No Light Source Calculation

    // flat, no texture
    // mode=0x29, flag=0x1, ilen=0x3, olen=0x5
    public class QuadFlatUnlit : TMDPrimitivePacketData, IPacket, IColouredPacket
    {
        private readonly byte r, g, b;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public Vec3[] Colours => new[]
        {
            new Vec3(r / 255f, g / 255f, b / 255f)
        };

        public QuadFlatUnlit(BinaryReader br)
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
    }

    // flat, texture
    // mode=0x2D, flag=0x1, ilen=0x7, olen=0x9
    public class QuadFlatTexUnlit : TMDPrimitivePacketData, IPacket, ITexturedPacket, IColouredPacket
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
        public Vec3[] Colours => new[]
        {
            new Vec3(r / 255f, g / 255f, b / 255f)
        };

        public QuadFlatTexUnlit(BinaryReader br)
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
    }

    // grad, no texture
    // mode=0x39, flag=0x1, ilen=0x6, olen=0x8
    public class QuadGradUnlit : TMDPrimitivePacketData, IPacket, IColouredPacket
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly byte r3, g3, b3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
            new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
        };

        public QuadGradUnlit(BinaryReader br)
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
    }

    // grad, texture
    // mode=0x3D, flag=0x1, ilen=0xA, olen=0xC
    public class QuadGradTexUnlit : TMDPrimitivePacketData, IPacket, ITexturedPacket, IColouredPacket
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
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
            new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
        };

        public QuadGradTexUnlit(BinaryReader br)
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
    }

    #endregion

    #region 4 Vertex Poly with Light Source Calculation

    // flat, no texture (solid)
    // mode=0x28, flag=0x0, ilen=0x4, olen=0x5
    public class QuadFlatLit : TMDPrimitivePacketData, IPacket, IColouredPacket, ILitPacket
    {
        private readonly byte r, g, b;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0};

        public Vec3[] Colours => new[]
        {
            new Vec3(r / 255f, g / 255f, b / 255f)
        };

        public QuadFlatLit(BinaryReader br)
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
    }

    // flat, no texture (grad)
    // mode=0x28, flag=0x4, ilen=0x7, olen=0x8
    public class QuadFlatGradLit : TMDPrimitivePacketData, IPacket, IColouredPacket, ILitPacket
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly byte r3, g3, b3;
        private readonly ushort n0;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0};
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
            new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
        };

        public QuadFlatGradLit(BinaryReader br)
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
    }

    // flat, texture
    // mode=0x2C, flag=0x0, ilen=0x7, olen=0x9
    public class QuadFlatTexLit : TMDPrimitivePacketData, IPacket, ITexturedPacket, ILitPacket
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

        public QuadFlatTexLit(BinaryReader br)
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
    }

    // gouraud, no texture (solid)
    // mode=0x38, flag=0x0, ilen=0x5, olen=0x8
    public class QuadShadedLit : TMDPrimitivePacketData, IPacket, IColouredPacket, ILitPacket
    {
        private readonly byte r, g, b;
        private readonly ushort n0, n1, n2, n3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0, n1, n2, n3};
        public Vec3[] Colours => new[]
        {
            new Vec3(r / 255f, g / 255f, b / 255f)
        };

        public QuadShadedLit(BinaryReader br)
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
    }

    // gouraud, no texture (grad)
    // mode=0x38, flag=0x4, ilen=0x8, olen=0x8
    public class QuadShadedGradLit : TMDPrimitivePacketData, IPacket, IColouredPacket, ILitPacket
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly byte r2, g2, b2;
        private readonly byte r3, g3, b3;
        private readonly ushort n0, n1, n2, n3;
        private readonly ushort p0, p1, p2, p3;

        public int[] Vertices => new int[] {p0, p1, p2, p3};
        public int[] Normals => new int[] {n0, n1, n2, n3};
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f),
            new Vec3(r2 / 255f, g2 / 255f, b2 / 255f),
            new Vec3(r3 / 255f, g3 / 255f, b3 / 255f)
        };

        public QuadShadedGradLit(BinaryReader br)
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
    }

    // gouraud, texture
    // mode=0x3C, flag=0x0, ilen=0x8, olen=0xC
    public class QuadShadedTexLit : TMDPrimitivePacketData, IPacket, ITexturedPacket, ILitPacket
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

        public QuadShadedTexLit(BinaryReader br)
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
    }

    #endregion

    #region Straight Line

    // mode=0x40, flag=0x1, ilen=0x2, olen=0x3
    public class LineFlat : TMDPrimitivePacketData, IPacket, IColouredPacket
    {
        private readonly byte r, g, b;
        private readonly ushort p0, p1;

        public int[] Vertices => new int[] {p0, p1};
        public Vec3[] Colours => new[]
        {
            new Vec3(r / 255f, g / 255f, b / 255f)
        };

        public LineFlat(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            br.ReadByte();
            p0 = br.ReadUInt16();
            p1 = br.ReadUInt16();
        }
    }

    // mode=0x50, flag=0x1, ilen=0x3, olen=0x4
    public class LineGrad : TMDPrimitivePacketData, IPacket, IColouredPacket
    {
        private readonly byte r0, g0, b0;
        private readonly byte r1, g1, b1;
        private readonly ushort p0, p1;

        public int[] Vertices => new int[] {p0, p1};
        public Vec3[] Colours => new[]
        {
            new Vec3(r0 / 255f, g0 / 255f, b0 / 255f),
            new Vec3(r1 / 255f, g1 / 255f, b1 / 255f)
        };

        public LineGrad(BinaryReader br)
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
    }

    #endregion
}
