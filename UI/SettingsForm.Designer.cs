// UI/SettingsForm.Designer.cs
namespace TextTranslationPlugin.UI
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblApiKey = new System.Windows.Forms.Label();
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.lblBaseUrl = new System.Windows.Forms.Label();
            this.txtBaseUrl = new System.Windows.Forms.TextBox();
            this.lblModel = new System.Windows.Forms.Label();
            this.txtModel = new System.Windows.Forms.TextBox();
            this.lblSystemPrompt = new System.Windows.Forms.Label();
            this.txtSystemPrompt = new System.Windows.Forms.TextBox();
            this.lblSourceLayer = new System.Windows.Forms.Label();
            this.txtSourceLayer = new System.Windows.Forms.TextBox();
            this.lblTargetLayer = new System.Windows.Forms.Label();
            this.txtTargetLayer = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblApiKey
            // 
            this.lblApiKey.AutoSize = true;
            this.lblApiKey.Location = new System.Drawing.Point(12, 15);
            this.lblApiKey.Name = "lblApiKey";
            this.lblApiKey.Size = new System.Drawing.Size(53, 12);
            this.lblApiKey.TabIndex = 0;
            this.lblApiKey.Text = "API Key:";
            // 
            // txtApiKey
            // 
            this.txtApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtApiKey.Location = new System.Drawing.Point(110, 12);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.Size = new System.Drawing.Size(462, 21);
            this.txtApiKey.TabIndex = 1;
            // 
            // lblBaseUrl
            // 
            this.lblBaseUrl.AutoSize = true;
            this.lblBaseUrl.Location = new System.Drawing.Point(12, 42);
            this.lblBaseUrl.Name = "lblBaseUrl";
            this.lblBaseUrl.Size = new System.Drawing.Size(59, 12);
            this.lblBaseUrl.TabIndex = 2;
            this.lblBaseUrl.Text = "Base URL:";
            // 
            // txtBaseUrl
            // 
            this.txtBaseUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaseUrl.Location = new System.Drawing.Point(110, 39);
            this.txtBaseUrl.Name = "txtBaseUrl";
            this.txtBaseUrl.Size = new System.Drawing.Size(462, 21);
            this.txtBaseUrl.TabIndex = 3;
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(12, 69);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(41, 12);
            this.lblModel.TabIndex = 4;
            this.lblModel.Text = "Model:";
            // 
            // txtModel
            // 
            this.txtModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModel.Location = new System.Drawing.Point(110, 66);
            this.txtModel.Name = "txtModel";
            this.txtModel.Size = new System.Drawing.Size(462, 21);
            this.txtModel.TabIndex = 5;
            // 
            // lblSystemPrompt
            // 
            this.lblSystemPrompt.AutoSize = true;
            this.lblSystemPrompt.Location = new System.Drawing.Point(12, 96);
            this.lblSystemPrompt.Name = "lblSystemPrompt";
            this.lblSystemPrompt.Size = new System.Drawing.Size(89, 12);
            this.lblSystemPrompt.TabIndex = 6;
            this.lblSystemPrompt.Text = "System Prompt:";
            // 
            // txtSystemPrompt
            // 
            this.txtSystemPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSystemPrompt.Location = new System.Drawing.Point(110, 93);
            this.txtSystemPrompt.Multiline = true;
            this.txtSystemPrompt.Name = "txtSystemPrompt";
            this.txtSystemPrompt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSystemPrompt.Size = new System.Drawing.Size(462, 120);
            this.txtSystemPrompt.TabIndex = 7;
            // 
            // lblSourceLayer
            // 
            this.lblSourceLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSourceLayer.AutoSize = true;
            this.lblSourceLayer.Location = new System.Drawing.Point(12, 222);
            this.lblSourceLayer.Name = "lblSourceLayer";
            this.lblSourceLayer.Size = new System.Drawing.Size(83, 12);
            this.lblSourceLayer.TabIndex = 8;
            this.lblSourceLayer.Text = "Source Layer:";
            // 
            // txtSourceLayer
            // 
            this.txtSourceLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceLayer.Location = new System.Drawing.Point(110, 219);
            this.txtSourceLayer.Name = "txtSourceLayer";
            this.txtSourceLayer.Size = new System.Drawing.Size(462, 21);
            this.txtSourceLayer.TabIndex = 9;
            // 
            // lblTargetLayer
            // 
            this.lblTargetLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTargetLayer.AutoSize = true;
            this.lblTargetLayer.Location = new System.Drawing.Point(12, 249);
            this.lblTargetLayer.Name = "lblTargetLayer";
            this.lblTargetLayer.Size = new System.Drawing.Size(83, 12);
            this.lblTargetLayer.TabIndex = 10;
            this.lblTargetLayer.Text = "Target Layer:";
            // 
            // txtTargetLayer
            // 
            this.txtTargetLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTargetLayer.Location = new System.Drawing.Point(110, 246);
            this.txtTargetLayer.Name = "txtTargetLayer";
            this.txtTargetLayer.Size = new System.Drawing.Size(462, 21);
            this.txtTargetLayer.TabIndex = 11;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(416, 280);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(497, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(584, 315);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtTargetLayer);
            this.Controls.Add(this.lblTargetLayer);
            this.Controls.Add(this.txtSourceLayer);
            this.Controls.Add(this.lblSourceLayer);
            this.Controls.Add(this.txtSystemPrompt);
            this.Controls.Add(this.lblSystemPrompt);
            this.Controls.Add(this.txtModel);
            this.Controls.Add(this.lblModel);
            this.Controls.Add(this.txtBaseUrl);
            this.Controls.Add(this.lblBaseUrl);
            this.Controls.Add(this.txtApiKey);
            this.Controls.Add(this.lblApiKey);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 350);
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AI翻译插件 - 设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblApiKey;
        private System.Windows.Forms.TextBox txtApiKey;
        private System.Windows.Forms.Label lblBaseUrl;
        private System.Windows.Forms.TextBox txtBaseUrl;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.TextBox txtModel;
        private System.Windows.Forms.Label lblSystemPrompt;
        private System.Windows.Forms.TextBox txtSystemPrompt;
        private System.Windows.Forms.Label lblSourceLayer;
        private System.Windows.Forms.TextBox txtSourceLayer;
        private System.Windows.Forms.Label lblTargetLayer;
        private System.Windows.Forms.TextBox txtTargetLayer;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}