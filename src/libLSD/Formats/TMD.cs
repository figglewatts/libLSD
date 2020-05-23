using System;
using System.IO;
using libLSD.Exceptions;
using libLSD.Interfaces;
using libLSD.Types;
using libLSD.Util;

namespace libLSD.Formats
{
    public struct TMD : IWriteable
    {
        public TMDHeader Header;
        public TMDObject[] ObjectTable;

	    public readonly uint NumberOfVertices;

        public TMD(BinaryReader b)
        {
            Header = new TMDHeader(b);
	        NumberOfVertices = 0;
            ObjectTable = new TMDObject[Header.NumObjects];
	        uint objTableTop = (uint)b.BaseStream.Position;
            for (int i = 0; i < Header.NumObjects; i++)
            {
                ObjectTable[i] = new TMDObject(b, Header.FixP, objTableTop);
                NumberOfVertices += ObjectTable[i].NumVertices;
            }
        }

        public void Write(BinaryWriter bw)
        {
            Header.Write(bw);
            foreach (var obj in ObjectTable)
            {
                obj.Write(bw);
            }

            foreach (var obj in ObjectTable)
            {
                foreach (var prim in obj.Primitives)
                {
                    prim.Write(bw);
                }
            }

            foreach (var obj in ObjectTable)
            {
                foreach (var vert in obj.Vertices)
                {
                    vert.Write(bw);
                }
            }

            foreach (var obj in ObjectTable)
            {
                foreach (var norm in obj.Normals)
                {
                    norm.Write(bw);
                }
            }
        }
    }

    public struct TMDHeader : IWriteable
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

