using System;
using System.IO;
using libLSD.Interfaces;
using libLSD.Types;

namespace libLSD.Formats.Packets
{
    /// <summary>
    /// Abstract base class for TOD packet data.
    /// </summary>
    public abstract class TODPacketData : IWriteable
    {
        /// <summary>
        /// The type of packet data. It can either be absolute, where the value is set to whatever is contained in the
        /// packet, or it can be differential, where the packet contains delta values.
        /// </summary>
        public enum PacketDataType
        {
            Absolute,
            Differential
        }

        /// <summary>
        /// The flag of the packet.
        /// </summary>
        protected int Flag;

        /// <summary>
        /// Create packet data with a given flag.
        /// </summary>
        /// <param name="flag">The flag.</param>
        protected TODPacketData(int flag) { Flag = flag; }

        /// <summary>
        /// Write this packet data to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public abstract void Write(BinaryWriter bw);
    }

    /// <summary>
    /// This packet sets attributes about an object.
    /// </summary>
    public class TODAttributePacketData : TODPacketData
    {
        /// <summary>
        /// A mask which indicates the section that changes value.
        /// </summary>
        [Flags]
        public enum DifferenceMask : uint
        {
            MaterialDamping = 0b11,
            LightingModeFog = 1 << 2,
            LightingModeMaterial = 1 << 3,
            LightingMode = 1 << 4,
            LightSource = 1 << 5,
            NearZOverflow = 1 << 6,
            BackClipping = 1 << 7,
            SemiTransparencyType = 0x30000000,
            SemiTransparencyToggle = 1 << 29,
            Display = 1 << 30
        }

        /// <summary>
        /// The difference mask of this packet.
        /// </summary>
        public DifferenceMask Mask => (DifferenceMask)_mask;

        /// <summary>
        /// The new data for the masked values.
        /// </summary>
        public readonly uint NewValues;

        private readonly uint _mask;

        /// <summary>
        /// Create this packet from a binary stream.
        /// </summary>
        /// <param name="br">The stream.</param>
        /// <param name="flag">The packet flag.</param>
        public TODAttributePacketData(BinaryReader br, int flag)
            : base(flag)
        {
            _mask = br.ReadUInt32();
            NewValues = br.ReadUInt32();
        }

        /// <summary>
        /// Write this packet to a stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public override void Write(BinaryWriter bw)
        {
            bw.Write(_mask);
            bw.Write(NewValues);
        }
    }

    /// <summary>
    /// This packet sets coordinates of the object via rotation, scale, and translation.
    /// </summary>
    public class TODCoordinatePacketData : TODPacketData
    {
        /// <summary>
        /// Whether the transformation matrix is absolute or differential from prior frame.
        /// </summary>
        public PacketDataType MatrixType => (PacketDataType)(Flag & 1);

        /// <summary>
        /// If we have a rotation transformation to apply.
        /// </summary>
        public bool HasRotation => ((Flag >> 1) & 0x1) == 1;

        /// <summary>
        /// If we have a scale transformation to apply.
        /// </summary>
        public bool HasScale => ((Flag >> 2) & 0x1) == 1;

        /// <summary>
        /// If we have a translation transformation to apply.
        /// </summary>
        public bool HasTranslation => ((Flag >> 3) & 0x1) == 1;

        /// <summary>
        /// The rotation around X.
        /// </summary>
        public readonly int RotX;

        /// <summary>
        /// The rotation around Y.
        /// </summary>
        public readonly int RotY;

        /// <summary>
        /// The rotation around Z.
        /// </summary>
        public readonly int RotZ;

        /// <summary>
        /// The X scale.
        /// </summary>
        public readonly short ScaleX;

        /// <summary>
        /// The Y scale.
        /// </summary>
        public readonly short ScaleY;

        /// <summary>
        /// The Z scale.
        /// </summary>
        public readonly short ScaleZ;

        /// <summary>
        /// The X translation.
        /// </summary>
        public readonly int TransX;

        /// <summary>
        /// The Y translation.
        /// </summary>
        public readonly int TransY;

        /// <summary>
        /// The Z translation.
        /// </summary>
        public readonly int TransZ;

        /// <summary>
        /// Create this packet from a stream.
        /// </summary>
        /// <param name="br">The stream.</param>
        /// <param name="flag">Packet flag.</param>
        public TODCoordinatePacketData(BinaryReader br, int flag)
            : base(flag)
        {
            if (HasRotation)
            {
                RotX = br.ReadInt32();
                RotY = br.ReadInt32();
                RotZ = br.ReadInt32();
            }

            if (HasScale)
            {
                ScaleX = br.ReadInt16();
                ScaleY = br.ReadInt16();
                ScaleZ = br.ReadInt16();
                br.ReadBytes(2); // skip 2 bytes
            }

            if (HasTranslation)
            {
                TransX = br.ReadInt32();
                TransY = br.ReadInt32();
                TransZ = br.ReadInt32();
            }
        }

