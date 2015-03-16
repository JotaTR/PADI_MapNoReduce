using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;

namespace MapNoReduce {

	class Client {
       
        public delegate string RemoteAsyncDelegate();
        // This is the call that the AsyncCallBack delegate will reference.
        public static void OurRemoteAsyncCallBack(IAsyncResult ar)
        {
            // Alternative 2: Use the callback to get the return value
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: " + del.EndInvoke(ar));

            return;
        }

		static void Main() {
			
            TcpChannel channel = new TcpChannel();
			ChannelServices.RegisterChannel(channel,true);

			MyRemoteObject obj = (MyRemoteObject) Activator.GetObject(
				typeof(MyRemoteObject),
				"tcp://localhost:8086/MyRemoteObjectName");

	 		try
	 		{
                clsPerson p = new clsPerson();
                p.FirstName = "John";
                p.MI = "A";
                p.LastName = "Smith";
                obj.write_XML(p);
                Console.WriteLine( obj.read_XML());

                // change this to true to use the callback (alt.2)
                bool useCallback = false;

                if (!useCallback)
                {
                    // Create delegate to remote method
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.read_XML);
                    // Call delegate to remote method
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);

                    // Wait for the end of the call and then explictly call EndInvoke
                    RemAr.AsyncWaitHandle.WaitOne();
                    Console.WriteLine(RemoteDel.EndInvoke(RemAr));

                } else {
                    // Create delegate to remote method
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.read_XML);
                    // Create delegate to local callback
                    AsyncCallback RemoteCallback = new AsyncCallback(Client.OurRemoteAsyncCallBack);
                    
                    // Call remote method
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
                }


	 		}
	 		catch(SocketException)
	 		{
	 			System.Console.WriteLine("Could not locate server");
	 		}

			Console.ReadLine();
		}
	}
}