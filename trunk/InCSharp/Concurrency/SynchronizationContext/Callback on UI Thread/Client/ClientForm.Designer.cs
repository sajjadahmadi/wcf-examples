namespace WcfExamples
{
    partial class ClientForm
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
            this.countTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.IncButton = new System.Windows.Forms.Button();
            this.DecButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // countTextBox
            // 
            this.countTextBox.Location = new System.Drawing.Point(93, 53);
            this.countTextBox.Name = "countTextBox";
            this.countTextBox.Size = new System.Drawing.Size(100, 20);
            this.countTextBox.TabIndex = 3;
            this.countTextBox.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(46, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Current";
            // 
            // IncButton
            // 
            this.IncButton.Location = new System.Drawing.Point(93, 109);
            this.IncButton.Name = "IncButton";
            this.IncButton.Size = new System.Drawing.Size(75, 23);
            this.IncButton.TabIndex = 4;
            this.IncButton.Text = "Increment";
            this.IncButton.UseVisualStyleBackColor = true;
            this.IncButton.Click += new System.EventHandler(this.IncButton_Click);
            // 
            // DecButton
            // 
            this.DecButton.Location = new System.Drawing.Point(93, 138);
            this.DecButton.Name = "DecButton";
            this.DecButton.Size = new System.Drawing.Size(75, 23);
            this.DecButton.TabIndex = 5;
            this.DecButton.Text = "Decrement";
            this.DecButton.UseVisualStyleBackColor = true;
            this.DecButton.Click += new System.EventHandler(this.DecButton_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.DecButton);
            this.Controls.Add(this.IncButton);
            this.Controls.Add(this.countTextBox);
            this.Controls.Add(this.label1);
            this.Name = "ClientForm";
            this.Text = "Counter Client";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClientForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox countTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button IncButton;
        private System.Windows.Forms.Button DecButton;
    }
}

