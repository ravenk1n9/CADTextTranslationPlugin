// UI/AboutForm.cs
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace TextTranslationPlugin.UI
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            LoadAssemblyInfo();
        }

        private void LoadAssemblyInfo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var titleAttribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (titleAttribute != null)
            {
                lblTitle.Text = titleAttribute.Title;
            }

            lblVersion.Text = $"版本: {assembly.GetName().Version}";

            var descriptionAttribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                string fullDescription = descriptionAttribute.Description;
                lblDescription.Text = fullDescription;

                const string urlPrefix = "https://";
                int linkStart = fullDescription.IndexOf(urlPrefix);

                if (linkStart != -1)
                {
                    int linkLength = fullDescription.Length - linkStart;
                    lblDescription.LinkArea = new LinkArea(linkStart, linkLength);
                }
            }

        }

        private void lblDescription_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string url = e.Link.LinkData as string ?? lblDescription.Text.Substring(e.Link.Start, e.Link.Length);
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}