using System;
using System.IO;
using libLSD.Exceptions;
using libLSD.Formats.Packets;
using libLSD.Interfaces;
using libLSD.Types;
using libLSD.Util;

namespace libLSD.Formats
{
    /// <summary>
    /// A TMD file stores 3D model data. They contain vertex data, normal data, and primitive data.
    /// The primitive data is different 'packets' that get sent to the GPU to tell it to draw triangles/quads with
    /// different lighting, shading and texturing.
    ///
    /// TMDs can contain multiple 'objects'. These are essentially independent models. In LSD they are used to store
    /// separate tiles in an LBD file, or separate parts of animated models (like arms/legs etc).
    /// </summary>
    public struct TMD : IWriteable
    {
        /// <summary>
        /// The header of the TMD file.
        /// </summary>
        public TMDHeader Header;

        /// <summary>
        /// The TMD's object table.
        /// </summary>
        public TMDObject[] ObjectTable;

        /// <summary>
        /// The number of vertices in this TMD file.
        /// </summary>
        public readonly uint NumberOfVertices;

        /// <summary>
        /// Create a new TMD by reading from a binary stream.
        /// </summary>
        /// <param name="b">The binary stream.</param>
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

        /// <summary>
        /// Write this TMD to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
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

    /// <summary>
    /// The header of a TMD file.
    /// </summary>
    public struct TMDHeader : IWriteable
    {
        /// <summary>
        /// The TMD magic number. Should always be 0x41.
        /// </summary>
        public readonly uint ID;

        /// <summary>
        /// The number of objects in this TMD.
        /// </summary>
        public readonly uint NumObjects;

        /// <summary>
        /// Whether or not the pointers in the object table are real addresses or offsets. If true, they are actual
        /// addresses within the file, if false they are offsets from the top of the object block.
        /// </summary>
        public bool FixP { get => (_flags & 0x1) == 1; }

        private readonly uint _flags;

        private const int MAGIC_NUMBER = 0x41;

        /// <summary>
        /// Create a new TMD header by reading from a binary stream.
        /// </summary>
        /// <param name="b">The binary stream.</param>
        /// <exception cref="BadFormatException">If the TMD magic number in this file was not 0x41.</exception>
        public TMDHeader(BinaryReader b)
        {
            ID = b.ReadUInt32();
            _flags = b.ReadUInt32();
            NumObjects = b.ReadUInt32();

            if (ID != MAGIC_NUMBER)
                throw new BadFormatException("TMD file did not have correct magic number");
        }

        /// <summary>
        /// Write this TMD header to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(_flags);
            bw.Write(NumObjects);
        }
    }

    /// <summary>
    /// An object within the TMD. Stored in the TMD's object table. Contains pointers to vertices, normals, and
    /// primitives.
    /// </summary>
    public struct TMDObject : IWriteable
    {
        /// <summary>
        /// The address or offset of the vertices of this object.
        /// </summary>
        public readonly uint VerticesAddress;

        /// <summary>
        /// The number of vertices in this object.
        /// </summary>
        public readonly uint NumVertices;

        /// <summary>
        /// The address or offset of the normals of this object.
        /// </summary>
        public readonly uint NormalsAddress;

        /// <summary>
        /// The number of normals in this object.
        /// </summary>
        public readonly uint NumNormals;

        /// <summary>
        /// The address or offset of the primitives of this object.
        /// </summary>
        public readonly uint PrimitivesAddress;

        /// <summary>
        /// The number of primitives in this object.
        /// </summary>
        public readonly uint NumPrimitives;

        /// <summary>
        /// The scale of this TMD. 2^Scale is the scale factor./>
        /// </summary>
        public readonly int Scale; // 2^Scale is the scale factor of the TMD

        /// <summary>
        /// The primitives of this object.
        /// </summary>
        public readonly TMDPrimitivePacket[] Primitives;

        /// <summary>
        /// The vertices of this object.
        /// </summary>
        public readonly Vec3[] Vertices;

        /// <summary>
        /// The normals of this object.
        /// </summary>
        public readonly TMDNormal[] Normals;

        /// <summary>
        /// Create a new TMD object by reading from a binary stream.
        /// </summary>
        /// <param name="b">The binary stream.</param>
        /// <param name="fixp">The FixP flag, from the TMD header.</param>
        /// <param name="objTableTop">The address of the start of the TMD's object table.</param>
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

            uint cachedEndPos = (uint)b.BaseStream.Position;

            b.BaseStream.Seek(fixp ? PrimitivesAddress : PrimitivesAddress + objTableTop, SeekOrigin.Begin);
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

        /// <summary>
        /// Write this TMD object to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
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

    /// <summary>
    /// A TMD primitive packet. For the most part, this is the raw data that gets sent to the GPU to draw the object.
    /// There's a bunch of different types of these - quads and tries, all with different properties like shading,
    /// texture mapping, and lighting.
    /// </summary>
    public class TMDPrimitivePacket : IWriteable
    {
        /// <summary>
        /// The types of primitive packet available.
        /// </summary>
        public enum Types
        {
            POLYGON = 0b1,
            LINE = 0b10,
            SPRITE = 0b11
        }

