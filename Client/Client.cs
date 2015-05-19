using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;
using PADI_MapNoReduce;
using System.Collections.Generic;



namespace PADI_MapNoReduce
{
    class Program
    {

        public void Init(String EntryURL)
        {
            TcpChannel channel = new TcpChannel(Int32.Parse(EntryURL));
            ChannelServices.RegisterChannel(channel, true);
        }

        static void Main(string[] args)
        {
            string mapperName = args[0];
            int splitNumber = Int32.Parse(args[3]);
            String clientURL = args[4];
            String file = args[5];
            WorkerInterface mt = (WorkerInterface)Activator.GetObject(
                typeof(WorkerInterface),
                "tcp://localhost:30001/W");
            try
            {
                byte[] code = File.ReadAllBytes(args[2]);
                Console.WriteLine(mt.SendMapperService(code, mapperName, splitNumber, clientURL, file));
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not locate server");
            }
            Console.ReadLine();
        }
    }


        class Cliente
        {
            private bool isWaitingResult = false;
            private string _inputFilePath;
            private int _nSplits;
            private string _outputPath;
            private IMapper _mapImplementName;
            private string _mapImplementPath;
            private IList<KeyValuePair<string, string>> _result;
            private string[] _textSplit;
            private static TcpChannel channel;
            String idCliente;


            public Cliente(TcpChannel canal, String id)
            {
                channel = canal;
                idCliente = id;
            }

            public void submit(string inputPath, int splits, string outputPath, IMapper mapper, string dllPath)
            {
                while (isWaitingResult)
                { }
                isWaitingResult = true;
                _inputFilePath = inputPath;
                _nSplits = splits;
                _outputPath = outputPath;
                _mapImplementName = mapper;
                _mapImplementPath = dllPath;

                string txt = System.IO.File.ReadAllText(_inputFilePath);

                char[] characterSpliters = { '\n', '\r' };
                _textSplit = txt.Split(characterSpliters);

                // Send job to workers?
                Worker worker = (Worker)Activator.GetObject(typeof(Worker), "tcp://localhost:30001/W");
                byte[] code = System.IO.File.ReadAllBytes(dllPath);
            }

            public string[] sendSplit(string filepath, int start, int end)
            {
                int nLines = start - end;
                string[] result = new string[nLines];
                for (int i = 0; i < nLines; i++)
                {
                    result[i] = _textSplit[start + i];
                }
                return result;
            }

            public void receiveOutput(IList<KeyValuePair<string, string>> result, string outputPath)
            {
                _result = result;
                // Send result to output path

                isWaitingResult = false;
            }
        }
    
}