        /// <summary>
        /// Write this packet to a stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public override void Write(BinaryWriter bw)
        {
            if (HasRotation)
            {
                bw.Write(RotX);
                bw.Write(RotY);
                bw.Write(RotZ);
            }

            if (HasScale)
            {
                bw.Write(ScaleX);
                bw.Write(ScaleY);
                bw.Write(ScaleZ);
                bw.Write((short)0);
            }

            if (HasTranslation)
            {
                bw.Write(TransX);
                bw.Write(TransY);
                bw.Write(TransZ);
            }
        }
    }

    /// <summary>
    /// Sets an object ID.
    /// </summary>
    public class TODObjectIDPacketData : TODPacketData
    {
        /// <summary>
        /// The object ID.
        /// </summary>
        public readonly ushort ObjectID;

        /// <summary>
        /// Create this packet from a binary stream.
        /// </summary>
        /// <param name="br">The stream.</param>
        /// <param name="flag">Packet flag.</param>
        public TODObjectIDPacketData(BinaryReader br, int flag)
            : base(flag)
        {
            ObjectID = br.ReadUInt16();
            br.ReadBytes(2); // skip 2 bytes
        }

        /// <summary>
        /// Write this packet to a stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public override void Write(BinaryWriter bw)
        {
            bw.Write(ObjectID);
            bw.Write((short)0);
        }
    }

    /// <summary>
    /// A matrix is stored in packet data.
    /// </summary>
    public class TODMatrixPacketData : TODPacketData
    {
        /// <summary>
        /// Rotation matrix.
        /// </summary>
        public FixedPoint16Bit[,] Rotation;

        /// <summary>
        /// Translation matrix.
        /// </summary>
        public FixedPoint32Bit[] Translation;

        /// <summary>
        /// Create this packet from a stream.
        /// </summary>
        /// <param name="br">The stream.</param>
        /// <param name="flag">Packet flag.</param>
        public TODMatrixPacketData(BinaryReader br, int flag)
            : base(flag)
        {
            Rotation = new FixedPoint16Bit[3, 3];
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Rotation[y, x] = new FixedPoint16Bit(br.ReadBytes(2));
                }
            }

            br.ReadBytes(2); // skip 2 bytes

