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
	public struct MML : IWriteable
	{
		public readonly uint ID;
		public readonly uint NumberOfMOMs;
		public readonly uint[] MOMOffsets;
		public readonly MOM[] MOMs;

		private uint _mmlTop;

		public MML(BinaryReader br)
		{
			_mmlTop = (uint)br.BaseStream.Position;

			ID = br.ReadUInt32();
			if (ID != 0x204C4D4D)
				throw new BadFormatException("MML file did not have correct magic number!");

			NumberOfMOMs = br.ReadUInt32();

			MOMOffsets = new uint[NumberOfMOMs];
			for (int i = 0; i < NumberOfMOMs; i++)
			{
				MOMOffsets[i] = br.ReadUInt32();
			}

			MOMs = new MOM[NumberOfMOMs];
			for (int i = 0; i < NumberOfMOMs; i++)
			{
				br.BaseStream.Seek(_mmlTop + MOMOffsets[i], SeekOrigin.Begin);
				MOMs[i] = new MOM(br);
			}
		}

	    public void Write(BinaryWriter bw)
	    {
	        uint mmlTop = (uint) bw.BaseStream.Position;

            bw.Write(ID);
	        bw.Write(NumberOfMOMs);
	        foreach (uint offset in MOMOffsets)
	        {
	            bw.Write(offset);
	        }

	        int i = 0;
	        foreach (var mom in MOMs)
	        {
	            bw.BaseStream.Seek(mmlTop + MOMOffsets[i], SeekOrigin.Begin);
                mom.Write(bw);

	            i++;
	        }
	    }
	}
}
