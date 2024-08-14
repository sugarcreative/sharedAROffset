using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;

    public FixedString64Bytes name;

    public int score;

    public int health;

    public int deaths;

    public bool isDead;

    public FixedString64Bytes color;

    public bool isReady;

    public PlayerData(ulong clientId, FixedString64Bytes name, int score, int health)
    {
        this.clientId = clientId;
        this.name = name;
        this.score = score;
        this.health = health;
        deaths = 0;
        isDead = false;
        color = "#FFFFFF";
        isReady = false;
    }

    public bool Equals(PlayerData other)
    {
        throw new NotImplementedException();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref isDead);
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref isReady);
    }
}
