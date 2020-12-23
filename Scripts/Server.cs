using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class Server : MonoBehaviour
{
    readonly int port = 8888;
    readonly string connectionKey = "key";
    readonly int maxConnections = 100;

    EventBasedNetListener netListenerServer;
    NetManager netManagerServer;
    NetPacketProcessor netPacketProcessorServer;

    void Start()
    {       
        //Listener es el encargado de gestionar todos los paquetes recibidos
        netListenerServer = new EventBasedNetListener();
        //Packet processor es el encargado de procesar los paquetes creados por nosotros, pudiendo leerlos y enviarlos
        netManagerServer = new NetManager(netListenerServer);
        //Manager es donde tenemos todos los datos de la conexion, como el peer
        netPacketProcessorServer = new NetPacketProcessor();
        //netManagerServer.NatPunchEnabled = true;

        netManagerServer.Start(port);

        //Evento recibido al recibir un intento de conexion
        netListenerServer.ConnectionRequestEvent += (request) =>
        {
            if (netManagerServer.ConnectedPeersCount < maxConnections)
                request.AcceptIfKey(connectionKey);
            else
                request.Reject();
        };

        //Evento estandar, cualquiera que no es especial como conectarse, desconectarse o ping
        netListenerServer.NetworkReceiveEvent += (peer, reader, deliveryMethod) =>
        {
            //Lee los paquetes recibidos y llama a los listeners de cada paquete de los que haya recibido
            netPacketProcessorServer.ReadAllPackets(reader, peer);
        };

        //Aqui estaran todos los listeners necesarios para cada paquete
        RegisterPacketsListeners();
        //Evento lanzado al tener un nuevo cliente conectado
        netListenerServer.PeerConnectedEvent += (client) =>
        {
            Debug.Log("Client connected: " + client);
            netPacketProcessorServer.Send(client, new WelcomePacket() { NumberValue = 1, StringValue = "Hola" }, DeliveryMethod.ReliableOrdered);
            netPacketProcessorServer.Send(client, new PositionPacktet() { position= new Vector3(5,6,7) }, DeliveryMethod.ReliableOrdered);
        };
    }

    int hp = 100;
    public void RegisterPacketsListeners()
    {
        //Registramos el serializador del nuevo tipo creado
        netPacketProcessorServer.RegisterNestedType(SeralizeVector3.Serialize, SeralizeVector3.Deserialize);

        //Registramos el serializador creado al implementar INetSerializar
        netPacketProcessorServer.RegisterNestedType<Cat>(() => new Cat()); 

        //Crea el listener que se activara cuando se reciba un paquete de ese tipo
        netPacketProcessorServer.SubscribeReusable<HpPlayerDecreasedPacket, NetPeer>((packet, peer) =>
        {
            //En este caso cada vez que recibamos este paquete,
            //reenviaremos la nueva vida a todos los clientes que no sean el que envio este paquete
            hp -= packet.DecreasedQuantity;
            HpPlayerChanged hpPacket = new HpPlayerChanged() { NewHp=hp };
            SendToAllExceptSender(hpPacket, peer, DeliveryMethod.ReliableOrdered);
            //podriamos reenviar el propio paquete recibido con 
            //SendToAllExceptSender(packet, peer, DeliveryMethod.ReliableOrdered);
        });
    }

    private void SendToAllExceptSender<T>(T packet, NetPeer peer, DeliveryMethod deliveryMethod) where T : class, new()
    {
        foreach (NetPeer sendPeer in netManagerServer.ConnectedPeerList)
        {
            if (sendPeer.Id == peer.Id)
            {
                netPacketProcessorServer.Send(sendPeer, packet, deliveryMethod);
            }
        }
    }
    void Update()
    {
        netManagerServer.PollEvents();
    }
}