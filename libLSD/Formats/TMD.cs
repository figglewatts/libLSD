using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using libLSD.Exceptions;
using libLSD.Types;
using libLSD.Util;

namespace libLSD.Formats
{
    public class TMD
    {
        public TMDHeader Header;
        public TMDObject[] ObjectTable;

        public TMD(BinaryReader b)
        {
            Header = new TMDHeader(b);

            ObjectTable = new TMDObject[Header.NumObjects];
            for (int i = 0; i < Header.NumObjects; i++)
            {
                ObjectTable[i] = new TMDObject(b, Header.FixP);
            }
        }
    }

    public struct TMDHeader
    {
        public readonly uint ID;
        public readonly uint NumObjects;

        public bool FixP { get => (_flags & 0x1) == 1; }

        private readonly uint _flags;

        public TMDHeader(BinaryReader b)
        {
            ID = b.ReadUInt32();
            _flags = b.ReadUInt32();
            NumObjects = b.ReadUInt32();

            if (ID != 0x41)
                throw new BadFormatException("TMD file did not have correct magic number");
        }
    }

    public struct TMDObject
    {
        public readonly uint VerticesAddress;
        public readonly uint NumVertices;
        public readonly uint NormalsAddress;
        public readonly uint NumNormals;
        public readonly uint PrimitivesAddress;
        public readonly uint NumPrimitives;
        public readonly int Scale; // 2^Scale is the scale factor of the TMD

        public readonly TMDPrimitivePacket[] Primitives;
        public readonly Vec3[] Vertices;
        public readonly TMDNormal[] Normals;

        private uint ObjectAddress;

        public TMDObject(BinaryReader b, bool fixp)
        {
            ObjectAddress = (uint)b.BaseStream.Position;
            VerticesAddress = b.ReadUInt32();
            NumVertices = b.ReadUInt32();
            NormalsAddress = b.ReadUInt32();
            NumNormals = b.ReadUInt32();
            PrimitivesAddress = b.ReadUInt32();
            NumPrimitives = b.ReadUInt32();
            Scale = b.ReadInt32();
            Primitives = new TMDPrimitivePacket[NumPrimitives];
            Vertices = new Vec3[NumVertices];
            Normals = new TMDNormal[NumNormals];

            b.BaseStream.Seek(fixp ? PrimitivesAddress : PrimitivesAddress + ObjectAddress, SeekOrigin.Begin);
            for (int i = 0; i < NumPrimitives; i++)
            {
                Primitives[i] = new TMDPrimitivePacket(b);
            }

            b.BaseStream.Seek(fixp ? VerticesAddress : VerticesAddress + ObjectAddress, SeekOrigin.Begin);
            for (int i = 0; i < NumVertices; i++)
            {
                Vertices[i] = new Vec3(b);
            }

            b.BaseStream.Seek(fixp ? NormalsAddress : NormalsAddress + ObjectAddress, SeekOrigin.Begin);
            for (int i = 0; i < NumNormals; i++)
            {
                Normals[i] = new TMDNormal(b);
            }
        }
    }

