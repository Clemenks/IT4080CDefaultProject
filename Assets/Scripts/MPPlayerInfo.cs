using MLAPI.Serialization;
using System;
using UnityEngine.UI;

public struct MPPlayerInfo : MLAPI.Serialization.INetworkSerializable
{
    public ulong networkClientId;
    public string networkPlayerName;
    public bool networkPlayerReady;

    public MPPlayerInfo(ulong nwClientId, string nwPName, bool playerReady)
    {
        networkClientId = nwClientId;
        networkPlayerName = nwPName;
        networkPlayerReady = playerReady;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref networkClientId);
        serializer.Serialize(ref networkPlayerName);
        serializer.Serialize(ref networkPlayerReady);
    }
    
}
