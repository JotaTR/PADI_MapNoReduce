using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
  public partial class PuppetMasterForm : Form
  {
    public PuppetMasterForm()
    {
      InitializeComponent();
    }

    private void PuppetMasterForm_Load(object sender, EventArgs e)
    {

    }

    private void Submit_Click(object sender, EventArgs e)
    {
      string[] linesToInterpret = CommandTextLine.Lines;

      foreach (string textToInterpret in linesToInterpret)
      {
        char[] whitespaces = new char[] { ' ', '\t', '\n', '\r' };
        //string textToInterpret = CommandTextLine.Text;
        string[] parsedText = textToInterpret.Split(whitespaces);

        PuppetMaster targetPM;

        switch (parsedText[0])
        {
          // Submits a job to the workers/server
          case "SUBMIT":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            targetPM.SubmitJob(parsedText[1], parsedText[2],
                        parsedText[3], Int32.Parse(parsedText[4]),
                        parsedText[5], parsedText[6]
                        );
            break;
          // Creates a new worker
          case "WORKER":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), parsedText[2]);
            targetPM.CreateWorker(parsedText[3], Int32.Parse(parsedText[1]));
            break;
          // Stops execution of other command scripts
          case "WAIT":
            //targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            // Parses the seconds it has to wait, and converts them to milliseconds
            int time = Int32.Parse(parsedText[1]) * 1000;
            //targetPM.Wait(time);
            System.Threading.Thread.Sleep(time);
            break;
          // Makes all workers/job trackers submit/print their current status
          case "STATUS":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            targetPM.StatusReport();
            break;
          // Delays a given worker process
          case "SLOWW":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            targetPM.SlowWorker(Int32.Parse(parsedText[1]), Int32.Parse(parsedText[2]));
            break;
          // Disables a given worker process's Worker Functions
          case "FREEZEW":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            targetPM.CallFreeze(Int32.Parse(parsedText[1]), FreezeType.FREEZEW);
            break;
          // (Re)enables a given worker process's Worker Functions
          case "UNFREEZEW":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            targetPM.CallFreeze(Int32.Parse(parsedText[1]), FreezeType.UNFREEZEW);
            break;
          // Disables a given worker process's Job Tracker Functions
          case "FREEZEC":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            targetPM.CallFreeze(Int32.Parse(parsedText[1]), FreezeType.FREEZEC);
            break;
          // (Re)enables a given worker process's Job Tracker Functions
          case "UNFREEZEC":
            targetPM = (PuppetMaster)Activator.GetObject(typeof(PuppetMaster), "tcp://localhost:20001/PM");
            targetPM.CallFreeze(Int32.Parse(parsedText[1]), FreezeType.UNFREEZEC);
            break;
          default:
            System.Windows.Forms.MessageBox.Show("Please input a correct Command");
            break;
        }
      }
    }

  }
}
