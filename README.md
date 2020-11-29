Falta:
-Elegir arquitectura
-Conexion entre clientes directa
-Conexion a un servidor que transmita los datos
-Conexion a un servidor que intenta conectarlos entre ellos, si no pueden hace de relay y transmite los datos entre ellos
-Servidor autoritativo, solo se envian los inputs y el servidor los valida y maneja toda la logica
-(los clientes pueden hacer la logica tmabien para ser mas responsible, pero la que mandara sera la del servidor)


La arquitectura de scripts planteada sera:
Clase cliente y clase servidor, son las que llevaran toda la logica de las conexiones
Cuando se quiera enviar algo, se llamara a la clase cliente (singleton) y al metodo que envie ese dato
Al recibir un dato, la clase cliente pondra el dato o llamara al metodo de la clase donde deberia estar


Para enviar un nuevo dato primero debemos crear la clase del paquete en PacketList
Esta puede contener cualquier tipo primitivo de datos
Luego debemos crear el listener en el cliente y en el servidor, esto se hara en el metodo de RegisterPacketsListeners
Aqui deberiamos llamar a la clase que tenga que recibir los datos y pasarselos
Para enviar el dato podemos hacerlo con:
 netPacketProcessorServer.Send(client, new WelcomePacket() { NumberValue = 1}, DeliveryMethod.ReliableOrdered);
Si somos el servidor podemos enviarlo a todos con 
 HpPlayerChanged hpPacket = new HpPlayerChanged() { newHp=hp };
 netManagerServer.SendToAll(netPacketProcessorServer.Write(hpPacket), DeliveryMethod.ReliableOrdered);
 
El ultimo parametro de estos metodos es el tipo de canal a usar, si queremos que unos paquetes como los de
un chat lleguen seguro y en orden adecuado, podriamos usar ReliableOrderer
Si al contrario queremos enviar una posicion continuamente solo nos servira la ultima por lo que usariamos Sequenced
ya que en este caso nos da igual que se pierdan paquetes porque la enviamos todo el rato
Si al contrario la posicion se enviara de vez en cuando, usariamos ReliableSequenced que nos asegura que 
el ultimo paquete siempre llegara

 
 
Al usar el netPacketProcessor, introduce 8 bytes extras en cada paquete, para paquetes como la posicion que se envia todo el rato
Podriamos enviar un grupo de bytes con un id al principio
Y en hacer en el networkReciveEvent data.PeekByte() para saber si es el paquete
Realmente lia bastante el codigo para lo que ganamos, 8bytes60 envios por segundo3600 segundos = 1,7mb de overhead por hora
 NetDataWriter writer = new NetDataWriter(); 
 writer.Put("123456789");
 netManagerClient.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
 
 netListenerServer.NetworkReceiveEvent += (peer, reader, deliveryMethod) =>
 {
Debug.Log("byte " + reader.PeekByte());
netPacketProcessorServer.ReadAllPackets(reader, peer);
 };
 
 
https://eddieabbondanz.io/post/unity/litenetlib-basic-server-client/
https://eddieabbondanz.io/post/unity/litenetlib-delivery-methods/
https://eddieabbondanz.io/post/unity/litenetlib-sending-data/

 

Solo se pueden enviar datos basicos, para enviar clases o Vector3 tendremos que serializarlos
Crearemos la clase en PacketList y crearemos una clase para serializarla con los metodos Serialize y Deserialize
Finalmente registramos el serializador en RegisterPacketListeners con:(SeralizeVector3.Serialize, SeralizeVector3.Deserialize);
Esta manera se usara cuando queramos crearr un paquete de un tipo ya existente, al no poder alterar su definicion no podemos implementar INetSerializable

Si definimos nosotros la clase bastara con implementar INetSerializable en la propia clase que usemos en el juego, no volver a crearla en packet list.
Por ejemplo, si nuestro juego emplea la clase Cat, al implementar INetSerializable ya podremos enviarla tal cual.