        /// <summary>
        /// Settings for this primitive packet.
        /// </summary>
        [Flags]
        public enum PrimitiveFlags
        {
            Lighting = 1,    // set if light source calculation not carried out
            DoubleSided = 2, // set if poly is double sided
            Gradient = 4     // set if poly is gradated, else single colour
        }

        /// <summary>
        /// Rendering options for this primitive packet.
        /// </summary>
        [Flags]
        public enum OptionsFlags
        {
            BrightnessCalculated = 1, // brightness calculation (off draws tex as-is)
            AlphaBlended = 2,         // translucency processing
            Textured = 4,             // texture specification
            Quad = 8,                 // set if 4-vertex primitive
            GouraudShaded = 16        // 0 = flat shading, 1 = gouraud
        }

        /// <summary>
        /// Sprite sizes, for sprite primitive packets.
        /// </summary>
        public enum SpriteSizes
        {
            FREE_SIZE = 0,
            ONE = 1,
            EIGHT = 2,
            SIXTEEN = 3
        }

        /// <summary>
        /// The type of this primitive packet.
        /// </summary>
        public Types Type
        {
            get => (Types)((_mode & TYPE_MASK) >> 5);
            protected set => this._mode = BitTwiddling.Merge<byte>(_mode, value, TYPE_MASK, 5);
        }

        /// <summary>
        /// The rendering options for this primitive packet.
        /// </summary>
        public OptionsFlags Options
        {
            get => (OptionsFlags)(_mode & OPTIONS_MASK);
            protected set => this._mode = BitTwiddling.Merge<byte>(_mode, value, OPTIONS_MASK);
        }

        /// <summary>
        /// The settings for this primitive packet.
        /// </summary>
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

        /// <summary>
        /// The length (in words) of packet data.
        /// </summary>
        public byte ILen;

        /// <summary>
        /// The length (in words) of 2D drawing primitives.
        /// </summary>
        public byte OLen;

        private byte _mode;
        private byte _flag;

        private const uint TYPE_MASK = 0b11100000;
        private const uint OPTIONS_MASK = 0b11111;
        private const uint FLAGS_MASK = 0b111;
        private const uint SPRITE_SIZE_MASK = 0b11000;

        /// <summary>
        /// The primitive packet data itself.
        /// </summary>
        public readonly ITMDPrimitivePacket PacketData;

        /// <summary>
        /// Create a new primitive packet by reading from a binary stream.
        /// </summary>
        /// <param name="b">The binary stream.</param>
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

