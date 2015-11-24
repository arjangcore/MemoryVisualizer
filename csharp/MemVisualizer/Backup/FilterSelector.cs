using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MemVisualizer
{
	/// <summary>
	/// Summary description for FilterSelector.
	/// </summary>
	public class FilterSelector : System.Windows.Forms.Form
	{
        private System.Windows.Forms.TextBox mFilterText;
        /// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button mButtonOk;
		private System.Windows.Forms.Button mButtonCancel;
        private TextBox mFilterSize;
        private Label label1;
        private Label label2;
		private static String mFilter = "";
        private static int mSize = 0;
        private static bool mMaxMin = false;

		public FilterSelector()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			mFilterText.Text = mFilter;
            mFilterSize.Text = mSize.ToString();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterSelector));
            this.mFilterText = new System.Windows.Forms.TextBox();
            this.mButtonOk = new System.Windows.Forms.Button();
            this.mButtonCancel = new System.Windows.Forms.Button();
            this.mFilterSize = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mFilterText
            // 
            this.mFilterText.Location = new System.Drawing.Point(51, 0);
            this.mFilterText.Name = "mFilterText";
            this.mFilterText.Size = new System.Drawing.Size(227, 20);
            this.mFilterText.TabIndex = 0;
            // 
            // mButtonOk
            // 
            this.mButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mButtonOk.Location = new System.Drawing.Point(118, 55);
            this.mButtonOk.Name = "mButtonOk";
            this.mButtonOk.Size = new System.Drawing.Size(75, 23);
            this.mButtonOk.TabIndex = 2;
            this.mButtonOk.Text = "&Ok";
            this.mButtonOk.Click += new System.EventHandler(this.ButtonOKClick);
            // 
            // mButtonCancel
            // 
            this.mButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mButtonCancel.Location = new System.Drawing.Point(198, 55);
            this.mButtonCancel.Name = "mButtonCancel";
            this.mButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.mButtonCancel.TabIndex = 3;
            this.mButtonCancel.Text = "&Cancel";
            this.mButtonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // mFilterSize
            // 
            this.mFilterSize.Location = new System.Drawing.Point(171, 26);
            this.mFilterSize.Name = "mFilterSize";
            this.mFilterSize.Size = new System.Drawing.Size(107, 20);
            this.mFilterSize.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Size (use < or > as \'<128\'):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Tag: ";
            // 
            // FilterSelector
            // 
            this.AcceptButton = this.mButtonOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.mButtonCancel;
            this.ClientSize = new System.Drawing.Size(278, 85);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mFilterSize);
            this.Controls.Add(this.mButtonCancel);
            this.Controls.Add(this.mButtonOk);
            this.Controls.Add(this.mFilterText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FilterSelector";
            this.Text = "FilterSelector";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void ButtonOKClick(object sender, System.EventArgs e)
		{
			mFilter = mFilterText.Text;
            
            String filtersizestr = mFilterSize.Text.Trim();
            if (filtersizestr.StartsWith("<"))
            {
                mMaxMin = true; // means the size provided is the max size interested to search for.
                filtersizestr = filtersizestr.Substring(1);
            }
            else if (filtersizestr.StartsWith(">"))
            {
                mMaxMin = false; // means the size provided is the min size interested to search for.
                filtersizestr = filtersizestr.Substring(1);

            }
            else
                filtersizestr = "";

            if (filtersizestr == "")
                mSize = 0;
            else
                mSize = Convert.ToInt32(filtersizestr);

			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void ButtonCancelClick(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();		
		}

		public string GetFilter()
		{
			return mFilter;
		}

        public int GetSize()
        {
            return mSize;
        }

        public bool GetDirection()
        {
            return mMaxMin;
        }

    }	
}
