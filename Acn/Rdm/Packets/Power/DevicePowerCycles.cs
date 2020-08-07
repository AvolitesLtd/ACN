﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LXProtocols.Acn.Rdm.Packets.Power
{
    public class DevicePowerCycles
    {
        public class Get : RdmRequestPacket
        {
            public Get()
                : base(RdmCommands.Get, RdmParameters.DevicePowerCycles )
            {
            }

            #region Read and Write

            protected override void ReadData(RdmBinaryReader data)
            {
            }

            protected override void WriteData(RdmBinaryWriter data)
            {
            }

            #endregion
        }

        public class GetReply : RdmResponsePacket
        {
            public GetReply()
                : base(RdmCommands.GetResponse, RdmParameters.DevicePowerCycles)
            {
            }

            public int PowerCycles { get; set; }

            #region Read and Write

            protected override void ReadData(RdmBinaryReader data)
            {
                PowerCycles = data.ReadNetwork32();
            }

            protected override void WriteData(RdmBinaryWriter data)
            {
                data.WriteNetwork(PowerCycles);
            }

            #endregion
        }

        public class Set : RdmRequestPacket
        {
            public Set()
                : base(RdmCommands.Set, RdmParameters.DevicePowerCycles)
            {
            }

            public int PowerCycles { get; set; }

            #region Read and Write

            protected override void ReadData(RdmBinaryReader data)
            {
                PowerCycles = data.ReadNetwork32();
            }

            protected override void WriteData(RdmBinaryWriter data)
            {
                data.WriteNetwork(PowerCycles);
            }

            #endregion
        }

        public class SetReply : RdmResponsePacket
        {
            public SetReply()
                : base(RdmCommands.SetResponse, RdmParameters.DevicePowerCycles)
            {
            }

            #region Read and Write

            protected override void ReadData(RdmBinaryReader data)
            {
            }

            protected override void WriteData(RdmBinaryWriter data)
            {
            }

            #endregion
        }
    }
}
