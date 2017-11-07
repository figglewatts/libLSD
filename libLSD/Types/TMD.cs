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
        public byte Mode;
        public byte Flag;
        public byte ILen; // length (in words) of packet data
        public byte OLen; // length (words) of 2D drawing primitives

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
