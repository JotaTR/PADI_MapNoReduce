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
using System.Threading;

using Worker_JobTracker;
using PADI_MapNoReduce;

/// PUPPETMASTER: Rui

namespace PuppetMaster
{
  class WorkerArguments
  {
    public int id;
    public string pmURL;
    public string workerURL;
    public string entryURL;

    public WorkerArguments (int wID, string puppetmasterURL, string wURL, string eURL)
    {
      id = wID;
      pmURL = puppetmasterURL;
      workerURL = wURL;
      entryURL = eURL;
    }
  }
  class DelayArguments
  {
    public int time;
    public Thread wt;

    public DelayArguments(int t, Thread worker)
    {
      time = t;
      wt = worker;
    }
  }

  public enum FreezeType {FREEZEW, UNFREEZEW, FREEZEC, UNFREEZEC};
  class PuppetMaster : MarshalByRefObject
  {
    // int: Worker ID
    // Object: Worker instance
    private Dictionary<int, Worker_JobTracker.Program> workerList = new Dictionary<int, Worker_JobTracker.Program>();
    private Dictionary<int, Thread> workerThreadList = new Dictionary<int, Thread>();
    private int _ID;
    private int _numberPM = 0;

    private PADI_MapNoReduce.Program _userApp;
    private PADI_MapNoReduce.Cliente client;

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

    public PADI_MapNoReduce.Program userApp
    {
      get { return _userApp; }
      set { _userApp = value; }
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

      client = userApp.Init(this.ID - 30000, entryURL);
      
      byte[] code = System.IO.File.ReadAllBytes(mapClassPath);

      object ClassObj = null;
      Assembly assembly = Assembly.Load(code);
      // Walk through each type in the assembly looking for our class
      foreach (Type type in assembly.GetTypes()) {
        if (type.IsClass == true) {
          if (type.FullName.EndsWith("." + mapClassName)) {
            // create an instance of the object
            ClassObj = Activator.CreateInstance(type);
          }
        }
      }

      IMapper mapper = (IMapper) ClassObj;

      client.submit(inputPath, nSplits, outputPath, mapper, mapClassPath);
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

      PuppetMaster[] pmA = GetPuppetMasters();
      bool noWorkerWithID = true;

      foreach (PuppetMaster pm in pmA)
      {
        if (pm.hasWorkerID(id))
        {
          noWorkerWithID = false;
        }
      }

      if (noWorkerWithID)
      {
        Thread worker = new Thread(new ParameterizedThreadStart(this.newWorker));
        worker.Start(new WorkerArguments(id, "tcp://localhost:" + ID + "/PM", workerURL, entryURL));
        workerThreadList.Add(id, worker);
      }
      else
      {
        System.Windows.Forms.MessageBox.Show("ID: " + id + " already exists");
      }
    }

    public void newWorker(object arg)
    {
      WorkerArguments wa = (WorkerArguments)arg;
      Worker_JobTracker.Program w = new Worker_JobTracker.Program(wa.id, wa.pmURL, wa.workerURL, wa.entryURL);
      workerList.Add(wa.id, w);
    }

