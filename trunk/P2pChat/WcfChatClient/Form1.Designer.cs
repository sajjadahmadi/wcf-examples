namespace Microsoft.ServiceModel.Samples
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
            this.displayNameTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.receiveBox = new System.Windows.Forms.RichTextBox();
            this.sendText = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // displayNameTextBox
            // 
            this.displayNameTextBox.Location = new System.Drawing.Point(114, 74);
            this.displayNameTextBox.Name = "displayNameTextBox";
            this.displayNameTextBox.Size = new System.Drawing.Size(130, 20);
            this.displayNameTextBox.TabIndex = 0;
            this.displayNameTextBox.Text = "Anonymous";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(96, 126);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 1;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Display Name";
            // 
            // receiveText
            // 
            this.receiveBox.Location = new System.Drawing.Point(39, 177);
            this.receiveBox.Name = "receiveText";
            this.receiveBox.Size = new System.Drawing.Size(271, 96);
            this.receiveBox.TabIndex = 3;
            this.receiveBox.Text = "";
            // 
            // sendText
            // 
            this.sendText.Location = new System.Drawing.Point(39, 288);
            this.sendText.Name = "sendText";
            this.sendText.Size = new System.Drawing.Size(271, 29);
            this.sendText.TabIndex = 4;
            this.sendText.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 408);
            this.Controls.Add(this.sendText);
            this.Controls.Add(this.receiveBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.displayNameTextBox);
            this.Name = "Form1";
            this.Text = "Chat Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox displayNameTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox receiveBox;
        private System.Windows.Forms.RichTextBox sendText;
    }
}

