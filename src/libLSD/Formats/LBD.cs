using System;
using System.IO;
using libLSD.Interfaces;
using libLSD.Util;

namespace libLSD.Formats
{
    /// <summary>
    /// An LBD is a single chunk of a level in LSD. Levels consist of a bunch of chunks arranged in a certain way.
    /// The chunks can either be arranged horizontally or vertically. If they are arranged vertically then they are
    /// all just put on the same location. Each chunk tile uses its TileHeight to offset above.
    /// If they are arranged horizontally, it is in a honeycomb-like pattern.
    /// 
    /// An LBD is used to store a bunch of things:
    /// - Chunk tiles in a TMD file
    /// - Chunk tile layout
    /// - Chunk interactive objects in an MML file (this contains MOM files).
    /// </summary>
    [Serializable]
    public struct LBD : IWriteable
    {
        /// <summary>
        /// The header of the LBD file.
        /// </summary>
        public readonly LBDHeader Header;

        /// <summary>
        /// The tile layout of this chunk.
        /// </summary>
        public readonly Serializable2DArray<LBDTile> TileLayout;

        /// <summary>
        /// The array of extra tiles.
        /// </summary>
        public readonly LBDTile[] ExtraTiles;

        /// <summary>
        /// TMD file storing chunk tiles as objects.
        /// </summary>
        public readonly TMD Tiles;

        /// <summary>
        /// Optional MML file storing interactive objects and their animations.
        /// </summary>
        public readonly MML? MML;

        /// <summary>
        /// Read an LBD file from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public LBD(BinaryReader br)
        {
            Header = new LBDHeader(br);

            TileLayout = new Serializable2DArray<LBDTile>(Header.TileWidth, Header.TileHeight);
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
                ExtraTiles[i] = new LBDTile(br, Header.AddressOffset, Header.ExtraTilesOffset);
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

        /// <summary>
        /// Write this LBD file to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            Header.Write(bw);
            for (int y = 0; y < Header.TileHeight; y++)
            {
                for (int x = 0; x < Header.TileWidth; x++)
                {
                    TileLayout[x, y].Write(bw);
                }
            }

            foreach (LBDTile tile in ExtraTiles)
            {
                tile.Write(bw);
            }

            bw.BaseStream.Seek(Header.TilesTMDOffset + Header.AddressOffset, SeekOrigin.Begin);
            Tiles.Write(bw);

            if (Header.HasMML)
            {
                bw.BaseStream.Seek(Header.MMLOffset, SeekOrigin.Begin);
                MML?.Write(bw);
            }
        }
    }

    /// <summary>
    /// The header of an LBD file, containing various pieces of metadata about the file.
    /// </summary>
    [Serializable]
    public struct LBDHeader : IWriteable
    {
        /// <summary>
        /// The version of the LBD file format. Always 0x1.
        /// </summary>
        public readonly ushort Version;

        /// <summary>
        /// Whether or not this LBD file has an MML file with interactive objects and animations attached.
        /// </summary>
        public readonly bool HasMML;

        /// <summary>
        /// Always 0x18. This gets added to other memory offsets for some reason. Not sure why.
        /// </summary>
        public readonly uint AddressOffset;

        /// <summary>
        /// The offset to the TMD containing tiles. AddressOffset needs to be added to it.
        /// </summary>
        public readonly uint TilesTMDOffset;

        /// <summary>
        /// The length of the tiles TMD in bytes.
        /// </summary>
        public readonly uint TilesTMDLength;

        /// <summary>
        /// The offset to the MML file. This doesn't need the AddressOffset.
        /// **Note:** If the MML file doesn't exist then this points out of bounds.
        /// </summary>
        public readonly uint MMLOffset;

        /// <summary>
        /// The length of the MML file in bytes. This is 0 if the MML file is not present.
        /// </summary>
        public readonly uint MMLLength;

        /// <summary>
        /// An unknown value. It's 0x4C in every LBD file in the game.
        /// </summary>
        public readonly ushort UnknownValue; // always 0x4C

        /// <summary>
        /// The number of extra tiles in the extra tiles array.
        /// </summary>
        public readonly ushort NumberOfExtraTiles;

        /// <summary>
        /// The width of this chunk in tiles. It's 20 for every LBD file in the game.
        /// </summary>
        public readonly ushort TileWidth;

