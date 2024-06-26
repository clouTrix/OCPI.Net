﻿using System.Runtime.Serialization;

namespace OCPI;

public enum InterfaceRole : byte
{
    /// <summary>
    /// Sender Interface implementation.
    /// Interface implemented by the owner of data, so the Receiver can pull information from the data Sender/owner.
    /// </summary>
    [EnumMember(Value = "SENDER")]
    Sender = 11,

    /// <summary>
    /// Receiver Interface implementation
    ///  Interface implemented by the receiver of data, so the Sender/owner can Push information to the Receiver.
    /// </summary>
    [EnumMember(Value = "RECEIVER")]
    Receiver = 12,
}
