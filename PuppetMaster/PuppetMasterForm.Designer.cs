namespace PuppetMaster
{
  partial class PuppetMasterForm
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
      this.CommandTextLine = new System.Windows.Forms.TextBox();
      this.Submit = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // CommandTextLine
      // 
      this.CommandTextLine.AcceptsReturn = true;
      this.CommandTextLine.AcceptsTab = true;
      this.CommandTextLine.AllowDrop = true;
      this.CommandTextLine.Location = new System.Drawing.Point(12, 12);
      this.CommandTextLine.Multiline = true;
      this.CommandTextLine.Name = "CommandTextLine";
      this.CommandTextLine.Size = new System.Drawing.Size(333, 310);
      this.CommandTextLine.TabIndex = 1;
      // 
      // Submit
      // 
      this.Submit.Location = new System.Drawing.Point(378, 85);
      this.Submit.Name = "Submit";
      this.Submit.Size = new System.Drawing.Size(75, 23);
      this.Submit.TabIndex = 2;
      this.Submit.Text = "Submit";
      this.Submit.UseVisualStyleBackColor = true;
      this.Submit.Click += new System.EventHandler(this.Submit_Click);
      // 
      // PuppetMasterForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(482, 334);
      this.Controls.Add(this.Submit);
      this.Controls.Add(this.CommandTextLine);
      this.Name = "PuppetMasterForm";
      this.Text = "PuppetMaster";
      this.Load += new System.EventHandler(this.PuppetMasterForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox CommandTextLine;
    private System.Windows.Forms.Button Submit;
  }
}

