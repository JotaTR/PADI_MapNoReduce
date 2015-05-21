using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;
using PADI_MapNoReduce;
using System.Collections.Generic;



namespace Cliente
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


        public class Cliente : MarshalByRefObject, ClientInterface
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
            int currentsplit;
            int splitsReceived;
            List<int> splitslist;

            public static Program p;
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
                currentsplit = 1;
                //_inputFilePath = inputPath;
                //_nSplits = splits;
                _outputPath = outputPath;
                //_mapImplementName = mapper;
                //_mapImplementPath = dllPath;
                splitsReceived = 0;

                string txt = System.IO.File.ReadAllText(_inputFilePath);

                char[] characterSpliters = { '\n', '\r' };
                _textSplit = txt.Split(characterSpliters);
                int count = 0;
                using (var sr = new StreamReader(inputPath))
                {
                    while (sr.Read() != -1)
                        count++;
                }
               

                
                splitslist = new List<int>();
                for (int i = 0; i < splits; i++)
                 
                {
                    splitslist[i] = 0;
                }

                int j = 0;
                for (int i = 0; i < count; i++)
                {
                    splitslist[j]++;
                    if (j == (splits - 1))
                    {
                        j = 0;
                    }
                    else
                    {
                        j++;
                    }
                }

                // Send job to workers?
                WorkerInterfaceRef mt = (WorkerInterfaceRef)Activator.GetObject(
                typeof(WorkerInterfaceRef),
                workerURL);
                byte[] code = System.IO.File.ReadAllBytes(dllPath);
                String resposta = "";
                resposta = mt.submitJobService(splits, "tcp://localhost:" + (10000 + idCliente) + "/C", txt);
                while(resposta != "ok")
                {
                     mt = (WorkerInterfaceRef)Activator.GetObject(
                     typeof(WorkerInterfaceRef),
                     resposta);
                }
              
            }

        public SharedClass provideTask(int taskId, String text_file)
        {
            
            SharedClass result;
            result.code = code;
            result.split = split;
            result.className = className;
            int i;
            int startingCharPos = 0;
            int endingCharPos;
            string txt = System.IO.File.ReadAllText(_inputFilePath);
            for(i = 0; i < taskId - 1; i++)
            {
                startingCharPos += splitslist[i];
            }

            endingCharPos = startingCharPos + splitslist[taskId - 1] - 1;

            for (i = startingCharPos; i <= endingCharPos; i++)
            {

            }

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

        public void deliverTask(IList<KeyValuePair<string, string>> result, int i)
        {
            _result = result;
            splitsReceived++;
            // Send result to output path
            if (!System.IO.File.Exists(_outputPath))
            {
                System.IO.Directory.CreateDirectory(_outputPath);

            }

                string filename = ".\\" + _outputPath + "\\" + i + ".out";
                System.IO.File.WriteAllText(filename, result[i].Key + ": " + result[i].Value);
            if(splitsReceived >= _nSplits){
                isWaitingResult = false;
            }
        }

        }

     //Serviços disponibilizadios Pelo no
   /* internal class ClientServices : MarshalByRefObject, ClientInterface
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

    }*/
    
}