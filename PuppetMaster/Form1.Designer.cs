namespace PuppetMaster
{
  partial class Form1
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.CreateWorker = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // CreateWorker
      // 
      this.CreateWorker.Location = new System.Drawing.Point(351, 87);
      this.CreateWorker.Name = "CreateWorker";
      this.CreateWorker.Size = new System.Drawing.Size(97, 23);
      this.CreateWorker.TabIndex = 0;
      this.CreateWorker.Text = "CreateWorker";
      this.CreateWorker.UseVisualStyleBackColor = true;
      this.CreateWorker.Click += new System.EventHandler(this.CreateWorker_Click);
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(12, 87);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(333, 20);
      this.textBox1.TabIndex = 1;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(482, 334);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.CreateWorker);
      this.Name = "Form1";
      this.Text = "PuppetMaster";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button CreateWorker;
    private System.Windows.Forms.TextBox textBox1;
  }
}