        /// <summary>
        /// The height of this chunk in tiles. It's 20 for every LBD file in the game.
        /// </summary>
        public readonly ushort TileHeight;

        /// <summary>
        /// The offset to the extra tiles array. This isn't present in the header, and is instead calculated when
        /// an LBD header is created.
        /// </summary>
        public readonly int ExtraTilesOffset; // this is a calculated value

        /// <summary>
        /// The length of an LBD header.
        /// </summary>
        public const int Length = 0x20;

        /// <summary>
        /// Read an LBD header from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
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
            ExtraTilesOffset = Length + (TileWidth * TileHeight) * LBDTile.Length;
        }

        /// <summary>
        /// Write this LBD header to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(Version);
            bw.Write(HasMML ? (ushort)1 : (ushort)0);
            bw.Write(AddressOffset);
            bw.Write(TilesTMDOffset);
            bw.Write(TilesTMDLength);
            bw.Write(MMLOffset);
            bw.Write(MMLLength);
            bw.Write(UnknownValue);
            bw.Write(NumberOfExtraTiles);
            bw.Write(TileWidth);
            bw.Write(TileHeight);
        }
    }

    /// <summary>
    /// A single tile in an LBD chunk. References a TMD object in the tiles TMD and has a bunch of metadata describing
    /// rotation, height, and extra tiles.
    /// </summary>
    [Serializable]
    public struct LBDTile : IWriteable
    {
        /// <summary>
        /// The direction of a tile. Used to control rotating tiles.
        /// </summary>
        public enum TileDirections
        {
            Deg0 = 0,
            Deg90 = 1,
            Deg180 = 2,
            Deg270 = 3
        }

        /// <summary>
        /// True if the tile is visible, false otherwise.
        /// </summary>
        public readonly bool DrawTile;

        /// <summary>
        /// An unknown flag that is either 0 or 1.
        /// </summary>
        public readonly bool SolidTile;

        /// <summary>
        /// The object number of this tile in the tiles TMD file.
        /// </summary>
        public readonly ushort TileType;

        public int Footstep => FootstepSoundAndCollision & 0x7F;
        
        public bool Collision => (FootstepSoundAndCollision >> 7) > 0;
        
        /// <summary>
        /// Unknown value hypothesized to be to do with footstep sound and collisions. Potentially a bitfield.
        /// </summary>
        public readonly byte FootstepSoundAndCollision;

        /// <summary>
        /// The direction (rotation) of this tile.
        /// </summary>
        public readonly TileDirections TileDirection;

        /// <summary>
        /// The height offset of this tile.
        /// </summary>
        public readonly short TileHeight;

        /// <summary>
        /// The index into the extra tiles array of the extra tile to put on this tile.
        /// </summary>
        public readonly int ExtraTileIndex;

        /// <summary>
        /// The length of a TMD tile.
        /// </summary>
        public const int Length = 0xC;

        private uint _rawExtraTileOffset;

        /// <summary>
        /// Create an LBD tile from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <param name="addressOffset">The AddressOffset from the LBDHeader.</param>
        /// <param name="extraTilesTop">The offset of the extra tiles array in the LBD file.</param>
        public LBDTile(BinaryReader br, uint addressOffset, int extraTilesTop)
        {
            DrawTile = br.ReadByte() == 1;
            SolidTile = br.ReadByte() == 1;
            TileType = br.ReadUInt16();
            FootstepSoundAndCollision = br.ReadByte();
            TileDirection = (TileDirections)br.ReadByte();
            TileHeight = br.ReadInt16();
            _rawExtraTileOffset = br.ReadUInt32();

            if (_rawExtraTileOffset == 0)
            {
                ExtraTileIndex = -1;
            }
            else
            {
                ExtraTileIndex = (int)((_rawExtraTileOffset + addressOffset) - extraTilesTop) / Length;
            }
        }

        /// <summary>
        /// Write this LBD tile to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(DrawTile);
            bw.Write(SolidTile);
            bw.Write(TileType);
            bw.Write(FootstepSoundAndCollision);
            bw.Write((byte)TileDirection);
            bw.Write(TileHeight);
            bw.Write(_rawExtraTileOffset);
        }
    }
}
