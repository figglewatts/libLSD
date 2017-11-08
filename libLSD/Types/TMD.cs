using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    public class TMD
    {
        public TMDHeader Header;
        public TMDObject[] ObjectTable;
        public TMDPrimitive[] Primitives;
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

    public struct TMDPrimitive
    {
        public enum Types
        {
            POLYGON = 0b1,
            LINE = 0b10,
            SPRITE = 0b11
        }

        public Types Type
        {
            get => (TMDPrimitive.Types)((_mode & TYPE_MASK) >> 5);
            //set => this._mode = _mode ^ ((_mode ^ ))
        }

        



        public byte Flag;
        public byte ILen; // length (in words) of packet data
        public byte OLen; // length (words) of 2D drawing primitives

        private byte _mode;

        private const uint TYPE_MASK = 0b11100000;
    }

    // TODO: figure out way of implementing GPU packets
    // sidenote: GPU packets can be found in LIBGPU.H of psy-q includes

    public struct TMDPrimitiveClassification
    {
        
    }

    public struct TMDPrimitiveData
    {
        
    }

    public struct TMDNormal
    {
        
    }

    public struct TMDTexCoord
    {
        
    }
}
