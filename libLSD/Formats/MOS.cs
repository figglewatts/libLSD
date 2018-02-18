using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Exceptions;
using libLSD.Interfaces;

namespace libLSD.Formats
{
	public struct MOS : IWriteable
	{
		public readonly uint ID;
		public readonly uint NumberOfTODs;
		public readonly uint[] TODOffsets;
		public readonly TOD[] TODs;

		private uint _mosTop;

		public MOS(BinaryReader br)
		{
			_mosTop = (uint)br.BaseStream.Position;

			ID = br.ReadUInt32();

			if (ID != 0x20534F4D)
				throw new BadFormatException("MOS did not have correct magic number!");

			NumberOfTODs = br.ReadUInt32();

			TODOffsets = new uint[NumberOfTODs];
			for (int i = 0; i < NumberOfTODs; i++)
			{
				TODOffsets[i] = br.ReadUInt32();
			}

			TODs = new TOD[NumberOfTODs];
			for (int i = 0; i < NumberOfTODs; i++)
			{
				br.BaseStream.Seek(_mosTop + TODOffsets[i], SeekOrigin.Begin);
				TODs[i] = new TOD(br);
			}
		}

	    public void Write(BinaryWriter bw)
	    {
	        uint mosTop = (uint) bw.BaseStream.Position;

            bw.Write(ID);
            bw.Write(NumberOfTODs);
	        foreach (uint offset in TODOffsets)
	        {
	            bw.Write(offset);
	        }

	        int i = 0;
	        foreach (var tod in TODs)
	        {
	            bw.BaseStream.Seek(mosTop + TODOffsets[i], SeekOrigin.Begin);
                tod.Write(bw);

	            i++;
	        }
	    }
	}
}
