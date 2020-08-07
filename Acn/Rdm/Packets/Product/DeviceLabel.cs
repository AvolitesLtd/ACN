﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LXProtocols.Acn.Rdm.Packets.Product
{
    /// <summary>
    /// This parameter provides a means of setting a descriptive label for each device. This may be used
    /// for identifying a dimmer rack number or specifying the device’s location.
    /// </summary>
    public class DeviceLabel
    {
        public class Get : RdmRequestPacket
        {
            public Get()
                : base(RdmCommands.Get, RdmParameters.DeviceLabel)
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
                : base(RdmCommands.GetResponse, RdmParameters.DeviceLabel)
            {
            }

            public string Label { get; set; }

            protected override void ReadData(RdmBinaryReader data)
            {
                Label = data.ReadNetworkString(Header.ParameterDataLength);
            }

            protected override void WriteData(RdmBinaryWriter data)
            {
                data.Write(Encoding.ASCII.GetBytes(Label));
            }
        }

        public class Set : RdmRequestPacket
        {
            public Set()
                : base(RdmCommands.Set, RdmParameters.DeviceLabel)
            {
            }

            public string Label { get; set; }

            #region Read and Write

            protected override void ReadData(RdmBinaryReader data)
            {
                Label = Encoding.ASCII.GetString(data.ReadBytes(Header.ParameterDataLength));
            }

            protected override void WriteData(RdmBinaryWriter data)
            {
                data.Write(Encoding.ASCII.GetBytes(Label));
            }

            #endregion
        }

        public class SetReply : RdmResponsePacket
        {
            public SetReply()
                : base(RdmCommands.SetResponse, RdmParameters.DeviceLabel)
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