            Translation = new FixedPoint32Bit[3];
            for (int i = 0; i < 3; i++)
            {
                Translation[i] = new FixedPoint32Bit(br.ReadBytes(4));
            }
        }

        /// <summary>
        /// Write this packet to a stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public override void Write(BinaryWriter bw)
        {
            foreach (var rot in Rotation)
            {
                rot.Write(bw);
            }

            bw.Write((short)0);

            foreach (var trans in Translation)
            {
                trans.Write(bw);
            }
        }
    }

    /// <summary>
    /// Sets light source information.
    /// </summary>
    public class TODLightSourcePacketData : TODPacketData
    {
        /// <summary>
        /// Whether this is absolute or relative data.
        /// </summary>
        public PacketDataType DataType => (PacketDataType)(Flag & 0x1);

        /// <summary>
        /// Whether or not this has direction data.
        /// </summary>
        public bool HasDirection => ((Flag >> 1) & 0x1) == 1;

        /// <summary>
        /// Whether or not this has color data.
        /// </summary>
        public bool HasColor => ((Flag >> 2) & 0x1) == 1;

        /// <summary>
        /// X component of direction vector.
        /// </summary>
        public readonly FixedPoint32Bit DirX;

        /// <summary>
        /// Y component of direction vector.
        /// </summary>
        public readonly FixedPoint32Bit DirY;

        /// <summary>
        /// Z component of direction vector.
        /// </summary>
        public readonly FixedPoint32Bit DirZ;

        /// <summary>
        /// Color information.
        /// </summary>
        public readonly byte[] Color;

        /// <summary>
        /// Create this packet from a stream.
        /// </summary>
        /// <param name="br">The stream.</param>
        /// <param name="flag">Packet flag.</param>
        public TODLightSourcePacketData(BinaryReader br, int flag)
            : base(flag)
        {
            DirX = new FixedPoint32Bit(br.ReadBytes(4));
            DirY = new FixedPoint32Bit(br.ReadBytes(4));
            DirZ = new FixedPoint32Bit(br.ReadBytes(4));
            Color = br.ReadBytes(3);
            br.ReadByte(); // skip the last byte
        }

        /// <summary>
        /// Write this packet to a stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public override void Write(BinaryWriter bw)
        {
            DirX.Write(bw);
            DirY.Write(bw);
            DirZ.Write(bw);
            bw.Write(Color);
            bw.Write((byte)0);
        }
    }

    /// <summary>
    /// Sets viewpoint location.
    /// </summary>
    public class TODCameraPacketData : TODPacketData
    {
        /// <summary>
        /// Types of camera information.
        /// </summary>
        public enum CameraTypes
        {
            PositionAndAngle,
            TranslationAndRotation
        }

        /// <summary>
        /// The type of camera information in this packet.
        /// </summary>
        public CameraTypes CameraType => (CameraTypes)(Flag & 0x1);

        /// <summary>
        /// Whether this is absolute or relative data.
        /// </summary>
        public PacketDataType DataType => (PacketDataType)((Flag >> 1) & 0x1);

        /// <summary>
        /// If we have position and reference information.
        /// </summary>
        public bool HasPosAndRef => ((Flag >> 2) & 0x1) == 1;

        /// <summary>
        /// If we have rotation information.
        /// </summary>
        public bool HasRotation => ((Flag >> 2) & 0x1) == 1;

        /// <summary>
        /// If we have Z angle information.
        /// </summary>
        public bool HasZAngle => ((Flag >> 3) & 0x1) == 1;

        /// <summary>
        /// If we have translation information.
        /// </summary>
        public bool HasTranslation => ((Flag >> 3) & 0x1) == 1;

        /// <summary>
        /// Translation X;
        /// </summary>
        public readonly FixedPoint32Bit TransX;

        /// <summary>
        /// Translation Y.
        /// </summary>
        public readonly FixedPoint32Bit TransY;

        /// <summary>
        /// Translation Z.
        /// </summary>
        public readonly FixedPoint32Bit TransZ;

        /// <summary>
        /// Rotation around X.
        /// </summary>
        public readonly FixedPoint32Bit RotX;

        /// <summary>
        /// Rotation around Y.
        /// </summary>
        public readonly FixedPoint32Bit RotY;

        /// <summary>
        /// Rotation around Z.
        /// </summary>
        public readonly FixedPoint32Bit RotZ;

        /// <summary>
        /// The Z angle.
        /// </summary>
        public readonly FixedPoint32Bit ZAngle;

        /// <summary>
        /// Create this packet from a stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <param name="flag">Packet flag.</param>
        public TODCameraPacketData(BinaryReader br, int flag)
            : base(flag)
        {
            if (CameraType == CameraTypes.PositionAndAngle)
            {
                if (HasPosAndRef)
                {
                    TransX = new FixedPoint32Bit(br.ReadBytes(4));
                    TransY = new FixedPoint32Bit(br.ReadBytes(4));
                    TransZ = new FixedPoint32Bit(br.ReadBytes(4));
                    RotX = new FixedPoint32Bit(br.ReadBytes(4));
                    RotY = new FixedPoint32Bit(br.ReadBytes(4));
                    RotZ = new FixedPoint32Bit(br.ReadBytes(4));
                }

                if (HasZAngle)
                {
                    ZAngle = new FixedPoint32Bit(br.ReadBytes(4));
                }
            }
            else
            {
                if (HasRotation)
                {
                    RotX = new FixedPoint32Bit(br.ReadBytes(4));
                    RotY = new FixedPoint32Bit(br.ReadBytes(4));
                    RotZ = new FixedPoint32Bit(br.ReadBytes(4));
                }

                if (HasTranslation)
                {
                    TransX = new FixedPoint32Bit(br.ReadBytes(4));
                    TransY = new FixedPoint32Bit(br.ReadBytes(4));
                    TransZ = new FixedPoint32Bit(br.ReadBytes(4));
                }
            }
        }

        /// <summary>
        /// Write this packet to a stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public override void Write(BinaryWriter bw)
        {
            if (CameraType == CameraTypes.PositionAndAngle)
            {
                if (HasPosAndRef)
                {
                    TransX.Write(bw);
                    TransY.Write(bw);
                    TransZ.Write(bw);
                    RotX.Write(bw);
                    RotY.Write(bw);
                    RotZ.Write(bw);
                }

                if (HasZAngle)
                {
                    ZAngle.Write(bw);
                }
            }
            else
            {
                if (HasRotation)
                {
                    RotX.Write(bw);
                    RotY.Write(bw);
                    RotZ.Write(bw);
                }

                if (HasTranslation)
                {
                    TransX.Write(bw);
                    TransY.Write(bw);
                    TransZ.Write(bw);
                }
            }
        }
    }

    /// <summary>
    /// Controls the object.
    /// </summary>
    public class TODObjectControlPacketData : TODPacketData
    {
        /// <summary>
        /// The types of control.
        /// </summary>
        public enum ObjectControlType
        {
            Create,
            Kill
        }

        /// <summary>
        /// What kind of object control are we doing?
        /// </summary>
        public ObjectControlType ObjectControl => (ObjectControlType)(Flag & 0x1);

        /// <summary>
        /// Create this packet.
        /// </summary>
        /// <param name="flag">Packet flag.</param>
        public TODObjectControlPacketData(int flag)
            : base(flag) { }

        /// <summary>
        /// Write this packet to a stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public override void Write(BinaryWriter bw)
        {
            // intentionally empty
        }
    }
}
