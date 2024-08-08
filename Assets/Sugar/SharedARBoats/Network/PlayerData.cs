using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;

    public int score;

    public int health;

    public int deaths;

    public bool isDead;

    public PlayerData(ulong clientId, int score, int health)
    {
        this.clientId = clientId;
        this.score = score;
        this.health = health;
        deaths = 0;
        isDead = false;
    }

    public bool Equals(PlayerData other)
    {
        throw new NotImplementedException();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref isDead);
    }
}
