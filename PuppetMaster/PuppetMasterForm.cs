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
      char[] whitespaces = new char[] { ' ', '\t', '\n', '\r' };
      string textToInterpret = CommandTextLine.Text;
      string[] parsedText = textToInterpret.Split(whitespaces);

      switch (parsedText[0])
      {
        // Submits a job to the workers/server
        case "SUBMIT": 
          break;
        // Creates a new worker
        case "WORKER": 
          break;
        // Stops execution of other command scripts
        case "WAIT": break;
        // Makes all workers/job trackers submit/print their current status
        case "STATUS": break;
        // Delays a given worker process
        case "SLOWW": break;
        // Disables a given worker process's Worker Functions
        case "FREEZEW": break;
        // (Re)enables a given worker process's Worker Functions
        case "UNFREEZEW": break;
        // Disables a given worker process's Job Tracker Functions
        case "FREEZEC": break;
        // (Re)enables a given worker process's Job Tracker Functions
        case "UNFREEZEC": break;
        default:
          System.Windows.Forms.MessageBox.Show("Please input a correct Command");
          break; 
      }
    }
  }
}
