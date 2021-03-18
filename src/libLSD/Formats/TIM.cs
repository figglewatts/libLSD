using System;
using System.IO;
using libLSD.Exceptions;
using libLSD.Interfaces;
using libLSD.Types;

namespace libLSD.Formats
{
    /// <summary>
    /// A TIM file is a PSX texture image. It essentially stores colour data, supporting a variety of formats.
    /// </summary>
    public struct TIM : IWriteable
    {
        /// <summary>
        /// The header of this TIM file.
        /// </summary>
        public readonly TIMHeader Header;

        /// <summary>
        /// The optional colour lookup table of this TIM file.
        /// </summary>
        public readonly TIMColorLookup? ColorLookup;

        /// <summary>
        /// The pixel data of this TIM file.
        /// </summary>
        public readonly TIMPixelData PixelData;

        /// <summary>
        /// Create a new TIM by reading it from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public TIM(BinaryReader br)
        {
            Header = new TIMHeader(br);

            ColorLookup = null;
            if (Header.HasCLUT)
            {
                ColorLookup = new TIMColorLookup(br, Header);
            }

            PixelData = new TIMPixelData(br);
        }

        /// <summary>
        /// Write this TIM file to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            Header.Write(bw);
            if (Header.HasCLUT)
            {
                ColorLookup?.Write(bw);
            }

            PixelData.Write(bw);
        }

        /// <summary>
        /// Get the raw pixel data of this TIM file.
        /// </summary>
        /// <returns>A 2D array of IColor containing raw pixel data.</returns>
        /// <exception cref="NotSupportedException">If this TIM uses the 'Mixed' pixel mode. I haven't figured out
        /// how to load it yet.</exception>
        public IColor[,] GetImage()
        {
            switch (Header.PixelMode)
            {
                case TIMHeader.PixelModes.CLUT4Bit:
                {
                    return GetImageCLUT4Bit();
                }

                case TIMHeader.PixelModes.CLUT8Bit:
                {
                    return GetImageCLUT8Bit();
                }

                case TIMHeader.PixelModes.Direct15Bit:
                {
                    return GetImageDirect15Bit();
                }

                case TIMHeader.PixelModes.Direct24Bit:
                {
                    return GetImageDirect24Bit();
                }

                default:
                {
                    throw new NotSupportedException("Mixed pixel mode in TIM is not supported!");
                }
            }
        }

        /// <summary>
        /// Get the pixel data for a TIM using a 4-bit color lookup table.
        /// </summary>
        /// <param name="clutIndex">The index of the CLUT to get data for. Defaults to 0.</param>
        /// <returns>Pixel data.</returns>
        /// <exception cref="InvalidOperationException">If the TIM file does not have a CLUT.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If clutIndex is out of range.</exception>
        private IColor[,] GetImageCLUT4Bit(int clutIndex = 0)
        {
            if (!Header.HasCLUT || ColorLookup == null)
            {
                throw new InvalidOperationException(
                    $"Unable to get image via CLUT from TIM with pixel mode: '{Header.PixelMode}'");
            }

            if (ColorLookup.Value.CLUTLength - 1 < clutIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(clutIndex),
                    $"Unable to get image using CLUT {clutIndex}, image " +
                    $"only has {ColorLookup.Value.NumberOfCLUTs} CLUTs");
            }

