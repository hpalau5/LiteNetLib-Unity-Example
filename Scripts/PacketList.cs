using LiteNetLib.Utils;
using UnityEngine;

public class PacketsList
{

}

public class WelcomePacket
{
    public int NumberValue { get; set; }
    public string StringValue { get; set; }
    public float FloatValue { get; set; }
}

public class HpPlayerChanged
{
    public int NewHp { get; set; }
}
public class HpPlayerDecreasedPacket
{
    public int DecreasedQuantity { get; set; }
}

//Esta clase tiene un vector3, como no es un tipo primitivo hay que poder serializarlo
public class PositionPacktet
{
    public Vector3 position { get; set; }
}
//Clase parar serializar el vector3
//En el caso de vector3, como no podemos alterar la definicion para implementar INetSerializable usamos esta manera

public class SeralizeVector3
{
    public static void Serialize(NetDataWriter writer, Vector3 vector)
    {
        writer.Put(vector.x);
        writer.Put(vector.y);
        writer.Put(vector.z);
    }

    public static Vector3 Deserialize(NetDataReader reader)
    {
        return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}
//Un paquete para enviar un dato custom
public class CatPacket
{
    public Cat Cat { get; set; }
}

//Otra manera de serializar las cosas, implementar la interfaz INetSerializable
//De esta manera no necesitamos crear otra clase para serializar
//Esta manera es preferible si podemos crear nosotros el dato
public class Cat : INetSerializable
{
    public string Name { get; set; }
    public int Age { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name);
        writer.Put(Age);
    }

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString();
        Age = reader.GetInt();
    }
}