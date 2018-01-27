using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Exceptions;
using libLSD.Types;

namespace libLSD.Formats
{
    public class TIM
    {
        public readonly TIMHeader Header;
        public readonly TIMColorLookup? ColorLookup;
        public readonly TIMPixelData PixelData;

        public TIM(BinaryReader br)
        {
            Header = new TIMHeader(br);

            ColorLookup = null;
            if (Header.HasCLUT)
            {
                ColorLookup = new TIMColorLookup(br);
            }

            PixelData = new TIMPixelData(br);
        }

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

        private IColor[,] GetImageCLUT4Bit()
        {
            int imageWidth = PixelData.Width * 4;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageWidth, imageHeight];
            for (int i = 0; i < PixelData.Data.Length; i++)
            {
                ushort data = PixelData.Data[i];
                int x = (i * 4) % imageWidth;
                int y = (i * 4) / imageWidth;
                for (int j = 0; j < 4; j++)
                {
                    int clutLocation = data >> (4 * j) & 0xF;
                    image[x + j, y] = ColorLookup?.Data[clutLocation];
                }
            }
            return image;
        }

        private IColor[,] GetImageCLUT8Bit()
        {
            int imageWidth = PixelData.Width * 2;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageWidth, imageHeight];
            for (int i = 0; i < PixelData.Data.Length; i++)
            {
                ushort data = PixelData.Data[i];
                int x = (i * 2) % imageWidth;
                int y = (i * 2) / imageWidth;
                for (int j = 0; j < 2; j++)
                {
                    int clutLocation = data >> (8 * j) & 0xFF;
                    image[x + j, y] = ColorLookup?.Data[clutLocation];
                }
            }
            return image;
        }

        private IColor[,] GetImageDirect15Bit()
        {
            int imageWidth = PixelData.Width;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageWidth, imageHeight];
            for (int i = 0; i < PixelData.Data.Length; i++)
            {
                int x = i % imageWidth;
                int y = i / imageWidth;
                image[x, y] = new Color16Bit(PixelData.Data[i]);
            }
            return image;
        }

        private IColor[,] GetImageDirect24Bit()
        {
            int imageWidth = (PixelData.Width / 3) * 2;
            int imageHeight = PixelData.Height;
            IColor[,] image = new IColor[imageWidth, imageHeight];
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

                image[x1, y1] = new Color24Bit((ushort)r0, (ushort)g0, (ushort)b0); 
                image[x2, y2] = new Color24Bit((ushort)r1, (ushort)g1, (ushort)b1);
            }
            return image;
        }
    }

    /// <summary>
    /// The header of a TIM file, containing its ID and flags
    /// </summary>
    public struct TIMHeader
    {
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

        public PixelModes PixelMode => (PixelModes)(_flags & 0b111);

        /// <summary>
        /// Is true if this TIM file uses a CLUT (color lookup table), false otherwise
        /// </summary>
        public bool HasCLUT => ((_flags >> 3) & 0x1) == 1;

        private readonly uint _fileID;
        private readonly uint _flags;

        public TIMHeader(BinaryReader br)
        {
            _fileID = br.ReadUInt32();
            _flags = br.ReadUInt32();

            if (ID != 0x10)
                throw new BadFormatException("TIM file did not have correct magic number!");
            
            if (PixelMode == PixelModes.Mixed)
                throw new NotSupportedException("Mixed pixel mode in TIM is not supported!");
        }
    }

    public struct TIMColorLookup
    {
        /// <summary>
        /// The length of the CLUT in bytes
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
        /// Since a CLUT is 1 pixel high, then this number also doubles
        /// as the number of CLUTs in the TIM file
        /// </summary>
        public readonly ushort Height;

        /// <summary>
        /// The contents of the CLUT
        /// </summary>
        public readonly Color16Bit[] Data;

        public TIMColorLookup(BinaryReader br)
        {
            CLUTLength = br.ReadUInt32();
            XPosition = br.ReadUInt16();
            YPosition = br.ReadUInt16();
            Width = br.ReadUInt16();
            Height = br.ReadUInt16();

            int numColors = Width * Height;
            Data = new Color16Bit[numColors];
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new Color16Bit(br);
            }
        }
    }

    public struct TIMPixelData
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
    }
}
