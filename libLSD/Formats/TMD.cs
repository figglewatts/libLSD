using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Types;
using libLSD.Util;

namespace libLSD.Formats
{
    public class TMD
    {
        public TMDHeader Header;
        public TMDObject[] ObjectTable;
        public AbstractTMDPrimitivePacket[] Primitives;
        public Vec3[] Vertices;
        public TMDNormal[] Normals;
    }

    public struct TMDHeader
    {
        public uint ID;
        public uint Flags;
        public uint NumObjects;
    }

    public struct TMDObject
    {
        public uint VerticesAddress;
        public uint NumVertices;
        public uint NormalsAddress;
        public uint NumNormals;
        public uint PrimitivesAddress;
        public uint NumPrimitives;
        public int Scale; // 2^Scale is the scale factor of the TMD
    }

    public abstract class AbstractTMDPrimitivePacket
    {
        public enum Types
        {
            POLYGON = 0b1,
            LINE = 0b10,
            SPRITE = 0b11
        }

        [Flags]
        public enum PrimitiveFlags
        {
            LGT = 1,    // set if light source calculation not carried out
            FCE = 2,    // set if poly is double sided
            GRD = 4     // set if poly is gradated, else single colour
        }

        [Flags]
        public enum OptionsFlags
        {
            TGE = 1,    // brightness calculation (off draws tex as-is)
            ABE = 2,    // translucency processing
            TME = 4,    // texture specification
            QUAD = 8,   // set if 4-vertex primitive
            IIP = 16    // 0 = flat shading, 1 = gouraud
        }

        public enum SpriteSizes
        {
            FREE_SIZE = 0,
            ONE = 1,
            EIGHT = 2,
            SIXTEEN = 3
        }

        public Types Type
        {
            get => (Types)((_mode & TYPE_MASK) >> 5);
            protected set => this._mode = BitTwiddling.Merge<byte>(_mode, value, TYPE_MASK, 5);
        }

        public OptionsFlags Options
        {
            get => (OptionsFlags)(_mode & OPTIONS_MASK);
            protected set => this._mode = BitTwiddling.Merge<byte>(_mode, value, OPTIONS_MASK);
        }

        public PrimitiveFlags Flags
        {
            get => (PrimitiveFlags)(_flag & FLAGS_MASK);
            protected set => this._flag = BitTwiddling.Merge<byte>(_flag, value, FLAGS_MASK);
        }

        /// <summary>
        /// Should only be used with Sprite primitive packets, otherwise will overwrite options values.
        /// </summary>
        public SpriteSizes SpriteSize
        {
            get => (SpriteSizes)((_mode & SPRITE_SIZE_MASK) >> 3);
            protected set => this._mode = BitTwiddling.Merge<byte>(_mode, value, SPRITE_SIZE_MASK, 3);
        }

        public byte ILen; // length (in words) of packet data
        public byte OLen; // length (words) of 2D drawing primitives

        protected byte _mode;
        protected byte _flag;

        protected const uint TYPE_MASK = 0b11100000;
        protected const uint OPTIONS_MASK = 0b11111;
        protected const uint FLAGS_MASK = 0b111;
        protected const uint SPRITE_SIZE_MASK = 0b11000;
    }

    public class TMDPrimitivePacket<T> : AbstractTMDPrimitivePacket where T : TMDPrimitivePacketData
    {
        public T PacketData;
    }

    public abstract class TMDPrimitivePacketData {} // abstract base class for generic constraint

    // mode, flag, ilen, olen

    #region 3 Vertex Poly with No Light Source Calculation
    // Flat, no texture
    public class TriFlatUnlit : TMDPrimitivePacketData
    {
        // mode=0x21, flag=0x1, ilen=0x3, olen=0x4

        public byte r, g, b;
        public ushort p0, p1, p2;
    }

    // flat, texture
    public class TriFlatTexUnlit : TMDPrimitivePacketData
    {
        // mode=0x25, flag=0x1, ilen=0x6, olen=0x7

        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public byte r, g, b;
        public ushort p0, p1, p2;
    }

    // gradation, no texture
    public class TriGradUnlit : TMDPrimitivePacketData
    {
        // mode=0x31, flag=0x1, ilen=0x5, olen=0x6

        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort p0, p1, p2;
    }

    // gradation, texture
    public class TriGradTexUnlit : TMDPrimitivePacketData
    {
        // mode=0x35, flag=0x1, ilen=0x8, mode=0x9

        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort p0, p1, p2;
    }
    #endregion

    #region 3 Vertex Poly with Light Source Calculation

    // flat, no texture (solid)
    public class TriFlatLit : TMDPrimitivePacketData
    {
        // mode=0x20, flag=0x0, ilen=0x3, olen=0x4

        public byte r, g, b;
        public ushort n0;
        public ushort p0, p1, p2;
    }

    // flat, no texture (grad)
    public class TriFlatGradLit : TMDPrimitivePacketData
    {
        // mode=0x20, flag=0x4, ilen=0x5, olen=0x6

        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort n0;
        public ushort p0, p1, p2;
    }

    // flat, texture
    public class TriFlatTexLit : TMDPrimitivePacketData
    {
        // mode=0x24, flag=0x0, ilen=0x5, olen=0x7

        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public ushort n0;
        public ushort p0, p1, p2;
    }

    // gouraud, no texture (solid)
    public class TriShadedLit : TMDPrimitivePacketData
    {
        // mode=0x30, flag=0x0, ilen=0x4, olen=0x6

        public byte r, g, b;
        public ushort n0, n1, n2;
        public Vec3 p0, p1, p2;
    }

