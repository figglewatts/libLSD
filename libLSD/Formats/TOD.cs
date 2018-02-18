using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Exceptions;
using libLSD.Interfaces;
using libLSD.Types;

namespace libLSD.Formats
{
	public struct TOD : IWriteable
	{
		public readonly TODHeader Header;
		public readonly TODFrame[] Frames;

		public TOD(BinaryReader br)
		{
			Header = new TODHeader(br);
			Frames = new TODFrame[Header.NumberOfFrames - 1];
			for (int i = 0; i < Header.NumberOfFrames - 1; i++)
			{
				Frames[i] = new TODFrame(br);
			}
		}

	    public void Write(BinaryWriter bw)
	    {
	        Header.Write(bw);
	        foreach (var frame in Frames)
	        {
	            frame.Write(bw);
	        }
	    }
	}

	public struct TODHeader : IWriteable
	{
		public readonly byte ID;
		public readonly byte Version;

		/// <summary>
		/// Time in which one frame is displayed, in units of ticks.
		/// 1 tick is equal to 1/60 seconds.
		/// </summary>
		public readonly ushort Resolution;

		public readonly uint NumberOfFrames;

		public TODHeader(BinaryReader br)
		{
			ID = br.ReadByte();

			if (ID != 0x50)
				throw new BadFormatException("TOD did not have correct magic number!");

			Version = br.ReadByte();
			Resolution = br.ReadUInt16();
			NumberOfFrames = br.ReadUInt32();
		}

	    public void Write(BinaryWriter bw)
	    {
	        bw.Write(ID);
            bw.Write(Version);
            bw.Write(Resolution);
            bw.Write(NumberOfFrames);
	    }
	}

	public struct TODFrame : IWriteable
	{
		public readonly ushort FrameSize;
		public readonly ushort NumberOfPackets;
		public readonly uint FrameNumber;
		public TODPacket[] Packets;

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

	public struct TODPacket : IWriteable
	{
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

		public readonly ushort ObjectID;
		public PacketTypes PacketType => (PacketTypes)(_typeAndFlag & 0xF);
		public int Flag => (_typeAndFlag >> 4) & 0xF;
		public readonly byte PacketLength;
		public TODPacketData Data { get; private set; }

		private readonly byte _typeAndFlag;

		public TODPacket(BinaryReader br)
		{
			ObjectID = br.ReadUInt16();
			_typeAndFlag = br.ReadByte();
			PacketLength = br.ReadByte();
			Data = createTODPacketData(br, (PacketTypes)(_typeAndFlag & 0xF), ((_typeAndFlag >> 4) & 0xF));
		}

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

	    public void Write(BinaryWriter bw)
	    {
	        bw.Write(ObjectID);
            bw.Write(_typeAndFlag);
            bw.Write(PacketLength);
            Data.Write(bw);
	    }
	}

	#region Packet Data

	public abstract class TODPacketData : IWriteable
	{
		public enum PacketDataType
		{
			Absolute,
			Differential
		}

		protected int Flag;

		protected TODPacketData(int flag)
		{
			Flag = flag;
		}

	    public abstract void Write(BinaryWriter bw);
	}

	public class TODAttributePacketData : TODPacketData
	{
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

		public DifferenceMask Mask => (DifferenceMask)_mask;
		public readonly uint NewValues;

		private readonly uint _mask;

		public TODAttributePacketData(BinaryReader br, int flag)
			: base(flag)
		{
			_mask = br.ReadUInt32();
			NewValues = br.ReadUInt32();
		}

	    public override void Write(BinaryWriter bw)
	    {
	        bw.Write(_mask);
	        bw.Write(NewValues);
	    }
	}

	public class TODCoordinatePacketData : TODPacketData
	{
		public PacketDataType MatrixType => (PacketDataType) (Flag & 1);
		public bool HasRotation => ((Flag >> 1) & 0x1) == 1;
		public bool HasScale => ((Flag >> 2) & 0x1) == 1;
		public bool HasTranslation => ((Flag >> 3) & 0x1) == 1;

	    public readonly int RotX;
	    public readonly int RotY;
	    public readonly int RotZ;
	    public readonly short ScaleX;
	    public readonly short ScaleY;
	    public readonly short ScaleZ;
		public readonly int TransX;
		public readonly int TransY;
		public readonly int TransZ;

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

	// used for packet types 3 and 4, TMD Data ID, and Parent Object ID
	public class TODObjectIDPacketData : TODPacketData
	{
		public readonly ushort ObjectID;

		public TODObjectIDPacketData(BinaryReader br, int flag)
			: base(flag)
		{
			ObjectID = br.ReadUInt16();
			br.ReadBytes(2); // skip 2 bytes
		}

	    public override void Write(BinaryWriter bw)
	    {
	        bw.Write(ObjectID);
            bw.Write((short)0);
	    }
	}

	public class TODMatrixPacketData : TODPacketData
	{
		public FixedPoint16Bit[,] Rotation;
		public FixedPoint32Bit[] Translation;

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

	public class TODLightSourcePacketData : TODPacketData
	{
		public PacketDataType DataType => (PacketDataType) (Flag & 0x1);
		public bool HasDirection => ((Flag >> 1) & 0x1) == 1;
		public bool HasColor => ((Flag >> 2) & 0x1) == 1;

		public readonly FixedPoint32Bit DirX;
		public readonly FixedPoint32Bit DirY;
		public readonly FixedPoint32Bit DirZ;
		public readonly byte[] Color;

		public TODLightSourcePacketData(BinaryReader br, int flag)
			: base(flag)
		{
			DirX = new FixedPoint32Bit(br.ReadBytes(4));
			DirY = new FixedPoint32Bit(br.ReadBytes(4));
			DirZ = new FixedPoint32Bit(br.ReadBytes(4));
			Color = br.ReadBytes(3);
			br.ReadByte(); // skip the last byte
		}

	    public override void Write(BinaryWriter bw)
	    {
	        DirX.Write(bw);
            DirY.Write(bw);
            DirZ.Write(bw);
	        bw.Write(Color);
            bw.Write((byte)0);
	    }
	}

	public class TODCameraPacketData : TODPacketData
	{
		public enum CameraTypes
		{
			PositionAndAngle,
			TranslationAndRotation
		}

		public CameraTypes CameraType => (CameraTypes) (Flag & 0x1);
		public PacketDataType DataType => (PacketDataType) ((Flag >> 1) & 0x1);
		public bool HasPosAndRef => ((Flag >> 2) & 0x1) == 1;
		public bool HasRotation => ((Flag >> 2) & 0x1) == 1;
		public bool HasZAngle => ((Flag >> 3) & 0x1) == 1;
		public bool HasTranslation => ((Flag >> 3) & 0x1) == 1;

		public readonly FixedPoint32Bit TransX;
		public readonly FixedPoint32Bit TransY;
		public readonly FixedPoint32Bit TransZ;
		public readonly FixedPoint32Bit RotX;
		public readonly FixedPoint32Bit RotY;
		public readonly FixedPoint32Bit RotZ;
		public readonly FixedPoint32Bit ZAngle;

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

	public class TODObjectControlPacketData : TODPacketData
	{
		public enum ObjectControlType
		{
			Create,
			Kill
		}

		public ObjectControlType ObjectControl => (ObjectControlType)(Flag & 0x1);
		
		public TODObjectControlPacketData(int flag)
			: base(flag) { }

	    public override void Write(BinaryWriter bw)
	    {
	        // intentionally empty
	    }
	}



	#endregion
}
