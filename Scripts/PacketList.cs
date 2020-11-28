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