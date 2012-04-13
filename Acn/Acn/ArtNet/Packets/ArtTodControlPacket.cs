using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Acn.ArtNet.IO;

namespace Acn.ArtNet.Packets
{
    public enum ArtTodControlCommand
    {
        AtcNone = 0,
        AtcFlash = 1
    }

    internal class ArtTodControlPacket:ArtNetPacket
    {
        public ArtTodControlPacket()
            : base(ArtNetOpCodes.TodControl)
        {
        }

        public ArtTodControlPacket(ArtNetRecieveData data)
            : base(data)
        {
            
        }

        #region Packet Properties

        public byte Net { get; set; }

        public ArtTodControlCommand Command { get; set; }

        public byte Address { get; set; }
	
	
        #endregion

        public override void ReadData(System.IO.BinaryReader data)
        {
            base.ReadData(data);

            data.BaseStream.Seek(9, System.IO.SeekOrigin.Current);
            Net = data.ReadByte();
            Command = (ArtTodControlCommand) data.ReadByte();
            Address = data.ReadByte();
        }

        public override void WriteData(System.IO.BinaryWriter data)
        {
            base.WriteData(data);

            data.Write(new byte[9]);
            data.Write(Net);
            data.Write((byte) Command);
            data.Write(Address);
        }
	

    }
}