using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

using Worker_JobTracker;

/// PUPPETMASTER: Rui

namespace PuppetMaster
{
  class PuppetMaster
  {
    // int: Worker ID
    // Object: Worker instance
    private Dictionary<int, Worker_JobTracker.Worker> workerList = new Dictionary<int, Worker_JobTracker.Worker>();
    private int currentID = 0;

    // Functions

    // Launch new Worker process
    public void createWorker(string workerURL, int id = 0)
    {
      if (id <= 0 && !workerList.ContainsKey(currentID))
      {
        // add URL
        workerList.Add(currentID, new Worker_JobTracker.Worker(workerURL, currentID));
        currentID++;
      }
      else if (!workerList.ContainsKey(id))
      {
        // add URL
        workerList.Add(id, new Worker_JobTracker.Worker(workerURL, id));
      }
      else
      {
        // THROW ERROR/EXCEPTION
      }
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    //[STAThread]
    void Main()
    {
      TcpChannel channel = new TcpChannel(20001);

      createWorker("insertWorkerURL");

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new PuppetMasterForm());
    }
  }
}