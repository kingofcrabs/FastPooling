namespace OptimizePooling
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblGridCnt = new System.Windows.Forms.Label();
            this.txtGridCnt = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.lblReadedBarcodes = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblBatchNumber = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.btnSetGridCnt = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblGridCnt
            // 
            this.lblGridCnt.AutoSize = true;
            this.lblGridCnt.Location = new System.Drawing.Point(12, 12);
            this.lblGridCnt.Name = "lblGridCnt";
            this.lblGridCnt.Size = new System.Drawing.Size(59, 12);
            this.lblGridCnt.TabIndex = 0;
            this.lblGridCnt.Text = "Grid数量:";
            // 
            // txtGridCnt
            // 
            this.txtGridCnt.Location = new System.Drawing.Point(79, 8);
            this.txtGridCnt.Name = "txtGridCnt";
            this.txtGridCnt.Size = new System.Drawing.Size(100, 21);
            this.txtGridCnt.TabIndex = 1;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(14, 61);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(565, 400);
            this.dataGridView1.TabIndex = 2;
            // 
            // lblReadedBarcodes
            // 
            this.lblReadedBarcodes.AutoSize = true;
            this.lblReadedBarcodes.Location = new System.Drawing.Point(12, 46);
            this.lblReadedBarcodes.Name = "lblReadedBarcodes";
            this.lblReadedBarcodes.Size = new System.Drawing.Size(77, 12);
            this.lblReadedBarcodes.TabIndex = 3;
            this.lblReadedBarcodes.Text = "已读取条码：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(416, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "批次号：";
            // 
            // lblBatchNumber
            // 
            this.lblBatchNumber.AutoSize = true;
            this.lblBatchNumber.Location = new System.Drawing.Point(464, 46);
            this.lblBatchNumber.Name = "lblBatchNumber";
            this.lblBatchNumber.Size = new System.Drawing.Size(0, 12);
            this.lblBatchNumber.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 468);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "提示：";
            // 
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(14, 484);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(565, 63);
            this.txtInfo.TabIndex = 7;
            // 
            // btnSetGridCnt
            // 
            this.btnSetGridCnt.Location = new System.Drawing.Point(185, 6);
            this.btnSetGridCnt.Name = "btnSetGridCnt";
            this.btnSetGridCnt.Size = new System.Drawing.Size(75, 23);
            this.btnSetGridCnt.TabIndex = 8;
            this.btnSetGridCnt.Text = "设置";
            this.btnSetGridCnt.UseVisualStyleBackColor = true;
            this.btnSetGridCnt.Click += new System.EventHandler(this.btnSetGridCnt_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 551);
            this.Controls.Add(this.btnSetGridCnt);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblBatchNumber);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblReadedBarcodes);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.txtGridCnt);
            this.Controls.Add(this.lblGridCnt);
            this.Name = "MainForm";
            this.Text = "Optimized Pooling";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblGridCnt;
        private System.Windows.Forms.TextBox txtGridCnt;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label lblReadedBarcodes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblBatchNumber;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Button btnSetGridCnt;
    }
}

