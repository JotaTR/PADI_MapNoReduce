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
    public class Program
    {

        public Cliente Init(int clientID, String EntryURL)
        {
            Cliente c = new Cliente(clientID, EntryURL);

            TcpChannel channel = new TcpChannel(10000 + clientID);

            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(c, "C", typeof(Cliente));

            return c;
        }

        public Program()
        {

        }

        static void Main(string[] args)
        {
            /*string mapperName = args[0];
            int splitNumber = Int32.Parse(args[3]);
            String clientURL = args[4];
            String file = args[5];
            /*WorkerInterfaceRef mt = (WorkerInterfaceRef)Activator.GetObject(
                typeof(WorkerInterfaceRef),
                "tcp://localhost:30001/W");
            try
            {
                byte[] code = File.ReadAllBytes(args[2]);
                Console.WriteLine(mt.submitJobService(splitNumber, clientURL, file));
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not locate server");
            }*/
            Console.ReadLine();
        }
    }


        class Cliente : MarshalByRefObject
        {
            private bool isWaitingResult = false;
            private string _inputFilePath;
            private int _nSplits;
            private string _outputPath;
            private IMapper _mapImplementName;
            private string _mapImplementPath;
            private IList<KeyValuePair<string, string>> _result;
            private string[] _textSplit;
            int idCliente;
            String workerURL;


            public Cliente(int id, string entryURL)
            {
                idCliente = id;
                workerURL = entryURL;
            }

            public void submit(string inputPath, int splits, string outputPath, IMapper mapper, string dllPath)
            {
                while (isWaitingResult)
                { }
                isWaitingResult = true;
                //_inputFilePath = inputPath;
                //_nSplits = splits;
                _outputPath = outputPath;
                //_mapImplementName = mapper;
                //_mapImplementPath = dllPath;

                string txt = System.IO.File.ReadAllText(_inputFilePath);

                char[] characterSpliters = { '\n', '\r' };
                _textSplit = txt.Split(characterSpliters);

                // Send job to workers?
                WorkerInterfaceRef mt = (WorkerInterfaceRef)Activator.GetObject(
                typeof(WorkerInterfaceRef),
                workerURL);
                byte[] code = System.IO.File.ReadAllBytes(dllPath);
                String resposta = "";
                resposta = mt.submitJobService(splits, "tcp://localhost:" + (10000 + idCliente) + "/C", txt);
                if(resposta == "ok")
                {

                }
                else
                {
                    mt = (WorkerInterfaceRef)Activator.GetObject(
                        typeof(WorkerInterfaceRef),
                        resposta);
                }
              
            }

        }

     //Serviços disponibilizadios Pelo no
    internal class ClientServices : MarshalByRefObject, ClientInterface
    {

        private IList<KeyValuePair<string, string>> _result;
        private string[] _textSplit;
      
        currentsplit
        nsplits
         

        public SharedClass provideTask(int taskId, byte[] code, String split, String className, String text_file)
        {
            SharedClass result;
            result.code = code;
            result.split = split;
            result.className = className;
            string txt = System.IO.File.ReadAllText(_inputFilePath);

            char[] characterSpliters = { '\n', '\r' };
            _textSplit = txt.Split(characterSpliters);
            int nLines = start - end;
            string[] result = new string[nLines];
            for (int i = 0; i < nLines; i++)
            {
                result[i] = _textSplit[start + i];
            }
            return result;
        }

        public  void deliverTask( IList<KeyValuePair<string, string>> result, string outputPath)
        {
            _result = result;
            // Send result to output path
            if (!System.IO.File.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);

            }

            for (int i = currentsplit; i < result.Count; i++)
            {
                string filename = "./" + outputPath + "/" + i + ".out";
                System.IO.File.WriteAllText(filename, result[i].Key + ": " + result[i].Value);
                currentsplit = i;
            }
            if(currentsplit >= nsplits){
                isWaitingResult = false;
            }
        }

    }
    
}