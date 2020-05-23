using System;
using System.IO;
using libLSD.Exceptions;
using libLSD.Formats.Packets;
using libLSD.Interfaces;

namespace libLSD.Formats
{
    /// <summary>
    /// The TOD format is used for storing information along the flow of time, relative to a 3-dimensional object.
    /// For each frame in a 3D animation, the TOD file describes the required data to pose a 3D model for that frame.
    /// </summary>
    public struct TOD : IWriteable
    {
        /// <summary>
        /// The header of this TOD file.
        /// </summary>
        public readonly TODHeader Header;

        /// <summary>
        /// The animation frames in this TOD file.
        /// </summary>
        public readonly TODFrame[] Frames;

        /// <summary>
        /// Create a new TOD file by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public TOD(BinaryReader br)
        {
            Header = new TODHeader(br);
            Frames = new TODFrame[Header.NumberOfFrames - 1];
            for (int i = 0; i < Header.NumberOfFrames - 1; i++)
            {
                Frames[i] = new TODFrame(br);
            }
        }

        /// <summary>
        /// Write this TOD file to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            Header.Write(bw);
            foreach (var frame in Frames)
            {
                frame.Write(bw);
            }
        }
    }

    /// <summary>
    /// The header of a TOD file.
    /// </summary>
    public struct TODHeader : IWriteable
    {
        /// <summary>
        /// The magic number of a TOD file. Always 0x50.
        /// </summary>
        public readonly byte ID;

        /// <summary>
        /// The version of this animation.
        /// </summary>
        public readonly byte Version;

        /// <summary>
        /// Time in which one frame is displayed, in units of ticks.
        /// 1 tick is equal to 1/60 seconds.
        /// </summary>
        public readonly ushort Resolution;

        /// <summary>
        /// The number of frames in this animation.
        /// </summary>
        public readonly uint NumberOfFrames;

        private const int MAGIC_NUMBER = 0x50;

        /// <summary>
        /// Create a new TOD header by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <exception cref="BadFormatException">If the TOD did not have the correct magic number.</exception>
        public TODHeader(BinaryReader br)
        {
            ID = br.ReadByte();

            if (ID != MAGIC_NUMBER)
                throw new BadFormatException("TOD did not have correct magic number!");

            Version = br.ReadByte();
            Resolution = br.ReadUInt16();
            NumberOfFrames = br.ReadUInt32();
        }

        /// <summary>
        /// Write this TOD header to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(Version);
            bw.Write(Resolution);
            bw.Write(NumberOfFrames);
        }
    }

    /// <summary>
    /// A single frame in a TOD animation. A frame consists of a bunch of packets that describe what to do
    /// with different components of a 3D model (like transformations etc).
    /// </summary>
    public struct TODFrame : IWriteable
    {
        /// <summary>
        /// The size of this frame.
        /// </summary>
        public readonly ushort FrameSize;

        /// <summary>
        /// The number of packets in this frame.
        /// </summary>
        public readonly ushort NumberOfPackets;

        /// <summary>
        /// The index of this frame.
        /// </summary>
        public readonly uint FrameNumber;

        /// <summary>
        /// This frame's packet data.
        /// </summary>
        public TODPacket[] Packets;

        /// <summary>
        /// Create a new TOD frame by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public TODFrame(BinaryReader br)
        {
            FrameSize = br.ReadUInt16();
            NumberOfPackets = br.ReadUInt16();
            FrameNumber = br.ReadUInt32();
            Packets = new TODPacket[NumberOfPackets];
            for (int i = 0; i < NumberOfPackets; i++)
            {
                Packets[i] = new TODPacket(br);
            }
        }

        /// <summary>
        /// Write this TOD frame to a binary stream.
        /// </summary>
        /// <param name="bw"></param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(FrameSize);
            bw.Write(NumberOfPackets);
            bw.Write(FrameNumber);
            foreach (var packet in Packets)
            {
                packet.Write(bw);
            }
        }
    }

    /// <summary>
    /// A TOD packet. A packet describes how to do one specific action in a frame, like a transformation.
    /// </summary>
    public struct TODPacket : IWriteable
    {
        /// <summary>
        /// The different types of TOD packets there can be.
        /// </summary>
        public enum PacketTypes
        {
            Attribute,
            Coordinate,
            TMDDataID,
            ParentObjectID,
            MatrixValue,
            TMDData,
            LightSource,
            Camera,
            ObjectControl
        }

        /// <summary>
        /// The ID of the object to act on.
        /// </summary>
        public readonly ushort ObjectID;

        /// <summary>
        /// The type of this packet.
        /// </summary>
        public PacketTypes PacketType => (PacketTypes)(_typeAndFlag & 0xF);

        /// <summary>
        /// The meaning of this flag varies from packet to packet.
        /// </summary>
        public int Flag => (_typeAndFlag >> 4) & 0xF;

        /// <summary>
        /// The length of this packet in words.
        /// </summary>
        public readonly byte PacketLength;

        /// <summary>
        /// The actual data of this packet.
        /// </summary>
        public TODPacketData Data { get; private set; }

        private readonly byte _typeAndFlag;

        /// <summary>
        /// Create a new TOD packet by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public TODPacket(BinaryReader br)
        {
            ObjectID = br.ReadUInt16();
            _typeAndFlag = br.ReadByte();
            PacketLength = br.ReadByte();
            Data = createTODPacketData(br, (PacketTypes)(_typeAndFlag & 0xF), ((_typeAndFlag >> 4) & 0xF));
        }

        /// <summary>
        /// Create a some TOD packet data by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <param name="type">The type of this packet.</param>
        /// <param name="flag">The flag of the packet.</param>
        /// <returns>The new packet.</returns>
        /// <exception cref="NotSupportedException">If the packet type was unsupported.</exception>
        private static TODPacketData createTODPacketData(BinaryReader br, PacketTypes type, int flag)
        {
            switch (type)
            {
                case PacketTypes.Attribute:
                {
                    return new TODAttributePacketData(br, flag);
                }
                case PacketTypes.Coordinate:
                {
                    return new TODCoordinatePacketData(br, flag);
                }
                case PacketTypes.TMDData:
                {
                    throw new NotSupportedException("PacketType TMDData is not currently supported!");
                }
                case PacketTypes.TMDDataID:
                case PacketTypes.ParentObjectID:
                {
                    return new TODObjectIDPacketData(br, flag);
                }
                case PacketTypes.MatrixValue:
                {
                    return new TODMatrixPacketData(br, flag);
                }
                case PacketTypes.LightSource:
                {
                    return new TODLightSourcePacketData(br, flag);
                }
                case PacketTypes.Camera:
                {
                    return new TODCameraPacketData(br, flag);
                }
                case PacketTypes.ObjectControl:
                {
                    return new TODObjectControlPacketData(flag);
                }
                default:
                {
                    throw new NotSupportedException($"Packet type 0x{(int)type:X} is not supported");
                }
            }
        }

        /// <summary>
        /// Write this TOD packet to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(ObjectID);
            bw.Write(_typeAndFlag);
            bw.Write(PacketLength);
            Data.Write(bw);
        }
    }
}