        public void Write(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(_flags);
            bw.Write(NumObjects);
        }
    }

    public struct TMDObject : IWriteable
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

        public TMDObject(BinaryReader b, bool fixp, uint objTableTop)
        {
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

            uint cachedEndPos = (uint) b.BaseStream.Position;

            b.BaseStream.Seek(fixp ? PrimitivesAddress: PrimitivesAddress + objTableTop, SeekOrigin.Begin);
            for (int i = 0; i < NumPrimitives; i++)
            {
                Primitives[i] = new TMDPrimitivePacket(b);
            }

            b.BaseStream.Seek(fixp ? VerticesAddress : VerticesAddress + objTableTop, SeekOrigin.Begin);
            for (int i = 0; i < NumVertices; i++)
            {
                Vertices[i] = new Vec3(b);
            }

            b.BaseStream.Seek(fixp ? NormalsAddress : NormalsAddress + objTableTop, SeekOrigin.Begin);
            for (int i = 0; i < NumNormals; i++)
            {
                Normals[i] = new TMDNormal(b);
            }

            b.BaseStream.Seek(cachedEndPos, SeekOrigin.Begin);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(VerticesAddress);
            bw.Write(NumVertices);
            bw.Write(NormalsAddress);
            bw.Write(NumNormals);
            bw.Write(PrimitivesAddress);
            bw.Write(NumPrimitives);
            bw.Write(Scale);
        }
    }

    public class TMDPrimitivePacket : IWriteable
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

        public readonly IPrimitivePacket PacketData;

        public TMDPrimitivePacket(BinaryReader b)
        {
            OLen = b.ReadByte();
            ILen = b.ReadByte();
            _flag = b.ReadByte();
            _mode = b.ReadByte();
            int identifierMode = _mode & ~(1 << 1); // unset the AlphaBlend bit, as it doesn't affect packet layout
            int identifier = (identifierMode << 8) + _flag;
            PacketData = IPrimitivePacketFactory.Create((ushort)identifier, b);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(OLen);
            bw.Write(ILen);
            bw.Write(_flag);
            bw.Write(_mode);
            IWriteable w = PacketData as IWriteable;
            if (PacketData != null)
            {
                w.Write(bw);
            }
            else
            {
                throw new UnwriteableException($"Cannot write packet data with OLen {OLen}, ILen {ILen}, Flag {_flag:X}, " +
                                               $"and Mode {_mode:X}, packet data did not implement IWriteable");
            }
        }
    }

    internal static class IPrimitivePacketFactory
    {
        internal static Type GetPacketType(ushort identifier, byte olen, byte ilen, byte flag, byte mode)
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
                    return typeof(TriFlatLitPrimitive);
                case 0x2004:
                    return typeof(TriFlatGradLitPrimitive);
                case 0x2400:
                    return typeof(TriFlatTexLitPrimitive);
                case 0x3000:
                    return typeof(TriShadedLitPrimitive);
                case 0x3004:
                    return typeof(TriShadedGradLitPrimitive);
                case 0x3400:
                    return typeof(TriShadedTexLitPrimitive);

                case 0x2901:
                    return typeof(QuadFlatUnlit);
                case 0x2D01:
                    return typeof(QuadFlatTexUnlit);
                case 0x3901:
                    return typeof(QuadGradUnlit);
                case 0x3D01:
                    return typeof(QuadGradTexUnlit);
                case 0x2800:
                    return typeof(QuadFlatLitPrimitive);
                case 0x2804:
                    return typeof(QuadFlatGradLitPrimitive);
                case 0x2C00:
                    return typeof(QuadFlatTexLitPrimitive);
                case 0x3800:
                    return typeof(QuadShadedLitPrimitive);
                case 0x3804:
                    return typeof(QuadShadedGradLitPrimitive);
                case 0x3C00:
                    return typeof(QuadShadedTexLitPrimitive);

                case 0x4001:
                    return typeof(LineFlat);
                case 0x5001:
                    return typeof(LineGrad);
				default:
					string dbg = $"OLen: 0x{olen:X}, ILen: 0x{ilen:X}, flag: 0x{flag:X}, mode: 0x{mode:X}";
					string err = $"Unknown packet identifier: 0x{identifier:X}";
					//throw new BadFormatException(err);
					Console.WriteLine(err);
					Console.WriteLine("\t" + dbg);
					return null;
			}

            
        }

        internal static IPrimitivePacket Create(ushort identifier, BinaryReader br)
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
                    return new TriFlatLitPrimitive(br);
                case 0x2004:
                    return new TriFlatGradLitPrimitive(br);
                case 0x2400:
                    return new TriFlatTexLitPrimitive(br);
                case 0x3000:
                    return new TriShadedLitPrimitive(br);
                case 0x3004:
                    return new TriShadedGradLitPrimitive(br);
                case 0x3400:
                    return new TriShadedTexLitPrimitive(br);

                case 0x2901:
                    return new QuadFlatUnlit(br);
                case 0x2D01:
                    return new QuadFlatTexUnlit(br);
                case 0x3901:
                    return new QuadGradUnlit(br);
                case 0x3D01:
                    return new QuadGradTexUnlit(br);
                case 0x2800:
                    return new QuadFlatLitPrimitive(br);
                case 0x2804:
                    return new QuadFlatGradLitPrimitive(br);
                case 0x2C00:
                    return new QuadFlatTexLitPrimitive(br);
                case 0x3800:
                    return new QuadShadedLitPrimitive(br);
                case 0x3804:
                    return new QuadShadedGradLitPrimitive(br);
                case 0x3C00:
                    return new QuadShadedTexLitPrimitive(br);

                case 0x4001:
                    return new LineFlat(br);
                case 0x5001:
                    return new LineGrad(br);

				default:
					string err = $"Unknown packet identifier: 0x{identifier:X}";
					//throw new BadFormatException(err);
					Console.WriteLine(err);
					return null;
			}

            
        }
    }

    public struct TMDNormal : IWriteable
    {
        public FixedPoint16Bit X;
        public FixedPoint16Bit Y;
        public FixedPoint16Bit Z;

        public TMDNormal(BinaryReader br)
        {
            X = br.ReadBytes(2);
            Y = br.ReadBytes(2);
            Z = br.ReadBytes(2);
            br.ReadInt16();
        }

        public void Write(BinaryWriter bw)
        {
            X.Write(bw);
            Y.Write(bw);
            Z.Write(bw);
            bw.Write((short) 0);
        }
    }
}

