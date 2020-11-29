using System;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class Client : MonoBehaviour
{
    readonly int port = 8888;
    readonly string connectionKey = "key";
    readonly string ip = "83.61.31.71";

    EventBasedNetListener netListenerClient;
    NetManager netManagerClient;
    NetPacketProcessor netPacketProcessorClient;

    public void StartClientConnection()
    {
        //Listener es el encargado de gestionar todos los paquetes recibidos
        netListenerClient = new EventBasedNetListener();
        //Packet processor es el encargado de procesar los paquetes creados por nosotros, pudiendo leerlos y enviarlos
        netPacketProcessorClient = new NetPacketProcessor();
        //Manager es donde tenemos todos los datos de la conexion, como el peer
        netManagerClient = new NetManager(netListenerClient);
        //netManagerServer.NatPunchEnabled = true;

        //Evento recibido al conectarse al servidor
        netListenerClient.PeerConnectedEvent += (server) =>
        {
            Debug.Log("Connected to server: " + server);
        };

        //Evento estandar, cualquier que no es especial como conectarse, desconectarse o ping
        netListenerClient.NetworkReceiveEvent += (server, reader, deliveryMethod) =>
        {
            //Lee los paquetes recibidos y llama a los listeners de cada paquete de los que haya recibido
            netPacketProcessorClient.ReadAllPackets(reader, server);
        };

        //Aqui estaran todos los listeners necesarios para cada paquete
        RegisterPacketsListeners();

        netManagerClient.Start();
        netManagerClient.Connect(ip, port, connectionKey);
    }

    public void RegisterPacketsListeners()
    {       
        //Registramos el serializador del nuevo tipo creado
        netPacketProcessorClient.RegisterNestedType(SeralizeVector3.Serialize, SeralizeVector3.Deserialize);

        //Registramos el serializador creado al implementar INetSerializar
        netPacketProcessorClient.RegisterNestedType<Cat>(() => new Cat());

        //Crea el listener que se activara cuando se reciba un paquete de ese tipo
        netPacketProcessorClient.SubscribeReusable<WelcomePacket>((packet) =>
        {
            Debug.Log("Test packet received");
            Debug.Log(packet.NumberValue);
        });

        netPacketProcessorClient.SubscribeReusable<HpPlayerChanged>((packet) =>
        {
            Debug.Log("New hp received");
            Debug.Log(packet.NewHp);
        });

        netPacketProcessorClient.SubscribeReusable<PositionPacktet>((packet) =>
        {
            Debug.Log("New vector3 received");
            Debug.Log(packet.position);
        });
    }

    public void SendDecreaseHp(int decreaseQuantity)
    {
        netPacketProcessorClient.Send(netManagerClient.FirstPeer, new HpPlayerDecreasedPacket() { DecreasedQuantity = decreaseQuantity }, DeliveryMethod.ReliableOrdered);
    }
    void Update()
    {
        if (netManagerClient != null)
            netManagerClient.PollEvents();
    }
}