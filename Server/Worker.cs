using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using PADI_MapNoReduce;

/// WORKER : João

namespace PADI_MapNoReduce
{

	class Worker {

    // So they can perform Job Tracker functions, they need to know about other existing Worker processes
    private ArrayList workers = new ArrayList();

    private int id = 0;

    private string puppetMaster = "";
    private string serviceURL = "";

    public bool aliveOrNot()
    {
      return true;
    }


    // Receive New Job
    public void processSplits(string clientRequest, IMap mapping)
    {
      // Call other workers to help
      // Process
    }

		static void Main(string[] args) {

			TcpChannel channel = new TcpChannel(8086);
			ChannelServices.RegisterChannel(channel,true);

			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(MyRemoteObject),
				"MyRemoteObjectName",
				WellKnownObjectMode.Singleton);
      
			System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}
	}
}