            int imageWidth = PixelData.Width * 4;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageHeight, imageWidth];

            for (int i = 0; i < PixelData.Data.Length; i++)
            {
                ushort data = PixelData.Data[i];
                int x = (i * 4) % imageWidth;
                int y = (i * 4) / imageWidth;
                for (int j = 0; j < 4; j++)
                {
                    int clutLocation = data >> (4 * j) & 0xF;
                    image[y, x + j] = ColorLookup?.Data[clutIndex, clutLocation];
                }
            }

            return image;
        }

        /// <summary>
        /// Get the pixel data for a TIM using an 8-bit color lookup table.
        /// </summary>
        /// <param name="clutIndex">The index of the CLUT to get data for. Defaults to 0.</param>
        /// <returns>Pixel data.</returns>
        /// <exception cref="InvalidOperationException">If the TIM file does not have a CLUT.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If clutIndex is out of range.</exception>
        private IColor[,] GetImageCLUT8Bit(int clutIndex = 0)
        {
            if (!Header.HasCLUT || ColorLookup == null)
            {
                throw new InvalidOperationException(
                    $"Unable to get image via CLUT from TIM with pixel mode: '{Header.PixelMode}'");
            }

            if (ColorLookup.Value.CLUTLength - 1 < clutIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(clutIndex),
                    $"Unable to get image using CLUT {clutIndex}, image " +
                    $"only has {ColorLookup.Value.NumberOfCLUTs} CLUTs");
            }

            int imageWidth = PixelData.Width * 2;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageHeight, imageWidth];
            for (int i = 0; i < PixelData.Data.Length; i++)
            {
                ushort data = PixelData.Data[i];
                int x = (i * 2) % imageWidth;
                int y = (i * 2) / imageWidth;
                for (int j = 0; j < 2; j++)
                {
                    int clutLocation = data >> (8 * j) & 0xFF;
                    image[y, x + j] = ColorLookup?.Data[clutIndex, clutLocation];
                }
            }

            return image;
        }

        /// <summary>
        /// Get the pixel data for a TIM using 15-bit direct color mode.
        /// </summary>
        /// <returns>Pixel data.</returns>
        private IColor[,] GetImageDirect15Bit()
        {
            int imageWidth = PixelData.Width;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageHeight, imageWidth];
            for (int i = 0; i < PixelData.Data.Length; i++)
            {
                int x = i % imageWidth;
                int y = i / imageWidth;
                image[y, x] = new Color16Bit(PixelData.Data[i]);
            }

            return image;
        }

        /// <summary>
        /// Get the pixel data for a TIM using 24-bit direct color mode.
        /// </summary>
        /// <returns>Pixel data.</returns>
        private IColor[,] GetImageDirect24Bit()
        {
            int imageWidth = (PixelData.Width / 3) * 2;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageHeight, imageWidth];
            for (int i = 0; i < PixelData.Data.Length; i += 3)
            {
                int x1 = ((i / 3) * 2) % imageWidth;
                int x2 = (((i + 2) / 3) * 2) % imageWidth;
                int y1 = ((i / 3) * 2) / imageWidth;
                int y2 = (((i + 2) / 3) * 2) / imageWidth;

                ushort data1 = PixelData.Data[i];
                ushort data2 = PixelData.Data[i + 1];
                ushort data3 = PixelData.Data[i + 2];

                int r0 = data1 & 0xFF;
                int g0 = (data1 >> 8) & 0xFF;
                int b0 = data2 & 0xFF;
                int r1 = (data2 >> 8) & 0xFF;
                int g1 = data3 & 0xFF;
                int b1 = (data3 >> 8) & 0xFF;

                image[y1, x1] = new Color24Bit((byte)r0, (byte)g0, (byte)b0);
                image[y2, x2] = new Color24Bit((byte)r1, (byte)g1, (byte)b1);
            }

            return image;
        }
    }

    /// <summary>
    /// The header of a TIM file, containing its ID and flags
    /// </summary>
    public struct TIMHeader : IWriteable
    {
        /// <summary>
        /// The different pixel data modes a TIM file supports.
        /// </summary>
        public enum PixelModes
        {
            CLUT4Bit = 0,
            CLUT8Bit,
            Direct15Bit,
            Direct24Bit,
            Mixed
        }

        /// <summary>
        /// ID value of a TIM file is always 0x10
        /// </summary>
        public uint ID => _fileID & 0xFF;

        /// <summary>
        /// Should always be 0x0
        /// </summary>
        public uint VersionNumber => (_fileID >> 8) & 0xFF;

        /// <summary>
        /// The pixel data mode this TIM is using.
        /// </summary>
        public PixelModes PixelMode => (PixelModes)(_flags & 0b111);

        /// <summary>
        /// Is true if this TIM file uses a CLUT (color lookup table), false otherwise
        /// </summary>
        public bool HasCLUT => ((_flags >> 3) & 0x1) == 1;

        private readonly uint _fileID;
        private readonly uint _flags;

        /// <summary>
        /// Read a TIM header from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <exception cref="BadFormatException">If the TIM did not have the correct ID.</exception>
        /// <exception cref="NotSupportedException">If the TIM uses mixed pixel mode. I don't know how to load it yet.
        /// </exception>
        public TIMHeader(BinaryReader br)
        {
            _fileID = br.ReadUInt32();
            _flags = br.ReadUInt32();

            if (ID != 0x10)
                throw new BadFormatException("TIM file did not have correct magic number!");

            if (PixelMode == PixelModes.Mixed)
                throw new NotSupportedException("Mixed pixel mode in TIM is not supported!");
        }

        /// <summary>
        /// Write this TIM header to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(_fileID);
            bw.Write(_flags);
        }
    }

    /// <summary>
    /// A TIM colour lookup table, used to store colours in an efficient manner.
    /// </summary>
    public struct TIMColorLookup : IWriteable
    {
        /// <summary>
        /// The length of the CLUT in bytes (including header information).
        /// </summary>
        public readonly uint CLUTLength;

        /// <summary>
        /// The X position of the CLUT in the framebuffer
        /// </summary>
        public readonly ushort XPosition;

        /// <summary>
        /// The Y position of the CLUT in the framebuffer
        /// </summary>
        public readonly ushort YPosition;

        /// <summary>
        /// The width (in pixels) of the CLUT section
        /// </summary>
        public readonly ushort Width;

        /// <summary>
        /// The height (in pixels) of the CLUT section.
        /// </summary>
        public readonly ushort Height;

        /// <summary>
        /// The contents of the CLUT
        /// </summary>
        public readonly Color16Bit[,] Data;

        /// <summary>
        /// The number of CLUTs in this CLUT section.
        /// </summary>
        public int NumberOfCLUTs => ((int)CLUTLength - CLUT_HEADER_SIZE_BYTES) / _singleCLUTSizeBytes;

        // the size of a single CLUT varies based on whether we're in 4-bit or 8-bit CLUT mode
        private int _singleCLUTSizeBytes => _header.PixelMode == TIMHeader.PixelModes.CLUT4Bit ? 32 : 512;

        // the length of the header of a CLUT section (includes length, x,y pos, and width/height
        private const int CLUT_HEADER_SIZE_BYTES = 12;

        // the header of the TIM
        private TIMHeader _header;

        /// <summary>
        /// Read a color lookup table from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <param name="header">The header of the TIM.</param>
        public TIMColorLookup(BinaryReader br, TIMHeader header)
        {
            _header = header;

            CLUTLength = br.ReadUInt32();
            XPosition = br.ReadUInt16();
            YPosition = br.ReadUInt16();
            Width = br.ReadUInt16();
            Height = br.ReadUInt16();

            int singleClutLength = _header.PixelMode == TIMHeader.PixelModes.CLUT4Bit ? 16 : 256;
            int singleClutLengthBytes = singleClutLength * 2;
            int numCluts = ((int)CLUTLength - CLUT_HEADER_SIZE_BYTES) / singleClutLengthBytes;
            Data = new Color16Bit[numCluts, singleClutLength];
            for (int i = 0; i < numCluts; i++)
            {
                for (int j = 0; j < singleClutLength; j++)
                {
                    Data[i, j] = new Color16Bit(br);
                }
            }
        }

        /// <summary>
        /// Write this color lookup table to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(CLUTLength);
            bw.Write(XPosition);
            bw.Write(YPosition);
            bw.Write(Width);
            bw.Write(Height);
            int singleClutLength = _header.PixelMode == TIMHeader.PixelModes.CLUT4Bit ? 32 : 512;
            int numCluts = ((int)CLUTLength - CLUT_HEADER_SIZE_BYTES) / singleClutLength;
            for (int i = 0; i < numCluts; i++)
            {
                for (int j = 0; j < singleClutLength; j++)
                {
                    Data[i, j].Write(bw);
                }
            }
        }
    }

    /// <summary>
    /// Stores the actual pixel (color) data of the TIM file.
    /// </summary>
    public struct TIMPixelData : IWriteable
    {
        /// <summary>
        /// The length in bytes of the pixel data
        /// </summary>
        public readonly uint PixelDataLength;

        /// <summary>
        /// The X position of the pixel data in the framebuffer
        /// </summary>
        public readonly ushort XPosition;

        /// <summary>
        /// The Y position of the pixel data in the framebuffer
        /// </summary>
        public readonly ushort YPosition;

        /// <summary>
        /// The width (in 16-bit units) of the pixel data
        /// </summary>
        public readonly ushort Width;

        /// <summary>
        /// The height of the pixel data
        /// </summary>
        public readonly ushort Height;

        /// <summary>
        /// The pixel data
        /// </summary>
        public readonly ushort[] Data;

        /// <summary>
        /// Read TIM pixel data from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        public TIMPixelData(BinaryReader br)
        {
            PixelDataLength = br.ReadUInt32();
            XPosition = br.ReadUInt16();
            YPosition = br.ReadUInt16();
            Width = br.ReadUInt16();
            Height = br.ReadUInt16();

            int numPixels = Width * Height;
            Data = new ushort[numPixels];
            for (int i = 0; i < numPixels; i++)
            {
                Data[i] = br.ReadUInt16();
            }
        }

        /// <summary>
        /// Write this TIM pixel data to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(PixelDataLength);
            bw.Write(XPosition);
            bw.Write(YPosition);
            bw.Write(Width);
            bw.Write(Height);
            for (int i = 0; i < Data.Length; i++)
            {
                bw.Write(Data[i]);
            }
        }
    }
}
