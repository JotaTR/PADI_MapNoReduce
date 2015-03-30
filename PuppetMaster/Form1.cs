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
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void CreateWorker_Click(object sender, EventArgs e)
    {
      // Creates a new Worker process, and warns other Workers(Job Tracker) of new worker
    }
  }
}
