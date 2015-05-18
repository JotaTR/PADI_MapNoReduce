using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;
using PADI_MapNoReduce;


namespace PADI_MapNoReduce {
    class Program {
        static void Main(string[] args) {
            string mapperName = args[0];
            TcpChannel channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(channel, true);
            WorkerInterface mt = (WorkerInterface)Activator.GetObject(
                typeof(WorkerInterface),
                "tcp://localhost:30001/Worker");
            try {
                byte[] code = File.ReadAllBytes(args[1]);
                Console.WriteLine(mt.SendMapperService(code, mapperName, splitNumber, clientURL, file ));
            } catch (SocketException) {
                System.Console.WriteLine("Could not locate server");
            }
            Console.ReadLine();
        }
    
public static int splitNumber { get; set; }
public static string clientURL { get; set; }
public static string file { get; set; }}

     class Cliente
    {
        private static TcpChannel channel;
        String idCliente;


        public Cliente(TcpChannel canal, String id)
        {
            channel = canal;
            idCliente = id;
        }

        public void submit (string inputPath, int splits, string outputPath, MapNoReduce,  ) {

        }

        public void sendSplit(Worker worker){

        }

        public void receiveOutput(Worker worker){

        }
     }
}