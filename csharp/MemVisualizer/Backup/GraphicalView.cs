using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MemManager.Log;

namespace MemVisualizer
{
	/// <summary>
	/// Summary description for GraphicalView.
	/// </summary>
	public class GraphicalView : System.Windows.Forms.Form
	{
		private GVstruct mInfo2Graph;
		private Pen mPen;
		private SolidBrush mSolidBrush; 
		private const uint mSmallestBlock = 32;
		private const uint mLargestBlock = 32768;
		private const uint mBlockWidth  = 10;
		private const uint mBlockHeight = 10;
		private uint mBytesInBlock;
		private UInt16 [] mMemoryMap;
		private string [] mStatsLog;
        private UInt16[] mBytesInBlockCategory;
		private System.Windows.Forms.PictureBox mMemoryView;
		private MemVisualizer.MainWindow mMainViewWindow = null;
		private Log mLog;
		SnapShot mLogSnap;
        SnapShot mHighlightLogSnap = null;
		private ArrayList mAllocators;
        private ArrayList mCategories;
		private System.Windows.Forms.Panel mEnclosingPanel;
		private bool mHasMemoryBlockOccupancyBeenCalculated;
		private int mViewWidth = 1280 * 2;
		private int mViewHeight = 1024;
		private const int mMinWidth = 600;
		private const int mMinHeight = 700;
		private bool mWindowUpdate;
		private bool mPopulateList;
		private Bitmap mOffScreenBmp;
		private Graphics mOffScreenDC = null;
		private uint mStartIndexOfHeap;
		private uint mEndIndexOfHeap;
        private uint mNumberBlocks;
        private uint mBlocksInRow;
        private int mSelectedCategory;
		private String mCurrentAllocatorName;
        private uint mLowerBound = 0, mUpperBound = 0;
        private int mHighlightedLogEntry = -1;

		private System.Windows.Forms.GroupBox StatsBox;
		private System.Windows.Forms.ComboBox mBlockSizeMenu;
		private System.Windows.Forms.Label mLabelBlockSize;
		private System.Windows.Forms.ComboBox mAllocatorsSelect;
		private System.Windows.Forms.Label mLabelAllocators;
        private System.Windows.Forms.ComboBox mCatSelectBox;
        private System.Windows.Forms.Label mCatLabel;
        private System.Windows.Forms.Panel mInfoPanel;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox3;
		private System.Windows.Forms.PictureBox pictureBox4;
		private System.Windows.Forms.PictureBox pictureBox5;
		private System.Windows.Forms.Label mMFOC;
		private System.Windows.Forms.Label mMFIC;
		private System.Windows.Forms.Label mMEIC;
		private System.Windows.Forms.Label mMOA;
		private System.Windows.Forms.TextBox mStats;
		private System.Windows.Forms.GroupBox mAllocatorsAndBlockSize;

        private System.Threading.Mutex BlockSizeSelectionMutex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private TextBox mSearchAddTxt;
        private TextBox mSearchTagTxt;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public GraphicalView(MemVisualizer.MainWindow mainViewerWindow)
        {
            BlockSizeSelectionMutex = new System.Threading.Mutex();
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // TODO: Add any constructor code after InitializeComponent call
            mMainViewWindow = mainViewerWindow;

            mHasMemoryBlockOccupancyBeenCalculated = true;
            mWindowUpdate = false;
            mPopulateList = true;

            mInfo2Graph = new GVstruct();
            mPen = new Pen(Color.White, 1);
            mSolidBrush = new SolidBrush(Color.White);

            mStartIndexOfHeap = uint.MaxValue;
            mNumberBlocks = 0;
            mBytesInBlock = 8192 * 2;
            mBlocksInRow = 0;
            mEndIndexOfHeap = 0;

            // Set resize redraw
            SetStyle(ControlStyles.UserPaint, true);
            CreateBitmap(mMemoryView.Width, (int)mBlockHeight);
        }

