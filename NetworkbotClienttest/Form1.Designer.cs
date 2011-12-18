namespace NetworkbotClienttest
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox_steamID = new System.Windows.Forms.TextBox();
            this.textBox_steamname = new System.Windows.Forms.TextBox();
            this.textBox_chattype = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox1.Location = new System.Drawing.Point(0, 578);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(742, 20);
            this.textBox1.TabIndex = 0;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(742, 518);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // textBox_steamID
            // 
            this.textBox_steamID.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox_steamID.Location = new System.Drawing.Point(0, 558);
            this.textBox_steamID.Name = "textBox_steamID";
            this.textBox_steamID.Size = new System.Drawing.Size(742, 20);
            this.textBox_steamID.TabIndex = 2;
            this.textBox_steamID.Text = "STEAM_0:1:16516144";
            // 
            // textBox_steamname
            // 
            this.textBox_steamname.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox_steamname.Location = new System.Drawing.Point(0, 538);
            this.textBox_steamname.Name = "textBox_steamname";
            this.textBox_steamname.Size = new System.Drawing.Size(742, 20);
            this.textBox_steamname.TabIndex = 3;
            this.textBox_steamname.Text = "Ced";
            // 
            // textBox_chattype
            // 
            this.textBox_chattype.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox_chattype.Location = new System.Drawing.Point(0, 518);
            this.textBox_chattype.Name = "textBox_chattype";
            this.textBox_chattype.Size = new System.Drawing.Size(742, 20);
            this.textBox_chattype.TabIndex = 4;
            this.textBox_chattype.Text = "PM";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 598);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.textBox_chattype);
            this.Controls.Add(this.textBox_steamname);
            this.Controls.Add(this.textBox_steamID);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Onymity Client tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox_steamID;
        private System.Windows.Forms.TextBox textBox_steamname;
        private System.Windows.Forms.TextBox textBox_chattype;
    }
}

