namespace RC.DssServices.Test
{
    partial class MainForm
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
            this.label9 = new System.Windows.Forms.Label();
            this.lstDssLobbies = new System.Windows.Forms.ListBox();
            this.btnLeaveDss = new System.Windows.Forms.Button();
            this.btnJoinDss = new System.Windows.Forms.Button();
            this.btnCreateDss = new System.Windows.Forms.Button();
            this.btnClose6 = new System.Windows.Forms.Button();
            this.btnOpen6 = new System.Windows.Forms.Button();
            this.btnClose5 = new System.Windows.Forms.Button();
            this.btnOpen5 = new System.Windows.Forms.Button();
            this.btnClose4 = new System.Windows.Forms.Button();
            this.btnOpen4 = new System.Windows.Forms.Button();
            this.btnClose3 = new System.Windows.Forms.Button();
            this.btnOpen3 = new System.Windows.Forms.Button();
            this.btnClose2 = new System.Windows.Forms.Button();
            this.btnOpen2 = new System.Windows.Forms.Button();
            this.btnClose1 = new System.Windows.Forms.Button();
            this.btnOpen1 = new System.Windows.Forms.Button();
            this.btnClose0 = new System.Windows.Forms.Button();
            this.btnOpen0 = new System.Windows.Forms.Button();
            this.picChannelState6 = new System.Windows.Forms.PictureBox();
            this.picChannelState5 = new System.Windows.Forms.PictureBox();
            this.picChannelState4 = new System.Windows.Forms.PictureBox();
            this.picChannelState3 = new System.Windows.Forms.PictureBox();
            this.picChannelState2 = new System.Windows.Forms.PictureBox();
            this.picChannelState1 = new System.Windows.Forms.PictureBox();
            this.picChannelState0 = new System.Windows.Forms.PictureBox();
            this.picChannelStateHost = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboColorHost = new System.Windows.Forms.ComboBox();
            this.comboColor0 = new System.Windows.Forms.ComboBox();
            this.comboColor1 = new System.Windows.Forms.ComboBox();
            this.comboColor2 = new System.Windows.Forms.ComboBox();
            this.comboColor3 = new System.Windows.Forms.ComboBox();
            this.comboColor4 = new System.Windows.Forms.ComboBox();
            this.comboColor5 = new System.Windows.Forms.ComboBox();
            this.comboColor6 = new System.Windows.Forms.ComboBox();
            this.picDisplay = new System.Windows.Forms.PictureBox();
            this.btnStartSim = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelStateHost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 280);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(142, 13);
            this.label9.TabIndex = 51;
            this.label9.Text = "DSS-lobbies on the network:";
            // 
            // lstDssLobbies
            // 
            this.lstDssLobbies.FormattingEnabled = true;
            this.lstDssLobbies.Location = new System.Drawing.Point(12, 296);
            this.lstDssLobbies.Name = "lstDssLobbies";
            this.lstDssLobbies.Size = new System.Drawing.Size(624, 121);
            this.lstDssLobbies.TabIndex = 50;
            this.lstDssLobbies.SelectedIndexChanged += new System.EventHandler(this.lstDssLobbies_SelectedIndexChanged);
            // 
            // btnLeaveDss
            // 
            this.btnLeaveDss.Location = new System.Drawing.Point(352, 423);
            this.btnLeaveDss.Name = "btnLeaveDss";
            this.btnLeaveDss.Size = new System.Drawing.Size(100, 23);
            this.btnLeaveDss.TabIndex = 55;
            this.btnLeaveDss.Text = "Leave DSS";
            this.btnLeaveDss.UseVisualStyleBackColor = true;
            this.btnLeaveDss.Click += new System.EventHandler(this.btnLeaveDss_Click);
            // 
            // btnJoinDss
            // 
            this.btnJoinDss.Location = new System.Drawing.Point(246, 423);
            this.btnJoinDss.Name = "btnJoinDss";
            this.btnJoinDss.Size = new System.Drawing.Size(100, 23);
            this.btnJoinDss.TabIndex = 54;
            this.btnJoinDss.Text = "Join to DSS";
            this.btnJoinDss.UseVisualStyleBackColor = true;
            this.btnJoinDss.Click += new System.EventHandler(this.btnJoinDss_Click);
            // 
            // btnCreateDss
            // 
            this.btnCreateDss.Location = new System.Drawing.Point(140, 423);
            this.btnCreateDss.Name = "btnCreateDss";
            this.btnCreateDss.Size = new System.Drawing.Size(100, 23);
            this.btnCreateDss.TabIndex = 53;
            this.btnCreateDss.Text = "Create DSS";
            this.btnCreateDss.UseVisualStyleBackColor = true;
            this.btnCreateDss.Click += new System.EventHandler(this.btnCreateDss_Click);
            // 
            // btnClose6
            // 
            this.btnClose6.Location = new System.Drawing.Point(157, 230);
            this.btnClose6.Name = "btnClose6";
            this.btnClose6.Size = new System.Drawing.Size(52, 23);
            this.btnClose6.TabIndex = 87;
            this.btnClose6.Text = "Close";
            this.btnClose6.UseVisualStyleBackColor = true;
            this.btnClose6.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpen6
            // 
            this.btnOpen6.Location = new System.Drawing.Point(99, 230);
            this.btnOpen6.Name = "btnOpen6";
            this.btnOpen6.Size = new System.Drawing.Size(52, 23);
            this.btnOpen6.TabIndex = 86;
            this.btnOpen6.Text = "Open";
            this.btnOpen6.UseVisualStyleBackColor = true;
            this.btnOpen6.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose5
            // 
            this.btnClose5.Location = new System.Drawing.Point(157, 199);
            this.btnClose5.Name = "btnClose5";
            this.btnClose5.Size = new System.Drawing.Size(52, 23);
            this.btnClose5.TabIndex = 85;
            this.btnClose5.Text = "Close";
            this.btnClose5.UseVisualStyleBackColor = true;
            this.btnClose5.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpen5
            // 
            this.btnOpen5.Location = new System.Drawing.Point(99, 199);
            this.btnOpen5.Name = "btnOpen5";
            this.btnOpen5.Size = new System.Drawing.Size(52, 23);
            this.btnOpen5.TabIndex = 84;
            this.btnOpen5.Text = "Open";
            this.btnOpen5.UseVisualStyleBackColor = true;
            this.btnOpen5.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose4
            // 
            this.btnClose4.Location = new System.Drawing.Point(157, 168);
            this.btnClose4.Name = "btnClose4";
            this.btnClose4.Size = new System.Drawing.Size(52, 23);
            this.btnClose4.TabIndex = 83;
            this.btnClose4.Text = "Close";
            this.btnClose4.UseVisualStyleBackColor = true;
            this.btnClose4.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpen4
            // 
            this.btnOpen4.Location = new System.Drawing.Point(99, 168);
            this.btnOpen4.Name = "btnOpen4";
            this.btnOpen4.Size = new System.Drawing.Size(52, 23);
            this.btnOpen4.TabIndex = 82;
            this.btnOpen4.Text = "Open";
            this.btnOpen4.UseVisualStyleBackColor = true;
            this.btnOpen4.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose3
            // 
            this.btnClose3.Location = new System.Drawing.Point(157, 137);
            this.btnClose3.Name = "btnClose3";
            this.btnClose3.Size = new System.Drawing.Size(52, 23);
            this.btnClose3.TabIndex = 81;
            this.btnClose3.Text = "Close";
            this.btnClose3.UseVisualStyleBackColor = true;
            this.btnClose3.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpen3
            // 
            this.btnOpen3.Location = new System.Drawing.Point(99, 137);
            this.btnOpen3.Name = "btnOpen3";
            this.btnOpen3.Size = new System.Drawing.Size(52, 23);
            this.btnOpen3.TabIndex = 80;
            this.btnOpen3.Text = "Open";
            this.btnOpen3.UseVisualStyleBackColor = true;
            this.btnOpen3.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose2
            // 
            this.btnClose2.Location = new System.Drawing.Point(157, 106);
            this.btnClose2.Name = "btnClose2";
            this.btnClose2.Size = new System.Drawing.Size(52, 23);
            this.btnClose2.TabIndex = 79;
            this.btnClose2.Text = "Close";
            this.btnClose2.UseVisualStyleBackColor = true;
            this.btnClose2.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpen2
            // 
            this.btnOpen2.Location = new System.Drawing.Point(99, 106);
            this.btnOpen2.Name = "btnOpen2";
            this.btnOpen2.Size = new System.Drawing.Size(52, 23);
            this.btnOpen2.TabIndex = 78;
            this.btnOpen2.Text = "Open";
            this.btnOpen2.UseVisualStyleBackColor = true;
            this.btnOpen2.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose1
            // 
            this.btnClose1.Location = new System.Drawing.Point(157, 75);
            this.btnClose1.Name = "btnClose1";
            this.btnClose1.Size = new System.Drawing.Size(52, 23);
            this.btnClose1.TabIndex = 77;
            this.btnClose1.Text = "Close";
            this.btnClose1.UseVisualStyleBackColor = true;
            this.btnClose1.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpen1
            // 
            this.btnOpen1.Location = new System.Drawing.Point(99, 75);
            this.btnOpen1.Name = "btnOpen1";
            this.btnOpen1.Size = new System.Drawing.Size(52, 23);
            this.btnOpen1.TabIndex = 76;
            this.btnOpen1.Text = "Open";
            this.btnOpen1.UseVisualStyleBackColor = true;
            this.btnOpen1.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose0
            // 
            this.btnClose0.Location = new System.Drawing.Point(157, 44);
            this.btnClose0.Name = "btnClose0";
            this.btnClose0.Size = new System.Drawing.Size(52, 23);
            this.btnClose0.TabIndex = 75;
            this.btnClose0.Text = "Close";
            this.btnClose0.UseVisualStyleBackColor = true;
            this.btnClose0.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpen0
            // 
            this.btnOpen0.Location = new System.Drawing.Point(99, 44);
            this.btnOpen0.Name = "btnOpen0";
            this.btnOpen0.Size = new System.Drawing.Size(52, 23);
            this.btnOpen0.TabIndex = 74;
            this.btnOpen0.Text = "Open";
            this.btnOpen0.UseVisualStyleBackColor = true;
            this.btnOpen0.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // picChannelState6
            // 
            this.picChannelState6.Location = new System.Drawing.Point(68, 228);
            this.picChannelState6.Name = "picChannelState6";
            this.picChannelState6.Size = new System.Drawing.Size(25, 25);
            this.picChannelState6.TabIndex = 71;
            this.picChannelState6.TabStop = false;
            // 
            // picChannelState5
            // 
            this.picChannelState5.Location = new System.Drawing.Point(68, 197);
            this.picChannelState5.Name = "picChannelState5";
            this.picChannelState5.Size = new System.Drawing.Size(25, 25);
            this.picChannelState5.TabIndex = 70;
            this.picChannelState5.TabStop = false;
            // 
            // picChannelState4
            // 
            this.picChannelState4.Location = new System.Drawing.Point(68, 166);
            this.picChannelState4.Name = "picChannelState4";
            this.picChannelState4.Size = new System.Drawing.Size(25, 25);
            this.picChannelState4.TabIndex = 69;
            this.picChannelState4.TabStop = false;
            // 
            // picChannelState3
            // 
            this.picChannelState3.Location = new System.Drawing.Point(68, 135);
            this.picChannelState3.Name = "picChannelState3";
            this.picChannelState3.Size = new System.Drawing.Size(25, 25);
            this.picChannelState3.TabIndex = 68;
            this.picChannelState3.TabStop = false;
            // 
            // picChannelState2
            // 
            this.picChannelState2.Location = new System.Drawing.Point(68, 104);
            this.picChannelState2.Name = "picChannelState2";
            this.picChannelState2.Size = new System.Drawing.Size(25, 25);
            this.picChannelState2.TabIndex = 67;
            this.picChannelState2.TabStop = false;
            // 
            // picChannelState1
            // 
            this.picChannelState1.Location = new System.Drawing.Point(68, 73);
            this.picChannelState1.Name = "picChannelState1";
            this.picChannelState1.Size = new System.Drawing.Size(25, 25);
            this.picChannelState1.TabIndex = 66;
            this.picChannelState1.TabStop = false;
            // 
            // picChannelState0
            // 
            this.picChannelState0.Location = new System.Drawing.Point(68, 42);
            this.picChannelState0.Name = "picChannelState0";
            this.picChannelState0.Size = new System.Drawing.Size(25, 25);
            this.picChannelState0.TabIndex = 65;
            this.picChannelState0.TabStop = false;
            // 
            // picChannelStateHost
            // 
            this.picChannelStateHost.InitialImage = null;
            this.picChannelStateHost.Location = new System.Drawing.Point(68, 11);
            this.picChannelStateHost.Name = "picChannelStateHost";
            this.picChannelStateHost.Size = new System.Drawing.Size(25, 25);
            this.picChannelStateHost.TabIndex = 64;
            this.picChannelStateHost.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 240);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 63;
            this.label8.Text = "Guest 6:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 209);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 13);
            this.label7.TabIndex = 62;
            this.label7.Text = "Guest 5:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 178);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 61;
            this.label6.Text = "Guest 4:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 60;
            this.label5.Text = "Guest 3:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 59;
            this.label4.Text = "Guest 2:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 58;
            this.label3.Text = "Guest 1:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 57;
            this.label2.Text = "Guest 0:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 56;
            this.label1.Text = "Host:";
            // 
            // comboColorHost
            // 
            this.comboColorHost.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColorHost.FormattingEnabled = true;
            this.comboColorHost.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColorHost.Location = new System.Drawing.Point(215, 14);
            this.comboColorHost.Name = "comboColorHost";
            this.comboColorHost.Size = new System.Drawing.Size(88, 21);
            this.comboColorHost.TabIndex = 88;
            this.comboColorHost.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // comboColor0
            // 
            this.comboColor0.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor0.FormattingEnabled = true;
            this.comboColor0.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColor0.Location = new System.Drawing.Point(215, 46);
            this.comboColor0.Name = "comboColor0";
            this.comboColor0.Size = new System.Drawing.Size(88, 21);
            this.comboColor0.TabIndex = 89;
            this.comboColor0.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // comboColor1
            // 
            this.comboColor1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor1.FormattingEnabled = true;
            this.comboColor1.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColor1.Location = new System.Drawing.Point(215, 77);
            this.comboColor1.Name = "comboColor1";
            this.comboColor1.Size = new System.Drawing.Size(88, 21);
            this.comboColor1.TabIndex = 90;
            this.comboColor1.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // comboColor2
            // 
            this.comboColor2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor2.FormattingEnabled = true;
            this.comboColor2.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColor2.Location = new System.Drawing.Point(215, 108);
            this.comboColor2.Name = "comboColor2";
            this.comboColor2.Size = new System.Drawing.Size(88, 21);
            this.comboColor2.TabIndex = 91;
            this.comboColor2.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // comboColor3
            // 
            this.comboColor3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor3.FormattingEnabled = true;
            this.comboColor3.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColor3.Location = new System.Drawing.Point(215, 139);
            this.comboColor3.Name = "comboColor3";
            this.comboColor3.Size = new System.Drawing.Size(88, 21);
            this.comboColor3.TabIndex = 92;
            this.comboColor3.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // comboColor4
            // 
            this.comboColor4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor4.FormattingEnabled = true;
            this.comboColor4.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColor4.Location = new System.Drawing.Point(215, 170);
            this.comboColor4.Name = "comboColor4";
            this.comboColor4.Size = new System.Drawing.Size(88, 21);
            this.comboColor4.TabIndex = 93;
            this.comboColor4.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // comboColor5
            // 
            this.comboColor5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor5.FormattingEnabled = true;
            this.comboColor5.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColor5.Location = new System.Drawing.Point(215, 201);
            this.comboColor5.Name = "comboColor5";
            this.comboColor5.Size = new System.Drawing.Size(88, 21);
            this.comboColor5.TabIndex = 94;
            this.comboColor5.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // comboColor6
            // 
            this.comboColor6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor6.FormattingEnabled = true;
            this.comboColor6.Items.AddRange(new object[] {
            "White",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Cyan",
            "Orange",
            "Magenta"});
            this.comboColor6.Location = new System.Drawing.Point(215, 232);
            this.comboColor6.Name = "comboColor6";
            this.comboColor6.Size = new System.Drawing.Size(88, 21);
            this.comboColor6.TabIndex = 95;
            this.comboColor6.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            // 
            // picDisplay
            // 
            this.picDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picDisplay.Location = new System.Drawing.Point(309, 11);
            this.picDisplay.Name = "picDisplay";
            this.picDisplay.Size = new System.Drawing.Size(327, 242);
            this.picDisplay.TabIndex = 96;
            this.picDisplay.TabStop = false;
            // 
            // btnStartSim
            // 
            this.btnStartSim.Location = new System.Drawing.Point(422, 259);
            this.btnStartSim.Name = "btnStartSim";
            this.btnStartSim.Size = new System.Drawing.Size(106, 23);
            this.btnStartSim.TabIndex = 97;
            this.btnStartSim.Text = "Start simulation";
            this.btnStartSim.UseVisualStyleBackColor = true;
            this.btnStartSim.Click += new System.EventHandler(this.btnStartSim_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 451);
            this.Controls.Add(this.btnStartSim);
            this.Controls.Add(this.picDisplay);
            this.Controls.Add(this.comboColor6);
            this.Controls.Add(this.comboColor5);
            this.Controls.Add(this.comboColor4);
            this.Controls.Add(this.comboColor3);
            this.Controls.Add(this.comboColor2);
            this.Controls.Add(this.comboColor1);
            this.Controls.Add(this.comboColor0);
            this.Controls.Add(this.comboColorHost);
            this.Controls.Add(this.btnClose6);
            this.Controls.Add(this.btnOpen6);
            this.Controls.Add(this.btnClose5);
            this.Controls.Add(this.btnOpen5);
            this.Controls.Add(this.btnClose4);
            this.Controls.Add(this.btnOpen4);
            this.Controls.Add(this.btnClose3);
            this.Controls.Add(this.btnOpen3);
            this.Controls.Add(this.btnClose2);
            this.Controls.Add(this.btnOpen2);
            this.Controls.Add(this.btnClose1);
            this.Controls.Add(this.btnOpen1);
            this.Controls.Add(this.btnClose0);
            this.Controls.Add(this.btnOpen0);
            this.Controls.Add(this.picChannelState6);
            this.Controls.Add(this.picChannelState5);
            this.Controls.Add(this.picChannelState4);
            this.Controls.Add(this.picChannelState3);
            this.Controls.Add(this.picChannelState2);
            this.Controls.Add(this.picChannelState1);
            this.Controls.Add(this.picChannelState0);
            this.Controls.Add(this.picChannelStateHost);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLeaveDss);
            this.Controls.Add(this.btnJoinDss);
            this.Controls.Add(this.btnCreateDss);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lstDssLobbies);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Test program: RC.DssServices";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelState0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChannelStateHost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListBox lstDssLobbies;
        private System.Windows.Forms.Button btnLeaveDss;
        private System.Windows.Forms.Button btnJoinDss;
        private System.Windows.Forms.Button btnCreateDss;
        private System.Windows.Forms.Button btnClose6;
        private System.Windows.Forms.Button btnOpen6;
        private System.Windows.Forms.Button btnClose5;
        private System.Windows.Forms.Button btnOpen5;
        private System.Windows.Forms.Button btnClose4;
        private System.Windows.Forms.Button btnOpen4;
        private System.Windows.Forms.Button btnClose3;
        private System.Windows.Forms.Button btnOpen3;
        private System.Windows.Forms.Button btnClose2;
        private System.Windows.Forms.Button btnOpen2;
        private System.Windows.Forms.Button btnClose1;
        private System.Windows.Forms.Button btnOpen1;
        private System.Windows.Forms.Button btnClose0;
        private System.Windows.Forms.Button btnOpen0;
        private System.Windows.Forms.PictureBox picChannelState6;
        private System.Windows.Forms.PictureBox picChannelState5;
        private System.Windows.Forms.PictureBox picChannelState4;
        private System.Windows.Forms.PictureBox picChannelState3;
        private System.Windows.Forms.PictureBox picChannelState2;
        private System.Windows.Forms.PictureBox picChannelState1;
        private System.Windows.Forms.PictureBox picChannelState0;
        private System.Windows.Forms.PictureBox picChannelStateHost;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboColorHost;
        private System.Windows.Forms.ComboBox comboColor0;
        private System.Windows.Forms.ComboBox comboColor1;
        private System.Windows.Forms.ComboBox comboColor2;
        private System.Windows.Forms.ComboBox comboColor3;
        private System.Windows.Forms.ComboBox comboColor4;
        private System.Windows.Forms.ComboBox comboColor5;
        private System.Windows.Forms.ComboBox comboColor6;
        private System.Windows.Forms.PictureBox picDisplay;
        private System.Windows.Forms.Button btnStartSim;
    }
}