    public class TMDPrimitivePacket
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
            Lighting = 1,    // set if light source calculation not carried out
            DoubleSided = 2,    // set if poly is double sided
            Gradient = 4     // set if poly is gradated, else single colour
        }

        [Flags]
        public enum OptionsFlags
        {
            BrightnessCalculated = 1,    // brightness calculation (off draws tex as-is)
            AlphaBlended = 2,    // translucency processing
            Textured = 4,    // texture specification
            Quad = 8,   // set if 4-vertex primitive
            GouraudShaded = 16    // 0 = flat shading, 1 = gouraud
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

        private byte _mode;
        private byte _flag;

        private const uint TYPE_MASK = 0b11100000;
        private const uint OPTIONS_MASK = 0b11111;
        private const uint FLAGS_MASK = 0b111;
        private const uint SPRITE_SIZE_MASK = 0b11000;

        private dynamic _packetData;

        public dynamic PacketData { get => Convert.ChangeType(_packetData, PacketType); }

        private readonly Type PacketType;

        public TMDPrimitivePacket(BinaryReader b)
        {
            _mode = b.ReadByte();
            _flag = b.ReadByte();
            ILen = b.ReadByte();
            OLen = b.ReadByte();
            int identifier = (_mode << 8) + _flag;
            PacketType = TMDPrimitivePacketDataFactory.GetPacketType((ushort) identifier);
            _packetData = TMDPrimitivePacketDataFactory.Create((ushort)identifier, b);
        }
    }

    internal static class TMDPrimitivePacketDataFactory
    {
        internal static Type GetPacketType(ushort identifier)
        {
            switch (identifier)
            {
                case 0x2101:
                    return typeof(TriFlatUnlit);
                case 0x2501:
                    return typeof(TriFlatTexUnlit);
                case 0x3101:
                    return typeof(TriGradUnlit);
                case 0x3501:
                    return typeof(TriGradTexUnlit);
                case 0x2000:
                    return typeof(TriFlatLit);
                case 0x2004:
                    return typeof(TriFlatGradLit);
                case 0x2400:
                    return typeof(TriFlatTexLit);
                case 0x3000:
                    return typeof(TriShadedLit);
                case 0x3004:
                    return typeof(TriShadedGradLit);
                case 0x3400:
                    return typeof(TriShadedTexLit);

                case 0x2901:
                    return typeof(QuadFlatUnlit);
                case 0x2D01:
                    return typeof(QuadFlatTexUnlit);
                case 0x3901:
                    return typeof(QuadGradUnlit);
                case 0x3D01:
                    return typeof(QuadGradTexUnlit);
                case 0x2800:
                    return typeof(QuadFlatLit);
                case 0x2804:
                    return typeof(QuadFlatGradLit);
                case 0x2C00:
                    return typeof(QuadFlatTexLit);
                case 0x3800:
                    return typeof(QuadShadedLit);
                case 0x3804:
                    return typeof(QuadShadedGradLit);
                case 0x3C00:
                    return typeof(QuadShadedTexLit);

                case 0x4001:
                    return typeof(LineFlat);
                case 0x5001:
                    return typeof(LineGrad);
            }

            throw new BadFormatException($"Unknown packet identifier: {identifier:X}");
        }

        internal static TMDPrimitivePacketData Create(ushort identifier, BinaryReader br)
        {
            switch (identifier)
            {
                case 0x2101:
                    return new TriFlatUnlit(br);
                case 0x2501:
                    return new TriFlatTexUnlit(br);
                case 0x3101:
                    return new TriGradUnlit(br);
                case 0x3501:
                    return new TriGradTexUnlit(br);
                case 0x2000:
                    return new TriFlatLit(br);
                case 0x2004:
                    return new TriFlatGradLit(br);
                case 0x2400:
                    return new TriFlatTexLit(br);
                case 0x3000:
                    return new TriShadedLit(br);
                case 0x3004:
                    return new TriShadedGradLit(br);
                case 0x3400:
                    return new TriShadedTexLit(br);

                case 0x2901:
                    return new QuadFlatUnlit(br);
                case 0x2D01:
                    return new QuadFlatTexUnlit(br);
                case 0x3901:
                    return new QuadGradUnlit(br);
                case 0x3D01:
                    return new QuadGradTexUnlit(br);
                case 0x2800:
                    return new QuadFlatLit(br);
                case 0x2804:
                    return new QuadFlatGradLit(br);
                case 0x2C00:
                    return new QuadFlatTexLit(br);
                case 0x3800:
                    return new QuadShadedLit(br);
                case 0x3804:
                    return new QuadShadedGradLit(br);
                case 0x3C00:
                    return new QuadShadedTexLit(br);

                case 0x4001:
                    return new LineFlat(br);
                case 0x5001:
                    return new LineGrad(br);
            }

            throw new BadFormatException($"Unknown packet identifier: {identifier:X}");
        }
    }

    public abstract class TMDPrimitivePacketData { }

    #region 3 Vertex Poly with No Light Source Calculation
    // Flat, no texture
    // mode=0x21, flag=0x1, ilen=0x3, olen=0x4
    public class TriFlatUnlit : TMDPrimitivePacketData
    {
        public byte r, g, b;
        public ushort p0, p1, p2;

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
    public class TriFlatTexUnlit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public byte r, g, b;
        public ushort p0, p1, p2;

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
    public class TriGradUnlit : TMDPrimitivePacketData
    {
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort p0, p1, p2;

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
    public class TriGradTexUnlit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort p0, p1, p2;

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
    public class TriFlatLit : TMDPrimitivePacketData
    {
        public byte r, g, b;
        public ushort n0;
        public ushort p0, p1, p2;

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
    public class TriFlatGradLit : TMDPrimitivePacketData
    {
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort n0;
        public ushort p0, p1, p2;

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
    public class TriFlatTexLit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public ushort n0;
        public ushort p0, p1, p2;

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
    public class TriShadedLit : TMDPrimitivePacketData
    {
        public byte r, g, b;
        public ushort n0, n1, n2;
        public ushort p0, p1, p2;

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
    public class TriShadedGradLit : TMDPrimitivePacketData
    {
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public ushort n0, n1, n2;
        public ushort p0, p1, p2;

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
    public class TriShadedTexLit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public ushort n0, n1, n2;
        public ushort p0, p1, p2;

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
    public class QuadFlatUnlit : TMDPrimitivePacketData
    {
        public byte r, g, b;
        public ushort p0, p1, p2, p3;

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
    public class QuadFlatTexUnlit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public byte u3, v3;
        public byte r, g, b;
        public ushort p0, p1, p2, p3;

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
    public class QuadGradUnlit : TMDPrimitivePacketData
    {
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort p0, p1, p2, p3;

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
    public class QuadGradTexUnlit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public byte u3, v3;
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort p0, p1, p2, p3;

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
    public class QuadFlatLit : TMDPrimitivePacketData
    {
        public byte r, g, b;
        public ushort n0;
        public ushort p0, p1, p2, p3;

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
    public class QuadFlatGradLit : TMDPrimitivePacketData
    {
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort n0;
        public ushort p0, p1, p2, p3;

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
    public class QuadFlatTexLit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public byte u3, v3;
        public ushort n0;
        public ushort p0, p1, p2, p3;

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
    public class QuadShadedLit : TMDPrimitivePacketData
    {
        public byte r, g, b;
        public ushort n0, n1, n2, n3;
        public ushort p0, p1, p2, p3;

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
    public class QuadShadedGradLit : TMDPrimitivePacketData
    {
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public byte r2, g2, b2;
        public byte r3, g3, b3;
        public ushort n0, n1, n2, n3;
        public ushort p0, p1, p2, p3;

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
    public class QuadShadedTexLit : TMDPrimitivePacketData
    {
        public byte u0, v0;
        public TMDColorLookup cba;
        public byte u1, v1;
        public TMDTexture tsb;
        public byte u2, v2;
        public byte u3, v3;
        public ushort n0, n1, n2, n3;
        public ushort p0, p1, p2, p3;

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
    public class LineFlat : TMDPrimitivePacketData
    {
        public byte r, g, b;
        public ushort p0, p1;

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
    public class LineGrad : TMDPrimitivePacketData
    {
        public byte r0, g0, b0;
        public byte r1, g1, b1;
        public ushort p0, p1;

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

    public struct TMDColorLookup
    {
        public int XPosition
        {
            get => _cba & CLX_MASK;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLX_MASK);
        }

        public int YPosition
        {
            get => (_cba & CLY_MASK) >> 6;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLY_MASK, 6);
        }

        private const ushort CLX_MASK = 0b111111;
        private const ushort CLY_MASK = 0b111111111000000;

        private ushort _cba;

        private TMDColorLookup(ushort cba) { _cba = cba; }

        public static explicit operator ushort(TMDColorLookup cl) { return cl._cba; }

        public static implicit operator TMDColorLookup(ushort cba) { return new TMDColorLookup(cba); }
    }

    public struct TMDTexture
    {
        public enum TransparencyRates
        {
            RATE_50BACK_PLUS_50POLY = 0,    // 100% back + 50% poly
            RATE_100BACK_PLUS_100POLY = 1,  // 100% back + 100% poly
            RATE_100BACK_MINUS_100POLY = 2, // 100% back - 100% poly
            RATE_100BACK_MINUS_25POLY = 3   // 100% back - 25% poly
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

        public TransparencyRates AlphaBlendRate
        {
            get => (TransparencyRates)((_tsb & ABR_MASK) >> 5);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, ABR_MASK, 5);
        }

        public ColorModes ColorMode
        {
            get => (ColorModes)((_tsb & TPF_MASK) >> 7);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, TPF_MASK, 7);
        }

        private const ushort TPAGE_MASK = 0b11111;
        private const ushort ABR_MASK = 0b1100000;
        private const ushort TPF_MASK = 0b110000000;

        private ushort _tsb;

        private TMDTexture(ushort tsb) { _tsb = tsb; }

        public static explicit operator ushort(TMDTexture tex) { return tex._tsb; }

        public static implicit operator TMDTexture(ushort tsb) { return new TMDTexture(tsb); }
    }

    public struct TMDNormal
    {
        public FixedPoint X;
        public FixedPoint Y;
        public FixedPoint Z;

        public TMDNormal(BinaryReader br)
        {
            X = br.ReadBytes(2);
            Y = br.ReadBytes(2);
            Z = br.ReadBytes(2);
            br.ReadInt16();
        }
    }
}