        public void SetWindowSize(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        public void SetWindowLocation(Point DesktopLocation)
        {
            this.DesktopLocation = DesktopLocation;
        }

		public void FreeResources()
		{
			mOffScreenBmp.Dispose();
			mOffScreenDC.Dispose();
			mPen.Dispose();
			mSolidBrush.Dispose();
		}

		private void CreateBitmap(int width, int height)
		{
			if (mOffScreenDC != null)
			{
				mOffScreenBmp.Dispose();
				mOffScreenDC.Dispose();
			}
			mViewWidth = width;
			mViewHeight = height;
			mOffScreenBmp = new Bitmap(mViewWidth, mViewHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb); 
			mOffScreenDC = Graphics.FromImage(mOffScreenBmp);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
            FreeResources();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphicalView));
            this.mMemoryView = new System.Windows.Forms.PictureBox();
            this.mEnclosingPanel = new System.Windows.Forms.Panel();
            this.mInfoPanel = new System.Windows.Forms.Panel();
            this.StatsBox = new System.Windows.Forms.GroupBox();
            this.mStats = new System.Windows.Forms.TextBox();
            this.mAllocatorsAndBlockSize = new System.Windows.Forms.GroupBox();
            this.mCatSelectBox = new System.Windows.Forms.ComboBox();
            this.mCatLabel = new System.Windows.Forms.Label();
            this.mMOA = new System.Windows.Forms.Label();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.mMEIC = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.mMFIC = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.mMFOC = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.mBlockSizeMenu = new System.Windows.Forms.ComboBox();
            this.mLabelBlockSize = new System.Windows.Forms.Label();
            this.mAllocatorsSelect = new System.Windows.Forms.ComboBox();
            this.mLabelAllocators = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.mSearchAddTxt = new System.Windows.Forms.TextBox();
            this.mSearchTagTxt = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.mMemoryView)).BeginInit();
            this.mEnclosingPanel.SuspendLayout();
            this.mInfoPanel.SuspendLayout();
            this.StatsBox.SuspendLayout();
            this.mAllocatorsAndBlockSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // mMemoryView
            // 
            this.mMemoryView.BackColor = System.Drawing.Color.White;
            this.mMemoryView.Location = new System.Drawing.Point(0, 0);
            this.mMemoryView.Name = "mMemoryView";
            this.mMemoryView.Size = new System.Drawing.Size(837, 577);
            this.mMemoryView.TabIndex = 0;
            this.mMemoryView.TabStop = false;
            this.mMemoryView.Click += new System.EventHandler(this.BlockInfoGet);
            this.mMemoryView.Paint += new System.Windows.Forms.PaintEventHandler(this.mMemoryView_Paint);
            // 
            // mEnclosingPanel
            // 
            this.mEnclosingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mEnclosingPanel.AutoScroll = true;
            this.mEnclosingPanel.Controls.Add(this.mMemoryView);
            this.mEnclosingPanel.Location = new System.Drawing.Point(0, 0);
            this.mEnclosingPanel.Name = "mEnclosingPanel";
            this.mEnclosingPanel.Size = new System.Drawing.Size(839, 580);
            this.mEnclosingPanel.TabIndex = 1;
            this.mEnclosingPanel.Resize += new System.EventHandler(this.OnResize);
            // 
            // mInfoPanel
            // 
            this.mInfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mInfoPanel.Controls.Add(this.mSearchTagTxt);
            this.mInfoPanel.Controls.Add(this.StatsBox);
            this.mInfoPanel.Controls.Add(this.label2);
            this.mInfoPanel.Controls.Add(this.mSearchAddTxt);
            this.mInfoPanel.Controls.Add(this.mAllocatorsAndBlockSize);
            this.mInfoPanel.Controls.Add(this.label1);
            this.mInfoPanel.Location = new System.Drawing.Point(0, 577);
            this.mInfoPanel.Name = "mInfoPanel";
            this.mInfoPanel.Size = new System.Drawing.Size(843, 271);
            this.mInfoPanel.TabIndex = 2;
            // 
            // StatsBox
            // 
            this.StatsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.StatsBox.Controls.Add(this.mStats);
            this.StatsBox.ForeColor = System.Drawing.Color.Black;
            this.StatsBox.Location = new System.Drawing.Point(320, 8);
            this.StatsBox.Name = "StatsBox";
            this.StatsBox.Size = new System.Drawing.Size(517, 224);
            this.StatsBox.TabIndex = 4;
            this.StatsBox.TabStop = false;
            this.StatsBox.Text = "Allocations For Block";
            // 
            // mStats
            // 
            this.mStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mStats.Location = new System.Drawing.Point(3, 19);
            this.mStats.Multiline = true;
            this.mStats.Name = "mStats";
            this.mStats.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mStats.Size = new System.Drawing.Size(511, 202);
            this.mStats.TabIndex = 0;
            // 
            // mAllocatorsAndBlockSize
            // 
            this.mAllocatorsAndBlockSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mAllocatorsAndBlockSize.Controls.Add(this.mCatSelectBox);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mCatLabel);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mMOA);
            this.mAllocatorsAndBlockSize.Controls.Add(this.pictureBox5);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mMEIC);
            this.mAllocatorsAndBlockSize.Controls.Add(this.pictureBox4);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mMFIC);
            this.mAllocatorsAndBlockSize.Controls.Add(this.pictureBox3);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mMFOC);
            this.mAllocatorsAndBlockSize.Controls.Add(this.pictureBox1);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mBlockSizeMenu);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mLabelBlockSize);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mAllocatorsSelect);
            this.mAllocatorsAndBlockSize.Controls.Add(this.mLabelAllocators);
            this.mAllocatorsAndBlockSize.ForeColor = System.Drawing.Color.Black;
            this.mAllocatorsAndBlockSize.Location = new System.Drawing.Point(0, 8);
            this.mAllocatorsAndBlockSize.Name = "mAllocatorsAndBlockSize";
            this.mAllocatorsAndBlockSize.Size = new System.Drawing.Size(378, 224);
            this.mAllocatorsAndBlockSize.TabIndex = 3;
            this.mAllocatorsAndBlockSize.TabStop = false;
            this.mAllocatorsAndBlockSize.Text = "Allocators and Block Size";
            // 
            // mCatSelectBox
            // 
            this.mCatSelectBox.Location = new System.Drawing.Point(96, 64);
            this.mCatSelectBox.Name = "mCatSelectBox";
            this.mCatSelectBox.Size = new System.Drawing.Size(208, 24);
            this.mCatSelectBox.TabIndex = 19;
            this.mCatSelectBox.Text = "None";
            this.mCatSelectBox.SelectedIndexChanged += new System.EventHandler(this.CategorySelectedCB);
            // 
            // mCatLabel
            // 
            this.mCatLabel.BackColor = System.Drawing.Color.LightGray;
            this.mCatLabel.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mCatLabel.ForeColor = System.Drawing.Color.Black;
            this.mCatLabel.Location = new System.Drawing.Point(-13, 64);
            this.mCatLabel.Name = "mCatLabel";
            this.mCatLabel.Size = new System.Drawing.Size(112, 16);
            this.mCatLabel.TabIndex = 18;
            this.mCatLabel.Text = "Categories";
            this.mCatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mMOA
            // 
            this.mMOA.Location = new System.Drawing.Point(32, 202);
            this.mMOA.Name = "mMOA";
            this.mMOA.Size = new System.Drawing.Size(248, 16);
            this.mMOA.TabIndex = 17;
            this.mMOA.Text = "Memory Outside Allocator";
            // 
            // pictureBox5
            // 
            this.pictureBox5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pictureBox5.Location = new System.Drawing.Point(8, 202);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(16, 16);
            this.pictureBox5.TabIndex = 16;
            this.pictureBox5.TabStop = false;
            // 
            // mMEIC
            // 
            this.mMEIC.Location = new System.Drawing.Point(32, 180);
            this.mMEIC.Name = "mMEIC";
            this.mMEIC.Size = new System.Drawing.Size(248, 16);
            this.mMEIC.TabIndex = 15;
            this.mMEIC.Text = "Memory Empty";
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pictureBox4.Location = new System.Drawing.Point(8, 180);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(16, 16);
            this.pictureBox4.TabIndex = 14;
            this.pictureBox4.TabStop = false;
            // 
            // mMFIC
            // 
            this.mMFIC.Location = new System.Drawing.Point(30, 158);
            this.mMFIC.Name = "mMFIC";
            this.mMFIC.Size = new System.Drawing.Size(248, 16);
            this.mMFIC.TabIndex = 13;
            this.mMFIC.Text = "(Gradient)Memory Fill Inside Category";
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Tomato;
            this.pictureBox3.Location = new System.Drawing.Point(8, 158);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(16, 16);
            this.pictureBox3.TabIndex = 12;
            this.pictureBox3.TabStop = false;
            // 
            // mMFOC
            // 
            this.mMFOC.Location = new System.Drawing.Point(32, 136);
            this.mMFOC.Name = "mMFOC";
            this.mMFOC.Size = new System.Drawing.Size(256, 16);
            this.mMFOC.TabIndex = 9;
            this.mMFOC.Text = "(Gradient)Memory Fill Outside Category";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.pictureBox1.Location = new System.Drawing.Point(8, 136);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // mBlockSizeMenu
            // 
            this.mBlockSizeMenu.Location = new System.Drawing.Point(96, 98);
            this.mBlockSizeMenu.Name = "mBlockSizeMenu";
            this.mBlockSizeMenu.Size = new System.Drawing.Size(208, 24);
            this.mBlockSizeMenu.TabIndex = 7;
            this.mBlockSizeMenu.Text = "2048";
            this.mBlockSizeMenu.SelectedIndexChanged += new System.EventHandler(this.BlockSizeChange);
            // 
            // mLabelBlockSize
            // 
            this.mLabelBlockSize.BackColor = System.Drawing.Color.LightGray;
            this.mLabelBlockSize.Font = new System.Drawing.Font("Arial Black", 9.75F);
            this.mLabelBlockSize.ForeColor = System.Drawing.Color.Black;
            this.mLabelBlockSize.Location = new System.Drawing.Point(3, 100);
            this.mLabelBlockSize.Name = "mLabelBlockSize";
            this.mLabelBlockSize.Size = new System.Drawing.Size(96, 16);
            this.mLabelBlockSize.TabIndex = 6;
            this.mLabelBlockSize.Text = "Block Size";
            // 
            // mAllocatorsSelect
            // 
            this.mAllocatorsSelect.Location = new System.Drawing.Point(96, 24);
            this.mAllocatorsSelect.Name = "mAllocatorsSelect";
            this.mAllocatorsSelect.Size = new System.Drawing.Size(208, 24);
            this.mAllocatorsSelect.TabIndex = 5;
            this.mAllocatorsSelect.Text = "None";
            this.mAllocatorsSelect.SelectedIndexChanged += new System.EventHandler(this.AllocatorSelectedCB);
            // 
            // mLabelAllocators
            // 
            this.mLabelAllocators.BackColor = System.Drawing.Color.LightGray;
            this.mLabelAllocators.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mLabelAllocators.ForeColor = System.Drawing.Color.Black;
            this.mLabelAllocators.Location = new System.Drawing.Point(-16, 32);
            this.mLabelAllocators.Name = "mLabelAllocators";
            this.mLabelAllocators.Size = new System.Drawing.Size(112, 16);
            this.mLabelAllocators.TabIndex = 4;
            this.mLabelAllocators.Text = "Allocators";
            this.mLabelAllocators.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.LightGray;
            this.label1.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(-3, 241);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 27);
            this.label1.TabIndex = 5;
            this.label1.Text = "Search Address :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.LightGray;
            this.label2.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(349, 241);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 27);
            this.label2.TabIndex = 7;
            this.label2.Text = "Search Tag :";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mSearchAddTxt
            // 
            this.mSearchAddTxt.Location = new System.Drawing.Point(148, 243);
            this.mSearchAddTxt.Name = "mSearchAddTxt";
            this.mSearchAddTxt.Size = new System.Drawing.Size(152, 23);
            this.mSearchAddTxt.TabIndex = 8;
            this.mSearchAddTxt.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SearchAddressCB);
            // 
            // mSearchTagTxt
            // 
            this.mSearchTagTxt.Location = new System.Drawing.Point(471, 241);
            this.mSearchTagTxt.MaxLength = 64;
            this.mSearchTagTxt.Name = "mSearchTagTxt";
            this.mSearchTagTxt.Size = new System.Drawing.Size(201, 23);
            this.mSearchTagTxt.TabIndex = 9;
            this.mSearchTagTxt.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SearchTagCB);
            // 
            // GraphicalView
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(839, 860);
            this.Controls.Add(this.mInfoPanel);
            this.Controls.Add(this.mEnclosingPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.LightGray;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GraphicalView";
            this.Text = "Memory View";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GraphicalView_FormClosing);
            this.Resize += new System.EventHandler(this.OnResize);
            ((System.ComponentModel.ISupportInitialize)(this.mMemoryView)).EndInit();
            this.mEnclosingPanel.ResumeLayout(false);
            this.mInfoPanel.ResumeLayout(false);
            this.mInfoPanel.PerformLayout();
            this.StatsBox.ResumeLayout(false);
            this.StatsBox.PerformLayout();
            this.mAllocatorsAndBlockSize.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		private void mMemoryView_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Draw(e.Graphics);
		}
		
		/**
		 * The Block size is selected based on number of Rows that will be draw in the image. The range of rows is 50 - 250. 
		 */

		private void SelectAppropriateBlockSize()
		{
            BlockSizeSelectionMutex.WaitOne();
			uint widthInPixels = (uint)mMemoryView.Width;
			uint numOfRows = (((mUpperBound - mLowerBound) / mBytesInBlock) / (widthInPixels/mBlockWidth)) + 1;
			bool blockSizeSelected = false;
			const uint minNumOfRows = 50;
			const uint maxNumOfRows = 250;

            int currSelectedIndex = mBlockSizeMenu.SelectedIndex;
			//Iterate through the block sizes to pick the block size that creates rows 50-250 rows.
			while(!blockSizeSelected)
			{
				if (numOfRows <= minNumOfRows)  
				{
                    if (currSelectedIndex > 0)
					{
                        currSelectedIndex -= 1;
                        int tempSelectedIndex = currSelectedIndex;
						string BlockSizestring = (string)mBlockSizeMenu.Items[tempSelectedIndex];
                        mBytesInBlock = (uint)Int32.Parse(BlockSizestring, System.Globalization.NumberStyles.Integer);
                        numOfRows = (((mUpperBound - mLowerBound) / mBytesInBlock) / (widthInPixels / mBlockWidth)) + 1;
					}
					else
						blockSizeSelected = true;
				}
				else if(numOfRows >= maxNumOfRows)
				{
                    if (currSelectedIndex != mBlockSizeMenu.Items.Count - 1)
					{
                        currSelectedIndex++;
                        int tempSelectedIndex = currSelectedIndex;
						string BlockSizestring = (string)mBlockSizeMenu.Items[tempSelectedIndex];
                        mBytesInBlock = (uint)Int32.Parse(BlockSizestring, System.Globalization.NumberStyles.Integer);
                        numOfRows = (((mUpperBound - mLowerBound) / mBytesInBlock) / (widthInPixels / mBlockWidth)) + 1;
					}
					else
						blockSizeSelected = true;
				}

				else
					blockSizeSelected = true;				
			}
            mBlockSizeMenu.SelectedIndex = currSelectedIndex;
            BlockSizeSelectionMutex.ReleaseMutex();
		}
		
		/**
		 * Copy information to object member variables and initialize the menus.
		 */
        public void CopyGvInfo(GVstruct input, Log log, SnapShot logSnap, ArrayList allocinfo, int category)
		{
			mInfo2Graph = input;
			mLog = log;
			mLogSnap = logSnap;
			mAllocators = allocinfo;
            mCategories =  mLog.GetCategories();
            mSelectedCategory = category;
			// Initialize first time
			PopulateDropDownMenu(category);
			PopulateBlockSize();
			SelectAppropriateBlockSize();

		}

        /**
         * mLowerBound and mUpperBound Bound refer to the allocator's start and end address respectively
         * Lower and upper limit refer the start and end addresses of the category within the allocator
         */
        private void Translate(uint startAddress, uint allocSize, bool logIndex, uint lowerLimit, uint upperLimit,int indexInLog)
		{
			if (mHasMemoryBlockOccupancyBeenCalculated == true)
				return;

			LogEntry logEntry = (LogEntry)mLog[indexInLog];
			
			for (;;)
			{
				//the upper and lower limits change if allocations are being made within or outside the category
				if (lowerLimit <= startAddress && startAddress < upperLimit)
				{
					uint distanceRelativeToStart = startAddress - mLowerBound;
					uint index = distanceRelativeToStart/mBytesInBlock;
					uint relativeStartWithinBlock = distanceRelativeToStart - (index*mBytesInBlock);
					uint partOfBlockEmpty = mBytesInBlock - relativeStartWithinBlock;

                    string logString = "\r\n------------"
                                     + "\r\nTag: "
                                     + mLog.GetCategory(logEntry.category) + "::" + mLog.GetString(logEntry.nameString)
                                     + "\r\nAddress: 0x"
                                     + logEntry.address.ToString("X")
                                     + "\r\nIndex: "
                                     + logEntry.index.ToString()
                        //  + "\r\nCallstack: "
                                   //  + mLog.GetString(logEntry.stackTraceString)
                                     + "\r\nAllocated: "
                                     + logEntry.allocSize.ToString();

					if (logIndex)
					{
                        mStartIndexOfHeap = Math.Min(index, mStartIndexOfHeap);
                        mEndIndexOfHeap = Math.Max(index, mEndIndexOfHeap);
					}

					if (allocSize > partOfBlockEmpty)
					{
						allocSize = allocSize - partOfBlockEmpty;
						startAddress = (index+1)*mBytesInBlock + mLowerBound;
						mMemoryMap[index] +=(UInt16)partOfBlockEmpty;

                        System.Diagnostics.Debug.Assert(mMemoryMap[index] <= mBytesInBlock, "ERROR! Something wrong! blocksize smaller than what it contains!");

                        logString += "\r\nAllocated in Block: " + partOfBlockEmpty.ToString();

                        //add to log but check first
                        if (mStatsLog[index] == null || !mStatsLog[index].Contains(logString))
                        {
                            mStatsLog[index] += logString;
                        }
                        else
                            System.Diagnostics.Debug.Assert(false, "ERROR! This log has already been added??\n");
                       
                        if (logEntry.category == mSelectedCategory)
                            mBytesInBlockCategory[index] += (UInt16)partOfBlockEmpty;
                    }
					else
					{
						mMemoryMap[index] += (UInt16)allocSize;
                        System.Diagnostics.Debug.Assert(mMemoryMap[index] <= mBytesInBlock, "ERROR! Something wrong! blocksize smaller than what it contains!");

                        logString += "\r\nAllocated in Block: " + allocSize.ToString();
						
                        //add to log but check first
                        if (mStatsLog[index] == null || !mStatsLog[index].Contains(logString))
                        {
                            mStatsLog[index] += logString;
                        }
                        else
                            System.Diagnostics.Debug.Assert(false, "ERROR! This log has already been added??\n");

                        if (logEntry.category == mSelectedCategory)
                            mBytesInBlockCategory[index] += (UInt16)allocSize;

						if (logIndex && index >= mEndIndexOfHeap)
						{
							mEndIndexOfHeap+=1;	
						}
						break;
					}
				}
				else
					break;
			}
		}
	
		/* 
         * The Bitmap is updated with the memory consumption of each block. 
		 */ 
		private void UpdateBitmap()
		{
			uint totalallocatorsize = mUpperBound - mLowerBound;

			if (mHasMemoryBlockOccupancyBeenCalculated == false)
			{
				//The following code populates mMemoryMap with allocated space for the current category
                //and logs the start and end indices of the category within the allocator
                mMemoryMap = new UInt16[mNumberBlocks];				// Holds amount of memory allocated per block
                mStatsLog = new string[mNumberBlocks];				// String of allocations for each block
                mBytesInBlockCategory = new UInt16[mNumberBlocks];  //int holding how much allocated from mSelectedCategory per block
                mStartIndexOfHeap = uint.MaxValue;
                mEndIndexOfHeap = 0;

                for (int i = 0; i < mInfo2Graph.mMemoryBlockIndex.Count; i++)
				{
					int indexmLog = (int)mInfo2Graph.mMemoryBlockIndex[i];
                    LogEntry le = mLog[indexmLog];
					uint startPoint = le.address;
					uint allocSize  = le.allocSize;
#if DEBUG
                    uint endPoint = startPoint + allocSize;
                    Helpers.Verify.IsTrue((startPoint >= mLowerBound && startPoint < mUpperBound && endPoint <= mUpperBound));
                    if (!(startPoint >= mLowerBound && startPoint < mUpperBound && endPoint <= mUpperBound))
                        System.Diagnostics.Debug.Print( "!! UpdateBitmap: Why this address " + startPoint.ToString() + " is not in range?\n");
#endif
                    Translate(startPoint, allocSize, true, mLowerBound, mUpperBound, indexmLog);
				}

                // all these bellow are not doing anything useful yet. Come back to it later if needed!
//                 uint lowerLimit = (mStartIndexOfHeap * mBytesInBlock) + mLowerBound;
//                 uint upperLimit = mUpperBound;
// 				
// 				if (mEndIndexOfHeap != mMemoryMap.Length)
// 				{
//                     upperLimit = (mEndIndexOfHeap * mBytesInBlock) + mLowerBound;
// 				}
// 
// 				for (int i = 0; i < mLogSnap.Count; i++)
// 				{
// 					int indexLog = mLogSnap[i];
// 
// 					uint startPoint = mLog[indexLog].address;
// 					uint allocSize1 = mLog[indexLog].allocSize;
// 					uint allocSize2 = mLog[indexLog].allocSize;
// 
// 					uint endPoint  = startPoint + allocSize1;
// 					
// 					//If allocations starts within the allocator and carries over then size of the allocation is cut short.
// 					if (endPoint > upperLimit)
// 					{
// 						allocSize1 = upperLimit - startPoint;
// 					}
//                     if (endPoint > mUpperBound)
// 					{
//                         allocSize2 = mUpperBound - startPoint;
// 					}
// 					
// 					//Mapping the allocations that occur before or after the allocations for the category within the allocator
// 					if (mStartIndexOfHeap != 0 && mStartIndexOfHeap != uint.MaxValue)
// 					{
//                         Translate(startPoint, allocSize1, false, mLowerBound, upperLimit, indexLog);
// 					}
// 
// 					if (mEndIndexOfHeap != 0)
// 					{
//                         uint indexinmemory = (startPoint - mLowerBound) / mBytesInBlock;
// 						long sizeUsedForSignedCalculations = (long) allocSize2;
// 						//Since the whole log file is being parsed entries that fall within categories allocated space should not be repeated.
// 						//If an allocation that is not part of the category occurs within indexed space of category,
// 						//the starting index of the allocation is shifted to the next block and the allocation size is accordingly reduced.
// 						//The latter might occur when the block size selected is large.
// 						while((sizeUsedForSignedCalculations>0) && (indexinmemory < mEndIndexOfHeap))
// 						{
//                             sizeUsedForSignedCalculations -= (long)((((indexinmemory + 1) * mBytesInBlock) + mLowerBound) - startPoint);
//                             startPoint = ((indexinmemory + 1) * mBytesInBlock) + mLowerBound;
//                             indexinmemory = (startPoint - mLowerBound) / mBytesInBlock;
// 						}
// 						
// 						if (sizeUsedForSignedCalculations > 0)
// 						{
// 							allocSize2 = (uint) sizeUsedForSignedCalculations;
//                             Translate(startPoint, allocSize2, false, lowerLimit, mUpperBound, indexLog);
// 						}
// 					}
// 
// 				}
			}

			// Change menu title
            string title = String.Format("Graphical view - heap size {0}k", totalallocatorsize / 1024);
			this.Text = title;

			if (!mWindowUpdate)
				return;

			// Create bitmap of correct size
			uint widthInPixels = (uint)mMemoryView.Width;
            int nRows = (int)((totalallocatorsize / mBytesInBlock) / (widthInPixels / mBlockWidth)) + 1;

			// Reset height of memory view control and create bitmap
			mMemoryView.Height = (nRows * (int)mBlockHeight);
            CreateBitmap(mMemoryView.Width, nRows * (int)mBlockHeight);

			// Clear bitmap to white
			mSolidBrush.Color = Color.White;
			mOffScreenDC.FillRectangle(mSolidBrush, 0, 0, mViewWidth, mViewHeight);


            uint blocksInRowCount = 0; // Counter to track the end for the heap.
            int x = 0;
            int y = 0;
            int index = 0;
            LogEntry highlightedLogEntry = null;
            if (mHighlightedLogEntry >= 0)
                highlightedLogEntry = (LogEntry)mLog[mHighlightedLogEntry];
                
            while (y <= mViewHeight)
			{
				Color blockColor = Color.WhiteSmoke;
				if (index < mNumberBlocks)
				{
					uint baseval = 80;
					uint factor = 255;

					if (mMemoryMap[index] > 0)
					{
                        if (index >= mStartIndexOfHeap && index < mEndIndexOfHeap && mBytesInBlockCategory[index] > 0)
						{
							blockColor = Color.Tomato;
                            factor = mBytesInBlockCategory[index] * (256 - 2 * baseval);
                            factor /= mBytesInBlock;
                            factor += 2 * baseval;
                        }
						else
						{
							blockColor = Color.GreenYellow;
	                        factor = mMemoryMap[index] * (256 - baseval);
                            factor /= mBytesInBlock;
                            factor += baseval;
					    }

					}

                    uint blockAddress = mLowerBound + (uint)(index * mBytesInBlock);
                    // take care filtered logs, to be colored Aqua
                    if (mHighlightLogSnap != null)
                    {
                        for(int k = 0; k<mHighlightLogSnap.Count; k++)
                        {
                            LogEntry logEntry = (LogEntry)mLog[mHighlightLogSnap[k]];

                            if (blockAddress + mBytesInBlock >= logEntry.address &&
                                blockAddress <= logEntry.address + logEntry.allocSize)
                            {
                                blockColor = Color.Aqua;
                                factor = 256;
                                break;
                            }
                        }
                    }

                    // take care of selected bin, to be highlighted to yellow color
                    if (highlightedLogEntry != null && blockAddress + mBytesInBlock >= highlightedLogEntry.address &&
                        blockAddress <= highlightedLogEntry.address + highlightedLogEntry.allocSize)
                    {
                        blockColor = Color.Yellow;
                        factor = 256;
                    }

                    int R = (int)(blockColor.R * factor) / 256;
                    int G = (int)(blockColor.G * factor) / 256;
                    int B = (int)(blockColor.B * factor) / 256;
					blockColor = Color.FromArgb(R, G, B);	

				}
				else
				{
					blockColor = Color.Black;
				}
				mSolidBrush.Color = blockColor;
				
				// Draw cell
				mOffScreenDC.FillRectangle(mSolidBrush, x, y, mBlockWidth-1, mBlockHeight-1);

                x += (int)mBlockWidth;
				blocksInRowCount++;
				if(x >= widthInPixels)
				{
					x = 0;
                    y = y + (int)mBlockHeight;
					mBlocksInRow = blocksInRowCount;
					blocksInRowCount = 0;
				}
				index++;
			}
		}

		/**
		 * The function returns the start and end address of the allocator
		 */
		private void GetAllocatorBounds(string allocatorName, ref uint startAddress, ref uint endAddress)
		{
			for (int i = 0; i < mAllocators.Count; i++)
			{
				LogEntryAllocators allocinfo = (LogEntryAllocators)mAllocators[i];
				if (allocatorName == allocinfo.mName)
				{
					startAddress = allocinfo.mStartAddress;
					endAddress = allocinfo.mEndAddress;
					break;
				}
			}
		}

		private void Draw(Graphics g)
		{
			g.DrawImage(mOffScreenBmp, g.ClipBounds, g.ClipBounds, GraphicsUnit.Pixel);	
		}

		private void OnResize(object sender, System.EventArgs e)
		{
			mMemoryView.Focus();
			Control control = (Control)sender;
		
			if (mEnclosingPanel.Height != control.Height)
			{
				mEnclosingPanel.Height = control.Size.Height - 280; // 280 is chosen so that the enclosing panel is not covered by 
																	// the panel that contains the drop down menus and info about each block of memory
			}
			if (control.Size.Width >= mMinWidth)
			{
				if (mEnclosingPanel.Width != control.Size.Width)
				{
					mEnclosingPanel.Width = control.Size.Width-7; // to compensate for the padding of the scroll bars
					mWindowUpdate = true;
				}
				mMemoryView.Width = mEnclosingPanel.Width-17;	 // consciously making it smaller than the enclosing panel
																 // to prevent getting a horizontal scroll bar
			}
			else
			{
				Width = mMinWidth+24;							// The following integers are based on the above comments for this section of code
				if (mEnclosingPanel.Width != mMinWidth+17)
				{
					mEnclosingPanel.Width = mMinWidth+17;
					mEnclosingPanel.Height = mMinHeight;
					mWindowUpdate = true;
				}
				mMemoryView.Width = mMinWidth;
			}

			// Ensure width is a multiple of block width
            mMemoryView.Width -= mMemoryView.Width % (int)mBlockWidth;
            
            Array.Clear(mMemoryMap, 0, mMemoryMap.Length);
            Array.Clear(mBytesInBlockCategory, 0, mBytesInBlockCategory.Length);
            Array.Clear(mStatsLog, 0, mStatsLog.Length);
            mStartIndexOfHeap = uint.MaxValue;
            mEndIndexOfHeap = 0;

            UpdateView(true);
		}

        private void UpdateView(bool calculateMrmoryBlockOccupancy)
        {
            BlockSizeSelectionMutex.WaitOne();
            // Redraw			

            mWindowUpdate = true;
            mHasMemoryBlockOccupancyBeenCalculated = !calculateMrmoryBlockOccupancy;
            UpdateBitmap(); 
            mMemoryView.Refresh();
            BlockSizeSelectionMutex.ReleaseMutex();
        }
		/**
		 * Chooses the largest allocator is default since usually we want to see fragmentation in the larger allocated space.
		 */
		private int ChooseLargestAllocator()
		{
			int indexOfLargestAllocator = 0;
			uint sizeOfLargestAllocator = 0;
			for(int i = 0; i < mAllocatorsSelect.Items.Count; i++)
			{
				
				uint lowerBound = 0;
				uint upperBound = 0;
				GetAllocatorBounds((string)mAllocatorsSelect.Items[i],ref lowerBound, ref upperBound);
                if (sizeOfLargestAllocator < (upperBound - lowerBound))
                {
                    indexOfLargestAllocator = i;
                    mLowerBound = lowerBound;
                    mUpperBound = upperBound;
                }
			}
			return indexOfLargestAllocator;
		}

		/**
		 * All the allocators for the category are added to the drop down menu. 
		 * Each allocation for category is compared against every category to make sure every allocator for the category is recorded
		 */
		private void PopulateDropDownMenu(int category)
		{
			if (mPopulateList == true)
			{
                // first populate category dropdown
                for (int i = 0; i < mCategories.Count; i++)
                {
                    string cat = (string)mCategories[i];
                    mCatSelectBox.Items.Add(cat);
                }

                for (int i = 0; i < mAllocators.Count; i++)
                {
                    LogEntryAllocators allocinfo = (LogEntryAllocators)mAllocators[i];
                    if (mAllocators.Count == 1)
                    {
                        if (!mAllocatorsSelect.Items.Contains(allocinfo.mName))
                        {
                            mAllocatorsSelect.Items.Add(allocinfo.mName);
                        }
                        break;
                    }

                    for (int j = 0; j < mInfo2Graph.mMemoryBlockIndex.Count; j++)
                    {
                        int indexmLog = (int)mInfo2Graph.mMemoryBlockIndex[j];
                        uint startPoint = mLog[indexmLog].address;
                        uint allocSize = mLog[indexmLog].allocSize;
                        uint endPoint = startPoint + allocSize;
                        if ((mLog[indexmLog].category == category) && ((startPoint >= allocinfo.mStartAddress && startPoint < allocinfo.mEndAddress) ||
                            (endPoint >= allocinfo.mStartAddress && endPoint < allocinfo.mEndAddress)))
                        {
                            if (!mAllocatorsSelect.Items.Contains(allocinfo.mName))
                            {
                                mAllocatorsSelect.Items.Add(allocinfo.mName);
                            }
                            break;
                        }
                    }
                }

				// Force to select item.
			//	if (mAllocatorsSelect.Items.Count > 1)
				{
					mAllocatorsSelect.SelectedIndex = ChooseLargestAllocator();
				}
                if (mCatSelectBox.SelectedIndex != category)
                    mCatSelectBox.SelectedIndex = category;

				mPopulateList = false;
			}	
			
		}

        private void SearchAddressCB(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
            {
                mMemoryView.Focus();
                uint searchAddress = 0;
                TextBox control = (TextBox)sender;
                string searchaddressStr = control.Text.Replace("0x", "");
                try{
                    searchAddress = Convert.ToUInt32(searchaddressStr, 16);
                }
                catch (System.FormatException)
                {
                    System.Diagnostics.Debug.Print("ERROR! Wrong format for Address! Use Hex format");
                }
                uint relativeAddress = (searchAddress - mLowerBound);
                uint indexY = relativeAddress / (mBlocksInRow * mBytesInBlock);
                uint indexX = (relativeAddress - indexY * (mBlocksInRow * mBytesInBlock)) / mBytesInBlock;
                uint index = (indexY * mBlocksInRow) + indexX;
                uint address = mLowerBound + (index * mBytesInBlock);

                mStats.Text = ComputeMemoryBlockOccupancy(address, index);
                UpdateView(false);
            }
        }

        public static void FindObject(ArrayList array, Object o)
        {
            int index = array.BinarySearch(0, array.Count, o, null);

            if (index > 0)
            {
                Console.WriteLine("Address: {0} found at [{1}]", o, index);
            }
            else if (~index == array.Count)
            {
                Console.WriteLine("Address: {0} not found. "
                   + "No array Address has a greater value.", o);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Address: {0} not found. "
                   + "Next larger Address found at [{1}].", o, ~index);
            }
        }

        private void SearchTagCB(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                mMemoryView.Focus();
                TextBox control = (TextBox)sender;

                string searchaddressStr = control.Text;
                MemManager.Log.SnapShot FilterdLogSnap = null;
                if(searchaddressStr.Length > 0)
                    FilterdLogSnap = new MemManager.Log.SnapShot(mLog, mLogSnap, searchaddressStr, 0, true);
                UpdateHighlights(FilterdLogSnap);
            }
        }

        private void AllocatorSelectedCB(object sender, System.EventArgs e)
		{
            BlockSizeSelectionMutex.WaitOne();

			mMemoryView.Focus();
			ComboBox control = (ComboBox) sender;
			int index= control.SelectedIndex;
			mCurrentAllocatorName = (string)control.Items[index];
			GetAllocatorBounds(mCurrentAllocatorName, ref mLowerBound,ref mUpperBound);
			SelectAppropriateBlockSize();

            mNumberBlocks = (mUpperBound - mLowerBound) / mBytesInBlock;
            uint modulo = (mUpperBound - mLowerBound) % mBytesInBlock;
			if (modulo > 0)
			{
				mNumberBlocks = mNumberBlocks+1;
			}
            BlockSizeSelectionMutex.ReleaseMutex();
            UpdateView(true);
		}

        private void CategorySelectedCB(object sender, System.EventArgs e)
        {
            mMemoryView.Focus();
            ComboBox control = (ComboBox)sender;
            mSelectedCategory = control.SelectedIndex;
            UpdateView(true);
        }
		
		/**
		 * This function populates the different block sizes that can be chosen by the user
		 */
		private void PopulateBlockSize()
		{
			int index = 0;
			int selid = -1;
            for (int i = (int)mSmallestBlock; i <= (int)mLargestBlock; i = i * 2)
			{
				mBlockSizeMenu.Items.Add(i.ToString());
				if (i == mBytesInBlock)
					selid = index;
				index++;
			}
			if (selid != -1)
				mBlockSizeMenu.SelectedIndex = selid;
		}

		private void BlockSizeChange(object sender, System.EventArgs e)
		{
            BlockSizeSelectionMutex.WaitOne();

			mMemoryView.Focus();
			ComboBox control = (ComboBox) sender;
			int index= control.SelectedIndex;
			int newBlockSize =0;
			string BlockSizestring = (string)control.Items[index];
			newBlockSize = Int32.Parse(BlockSizestring,System.Globalization.NumberStyles.Integer);
			mBytesInBlock = (uint)newBlockSize;
			
			int position = -1;
			position = mAllocatorsSelect.SelectedIndex;
			if (position != -1)
			{				
				
				mCurrentAllocatorName = (string) mAllocatorsSelect.Items[position];
				GetAllocatorBounds(mCurrentAllocatorName, ref mLowerBound,ref mUpperBound);

                mNumberBlocks = (mUpperBound - mLowerBound) / mBytesInBlock;
                uint modulo = (mUpperBound - mLowerBound) % mBytesInBlock;
				if (modulo > 0)
				{
					mNumberBlocks = mNumberBlocks + 1;
				}
				mMemoryMap = new UInt16 [mNumberBlocks];
				mStatsLog = new string [mNumberBlocks];
                mBytesInBlockCategory = new UInt16[mNumberBlocks];  //int holding how much allocated from mSelectedCategory in this block
			
				mStartIndexOfHeap = uint.MaxValue;
				mEndIndexOfHeap= 0;
                BlockSizeSelectionMutex.ReleaseMutex();
                UpdateView(true);
			}
		}

        // graphical view block click callback.
		private void BlockInfoGet(object sender, System.EventArgs e)
		{
            mHighlightedLogEntry = -1;
           // mHighlightLogSnap = null;
			int positionAllocator = mAllocatorsSelect.SelectedIndex;
			if (positionAllocator!=-1)
			{
				Control control = (Control) sender;
				Point absoluteCoordinates = MousePosition;
				Point normalizedCoordinates = PointToClient(absoluteCoordinates);
				uint indexX = 0;
				uint indexY = 0;
				int verticalScroll = -1 * control.Top;				 // The coordinates are relative to location of the form. This variable gets the 
																	 // vertical position relative to top of the bmp which is scrollable
                indexY = (uint)(normalizedCoordinates.Y + verticalScroll) / mBlockHeight;
                indexX = (uint)normalizedCoordinates.X / mBlockWidth;
				uint index = (indexY*mBlocksInRow)+indexX;

				uint address = mLowerBound + (index * mBytesInBlock);

                mStats.Text = ComputeMemoryBlockOccupancy(address, index);

				// Redraw			
                UpdateView(false);
			}
		}

        string ComputeMemoryBlockOccupancy(uint address, uint index)
        {
            string memoryoccupancy = String.Empty;
            memoryoccupancy = "Block start address: " + address.ToString("X");

            if (index < mMemoryMap.Length && index >=0)
            {
                for (int logIndex = mLogSnap.mMaxLogIndex; logIndex >= 0; logIndex--)
                {
                    LogEntry logEntry = (LogEntry)mLog[logIndex];
                    if (address + mBytesInBlock >= logEntry.address && address <= logEntry.address + logEntry.allocSize)
                    {
                        if (mMemoryMap[index] > 0)
                        {
                            mHighlightedLogEntry = logIndex;
                        }
                        break;
                    }
                }

                memoryoccupancy += "\r\nTotal Occupied Memory in Block=" + mMemoryMap[index].ToString() + "\r\n";
                memoryoccupancy += mStatsLog[index];

                //Calculating the amount of empty space
                string amountOfEmptyMemory = string.Empty;
                bool backwardTraverseDone = false;
                bool forwardTraverseDone = false;
                double emptySpace = 0.0f;
                int indexBackwards = -1;
                if (index != 0)
                    indexBackwards = (int)index - 1;

                while (!forwardTraverseDone || !backwardTraverseDone)
                {
                    //Iterating forwards to calculate amount of free space
                    if (index < mMemoryMap.Length)
                    {
                        if (mMemoryMap[index] == 0)
                        {
                            emptySpace += (double)mBytesInBlock;
                            index++;
                        }
                        else
                            forwardTraverseDone = true;
                    }
                    else
                        forwardTraverseDone = true;

                    //Iterating backwards to calculate amount of free space
                    if (indexBackwards != -1)
                    {
                        if (mMemoryMap[indexBackwards] == 0)
                        {
                            emptySpace += (double)mBytesInBlock;
                            indexBackwards--;
                        }
                        else
                            backwardTraverseDone = true;
                    }
                    else
                        backwardTraverseDone = true;
                }
                if (emptySpace != 0)
                {
                    emptySpace /= 1024;
                    amountOfEmptyMemory = "\r\nAmount of continuous empty space: " + emptySpace.ToString() + " Kb";
                }

                memoryoccupancy += amountOfEmptyMemory;
            }
            else
            {
                memoryoccupancy += "Outside Allocator assigned Memory";
            }

            return memoryoccupancy;

        }

        /// <summary>
        /// This is just a callback function serving memory graph. For now we only serve the first selected category
        /// </summary>
        /// <param name="EnabledCategories"></param>
        public void UpdateCategories(bool[] EnabledCategories)
        {
            int catsize = EnabledCategories.Length;
            for(int i=0; i<catsize; i++)
            {
                if(EnabledCategories[i])
                {
                    mCatSelectBox.SelectedIndex = i;
                    break;
                }
            }
        }

		private void GraphicalView_FormClosing(object sender, FormClosingEventArgs e)
		{
			mMainViewWindow.CloseGraphicalView();
		}

        public void UpdateHighlights(SnapShot HSnapShot)
        {
            mHighlightedLogEntry = -1;
            mHighlightLogSnap = HSnapShot;
            // clear down memory map
     //       Array.Clear(mMemoryMap, 0, mMemoryMap.Length);
		
            UpdateView(false);
        }
        
		public void NewSnapShot(SnapShot logSnapShot)
		{
			mHighlightedLogEntry = -1;
          //  mHighlightLogSnap = null;
			mLogSnap = logSnapShot;

			mInfo2Graph = new GVstruct();
			for (int i = 0, e = mLogSnap.Count; i < e; i++)
			{
                LogEntry le = mLog[mLogSnap[i]];
#if DEBUG
                if (!(mLowerBound < le.address && le.address < mUpperBound))
                    System.Diagnostics.Debug.Print( "!! Why this address is not in range?\n");
#endif
                mInfo2Graph.mMemoryBlockIndex.Add(mLogSnap[i]);
			}

            Array.Clear(mMemoryMap, 0, mMemoryMap.Length);
            UpdateView(true);
		}

	}
}
