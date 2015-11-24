using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text.RegularExpressions;

namespace MemVisualizer
{
	/// <summary>
	/// Summary description for MemoryGraph.
	/// </summary>
	public class MemoryGraph : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.VScrollBar mGraphVScrollBar;
		private System.Windows.Forms.HScrollBar mGraphHScrollBar;
		private System.Windows.Forms.DataGrid mDataGrid;

		private class Range
		{
			public int mMin;
			public int mMax;

			public Range(int min, int max) { mMin = min; mMax = max; }
		}


		private class RangeObject
		{
			public int mMax;
            public ArrayList mCategoryMemory;
			public Range mLogIndex;

			public int mUnfilteredMax;
		}

		private int[] mRunningSumArray;
        private int[] mUnflilteredRunningSumArrays;
        private int mNumberOfCategories = 0;
        private ArrayList[] mCategoryRunningSumArrays;
        private RangeObject[] mMemoryGraphRangeData;
		private ArrayList mLabelsArray;
        private ArrayList mFramesArray;
		private int mMaxMemoryAllocation = 0;
        private int mMinMemoryAllocation = 0;
		private Range mHighlightIndexStart = null;
		private Range mHighlightIndexEnd = null;
		private int mGraphScale = 1;
        private int mTargetWidth = 0;
        private float mHeightUnit = 0.0f;
        private float mWidthUnit = 0.0f;
        private bool mCategoryEnabled = false;
        private bool[] mSelectedCategoryInices;
		private MemVisualizer.MainWindow mMainViewWindow = null;
		private MemManager.Log.Log mLog;
		private System.Windows.Forms.Panel mGraphPanel;
		private System.Windows.Forms.PictureBox mMemoryGraphPB;
		private System.Windows.Forms.Panel mMainPanel;
		private System.Windows.Forms.TextBox mFilterTextBox;
		private System.Windows.Forms.Button mFilterButton;
        private Button mCompButton;
        private TextBox mTextComp2;
        private TextBox mTextComp1;
        private bool mComparePressed = false;
        private bool mCompareProcessed = false;
        private CheckBox mCatEnabled;
        private Label label1;
        private ListBox mCatSelectionBox;
		private System.Windows.Forms.ComboBox mZoomComboControl;

        //  System.Drawing.Brush[] colorBrushes =  { Brushes.Blue, Brushes.Lime, Brushes.DeepPink};
        private static System.Drawing.Pen[] mCategoryPens = { Pens.Blue, Pens.Lime, Pens.DeepPink };


		public MemoryGraph(MemVisualizer.MainWindow mainViewerWindow, MemManager.Log.Log log)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			mGraphVScrollBar.Enabled = false;
			mGraphHScrollBar.Enabled = false;
            mTextComp1.Text = mTextComp2.Text = "";
            mComparePressed = mCompareProcessed = false;
			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			mMainViewWindow = mainViewerWindow;

			mLog = log;

			CreateMemoryGraph(log);

			mHighlightIndexStart = null;
			mHighlightIndexEnd = null;

