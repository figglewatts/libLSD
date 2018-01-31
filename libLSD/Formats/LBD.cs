using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Formats
{
	public struct LBD
	{
		public readonly LBDHeader Header;
		public readonly LBDTile[,] TileLayout;
		public readonly LBDTile[] ExtraTiles;
		public readonly TMD Tiles;
		public readonly MML? MML;

		public LBD(BinaryReader br)
		{
			Header = new LBDHeader(br);

			TileLayout = new LBDTile[Header.TileWidth, Header.TileHeight];
			for (int y = 0; y < Header.TileHeight; y++)
			{
				for (int x = 0; x < Header.TileWidth; x++)
				{
					TileLayout[x, y] = new LBDTile(br, Header.AddressOffset, Header.ExtraTilesOffset);
				}
			}

			ExtraTiles = new LBDTile[Header.NumberOfExtraTiles];
			for (int i = 0; i < Header.NumberOfExtraTiles; i++)
			{
				ExtraTiles[i] = new LBDTile(br, Header.AddressOffset);
			}

			br.BaseStream.Seek(Header.TilesTMDOffset + Header.AddressOffset, SeekOrigin.Begin);
			Tiles = new TMD(br);

			MML = null;
			if (Header.HasMML)
			{
				br.BaseStream.Seek(Header.MMLOffset, SeekOrigin.Begin);
				MML = new MML(br);
			}
		}
	}

	public struct LBDHeader
	{
		public readonly ushort Version;
		public readonly bool HasMML;
		public readonly uint AddressOffset;
		public readonly uint TilesTMDOffset;
		public readonly uint TilesTMDLength;
		public readonly uint MMLOffset;
		public readonly uint MMLLength;
		public readonly ushort UnknownValue; // always 0x4C
		public readonly ushort NumberOfExtraTiles;
		public readonly ushort TileWidth;
		public readonly ushort TileHeight;
		public readonly int ExtraTilesOffset; // this is a calculated value

		public const int Length = 0x20;

		public LBDHeader(BinaryReader br)
		{
			Version = br.ReadUInt16();
			HasMML = (br.ReadUInt16() == 1);
			AddressOffset = br.ReadUInt32();
			TilesTMDOffset = br.ReadUInt32();
			TilesTMDLength = br.ReadUInt32();
			MMLOffset = br.ReadUInt32();
			MMLLength = br.ReadUInt32();
			UnknownValue = br.ReadUInt16();
			NumberOfExtraTiles = br.ReadUInt16();
			TileWidth = br.ReadUInt16();
			TileHeight = br.ReadUInt16();
			ExtraTilesOffset = LBDHeader.Length + (TileWidth * TileHeight) * LBDTile.Length;
		}
	}

	public struct LBDTile
	{
		public enum TileDirections
		{
			Deg0 = 0,
			Deg90 = 1,
			Deg180 = 2,
			Deg270 = 3
		}

		public readonly bool DrawTile;
		public readonly byte UnknownFlag;
		public readonly ushort TileType;
		public readonly byte FootstepSoundAndCollision;
		public readonly TileDirections TileDirection;
		public readonly short TileHeight;
		public readonly int ExtraTileIndex;

		public const int Length = 0xC;

		public LBDTile(BinaryReader br, uint addressOffset, int extraTilesTop = 0)
		{
			DrawTile = br.ReadByte() == 1;
			UnknownFlag = br.ReadByte();
			TileType = br.ReadUInt16();
			FootstepSoundAndCollision = br.ReadByte();
			TileDirection = (TileDirections) br.ReadByte();
			TileHeight = br.ReadInt16();
			uint rawExtraTileOffset = br.ReadUInt32();

			if (rawExtraTileOffset == 0)
			{
				ExtraTileIndex = -1;
			}
			else
			{
				ExtraTileIndex = (int)((rawExtraTileOffset + addressOffset) - extraTilesTop) / LBDTile.Length;
			}
		}
	}
}