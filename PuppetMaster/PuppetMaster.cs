using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Reflection;


using Worker_JobTracker;
using PADI_MapNoReduce;

/// PUPPETMASTER: Rui

namespace PuppetMaster
{
  class PuppetMaster : MarshalByRefObject
  {
    // int: Worker ID
    // Object: Worker instance
    private Dictionary<int, Program> workerList = new Dictionary<int, Program>();
    private int currentWorkerID = 0;
    private int _ID;
    private int _numberPM = 0;

    public int ID
    {
      get { return _ID; }
      set { _ID = value; }
    }

    public int numberPM
    {
      get { return _numberPM; }
      set { _numberPM = value; }
    }

    // Functions

    // SubmitJob
    // Submit Jobs to worker processes
    public void SubmitJob(string entryURL, string inputPath,
                          string outputPath, int nSplits,
                          string mapClassName, string mapClassPath
      ) 
    {
      // Show Message for testing - remove later
      System.Windows.Forms.MessageBox.Show("Entry URL: " + entryURL + " Input Path: " + inputPath + " Output Path: " + outputPath +
                                            " Number of Splits: " + nSplits + " Map Class Name: " + mapClassName + " Map Class Path: " + mapClassPath);


      Worker worker = (Worker)Activator.GetObject(typeof(Worker), entryURL);
      string txt = System.IO.File.ReadAllText(inputPath);
      byte[] code = System.IO.File.ReadAllBytes(mapClassPath);

      object ClassObj = null;

      Assembly assembly = Assembly.Load(code);
      // Walk through each type in the assembly looking for our class
      foreach (Type type in assembly.GetTypes())
      {
        if (type.IsClass == true)
        {
          if (type.FullName.EndsWith("." + mapClassName))
          {
            // create an instance of the object
            ClassObj = Activator.CreateInstance(type);


                        // Dynamically Invoke the method
                        /*object[] args = new object[] { txt };
                        object resultObject = type.InvokeMember("Map",
                          BindingFlags.Default | BindingFlags.InvokeMethod,
                               null,
                               ClassObj,
                               args);
                        IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>) resultObject;
                        Console.WriteLine("Map call result was: ");
                        foreach (KeyValuePair<string, string> p in result) {
                            Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                        }*/

          }
        }
      }

      if (ClassObj == null)
      {
        // Throw Exception
      }

    }

    // CreateWorker: 
    // Launch new Worker process
    public void CreateWorker(string workerURL, int id = 0, string entryURL = null)
    {

      // Parse worker URL
      char[] urlDelimiterChars = {':' , '/'};
      string[] parsedURL = workerURL.Split(urlDelimiterChars);
      int workerPort = Int32.Parse(parsedURL[4]);

      // Show Message for testing - remove later
      System.Windows.Forms.MessageBox.Show("ID: " + id + " Port: " + workerPort);

      // Create new worker instance and store it in array/dictionary
      if (id <= 0 && !workerList.ContainsKey(currentWorkerID))
      {
        Program w = new Program(currentWorkerID, "tcp://localhost:"+ID+"/PM", workerURL, entryURL);
        workerList.Add(currentWorkerID, w);
        currentWorkerID++;
      }
      else if (!workerList.ContainsKey(id))
      {
        Program w = new Program(id, "tcp://localhost:" + ID + "/PM", workerURL, entryURL);
        workerList.Add(id, w);
        currentWorkerID = id;
      }
      else
      {
        // THROW ERROR/EXCEPTION
      }
    }

    // Wait: 
    // Stops execution of other command scripts
    public void Wait(int time) 
    {
      // Puts system to sleep
      System.Threading.Thread.Sleep(time);
    }

    // StatusReport:
    // Requests all workers/job trackers submit/print their current status
    public void StatusReport()
    {
      string result = "";
      for (int i = 1; i <= numberPM; i++)
      {
        int portN = 20000 + i;
        if (portN != ID)
        {
          PuppetMaster pm = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:" + portN + "/PM");
          result += pm.Report() + '\n';
        }
        else
        {
          result += Report() + '\n';
        }
      }
      System.Windows.Forms.MessageBox.Show(result);
    }

    public string Report()
    {
      // Add status requests to worker nodes
      return (ID + ": Nothing to Report");
    }

    // SlowWorker:
    // Delays (puts to sleep) a worker process
    public void SlowWorker(int workerID, int ms)
    {
      Program w;
      if (workerList.TryGetValue(workerID, out w))
      {

      }
    }

    // FreezeWorker:
    // Disables a given worker process's Worker Functions
    public void FreezeWorker(int workerID)
    {
      Program w;
      if(workerList.TryGetValue(workerID, out w))
      {
        if (w._W)
        {
          w._freeze = true;
        }
      }
    }

    // UnfreezeWorker:
    // Re-enables a given worker process's Worker Functions
    public void UnfreezeWorker(int workerID)
    {
      Program w;
      if (workerList.TryGetValue(workerID, out w))
      {
        if (w._W && w._freeze)
        {
          w._freeze = false;
        }
      }
    }

    // FreezeJobTracker:
    // Disables a given worker process's JobTracker Functions
    public void FreezeJobTracker(int workerID)
    {
      Program w;
      if (workerList.TryGetValue(workerID, out w))
      {
        if (w._JT)
        {
          w._freeze = true;
        }
      }
    }

    // UnfreezeJobTracker:
    // Re-enables a given worker process's JobTracker Functions
    public void UnfreezeJobTracker(int workerID)
    {
      Program w;
      if (workerList.TryGetValue(workerID, out w))
      {
        if (w._JT && w._freeze)
        {
          w._freeze = false;
        }
      }
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    //[STAThread]
    static void Main(string[] args)
    {
      PuppetMaster pm = new PuppetMaster();
      pm.numberPM = Int32.Parse(args[0]);

      pm.ID = Int32.Parse(args[1]);
      TcpChannel channel = new TcpChannel(pm.ID);

      ChannelServices.RegisterChannel(channel, false);

      RemotingServices.Marshal(pm, "PM", typeof(PuppetMaster));

        /*RemotingConfiguration.RegisterWellKnownServiceType(
              typeof(PuppetMaster),
              "PM",
              WellKnownObjectMode.Singleton
          );*/

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new PuppetMasterForm());
    }
  }
}