			for (int i = 1; i <= 10; ++i)
			{
				mZoomComboControl.Items.Add("Zoom " + i.ToString() + "x");
			}
			mZoomComboControl.SelectedIndex = 0;

		}

		private void CreateMemoryGraph(MemManager.Log.Log log)
		{
			mRunningSumArray = new int[log.Count];
            int[] categoryrunningSum = null;
            if (mCategoryEnabled)
            {
                mNumberOfCategories = log.GetNumberCategories() - 1;  // ignore first category which is basically no category
                mCatSelectionBox.Items.Add("All");
                for (int i = 0; i < mNumberOfCategories; i++)
                    mCatSelectionBox.Items.Add(log.GetCategory((byte)(i + 1)));
                mCatSelectionBox.SelectedIndex = 0;

                mCategoryRunningSumArrays = new ArrayList[mNumberOfCategories];
                categoryrunningSum = new int[mNumberOfCategories];
                for (int i = 0; i < mNumberOfCategories; i++)
                {
                    mCategoryRunningSumArrays[i] = new ArrayList(log.Count);
                    categoryrunningSum[i] = 0;
                }
            }
            else
            {
                if (mNumberOfCategories > 0)
                {
                    for (int i = 0; i < mNumberOfCategories; i++)
                    {
                        mCategoryRunningSumArrays[i].Clear();
                        int gen = System.GC.GetGeneration(mCategoryRunningSumArrays[i]);
                        System.GC.Collect(gen);
                        mCategoryRunningSumArrays[i] = null;
                    }
                    
                    mCategoryRunningSumArrays = null;
                }
                mNumberOfCategories = -1;
            }


            mUnflilteredRunningSumArrays = new int[log.Count];
			mLabelsArray = new ArrayList();
            mFramesArray = new ArrayList();

			mMaxMemoryAllocation = 0;
            mMinMemoryAllocation = 0;
			int runningSum = 0;
			int unfilteredRunningSum = 0;
			Regex regex = new Regex(mFilterTextBox.Text, RegexOptions.Compiled|RegexOptions.IgnoreCase);

            bool DoFiltering = (mFilterTextBox.Text != "");
			for (int i = 0; i < log.Count; i++)
			{
				MemManager.Log.LogEntry logentry = log[i];

                bool filtered = DoFiltering && !(regex.IsMatch(log.GetString(logentry.nameString)));

				if (!filtered)
				{
					if (logentry.type == 'A')
					{
						runningSum += (int)logentry.allocSize;
					}
					else if (logentry.type == 'F')
					{
						runningSum -= (int)logentry.allocSize;
					}
				}
				if (logentry.type == 'A')
				{
					unfilteredRunningSum += (int)logentry.allocSize;
                    if (mCategoryEnabled)
                        categoryrunningSum[logentry.category - 1] += (int)logentry.allocSize;
				}
				else if (logentry.type == 'F')
				{
					unfilteredRunningSum -= (int)logentry.allocSize;
                    if (mCategoryEnabled)
                        categoryrunningSum[logentry.category - 1] -= (int)logentry.allocSize;
                }
                else if (logentry.type == 'S')
                {
                    mFramesArray.Add(i);
                }
                else if (logentry.type == 'L')
					mLabelsArray.Add(i);

				mRunningSumArray[i] = runningSum;
				mUnflilteredRunningSumArrays[i] = unfilteredRunningSum;
                for (int k = 0; k < mNumberOfCategories; k++)
                    mCategoryRunningSumArrays[k].Add(categoryrunningSum[k]);

                mMaxMemoryAllocation = Math.Max(mMaxMemoryAllocation, unfilteredRunningSum);
                mMinMemoryAllocation = Math.Min(unfilteredRunningSum, mMinMemoryAllocation);
			}
			mFramesArray.Add(log.Count);

			int maxMem = 1;
			while (maxMem < mMaxMemoryAllocation)
			{
				maxMem *= 2;
			}
			mMaxMemoryAllocation = maxMem;

            int minMem = 1;
            while (-minMem > mMinMemoryAllocation)
            {
                minMem *= 2;
            }
            mMinMemoryAllocation = minMem == 1 ? 0 : minMem;

			GenerateGraphicalData();
		}

		private void GenerateGraphicalData()
		{
            int graphdataCount = mRunningSumArray.Length;
            if (mMemoryGraphPB.Width > 0 && graphdataCount > 0)
			{
                mTargetWidth = mMemoryGraphPB.Width * mGraphScale;
                mMemoryGraphRangeData = new RangeObject[mTargetWidth];
                mWidthUnit = (float)mTargetWidth / (float)graphdataCount;
				int prevIndex = 0;
              //  int rangeMin = (int)mRunningSumArray[0];
                int rangeMax = mRunningSumArray[0];

                int[] rangeCategoryMax = null;
                if(mCategoryEnabled)
                {
                    //  int[] rangeCategoryMin = new int[mNumberOfCategories];
                    rangeCategoryMax = new int[mNumberOfCategories];
                    for (int k = 0; k < mNumberOfCategories; k++)
                    {
                        // rangeCategoryMin[k] = (int)mCategoryRunningSumArrays[k][0];
                        rangeCategoryMax[k] = (int)mCategoryRunningSumArrays[k][0];
                    }
                }

                int unfilteredMax = mUnflilteredRunningSumArrays[0];
				int startIndex = 0;
                int gdataIndex = 0;
                float prop = (float)mTargetWidth / (float)(graphdataCount - 1);
                for (int dataIndex = 1; dataIndex < graphdataCount; dataIndex++)
				{
					int targetIndex = (int)((float)dataIndex * prop);
					if (targetIndex != prevIndex)
					{
                        System.Diagnostics.Debug.Assert(targetIndex - prevIndex == 1, "What is up!");
						
                        RangeObject range = new RangeObject();
                        range.mMax = rangeMax;
                        range.mLogIndex = new Range(startIndex, dataIndex - 1);
                        if (mCategoryEnabled)
                        {
                            mMemoryGraphRangeData[gdataIndex].mCategoryMemory = new ArrayList(mNumberOfCategories);

                            for (int k = 0; k < mNumberOfCategories; k++)
                            {
                                range.mCategoryMemory.Add(rangeCategoryMax[k]);
                                //  rangeCategoryMin[k] = (int)mCategoryRunningSumArrays[k][dataIndex];
                                rangeCategoryMax[k] = (int)mCategoryRunningSumArrays[k][dataIndex];
                            }
                        }

                        range.mUnfilteredMax = unfilteredMax;
                        mMemoryGraphRangeData[gdataIndex++] = range;

                        prevIndex = targetIndex;

						startIndex = dataIndex;
						//rangeMin = (int)mRunningSumArray[dataIndex];
						rangeMax = mRunningSumArray[dataIndex];
                        unfilteredMax = mUnflilteredRunningSumArrays[dataIndex];
					}
					else
					{
                        rangeMax = Math.Max(rangeMax, mRunningSumArray[dataIndex]);
                        for (int j = 0; j < mNumberOfCategories; j++)
                        {
                          //  rangeCategoryMin[j] = Math.Min(rangeCategoryMin[j], (int)mCategoryRunningSumArrays[j][dataIndex]);
                            rangeCategoryMax[j] = Math.Max(rangeCategoryMax[j], (int)mCategoryRunningSumArrays[j][dataIndex]);
                        }
                        unfilteredMax = Math.Max(unfilteredMax, mUnflilteredRunningSumArrays[dataIndex]);
                    }
				}
			}
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
            this.mGraphPanel = new System.Windows.Forms.Panel();
            this.mMemoryGraphPB = new System.Windows.Forms.PictureBox();
            this.mCatEnabled = new System.Windows.Forms.CheckBox();
            this.mGraphVScrollBar = new System.Windows.Forms.VScrollBar();
            this.mGraphHScrollBar = new System.Windows.Forms.HScrollBar();
            this.mMainPanel = new System.Windows.Forms.Panel();
            this.mDataGrid = new System.Windows.Forms.DataGrid();
            this.mFilterTextBox = new System.Windows.Forms.TextBox();
            this.mFilterButton = new System.Windows.Forms.Button();
            this.mZoomComboControl = new System.Windows.Forms.ComboBox();
            this.mCompButton = new System.Windows.Forms.Button();
            this.mTextComp2 = new System.Windows.Forms.TextBox();
            this.mTextComp1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.mCatSelectionBox = new System.Windows.Forms.ListBox();
            this.mGraphPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mMemoryGraphPB)).BeginInit();
            this.mMainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // mGraphPanel
            // 
            this.mGraphPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mGraphPanel.Controls.Add(this.mMemoryGraphPB);
            this.mGraphPanel.Location = new System.Drawing.Point(8, 40);
            this.mGraphPanel.Name = "mGraphPanel";
            this.mGraphPanel.Size = new System.Drawing.Size(1004, 427);
            this.mGraphPanel.TabIndex = 0;
            // 
            // mMemoryGraphPB
            // 
            this.mMemoryGraphPB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mMemoryGraphPB.Location = new System.Drawing.Point(5, 0);
            this.mMemoryGraphPB.Name = "mMemoryGraphPB";
            this.mMemoryGraphPB.Size = new System.Drawing.Size(988, 424);
            this.mMemoryGraphPB.TabIndex = 0;
            this.mMemoryGraphPB.TabStop = false;
            this.mMemoryGraphPB.MouseMove += new System.Windows.Forms.MouseEventHandler(this.memoryGraphPB_MouseMove);
            this.mMemoryGraphPB.Resize += new System.EventHandler(this.memoryGraphPB_Resize);
            this.mMemoryGraphPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.memoryGraphPB_MouseDown);
            this.mMemoryGraphPB.Paint += new System.Windows.Forms.PaintEventHandler(this.memoryGraphPB_Paint);
            this.mMemoryGraphPB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.memoryGraphPB_MouseUp);
            // 
            // mCatEnabled
            // 
            this.mCatEnabled.AutoSize = true;
            this.mCatEnabled.Location = new System.Drawing.Point(409, 9);
            this.mCatEnabled.Name = "mCatEnabled";
            this.mCatEnabled.Size = new System.Drawing.Size(110, 17);
            this.mCatEnabled.TabIndex = 11;
            this.mCatEnabled.Text = "Category Enabled";
            this.mCatEnabled.UseVisualStyleBackColor = true;
            this.mCatEnabled.CheckedChanged += new System.EventHandler(this.categoryEnabled_Click);
            // 
            // mGraphVScrollBar
            // 
            this.mGraphVScrollBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mGraphVScrollBar.Location = new System.Drawing.Point(1004, 32);
            this.mGraphVScrollBar.Name = "mGraphVScrollBar";
            this.mGraphVScrollBar.Size = new System.Drawing.Size(17, 435);
            this.mGraphVScrollBar.TabIndex = 2;
            this.mGraphVScrollBar.ValueChanged += new System.EventHandler(this.graphVScrollBar_ValueChanged);
            this.mGraphVScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.graphVScrollBar_Scroll);
            // 
            // mGraphHScrollBar
            // 
            this.mGraphHScrollBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mGraphHScrollBar.Location = new System.Drawing.Point(0, 467);
            this.mGraphHScrollBar.Name = "mGraphHScrollBar";
            this.mGraphHScrollBar.Size = new System.Drawing.Size(1004, 17);
            this.mGraphHScrollBar.TabIndex = 3;
            this.mGraphHScrollBar.ValueChanged += new System.EventHandler(this.graphHScrollBar_ValueChanged);
            this.mGraphHScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.graphHScrollBar_Scroll);
            // 
            // mMainPanel
            // 
            this.mMainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mMainPanel.Controls.Add(this.mDataGrid);
            this.mMainPanel.Location = new System.Drawing.Point(0, 491);
            this.mMainPanel.Name = "mMainPanel";
            this.mMainPanel.Size = new System.Drawing.Size(1028, 264);
            this.mMainPanel.TabIndex = 4;
            this.mMainPanel.SizeChanged += new System.EventHandler(this.panel1_SizeChanged);
            // 
            // mDataGrid
            // 
            this.mDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mDataGrid.DataMember = "";
            this.mDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.mDataGrid.Location = new System.Drawing.Point(8, 8);
            this.mDataGrid.Name = "mDataGrid";
            this.mDataGrid.Size = new System.Drawing.Size(1012, 248);
            this.mDataGrid.TabIndex = 0;
            this.mDataGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mDataGrid_MouseUp);
            // 
            // mFilterTextBox
            // 
            this.mFilterTextBox.Location = new System.Drawing.Point(149, 4);
            this.mFilterTextBox.Name = "mFilterTextBox";
            this.mFilterTextBox.Size = new System.Drawing.Size(168, 20);
            this.mFilterTextBox.TabIndex = 5;
            this.mFilterTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FilterTextBox_KeyPress);
            // 
            // mFilterButton
            // 
            this.mFilterButton.Location = new System.Drawing.Point(323, 1);
            this.mFilterButton.Name = "mFilterButton";
            this.mFilterButton.Size = new System.Drawing.Size(45, 24);
            this.mFilterButton.TabIndex = 6;
            this.mFilterButton.Text = "Filter";
            this.mFilterButton.Click += new System.EventHandler(this.FilterButton_Click);
            // 
            // mZoomComboControl
            // 
            this.mZoomComboControl.Location = new System.Drawing.Point(9, 4);
            this.mZoomComboControl.Name = "mZoomComboControl";
            this.mZoomComboControl.Size = new System.Drawing.Size(107, 21);
            this.mZoomComboControl.TabIndex = 7;
            this.mZoomComboControl.SelectedIndexChanged += new System.EventHandler(this.zoomComboControl_ValueChanged);
            // 
            // mCompButton
            // 
            this.mCompButton.Location = new System.Drawing.Point(949, 3);
            this.mCompButton.Name = "mCompButton";
            this.mCompButton.Size = new System.Drawing.Size(72, 24);
            this.mCompButton.TabIndex = 8;
            this.mCompButton.Text = "Compare";
            this.mCompButton.Click += new System.EventHandler(this.CompareButton_Click);
            // 
            // mTextComp2
            // 
            this.mTextComp2.Location = new System.Drawing.Point(846, 6);
            this.mTextComp2.Name = "mTextComp2";
            this.mTextComp2.Size = new System.Drawing.Size(97, 20);
            this.mTextComp2.TabIndex = 9;
            // 
            // mTextComp1
            // 
            this.mTextComp1.Location = new System.Drawing.Point(748, 6);
            this.mTextComp1.Name = "mTextComp1";
            this.mTextComp1.Size = new System.Drawing.Size(92, 20);
            this.mTextComp1.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(549, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select:";
            // 
            // mCatSelectionBox
            // 
            this.mCatSelectionBox.FormattingEnabled = true;
            this.mCatSelectionBox.IntegralHeight = false;
            this.mCatSelectionBox.Location = new System.Drawing.Point(595, 4);
            this.mCatSelectionBox.Name = "mCatSelectionBox";
            this.mCatSelectionBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.mCatSelectionBox.Size = new System.Drawing.Size(104, 26);
            this.mCatSelectionBox.TabIndex = 1;
            this.mCatSelectionBox.SelectedIndexChanged += new System.EventHandler(this.categorySelection_Enter);
            // 
            // MemoryGraph
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1028, 753);
            this.Controls.Add(this.mCatEnabled);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mCatSelectionBox);
            this.Controls.Add(this.mTextComp1);
            this.Controls.Add(this.mTextComp2);
            this.Controls.Add(this.mCompButton);
            this.Controls.Add(this.mZoomComboControl);
            this.Controls.Add(this.mFilterButton);
            this.Controls.Add(this.mFilterTextBox);
            this.Controls.Add(this.mMainPanel);
            this.Controls.Add(this.mGraphHScrollBar);
            this.Controls.Add(this.mGraphVScrollBar);
            this.Controls.Add(this.mGraphPanel);
            this.Name = "MemoryGraph";
            this.Text = "MemoryGraph";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MemoryGraph_Closing);
            this.mGraphPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mMemoryGraphPB)).EndInit();
            this.mMainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void memoryGraphPB_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			e.Graphics.Clear(Color.White);
            int fullMemSpan = mMaxMemoryAllocation + mMinMemoryAllocation;

			int xOffset = (mGraphScale > 1 ? ((mGraphScale - 1) * (int)mMemoryGraphPB.Width) * mGraphHScrollBar.Value / (mGraphHScrollBar.Maximum - mGraphHScrollBar.LargeChange) : 0);
			int yOffset = (mGraphScale > 1 ? ((mGraphScale - 1) * (int)mMemoryGraphPB.Height) * (mGraphVScrollBar.Value) / (mGraphVScrollBar.Maximum - mGraphVScrollBar.LargeChange) : 0);
			
            Range highlightStart = mHighlightIndexStart;
            Range highlightEnd = mHighlightIndexEnd;
            if (mHighlightIndexStart != null && mHighlightIndexEnd != null)
            {
                if (highlightEnd.mMin < highlightStart.mMin)
                {
                    highlightEnd = mHighlightIndexStart;
                    highlightStart = mHighlightIndexEnd;
                }
            }

            // draw the body of the graph...
            int labelIndex = 0;
			for (int i = 0; i + xOffset < mMemoryGraphRangeData.Length && i < (int)mMemoryGraphPB.Width ; i++)
			{
				RangeObject range = mMemoryGraphRangeData[i + xOffset];

                int y0 = (int)( mMaxMemoryAllocation * mHeightUnit) - yOffset;
                int y2 = (int)((mMaxMemoryAllocation - range.mMax) * mHeightUnit) - yOffset;
                int y3 = (int)((mMaxMemoryAllocation - range.mUnfilteredMax) * mHeightUnit) - yOffset;

				if (highlightStart != null && highlightEnd != null && highlightStart.mMin <= range.mLogIndex.mMax && highlightEnd.mMax >= range.mLogIndex.mMin)
				{ // draw highlighted area...
					e.Graphics.DrawLine( Pens.LemonChiffon, i, y0, i, (int)mMemoryGraphPB.Height);
					e.Graphics.DrawLine(Pens.Yellow, i, y2, i, (int)mMemoryGraphPB.Height);
				//	e.Graphics.DrawLine( Pens.Red, i, y1, i, y2);
				}
				else if (range.mLogIndex.mMin <= (int)mFramesArray[labelIndex] &&
					(int)mFramesArray[labelIndex] <= range.mLogIndex.mMax)
				{ // draw frame swap lines...
				//	e.Graphics.DrawLine(Pens.Black, i, 0, i, (int)mMemoryGraphPB.Height);
					e.Graphics.DrawLine(Pens.Black, i, y2, i, (int)mMemoryGraphPB.Height);
 				//	e.Graphics.DrawLine(Pens.Black, i, 0, i, y2);
				}
				else
				{   // draw the main area...
					e.Graphics.DrawLine( Pens.LightGray, i, y2, i, y3);
					e.Graphics.DrawLine( Pens.LightCoral, i, y2, i, (int)mMemoryGraphPB.Height);
				//	e.Graphics.DrawLine( Pens.Red, i, y1, i, y2);

                    // draw category points....
                    if (mNumberOfCategories > 0)
                    {
                        int prevy = 0;
                        int pym = (int)mMemoryGraphPB.Height;
                        for (int k = 0; k < mNumberOfCategories; k++)
                        {
                            if(mSelectedCategoryInices[0] || mSelectedCategoryInices[k] )
                            {
                                prevy += (int)range.mCategoryMemory[k];
                                int ym = (int)((mMaxMemoryAllocation - prevy) * mHeightUnit) - yOffset;
                                //  int yM = (int)((mMaxMemoryAllocation - ((Range)(range.mCategoryMemory[k])).mMax) * mHeightUnit) - yOffset;
                                //  e.Graphics.FillRectangle(colorBrushes[k%3], i, ym, 1, 1);
                                e.Graphics.DrawLine(mCategoryPens[k % 3], i, pym, i, ym);
                                pym = ym;

                            }
                        }
                   }
				}

				if ((int)mFramesArray[labelIndex] <= range.mLogIndex.mMin)
				{
					labelIndex++;
				}
			}

            // draw label vertical lines...
            int yloc = 0;
			for (int i = 0; i < mLabelsArray.Count; i++)
			{
				int logIndex = (int)mLabelsArray[i];
				MemManager.Log.LogEntry logentry = mLog[logIndex];
				int x = ConvertLogIndexToScreenX(logIndex);
                e.Graphics.DrawLine(Pens.LightGray, x, 0, x, (int)mMemoryGraphPB.Height);
                yloc += 27;
                if (yloc > mMemoryGraphPB.Height)
                    yloc = 0;
              //  System.Diagnostics.Debug.Print(mLog.GetString(logentry.nameString) + " at ({0},{1})", x, 10 + 25 * i);
                e.Graphics.DrawString(mLog.GetString(logentry.nameString), mMemoryGraphPB.Parent.Font, Brushes.BlueViolet, x, yloc);
			}

            // draw scale horizontal lines...
            int numBars = 4 * mGraphScale;
            float fullspan = (0== mMinMemoryAllocation) ? 0.0f : 0.5f;
            for (int i = 0; i < numBars; i++)
            {
                float muliplier = (float)(i) / (float)numBars ;
                int yPos = (int)((mMaxMemoryAllocation * (1.0f - muliplier)) * mHeightUnit) - yOffset;
               // int yPos = mMemoryGraphPB.Height - ((int)((mMinMemoryAllocation + (float)mMaxMemoryAllocation*muliplier) * oneUnit) - yOffset);
               if (yPos >= 0 && yPos <= mMemoryGraphPB.Height)
                {
                    string valueStr;
                   // int val = fullMemSpan;
                  //  val = (int)((float)val * (1.0 - muliplier) - (float)val * fullspan);
                    int val = (int)((float)mMaxMemoryAllocation * muliplier);
                    if (Math.Abs(val) >= 1024 * 1024)
                    {
                        valueStr = ((float)val / 1024.0f / 1024.0f).ToString("F2") + " MB";
                    }
                    else //if (val > 1024)
                        valueStr = ((float)val / 1024.0f).ToString("F2") + " KB";
                    
                    e.Graphics.DrawString(valueStr, mMemoryGraphPB.Parent.Font, Brushes.Black, 0, yPos);
                    e.Graphics.DrawLine(Pens.LightGray, 0, yPos, (int)mMemoryGraphPB.Width, yPos);
                }
            }
		}

		private void memoryGraphPB_Resize(object sender, System.EventArgs e)
		{
			GenerateGraphicalData();
		}

		private void zoomLevelControl_SelectedItemChanged(object sender, System.EventArgs e)
		{
			GenerateGraphicalData();
		}

		private void zoomComboControl_ValueChanged(object sender, System.EventArgs e)
		{
			if (mGraphScale == 1)
			{
				mGraphHScrollBar.Maximum = 100 * (mZoomComboControl.SelectedIndex + 1);
				mGraphVScrollBar.Maximum = 100 * (mZoomComboControl.SelectedIndex + 1);
				mGraphHScrollBar.Value = 0;
				mGraphVScrollBar.Value = mGraphVScrollBar.Maximum - mGraphVScrollBar.LargeChange + 1;
			}
			else
			{
				float y = (float)(mGraphVScrollBar.Value) / (float)(mGraphVScrollBar.Maximum - mGraphVScrollBar.LargeChange + 1);
				mGraphHScrollBar.Maximum = 100 * (mZoomComboControl.SelectedIndex + 1);
				mGraphVScrollBar.Maximum = 100 * (mZoomComboControl.SelectedIndex + 1);
				int newVal = (int)((float)(mGraphVScrollBar.Maximum - mGraphVScrollBar.LargeChange + 1) * y);
				if (newVal > mGraphVScrollBar.Maximum - mGraphVScrollBar.LargeChange + 1)
				{
					newVal = mGraphVScrollBar.Maximum + mGraphVScrollBar.LargeChange + 1;
				}
				mGraphVScrollBar.Value = newVal;
			}
			mGraphScale = mZoomComboControl.SelectedIndex + 1;
            mHeightUnit = (float)mMemoryGraphPB.Height * (float)mGraphScale / (float)mMaxMemoryAllocation;

			GenerateGraphicalData();
			mGraphVScrollBar.Enabled = (mGraphScale > 1);
			mGraphHScrollBar.Enabled = (mGraphScale > 1);

			mMemoryGraphPB.Refresh();
		}

		private void graphHScrollBar_ValueChanged(object sender, System.EventArgs e)
		{
			mMemoryGraphPB.Refresh();
		}

		private void graphVScrollBar_ValueChanged(object sender, System.EventArgs e)
		{
			mMemoryGraphPB.Refresh();
		}

		private void graphVScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			mMemoryGraphPB.Refresh();
		}

		private void graphHScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			mMemoryGraphPB.Refresh();
		}

		void PopulateDB()
		{
			DataSet myDataSet = new DataSet("DataSet");
			DataTable table;
			DataColumn column;
			DataGridColumnStyle columnstyle;
			string cats = "1Index|0type|0Category|0Name|1Alignment|1ActualSize|2Address";

			DataGridTableStyle tableStyle = new DataGridTableStyle(); 
			tableStyle.MappingName = "All"; 
			tableStyle.RowHeaderWidth = 32; 
			tableStyle.AlternatingBackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));

			table = new DataTable( "All" );
			table.BeginLoadData();
			foreach (string s in cats.Split('|'))
			{
				string n = s.Substring(1);

				column = new DataColumn();
				column.ColumnName = n;
				column.ReadOnly = true;
				column.Unique = false;				
				if (s[0] == '0')
				{
					column.DataType = System.Type.GetType("System.String");
					columnstyle = new DataGridTextBoxColumn();
				}
				else if (s[0] == '1')
				{
					column.DataType = System.Type.GetType("System.Int32");
					columnstyle = new DataGridTextBoxColumn();
				}
                else if (s[0] == '2')
                {
                    column.DataType = System.Type.GetType("System.UInt32");
                    columnstyle = new DataGridTextBoxColumn();
                }
                else //if (s[0] == '3')
				{
					column.DataType = System.Type.GetType("System.Boolean");
					columnstyle = new DataGridBoolColumn();
				}
				table.Columns.Add(column);
				
				columnstyle.MappingName = n;
				columnstyle.HeaderText = n;
				columnstyle.Width = 68;
                if (n == "Name")
                    columnstyle.Width = 480;
                else if (n == "Address")
                    columnstyle.Width = 70;
                else if (n == "Alignment")
                    columnstyle.Width = 50;
                else if (n == "Category")
                    columnstyle.Width = 80;
//                 else if (n == "Temporary")
//                     columnstyle.Width = 60;
				tableStyle.GridColumnStyles.Add( columnstyle );
			}

			if (mHighlightIndexStart != null && mHighlightIndexEnd != null )
			{

				Range highlightStart = mHighlightIndexStart;
				Range highlightEnd = mHighlightIndexEnd;
				if (highlightEnd.mMin < highlightStart.mMin)
				{
					highlightEnd = mHighlightIndexStart;
					highlightStart = mHighlightIndexEnd;
				}
				if (highlightStart.mMin < 0)
				{
					highlightStart.mMin = 0;
				}
				if (highlightEnd.mMax >= mLog.Count)
				{
					highlightEnd.mMax = mLog.Count - 1;
				}

				Regex regex = new Regex(mFilterTextBox.Text, RegexOptions.Compiled|RegexOptions.IgnoreCase);
				for (int i = highlightStart.mMin; i <= highlightEnd.mMax; i++)
				{
					MemManager.Log.LogEntry le = mLog[i];

					bool filtered = false;
					if (mFilterTextBox.Text != "")
					{
						filtered = !(regex.IsMatch(mLog.GetString(le.nameString)));
					}

					if (!filtered)
					{

						DataRow row = table.NewRow();
						row["Index"] = i;
						if (le.type == 'L')
						{
							row["type"] = "L";
							row["Name"] = mLog.GetString(le.nameString);
						}
                        else if (le.type == 'S')
                        {
                            row["type"] = "S";
                            row["Name"] = "Frame";
                        }
						else
						{
							if (le.type == 'A')
							{
								row["type"] = "Alloc";
							}
							else if (le.type == 'F')
							{
								row["type"] = "Free";
							}
							string categoryName = mLog.GetCategory(le.category);
							row["Category"] = categoryName;					
							row["Name"] = mLog.GetString(le.nameString);
					//		row["Alignment"] = le.alignment;
					//		row["ReqSize"] = le.reqSize;
							row["ActualSize"] = le.allocSize;
							row["Address"] = le.address;
						}
						table.Rows.Add(row);
					}
				}
			}
			table.EndLoadData();

			// Add the table to the dataset
			myDataSet.Tables.Add(table);

			// Make the dataGrid use our new table style and bind it to our table 
			mDataGrid.TableStyles.Clear();
			mDataGrid.TableStyles.Add(tableStyle); 

			// Set up grid bindings
			mDataGrid.SetDataBinding(myDataSet, "All");
		}

		private Range ConvertMouseXtoLogIndex(int mouseX)
		{
            int x = (int)((float)mouseX / mWidthUnit);

			int graphIndex = (mGraphScale > 1 ? ((mGraphScale - 1) * (int)mMemoryGraphPB.Width) * mGraphHScrollBar.Value / (mGraphHScrollBar.Maximum - mGraphHScrollBar.LargeChange) : 0);
			RangeObject range = mMemoryGraphRangeData[graphIndex];

			return new Range(x + range.mLogIndex.mMin, x + range.mLogIndex.mMax);
		}

		private int ConvertLogIndexToScreenX(int logIndex)
		{
            int x = (int)((float)logIndex * mWidthUnit);

			int offset = (mGraphScale > 1 ? ((mGraphScale - 1) * (int)mMemoryGraphPB.Width) * mGraphHScrollBar.Value / (mGraphHScrollBar.Maximum - mGraphHScrollBar.LargeChange) : 0);

			return x - offset;
		}

		public void HilightLogEntry(int logIndex)
		{
			mHighlightIndexStart = new Range(logIndex, logIndex);
			mHighlightIndexEnd = new Range(logIndex, logIndex);

			PopulateDB();
			mMemoryGraphPB.Refresh();
		}

		bool mousePressed = false;
		private void memoryGraphPB_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				mousePressed = true;
                if (mComparePressed)
                {
                    if (mCompareProcessed)
                    {
                        mTextComp1.Text = mTextComp2.Text = "";
                        mHighlightIndexEnd = mHighlightIndexStart = null;
                        mCompareProcessed = false;
                    }

                    if(mTextComp1.Text == "")
                    {
                        mHighlightIndexStart = ConvertMouseXtoLogIndex(e.X);
                        mTextComp1.Text = mHighlightIndexStart.mMin.ToString();
                    }
                    else if(mTextComp2.Text == "")
                    {
                        mHighlightIndexEnd = ConvertMouseXtoLogIndex(e.X);
                        mTextComp2.Text = mHighlightIndexEnd.mMax.ToString();
                    }
                }
                else
                {
                    mHighlightIndexStart = ConvertMouseXtoLogIndex(e.X);
				    mHighlightIndexEnd = ConvertMouseXtoLogIndex(e.X);
                }

				PopulateDB();
				mMemoryGraphPB.Refresh();
			}
		}

		private void memoryGraphPB_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (mousePressed)
			{
				mHighlightIndexEnd = ConvertMouseXtoLogIndex(e.X);
				PopulateDB();
				mMemoryGraphPB.Refresh();
			}
		}

		private void memoryGraphPB_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			memoryGraphPB_MouseMove(sender, e);
			mousePressed = false;
			if (mHighlightIndexEnd != null && mHighlightIndexStart != null)
			{
                if (mComparePressed && mTextComp2.Text != "")
                {
                    mMainViewWindow.UpdateSnapShot(mHighlightIndexStart.mMin);
                    mMainViewWindow.CompareSnapShot(mHighlightIndexEnd.mMax + 1);
                    PopulateDB();
                    mMemoryGraphPB.Refresh();
                    mComparePressed = false;
                    mCompareProcessed = true;
                }
                else
                    mMainViewWindow.UpdateSnapShot(mHighlightIndexEnd.mMax + 1);
			}
		}

		private void MemoryGraph_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			mMainViewWindow.CloseMemoryGraph();
		}

		private void panel1_SizeChanged(object sender, System.EventArgs e)
		{
			mMemoryGraphPB.Invalidate();
		}

        private void CompareButton_Click(object sender, System.EventArgs e)
        {
            mComparePressed = true;
            mHighlightIndexEnd = mHighlightIndexStart = null;
            mTextComp1.Text = mTextComp2.Text = "";
        }

		private void FilterButton_Click(object sender, System.EventArgs e)
		{
			CreateMemoryGraph(mLog);
			mMemoryGraphPB.Invalidate();
		}

		private void FilterTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
			{
				CreateMemoryGraph(mLog);
				mMemoryGraphPB.Invalidate();
			}		
		}

		private void mDataGrid_MouseUp(object sender, MouseEventArgs e)
		{
			System.Drawing.Point pt = new Point(e.X, e.Y);
			DataGrid.HitTestInfo hti = mDataGrid.HitTest(pt);
			if (hti.Type == DataGrid.HitTestType.Cell)
			{
				mDataGrid.CurrentCell = new DataGridCell(hti.Row, hti.Column);
				mDataGrid.Select(hti.Row);

				int index = (int)mDataGrid[mDataGrid.CurrentCell.RowNumber, 0];

				mMainViewWindow.UpdateSnapShot(index + 1);
			}  
		}

        private void categorySelection_Enter(object sender, EventArgs e)
        {
            if (sender is ListBox && mCategoryEnabled)
            {
                ListBox lb = sender as ListBox;
                if (mSelectedCategoryInices == null)
                    mSelectedCategoryInices = new bool[mNumberOfCategories + 1];
                Array.Clear(mSelectedCategoryInices, 0, mSelectedCategoryInices.Length);
          //      mSelectedCategoryInices[0] = true;
                foreach (int i in lb.SelectedIndices)
                {
                     mSelectedCategoryInices[i] = true;
                }
              
                mMemoryGraphPB.Invalidate();
                mMainViewWindow.UpdateCategories(mSelectedCategoryInices);
            }

        }

        private void categoryEnabled_Click(object sender, EventArgs e)
        {
            if(sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                if (cb.Checked)
                    mCategoryEnabled = true;
                else
                    mCategoryEnabled = false;

                CreateMemoryGraph(mLog);
                mMemoryGraphPB.Invalidate();
            }
        }

	}
}
