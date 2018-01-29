using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Exceptions;

namespace libLSD.Formats
{
	public struct MOM
	{
		public readonly uint ID;
		public readonly uint MOMLength;
		public readonly uint TMDOffset;
		public readonly MOS MOS;
		public readonly TMD TMD;

		private uint _momTop;

		public MOM(BinaryReader br)
		{
			_momTop = (uint) br.BaseStream.Position;

			ID = br.ReadUInt32();
			if (ID != 0x204D4F4D)
				throw new BadFormatException("MOM file did not have correct magic number!");

			MOMLength = br.ReadUInt32();
			TMDOffset = br.ReadUInt32();
			MOS = new MOS(br);

			br.BaseStream.Seek(_momTop + TMDOffset, SeekOrigin.Begin);
			TMD = new TMD(br);
		}
	}
}