    public bool hasWorkerID(int id)
    {
      return workerList.ContainsKey(id);
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
      string result = "Puppet Master " + ID + " :" + '\n';
      foreach (KeyValuePair<int, Worker_JobTracker.Program> worker in this.workerList)
      {
        result += " Worker " + worker.Key + " : ";
        WorkerInterfaceRef service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), worker.Value._address);
        WorkerState state = service.askNodeInfoService();
        result += "-Ready: " + state.ready.ToString() + " -Frozen: " + state.freeze.ToString() + " -Number of Tasks Remaining: " + state.tasks_remaining + '\n';
      }
      // Add status requests to worker nodes
      return result;
    }

    // SlowWorker:
    // Delays (puts to sleep) a worker process
    public void SlowWorker(int workerID, int ms)
    {
      Worker_JobTracker.Program w;
      Thread wt;
      if (workerList.TryGetValue(workerID, out w) && workerThreadList.TryGetValue(workerID, out wt))
      {
        Thread temp = new Thread(new ParameterizedThreadStart(this.DelayWorker));
        temp.Start(new DelayArguments(ms, wt));
      }
    }

    public void DelayWorker(object t)
    {
      DelayArguments args = (DelayArguments)t;
      args.wt.Suspend();
      Thread.Sleep(args.time);
      args.wt.Resume();
    }

    // FreezeWorker:
    // Disables a given worker process's Worker Functions
    public bool FreezeWorker(int workerID)
    {
      Worker_JobTracker.Program w;
      Thread wt;
      if (workerList.TryGetValue(workerID, out w) && workerThreadList.TryGetValue(workerID, out wt))
      {
        if (w._W)
        {
          w._freeze = true;
          wt.Suspend();
        }
        return true;
      }
      return false;
    }

    // UnfreezeWorker:
    // Re-enables a given worker process's Worker Functions
    public bool UnfreezeWorker(int workerID)
    {
      Worker_JobTracker.Program w;
      Thread wt;
      if (workerList.TryGetValue(workerID, out w) && workerThreadList.TryGetValue(workerID, out wt))
      {
        wt.Resume();
        if (w._W && w._freeze)
        {
          wt.Resume();
          w._freeze = false;
        }
        else
        {
          wt.Suspend();
        }
        return true;
      }
      return false;
    }

    // FreezeJobTracker:
    // Disables a given worker process's JobTracker Functions
    public bool FreezeJobTracker(int workerID)
    {
      Worker_JobTracker.Program w;
      Thread wt;
      if (workerList.TryGetValue(workerID, out w) && workerThreadList.TryGetValue(workerID, out wt))
      {
        if (w._JT)
        {
          w._freeze = true;
          wt.Suspend();
        }
        return true;
      }
      return false;
    }

    // UnfreezeJobTracker:
    // Re-enables a given worker process's JobTracker Functions
    public bool UnfreezeJobTracker(int workerID)
    {
      Worker_JobTracker.Program w;
      Thread wt;
      if (workerList.TryGetValue(workerID, out w) && workerThreadList.TryGetValue(workerID, out wt))
      {
        wt.Resume();
        if (w._JT && w._freeze)
        {
          wt.Resume();
          w._freeze = false;
        }
        else
        {
          wt.Suspend();
        }
        return true;
      }
      return false;
    }

    public void CallFreeze(int workerID, FreezeType fType)
    {
      PuppetMaster[] pmArray = GetPuppetMasters();
      switch (fType)
      {
        case FreezeType.FREEZEC:
          foreach (PuppetMaster pm in pmArray)
          {
            if (pm.FreezeJobTracker(workerID))
            {
              return;
            }
          }
          break;
        case FreezeType.FREEZEW:
          foreach (PuppetMaster pm in pmArray)
          {
            if (pm.FreezeWorker(workerID))
            {
              return;
            }
          }
          break;
        case FreezeType.UNFREEZEC:
          foreach (PuppetMaster pm in pmArray)
          {
            if (pm.UnfreezeJobTracker(workerID))
            {
              return;
            }
          }
          break;
        case FreezeType.UNFREEZEW:
          foreach (PuppetMaster pm in pmArray)
          {
            if (pm.UnfreezeWorker(workerID))
            {
              return;
            }
          }
          break;
      }
    }

    public PuppetMaster[] GetPuppetMasters()
    {
      PuppetMaster[] pms;
      if (numberPM < 2)
      {
        pms = new PuppetMaster[1];
        pms[0] = this;
        return pms;
      }
      else
      {
        pms = new PuppetMaster[numberPM];
        for (int i = 0; i < numberPM; i++)
        {
          int portN = 20001 + i;
          if (portN != ID)
          {
            pms[i] = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:" + portN + "/PM");
          }
          else
          {
            pms[i] = this;
          }
        }
        return pms;
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

      pm.userApp = new PADI_MapNoReduce.Program();

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new PuppetMasterForm());
    }
  }
}