        /// <summary>
        /// Write this primitive packet to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        /// <exception cref="UnwriteableException">If there was an error writing the packet data.</exception>
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
                throw new UnwriteableException(
                    $"Cannot write packet data with OLen {OLen}, ILen {ILen}, Flag {_flag:X}, " +
                    $"and Mode {_mode:X}, packet data did not implement IWriteable");
            }
        }
    }

    /// <summary>
    /// Factory class for creating primitive packets.
    /// </summary>
    internal static class IPrimitivePacketFactory
    {
        /// <summary>
        /// Gets the packet type by its identifier.
        /// </summary>
        /// <param name="identifier">Packet identifier.</param>
        /// <param name="olen">OLen of the packet.</param>
        /// <param name="ilen">ILen of the packet.</param>
        /// <param name="flag">Flags of the packet.</param>
        /// <param name="mode">Modes of the packet.</param>
        /// <returns>The packet type.</returns>
        internal static Type GetPacketType(ushort identifier, byte olen, byte ilen, byte flag, byte mode)
        {
            switch (identifier)
            {
                case 0x2101:
                    return typeof(TMDTriFlatUnlitPrimitivePacket);
                case 0x2501:
                    return typeof(TMDTriFlatTexUnlitPrimitivePacket);
                case 0x3101:
                    return typeof(TMDTriGradUnlitPrimitivePacket);
                case 0x3501:
                    return typeof(TMDTriGradTexUnlitPrimitivePacket);
                case 0x2000:
                    return typeof(TMDTriFlatLitPrimitivePacket);
                case 0x2004:
                    return typeof(TMDTriFlatGradLitPrimitivePacket);
                case 0x2400:
                    return typeof(TMDTriFlatTexLitPrimitivePacket);
                case 0x3000:
                    return typeof(TMDTriShaderLitPrimitivePacket);
                case 0x3004:
                    return typeof(TMDTriShadedGradLitPrimitivePacket);
                case 0x3400:
                    return typeof(TMDTriShadedTexLitPrimitivePacket);

                case 0x2901:
                    return typeof(TMDQuadFlatUnlitPrimitivePacket);
                case 0x2D01:
                    return typeof(TMDQuadFlatTexUnlitPrimitivePacket);
                case 0x3901:
                    return typeof(TMDQuadGradUnlitPrimitivePacket);
                case 0x3D01:
                    return typeof(TMDQuadGradTexUnlitPrimitivePacket);
                case 0x2800:
                    return typeof(TMDQuadFlatLitPrimitivePacket);
                case 0x2804:
                    return typeof(TMDQuadFlatGradLitPrimitivePacket);
                case 0x2C00:
                    return typeof(TMDQuadFlatTexLitPrimitivePacket);
                case 0x3800:
                    return typeof(TMDQuadShadedLitPrimitivePacket);
                case 0x3804:
                    return typeof(TMDQuadShadedGradLitPrimitivePacket);
                case 0x3C00:
                    return typeof(TMDQuadShadedTexLitPrimitivePacket);

                case 0x4001:
                    return typeof(TMDLineFlatPrimitivePacket);
                case 0x5001:
                    return typeof(TMDLineGradPrimitivePacket);
                default:
                    string dbg = $"OLen: 0x{olen:X}, ILen: 0x{ilen:X}, flag: 0x{flag:X}, mode: 0x{mode:X}";
                    string err = $"Unknown packet identifier: 0x{identifier:X}";
                    //throw new BadFormatException(err);
                    Console.WriteLine(err);
                    Console.WriteLine("\t" + dbg);
                    return null;
            }
        }

        /// <summary>
        /// Create a new primitive packet by reading from a binary stream.
        /// </summary>
        /// <param name="identifier">The identifier of this packet.</param>
        /// <param name="br">The binary stream.</param>
        /// <returns>The new packet.</returns>
        internal static ITMDPrimitivePacket Create(ushort identifier, BinaryReader br)
        {
            switch (identifier)
            {
                case 0x2101:
                    return new TMDTriFlatUnlitPrimitivePacket(br);
                case 0x2501:
                    return new TMDTriFlatTexUnlitPrimitivePacket(br);
                case 0x3101:
                    return new TMDTriGradUnlitPrimitivePacket(br);
                case 0x3501:
                    return new TMDTriGradTexUnlitPrimitivePacket(br);
                case 0x2000:
                    return new TMDTriFlatLitPrimitivePacket(br);
                case 0x2004:
                    return new TMDTriFlatGradLitPrimitivePacket(br);
                case 0x2400:
                    return new TMDTriFlatTexLitPrimitivePacket(br);
                case 0x3000:
                    return new TMDTriShaderLitPrimitivePacket(br);
                case 0x3004:
                    return new TMDTriShadedGradLitPrimitivePacket(br);
                case 0x3400:
                    return new TMDTriShadedTexLitPrimitivePacket(br);

                case 0x2901:
                    return new TMDQuadFlatUnlitPrimitivePacket(br);
                case 0x2D01:
                    return new TMDQuadFlatTexUnlitPrimitivePacket(br);
                case 0x3901:
                    return new TMDQuadGradUnlitPrimitivePacket(br);
                case 0x3D01:
                    return new TMDQuadGradTexUnlitPrimitivePacket(br);
                case 0x2800:
                    return new TMDQuadFlatLitPrimitivePacket(br);
                case 0x2804:
                    return new TMDQuadFlatGradLitPrimitivePacket(br);
                case 0x2C00:
                    return new TMDQuadFlatTexLitPrimitivePacket(br);
                case 0x3800:
                    return new TMDQuadShadedLitPrimitivePacket(br);
                case 0x3804:
                    return new TMDQuadShadedGradLitPrimitivePacket(br);
                case 0x3C00:
                    return new TMDQuadShadedTexLitPrimitivePacket(br);

                case 0x4001:
                    return new TMDLineFlatPrimitivePacket(br);
                case 0x5001:
                    return new TMDLineGradPrimitivePacket(br);

                default:
                    string err = $"Unknown packet identifier: 0x{identifier:X}";
                    //throw new BadFormatException(err);
                    Console.WriteLine(err);
                    return null;
            }
        }
    }

    /// <summary>
    /// A normal, as stored in a TMD.
    /// </summary>
    public struct TMDNormal : IWriteable
    {
        /// <summary>
        /// X component of the normal.
        /// </summary>
        public FixedPoint16Bit X;

        /// <summary>
        /// Y component of the normal.
        /// </summary>
        public FixedPoint16Bit Y;

        /// <summary>
        /// Z component of the normal.
        /// </summary>
        public FixedPoint16Bit Z;

        /// <summary>
        /// Create a new TMD normal by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public TMDNormal(BinaryReader br)
        {
            X = br.ReadBytes(2);
            Y = br.ReadBytes(2);
            Z = br.ReadBytes(2);
            br.ReadInt16();
        }

        /// <summary>
        /// Write this TMD normal to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            X.Write(bw);
            Y.Write(bw);
            Z.Write(bw);
            bw.Write((short)0);
        }
    }
}
