using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Util;

namespace libLSD.Types
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

    // TODO: figure out way of implementing GPU packets
    // sidenote: GPU packets can be found in LIBGPU.H of psy-q includes

    public class TMDPrimitivePacket<T> : AbstractTMDPrimitivePacket where T : TMDPrimitivePacketData
    {
        
    }

    public abstract class TMDPrimitivePacketData
    {
        
    }

    public struct TMDNormal
    {
        
    }

    public struct TMDTexCoord
    {
        
    }
}