    // gouraud, no texture (grad)
    public class TriShadedGradLit : TMDPrimitivePacketData
    {
        // mode=0x30, flag=0x4, ilen=0x6, olen=0x6

        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort n0, n1, n2;
        public ushort p0, p1, p2;
    }

    // gouraud, texture
    public class TriShadedTexLit : TMDPrimitivePacketData
    {
        // mode=0x34, flag=0x0, ilen=0x6, olen=0x9
        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public ushort n0, n1, n2;
        public ushort p0, p1, p2;
    }

    #endregion

    #region 4 Vertex Poly with No Light Source Calculation

    // flat, no texture
    public class QuadFlatUnlit : TMDPrimitivePacketData
    {
        // mode=0x29, flag=0x1, ilen=0x3, olen=0x5

        public byte r, g, b;
        public ushort p0, p1, p2, p3;
    }

    // flat, texture
    public class QuadFlatTexUnlit : TMDPrimitivePacketData
    {
        // mode=0x2D, flag=0x1, ilen=0x7, olen=0x9

        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public byte u3, v3;
        public byte r, g, b;
        public ushort p0, p1, p2, p3;
    }

    // grad, no texture
    public class QuadGradUnlit : TMDPrimitivePacketData
    {
        // mode=0x39, flag=0x1, ilen=0x6, olen=0x8

        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort p0, p1, p2, p3;
    }

    // grad, texture
    public class QuadGradTexUnlit : TMDPrimitivePacketData
    {
        // mode=0x3D, flag=0x1, ilen=0xA, olen=0xC

        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public byte u3, v3;
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort p0, p1, p2, p3;
    }

    #endregion

    #region 4 Vertex Poly with Light Source Calculation

    // flat, no texture (solid)
    public class QuadFlatLit : TMDPrimitivePacketData
    {
        // mode=0x28, flag=0x0, ilen=0x4, olen=0x5

        public byte r, g, b;
        public ushort n0;
        public ushort p0, p1, p2, p3;
    }

    // flat, no texture (grad)
    public class QuadFlatGradLit : TMDPrimitivePacketData
    {
        // mode=0x28, flag=0x4, ilen=0x7, olen=0x8

        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort n0;
        public ushort p0, p1, p2, p3;
    }

    // flat, texture
    public class QuadFlatTexLit : TMDPrimitivePacketData
    {
        // mode=0x2C, flag=0x0, ilen=0x7, olen=0x9

        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public byte u3, v3;
        public ushort n0;
        public ushort p0, p1, p2, p3;
    }

    // gouraud, no texture (solid)
    public class QuadShadedLit : TMDPrimitivePacketData
    {
        // mode=0x38, flag=0x0, ilen=0x5, olen=0x8

        public byte r, g, b;
        public ushort n0, n1, n2, n3;
        public ushort p0, p1, p2, p3;
    }

    // gouraud, no texture (grad)
    public class QuadShadedGradLit : TMDPrimitivePacketData
    {
        // mode=0x38, flag=0x4, ilen=0x8, olen=0x8

        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort n0, n1, n2, n3;
        public ushort p0, p1, p2, p3;
    }

    // gouraud, texture
    public class QuadShadedTexLit : TMDPrimitivePacketData
    {
        // mode=0x3C, flag=0x0, ilen=0x8, olen=0xC

        public byte u0, v0;
        public TMDCBA cba;
        public byte u1, v1;
        public TMDTSB tsb;
        public byte u2, v2;
        public byte u3, v3;
        public ushort n0, n1, n2, n3;
        public ushort p0, p1, p2, p3;
    }

    #endregion

    #region Straight Line

    public class LineFlat : TMDPrimitivePacketData
    {
        // mode=0x40, flag=0x1, ilen=0x2, olen=0x3

        public byte r, g, b;
        public ushort p0, p1;
    }

    public class LineGrad : TMDPrimitivePacketData
    {
        // mode=0x50, flag=0x1, ilen=0x3, olen=0x4

        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public ushort p0, p1;
    }

    #endregion

    public struct TMDCBA
    {
        public int CLX
        {
            get => _cba & CLX_MASK;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLX_MASK);
        }

        public int CLY
        {
            get => (_cba & CLY_MASK) >> 6;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLY_MASK, 6);
        }

        private const ushort CLX_MASK = 0b111111;
        private const ushort CLY_MASK = 0b111111111000000;

        private ushort _cba;
    }

    public struct TMDTSB
    {
        public enum TransparencyRates
        {
            FIFTYB_PLUS_FIFTYP = 0,
            HUNDREDB_PLUS_HUNDREDP = 1,
            HUNDREDB_MIN_HUNDREDP = 2,
            HUNDREDB_PLUS_25P = 3
        }

        public enum ColorModes
        {
            FOURBIT = 0,
            EIGHTBIT = 1,
            FIFTEENBIT = 2
        }

        public int TexturePageNumber
        {
            get => _tsb & TPAGE_MASK;
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, TPAGE_MASK);
        }

        public TransparencyRates ABR
        {
            get => (TransparencyRates)((_tsb & ABR_MASK) >> 5);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, ABR_MASK, 5);
        }

        public ColorModes TPF
        {
            get => (ColorModes)((_tsb & TPF_MASK) >> 7);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, TPF_MASK, 7);
        }

        private const ushort TPAGE_MASK = 0b11111;
        private const ushort ABR_MASK = 0b1100000;
        private const ushort TPF_MASK = 0b110000000;

        private ushort _tsb;
    }

    public struct TMDNormal
    {
        public FixedPoint X;
        public FixedPoint Y;
        public FixedPoint Z;
    }
}
