/*
 * Simple but effective Memory Log/Snapshot viewer..
 */
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
	/// Summary description for Form1.
	/// </summary>
	public class MainWindow : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.DataGrid mDataGrid;
		private System.Windows.Forms.MainMenu mMainMenu;
		private System.Windows.Forms.MenuItem mMenuItemOpen;
		private System.Windows.Forms.MenuItem mMenuItemExit;
		private System.Windows.Forms.StatusBar mStatusBar;
		private System.Windows.Forms.OpenFileDialog mOpenFileDialog;
		private System.Windows.Forms.MenuItem mMenuItemGoto;
		private System.Windows.Forms.MenuItem mMenuItemFilter;
		private System.Windows.Forms.SaveFileDialog mSaveFileDialog;
		private System.Windows.Forms.MenuItem mMenuItemSave;
		private System.Windows.Forms.MenuItem mMenuItemView;
		//private System.Windows.Forms.MenuItem mMenuItemCallstack;
		private System.Windows.Forms.MenuItem mMenuItemHW;
		private System.Windows.Forms.MenuItem mMenuItemEndOfLog;
		private System.Windows.Forms.MenuItem mMenuItemGV;
        private System.Windows.Forms.MenuItem mMenuItemMG;
        private IContainer components;

		private MemoryGraph mMemoryGraph = null;
        private MenuItem menuItem2;
        private MenuItem mCategoryView;
        private MenuItem mHeapView;
		private GraphicalView mGraphicalView = null;
        private bool mCategoryMode = false;
        private string mFilename = string.Empty;
        private string mFilter = string.Empty;

		public MainWindow()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            string displayMode = LoadFromRegistry("displayMode", "category");
            if(displayMode.Equals("category"))
            {
                mCategoryView.Checked = true;
                mHeapView.Checked = false;
                mCategoryMode = true;
            }
            else
            {
                mCategoryView.Checked = false;
                mHeapView.Checked = true;
                mCategoryMode = false;
            }
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			mMenuItemView.Enabled = false;
			//mMenuItemCallstack.Enabled = false;
			mMenuItemGV.Enabled = false;
			mMenuItemMG.Enabled = false;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.mMainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mMenuItemOpen = new System.Windows.Forms.MenuItem();
            this.mMenuItemSave = new System.Windows.Forms.MenuItem();
            this.mMenuItemExit = new System.Windows.Forms.MenuItem();
            this.mMenuItemView = new System.Windows.Forms.MenuItem();
            this.mMenuItemGoto = new System.Windows.Forms.MenuItem();
            this.mMenuItemEndOfLog = new System.Windows.Forms.MenuItem();
            this.mMenuItemHW = new System.Windows.Forms.MenuItem();
           // this.mMenuItemCallstack = new System.Windows.Forms.MenuItem();
            this.mMenuItemFilter = new System.Windows.Forms.MenuItem();
            this.mMenuItemGV = new System.Windows.Forms.MenuItem();
            this.mMenuItemMG = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.mCategoryView = new System.Windows.Forms.MenuItem();
            this.mHeapView = new System.Windows.Forms.MenuItem();
            this.mStatusBar = new System.Windows.Forms.StatusBar();
            this.mDataGrid = new System.Windows.Forms.DataGrid();
            this.mOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.mSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.mDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // mMainMenu
            // 
            this.mMainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.mMenuItemView});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mMenuItemOpen,
            this.mMenuItemSave,
            this.mMenuItemExit});
            this.menuItem1.Text = "&File";
            // 
            // mMenuItemOpen
            // 
            this.mMenuItemOpen.Index = 0;
            this.mMenuItemOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.mMenuItemOpen.Text = "&Open";
            this.mMenuItemOpen.Click += new System.EventHandler(this.mMenuItemOpen_Click);
            // 
            // mMenuItemSave
            // 
            this.mMenuItemSave.Enabled = false;
            this.mMenuItemSave.Index = 1;
            this.mMenuItemSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.mMenuItemSave.Text = "&Save As";
            this.mMenuItemSave.Click += new System.EventHandler(this.menuItemSave_Click);
            // 
            // mMenuItemExit
            // 
            this.mMenuItemExit.Index = 2;
            this.mMenuItemExit.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
            this.mMenuItemExit.Text = "E&xit";
            this.mMenuItemExit.Click += new System.EventHandler(this.mMenuItemExit_Click);
            // 
            // mMenuItemView
            // 
            this.mMenuItemView.Index = 1;
            this.mMenuItemView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mMenuItemGoto,
            this.mMenuItemEndOfLog,
            this.mMenuItemHW,
           // this.mMenuItemCallstack,
            this.mMenuItemFilter,
            this.mMenuItemGV,
            this.mMenuItemMG,
            this.menuItem2});
            this.mMenuItemView.Text = "&View";
            // 
            // mMenuItemGoto
            // 
            this.mMenuItemGoto.Index = 0;
            this.mMenuItemGoto.Text = "&Move to Label";
            // 
            // mMenuItemEndOfLog
            // 
            this.mMenuItemEndOfLog.Index = 1;
            this.mMenuItemEndOfLog.Text = "Move to &End of Log";
            this.mMenuItemEndOfLog.Click += new System.EventHandler(this.mMenuItemEndOfLog_Click);
            // 
            // mMenuItemHW
            // 
            this.mMenuItemHW.Index = 2;
            this.mMenuItemHW.Shortcut = System.Windows.Forms.Shortcut.F9;
            this.mMenuItemHW.Text = "Move to High &Watermark";
            // 
            // mMenuItemCallstack
            // 
//             this.mMenuItemCallstack.Enabled = false;
//             this.mMenuItemCallstack.Index = 7;
//             this.mMenuItemCallstack.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
//             this.mMenuItemCallstack.Text = "View &Callstack";
//             this.mMenuItemCallstack.Click += new System.EventHandler(this.menuItemCallstack_Click);
            // 
            // mMenuItemFilter
            // 
            this.mMenuItemFilter.Index = 3;
            this.mMenuItemFilter.Shortcut = System.Windows.Forms.Shortcut.CtrlF;
            this.mMenuItemFilter.Text = "Filter";
            this.mMenuItemFilter.Click += new System.EventHandler(this.mMenuItemFilter_Click);
            // 
            // mMenuItemGV
            // 
            this.mMenuItemGV.Index = 4;
            this.mMenuItemGV.Text = "Graphical View";
            // 
            // mMenuItemMG
            // 
            this.mMenuItemMG.Index = 5;
            this.mMenuItemMG.Text = "Memory Graph";
            this.mMenuItemMG.Click += new System.EventHandler(this.mMenuItemMG_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 6;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mCategoryView,
            this.mHeapView});
            this.menuItem2.Text = "Display Mode";
            // 
            // mCategoryView
            // 
            this.mCategoryView.Index = 0;
            this.mCategoryView.Text = "Category";
            this.mCategoryView.Click += new System.EventHandler(this.mCategoryView_Click);
            // 
            // mHeapView
            // 
            this.mHeapView.Index = 1;
            this.mHeapView.Text = "Heap";
            this.mHeapView.Click += new System.EventHandler(this.mHeapView_Click);
            // 
            // mStatusBar
            // 
            this.mStatusBar.Location = new System.Drawing.Point(0, 598);
            this.mStatusBar.Name = "mStatusBar";
            this.mStatusBar.Size = new System.Drawing.Size(804, 22);
            this.mStatusBar.TabIndex = 0;
            // 
            // mDataGrid
            // 
            this.mDataGrid.AllowDrop = true;
            this.mDataGrid.AlternatingBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.mDataGrid.CaptionVisible = false;
            this.mDataGrid.DataMember = "";
            this.mDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.mDataGrid.Location = new System.Drawing.Point(0, 0);
            this.mDataGrid.Name = "mDataGrid";
            this.mDataGrid.ReadOnly = true;
            this.mDataGrid.Size = new System.Drawing.Size(804, 598);
            this.mDataGrid.TabIndex = 1;
            this.mDataGrid.DragEnter += new System.Windows.Forms.DragEventHandler(this.SupportDragEnter);
            this.mDataGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mDataGrid_MouseUp);
            this.mDataGrid.KeyUp += new System.Windows.Forms.KeyEventHandler(this.mDataGrid_KeyUp);
            this.mDataGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.SupportDragDrop);
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(804, 620);
            this.Controls.Add(this.mDataGrid);
            this.Controls.Add(this.mStatusBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mMainMenu;
            this.Name = "MainWindow";
            this.Text = "Mem Visualizer";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.SupportDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.SupportDragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.mDataGrid)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] argv) 
		{
			MainWindow app = new MainWindow();
			int argc = argv.Length;
			if (argc >= 1)
			{
				app.LoadLog(argv[0]);
			}
			Application.Run( app );
		}

		//-------------------------------------------------------------------
		public struct Item
		{
			public int count;  // number of repeat of this log entry in Log file
			public int index;  // index of the log item in Log file
		};
		
		//-------------------------------------------------------------------
		static MemManager.Log.Log mLog;
		ArrayList mAllocators;
		MemManager.Log.SnapShot mLogSnap;
		ArrayList mCollapsedView;

		//-------------------------------------------------------------------
		class CompareCategorySizeAndName : IComparer
		{
			int System.Collections.IComparer.Compare(object x, object y)
			{
				MemManager.Log.LogEntry a = (MemManager.Log.LogEntry)x;
				MemManager.Log.LogEntry b = (MemManager.Log.LogEntry)y;
				int ret;
				
				ret = ((int)a.category - (int)b.category);
				if (ret == 0)
					ret = ((int)b.allocSize - (int)a.allocSize);
// 				if (ret == 0)
// 					ret = ((int)b.reqSize - (int)a.reqSize);
				if (ret == 0)
					ret = ((int)a.nameString - (int)b.nameString);
				return ret;
			}
		};

		//-------------------------------------------------------------------
		class CollapsedViewSort : IComparer
		{
			int System.Collections.IComparer.Compare(object x, object y)
			{
				Item a = (Item)x;
				Item b = (Item)y;
                MemManager.Log.LogEntry le_a = mLog[a.index];
                MemManager.Log.LogEntry le_b = mLog[b.index];
                int ret;

                ret = ((int)le_a.category - (int)le_b.category);
				if (ret == 0)
                    ret = ((int)(le_b.allocSize * b.count) - (int)(le_a.allocSize * a.count));
				if (ret == 0)
                    ret = le_a.nameString - le_b.nameString;
				return ret;
			}
		};
			
		//-------------------------------------------------------------------
		void LoadLog(string fname)
		{
			if (!System.IO.File.Exists(fname))
				return;

			Cursor.Current = Cursors.WaitCursor;
			if (fname.ToLower().EndsWith(".bin"))
			{
				mLog = new MemManager.Log.Log();				
				if (mLog.Load(fname) == false)
				{
					Cursor.Current = Cursors.Default;
					System.Windows.Forms.MessageBox.Show("File was invalid, loading stopped", "Error");					
					return;
				}
                if (!mCategoryMode)
                    mLog.SwapToHeap();
				mLogSnap = new MemManager.Log.SnapShot(mLog, mLog.Count, false);
				mAllocators = mLog.GetAllocatorList();				
			}
			else
			{
				MemManager.Log.TCRLoader loader = new MemManager.Log.TCRLoader();
				mLog = loader.Load(fname);
				if (mLog == null)
				{
					Cursor.Current = Cursors.Default;
					System.Windows.Forms.MessageBox.Show("File was invalid, loading stopped", "Error");
					return;
				}
                if (!mCategoryMode)
                    mLog.SwapToHeap();
                mLogSnap = new MemManager.Log.SnapShot(mLog, mLog.Count, false);
                mAllocators = mLog.GetAllocatorList();
			}

            mFilename = fname;
			Cursor.Current = Cursors.Default;
            CreateInitialView();
        }

        private void CreateInitialView()
        {
            string fname = mFilename;
            mFilter = "";
			// Now Create initial view
			PopulateLabelList();
			PopulateHW();
			PopulateGraphicalView();
            PopulateFromSnapshot(mLog, mLogSnap, mFilter, 0, false);
			mMenuItemSave.Enabled = true;		// Enable save button
			
			// Update window title - enable view menu / callstack and graphical view
			this.Text = "Mem Visualizer - " + fname;
			mMenuItemView.Enabled = true;
//			mMenuItemCallstack.Enabled = mLog.FindFieldName("StackTrace");
			mMenuItemGV.Enabled = (mLog.GetAllocatorList().Count == 0) ? false : true;
			mMenuItemMG.Enabled = (mLog.GetFormat() == "MetricsMemoryLog");
			if (mMemoryGraph != null)
			{
				mMemoryGraph.Close();
				mMemoryGraph = null;
			}
			if (mGraphicalView != null)
			{
				mGraphicalView.Close();
				mGraphicalView = null;
			}
		}

		//-------------------------------------------------------------------
		void PopulateFromSnapshot(
			MemManager.Log.Log log,
			MemManager.Log.SnapShot snap,
			string filter,
            int size,
            bool dir
			)
		{
			// Make copy of view, ready to sort and collapse
            ArrayList view = new ArrayList(snap.Count);
			if (filter == "")
			{
				for (int i = 0, e = snap.Count; i < e; i++)
				{
					int logindex = snap[i];
					MemManager.Log.LogEntry le = log[ logindex ];
                    System.Diagnostics.Debug.Assert(le.type == 'A', "Error! All entries in snapshot should be allocations!");
                    if (le.index != (uint)logindex)
                        System.Diagnostics.Debug.Print("Something wrong with indexing");
					view.Add( le );
				}
			}
			else
			{
				Regex r = new Regex(filter, RegexOptions.Compiled|RegexOptions.IgnoreCase);
				for (int i = 0, e = snap.Count; i < e; i++)
				{
					int logindex = snap[i];
					MemManager.Log.LogEntry le = log[ logindex ];
                    //   le.index = (uint)logindex;
                    string name = mLog.GetString(le.nameString);
                    if (r.IsMatch(name))
                    {
                        if (size == 0)
                            view.Add(le);
                        else if (dir == false && le.allocSize >= size)
                            view.Add(le);
                        else if (dir == true && le.allocSize <= size)
                            view.Add(le);
                    }
                }
			}

            view.TrimToSize();

			// Sort view by category, size, and name-string
			view.Sort(new CompareCategorySizeAndName());

			// Create collapsed view
            mCollapsedView = new ArrayList(view.Count / 2);
			if (view.Count > 0)
			{
				MemManager.Log.LogEntry lastE = new MemManager.Log.LogEntry();
				lastE.category = 127;
                view.Add( lastE );	// append extra last object to make end logic easier!

				MemManager.Log.LogEntry last = (MemManager.Log.LogEntry)view[0];
				int count = 1;
				for (int i = 1, e = view.Count; i < e; i++)
				{
					MemManager.Log.LogEntry le = (MemManager.Log.LogEntry)view[i];
					if (last.category == le.category
						&& last.allocSize == le.allocSize
						&& last.nameString == le.nameString
				//		&& last.alignment == le.alignment
                 //	    &&	last.reqSize == le.reqSize)
                   )
					{
						count++;
					}
					else
					{
						Item it;

						it.count = count;
                        it.index = (int)last.index;

                        mCollapsedView.Add(it);
						last = le;
						count = 1;
					}
				}
			}

            mCollapsedView.TrimToSize();
            mCollapsedView.Sort(new CollapsedViewSort());
			PopulateDB(log);
		}

		//-------------------------------------------------------------------
		void PopulateDB( MemManager.Log.Log log )
		{
            DataSet myDataSet = new DataSet("DataSet");
			DataTable table;
			DataColumn column;
			DataGridColumnStyle columnstyle;
			string cats = "1Index|";
            if (mCategoryMode)
                cats += "0Category|";
            else
                cats += "0Heap|";

        //    cats+= "0Name|1Alignment|1ActualSize|1Count|1TotalSize";
            cats += "0Name|1ActualSize|1Count|1TotalSize";

			DataGridTableStyle tableStyle = new DataGridTableStyle(); 
			tableStyle.MappingName = "All"; 
			tableStyle.RowHeaderWidth = 10; 
			tableStyle.AlternatingBackColor = System.Drawing.Color.FromArgb(((System.Byte)(250)), ((System.Byte)(250)), ((System.Byte)(250)));

			table = new DataTable( "All" );
			table.BeginLoadData();
			foreach (string s in cats.Split('|'))
			{
				string n = s.Substring(1);

                //if (n == "Index")
                //    continue;
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
				else //if (s[0] == '2')
				{
					column.DataType = System.Type.GetType("System.Boolean");
					columnstyle = new DataGridBoolColumn();
				}
				table.Columns.Add(column);
				
				columnstyle.MappingName = n;
				columnstyle.HeaderText = n;
				columnstyle.Width = 70;
                if (n == "Name")
                    columnstyle.Width = 360;
                else if (n == "Heap")
                    columnstyle.Width = 70;
                else if (n == "Category")
                    columnstyle.Width = 90;
                else if (n == "Count" ) //|| n == "Alignment")
                    columnstyle.Width = 42;
				tableStyle.GridColumnStyles.Add( columnstyle );
			}

            for (int j = 0; j < mCollapsedView.Count; j++)
			{
                Item item = (Item)mCollapsedView[j];
                MemManager.Log.LogEntry le_a = log[item.index];

                string allocatorName = log.GetAllocator(le_a.allocator);
                string categoryName = mCategoryMode ? log.GetCategory(le_a.category) : log.GetCategory(0);

				DataRow row = table.NewRow();
				row["Index"] = item.index;
                if (mCategoryMode)
                    row["Category"] = categoryName;
                else
                    row["Heap"] = allocatorName;

             //   string removalString = log.GetAllocator(item.allocator) + "::";

             //   if (mCategoryMode)
              //      removalString += categoryName + "::";

                row["Name"] = log.GetString(le_a.nameString); //.Substring(removalString.Length);
           //     row["Alignment"] = item.alignment;
		//		row["ReqSize"] = item.requestedSize;
                row["ActualSize"] = le_a.allocSize;
				row["Count"] = item.count;
                row["TotalSize"] = item.count * le_a.allocSize;

                table.Rows.Add(row);
			}
			table.EndLoadData();

			// Add the table to the dataset
			myDataSet.Tables.Add(table);

			// Make the dataGrid use our new table style and bind it to our table 
			mDataGrid.TableStyles.Clear();
			mDataGrid.TableStyles.Add(tableStyle); 

			// Set up grid bindings
			mDataGrid.SetDataBinding(myDataSet, "All");

			// Update status
			UpdateStatusBar();
		}

		//-------------------------------------------------------------------
		void UpdateStatusBar()
		{
			uint total = 0;
			uint totalAll = 0;
			int allocs = 0;
			int allocsAll = 0;
			int selected = 0;

			for (int i = 0, e = mCollapsedView.Count; i < e; i++)
			{
				Item item = (Item)mCollapsedView[i];
                MemManager.Log.LogEntry le = mLog[item.index];
                uint size = (uint)(le.allocSize * item.count);

				allocsAll += item.count;
				totalAll += size;

				if (mDataGrid.IsSelected(i))
				{
					// FIXME: remove indexes
			//		int reqSize = (int)mDataGrid[i, 4];
					int actualSize = (int)mDataGrid[i, 3];
					int count = (int)mDataGrid[i, 4];

					size = (uint)(actualSize * count);
					selected++;
					total += size;
					allocs += count;
				}
			}
             
			uint amount = total;
			int numberAllocs = allocs;
			if (selected == 0)
			{
				amount = totalAll;
				numberAllocs = allocsAll;
			}

			// Show in bar			
			mStatusBar.Text = String.Format("{0} Bytes in {1} allocations", amount, numberAllocs);
		}

		//-------------------------------------------------------------------
		private void mMenuItemOpen_Click(object sender, System.EventArgs e)
		{
			mOpenFileDialog.Filter = "Log files (*.bin,*.xml,*.log)|*.bin;*.xml;*.log|All files (*.*)|*.*" ;
			mOpenFileDialog.FilterIndex = 1;
			mOpenFileDialog.FileName = LoadFromRegistry("last_log", "");

			DialogResult res = mOpenFileDialog.ShowDialog();
			if (res == DialogResult.OK)
			{
				LoadLog( mOpenFileDialog.FileName );
                SaveToRegistry("last_log", mOpenFileDialog.FileName);
            }
        }

        //-------------------------------------------------------------------
        private void mMenuItemExit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        //-------------------------------------------------------------------
        private void mMenuItemAbout_Click(object sender, System.EventArgs e)
        {
        }

        //-------------------------------------------------------------------
        private void PopulateLabelList()
        {
            ArrayList labels = mLog.GetLabels();
            if (mMenuItemGoto.IsParent)
                mMenuItemGoto.MenuItems.Clear();

            // Disable menu item if MRU list is empty
            if (labels.Count == 0)
            {
                mMenuItemGoto.Enabled = false;
                return;
            }

            // enable menu item and add child items
            mMenuItemGoto.Enabled = true;

            MenuItem item;
            for (int i = 0; i < labels.Count; i++)
            {
                MemManager.Log.LogEntry le = (MemManager.Log.LogEntry)labels[i];

                item = new MenuItem(mLog.GetString(le.nameString));
                item.Click += new EventHandler(this.LabelClicked);
                mMenuItemGoto.MenuItems.Add(item);
            }
        }

        //-------------------------------------------------------------------
        private void PopulateHW()
        {
            if (mMenuItemHW.IsParent)
                mMenuItemHW.MenuItems.Clear();

            // enable menu item and add child items
            mMenuItemHW.Enabled = true;
            int numCat = mLog.GetNumberCategories();

            MenuItem item;
            for (int i = 0; i < numCat; i++)
            {
                string cat = (string)mLog.GetCategory((byte)i);
                item = new MenuItem(cat);
                item.Click += new EventHandler(this.HWClicked);
                mMenuItemHW.MenuItems.Add(item);
            }
        }

        //-------------------------------------------------------------------
        private void LabelClicked(object sender, System.EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            ArrayList labels = mLog.GetLabels();
            if (mi != null)
            {
                string item = mi.ToString();
                MemManager.Log.LogEntry le = (MemManager.Log.LogEntry)labels[mi.Index];
                mLogSnap = new MemManager.Log.SnapShot(mLog, (int)le.index, false);

                // Repopulate
                PopulateFromSnapshot(mLog, mLogSnap, "", 0, false);

                if (mMemoryGraph != null)
                {
                    mMemoryGraph.HilightLogEntry((int)le.index);
                }
                if (mGraphicalView != null)
                {
                    mGraphicalView.NewSnapShot(mLogSnap);
                }
            }
        }

        //-------------------------------------------------------------------
        private void HWClicked(object sender, System.EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;

            checkSelectedItem(mi);

            if (mi != null)
            {
                string item = mi.ToString();
                int category = mLog.GetIndexForCategory(mi.Text);
                int index = mLog.FindHWIndexForCategory(category);
                mLogSnap = new MemManager.Log.SnapShot(mLog, index, false);
  //              PopulateFromSnapshot(mLog, mLogSnap, "^" + mi.Text + "::");
                PopulateFromSnapshot(mLog, mLogSnap, "", 0, false);
                if (mMemoryGraph != null)
                {
                    mMemoryGraph.HilightLogEntry(index);
                }
                if (mGraphicalView != null)
                {
                    mGraphicalView.NewSnapShot(mLogSnap);
                }
            }
        }

        //-------------------------------------------------------------------
        private void mDataGrid_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Drawing.Point pt = new Point(e.X, e.Y);
            DataGrid.HitTestInfo hti = mDataGrid.HitTest(pt);
            if (hti.Type == DataGrid.HitTestType.Cell)
            {
                mDataGrid.CurrentCell = new DataGridCell(hti.Row, hti.Column);
                mDataGrid.Select(hti.Row);
            }
            if (mLog != null)
                UpdateStatusBar();
        }

        //-------------------------------------------------------------------
        private void mMenuItemFilter_Click(object sender, System.EventArgs e)
        {
            FilterSelector fs = new FilterSelector();
            DialogResult res = fs.ShowDialog();
            if (res == DialogResult.OK)
            {
                mFilter = fs.GetFilter();
                int size = fs.GetSize();
                bool dir = fs.GetDirection();
                MemManager.Log.SnapShot FilterdLogSnap = null;
                if (mFilter != "")
                    FilterdLogSnap = new MemManager.Log.SnapShot(mLog, mLogSnap, mFilter, size, dir);

                PopulateFromSnapshot(mLog, mLogSnap, mFilter, size, dir);
                if (mMemoryGraph != null)
                {
               //     mMemoryGraph.HilightLogEntries(FilterdLogSnap);
                }
                if (mGraphicalView != null)
                {
                    mGraphicalView.UpdateHighlights(FilterdLogSnap);
                }
            }
        }

        //-------------------------------------------------------------------
        private void menuItemSave_Click(object sender, System.EventArgs e)
        {
            mSaveFileDialog.Filter = "Log files (*.bin)|*.bin";
            mSaveFileDialog.FilterIndex = 1;

            DialogResult res = mSaveFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                //change log to category mode
                if (!mCategoryMode)
                {
                    mLog.SwapToCategory();
                }
                mLog.Save(mSaveFileDialog.FileName);

                //change back to heap mode
                if (!mCategoryMode)
                {
                    mLog.SwapToHeap();
                }
            }
        }

        //-------------------------------------------------------------------
        private void SupportDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData("FileDrop", false);
            if (files.Length > 0)
            {
                LoadLog(files[0]);
            }
        }

        //-------------------------------------------------------------------
        private void SupportDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        //-------------------------------------------------------------------
        private void mDataGrid_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (mLog != null)
                UpdateStatusBar();
        }

        //-------------------------------------------------------------------
        void SaveToRegistry(string opt, string val)
        {
            Microsoft.Win32.RegistryKey app = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("MemManager-MemVisualizer");
            app.SetValue(opt, val);
        }

        //-------------------------------------------------------------------
        string LoadFromRegistry(string opt, string defaultvalue)
        {
            Microsoft.Win32.RegistryKey app = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("MemManager-MemVisualizer");
            Object o = app.GetValue(opt);
            if (o == null)
                return defaultvalue;
            return (string)(o);
        }

        //-------------------------------------------------------------------
//         private void menuItemCallstack_Click(object sender, System.EventArgs ev)
//         {
//             string appPath = Application.ExecutablePath;
//             int lastDir = appPath.LastIndexOf(@"\");
// 
//           //  string map = "D:\\projects\\buckeye\\buckeye\\main\\source\\DistPS3\\Launch_PS3d.elf";
//             string defaultLocationOfAddr2line = "D:\\usr\\local\\cell";
//             if (defaultLocationOfAddr2line != null)
//                 defaultLocationOfAddr2line += "\\host-win32\\ppu\\bin\\ppu-lv2-addr2line.exe";
// 
//             string pathToAddr2line = LoadFromRegistry("addr2linepath", defaultLocationOfAddr2line);
//             int item = -1;
//             for (int i = 0, e = mCollapsedView.Count; i < e; i++)
//             {
//                 if (mDataGrid.IsSelected(i))
//                 {
//                     item = i;
//                     break;
//                 }
//             }
// 
//             if (item == -1)
//             {
//                 item = mDataGrid.CurrentCell.RowNumber;
//             }
// 
//             if (item != -1)
//             {
//                 int colIndex = 0;
//                 int indexval = (int)mDataGrid[item, colIndex];
// /*
//                 CallStack opt = new CallStack();
//                 opt.SetMapPath(map);
//                 opt.SetAddr2linePath(pathToAddr2line);
//                 opt.SetLog(mLog);
//                 opt.SetEntryToLookUp(mLog.GetString(mLog[indexval].stackTraceString));
//                 opt.ShowDialog();
// 
//                 map = opt.GetMapPath();
//                 pathToAddr2line = opt.GetAddr2linePath();
//                 SaveToRegistry("addr2linepath", pathToAddr2line);
// */
//             }
//         }

        //-------------------------------------------------------------------
        private void mMenuItemEndOfLog_Click(object sender, System.EventArgs e)
        {
            mLogSnap = new MemManager.Log.SnapShot(mLog, mLog.Count, false);
            PopulateFromSnapshot(mLog, mLogSnap, "", 0, false);
            if (mMemoryGraph != null)
            {
                mMemoryGraph.HilightLogEntry(mLog.Count - 1);
            }
            if (mGraphicalView != null)
            {
                mGraphicalView.NewSnapShot(mLogSnap);
            }
        }

        //-------------------------------------------------------------------
        private void PopulateGraphicalView()
        {
            if (mMenuItemGV.IsParent)
                mMenuItemGV.MenuItems.Clear();

			// enable menu item and add child items
            int numCat = mLog.GetNumberCategories();
            mMenuItemGV.Enabled = true;
			MenuItem item;
			for (int i = 0; i < numCat; i++)
			{
                string cat = (string)mLog.GetCategory((byte)i);
				item = new MenuItem( cat );
				item.Click += new EventHandler(this.GVClicked);
				mMenuItemGV.MenuItems.Add(item);
			}
		}

		//------------------------------------------------------------------
		private void GVClicked(object sender, System.EventArgs e)
		{
			MenuItem mi = (MenuItem) sender;
            checkSelectedItem(mi);
            string displayMode = LoadFromRegistry("displayMode", "category");

            GVstruct infoToGraph = new GVstruct();
            bool resizeGV = false;
            int[] dim = new int[4];
            if (mGraphicalView == null)
            {
                mGraphicalView = new GraphicalView(this);
                //resizeGV = true;                
               // mGraphicalView.Close();
               // mGraphicalView = null;
            }

            int catIndex = 0;
			if (mi != null)
			{
				string item = mi.ToString();
                catIndex = mLog.GetIndexForCategory(mi.Text);
                if (displayMode == "heap")
                    catIndex = 0;  // 0 means no category, or i.e. all categories
                mLogSnap = new MemManager.Log.SnapShot(mLog, mLog.FindHWIndexForCategory(catIndex), true);
                infoToGraph.PopulateGVStructFromSnapshot(mLog, mLogSnap, catIndex);
			}
            mGraphicalView.CopyGvInfo(infoToGraph, mLog, mLogSnap, mAllocators, catIndex);
            if (resizeGV)
                mGraphicalView.SetWindowSize(dim[0], dim[1]);
            
            mGraphicalView.Show();

            //the following goes after show to work correctly
            if (resizeGV)
                mGraphicalView.SetWindowLocation(new Point(dim[2], dim[3]));
        }

        private void mCategoryView_Click(object sender, EventArgs e)
        {
            if (mCategoryMode)
                return;
            checkSelectedItem((MenuItem)sender);
            SaveToRegistry("displayMode", "category");
            mCategoryMode = true;
            mLog.SwapToCategory();
            mLogSnap = new MemManager.Log.SnapShot(mLog, mLog.Count, false);
            CreateInitialView();
        }

        private void mHeapView_Click(object sender, EventArgs e)
        {
            if (!mCategoryMode)
                return;
            checkSelectedItem((MenuItem)sender);
            SaveToRegistry("displayMode", "heap");
            mCategoryMode = false;
            mLog.SwapToHeap();
            mLogSnap = new MemManager.Log.SnapShot(mLog, mLog.Count, false);
            CreateInitialView();
        }

        private void checkSelectedItem(MenuItem menuitem)
        {
            foreach (MenuItem item in menuitem.Parent.MenuItems)
            {
                if (item.Text.Equals(menuitem.Text))
                    item.Checked = true;
                else
                    item.Checked = false;
            }
        }

		private void mMenuItemMG_Click(object sender, System.EventArgs e)
		{
			if (mMemoryGraph == null)
			{
				mMemoryGraph = new MemoryGraph(this, mLog);
			}
			mMemoryGraph.Show();
		}		

        // this is a callback for memory graph for selecting categories. So snapshot is not changed. Just
        // categories selected are passed on to graphical view window.
        public void UpdateCategories(bool[] EnabledCategories)
        {
            if(mGraphicalView != null)
                mGraphicalView.UpdateCategories(EnabledCategories);
        }

		public void UpdateSnapShot(int index)
		{
			if (index >= mLog.Count)
			{
				index = mLog.Count - 1;
			}
            mLogSnap = new MemManager.Log.SnapShot(mLog, index, false);
            PopulateFromSnapshot(mLog, mLogSnap, mFilter, 0, false);

			if (mGraphicalView != null)
			{
				mGraphicalView.NewSnapShot(mLogSnap);
			}
		}

        public void CompareSnapShot(int index)
        {
            if (index >= mLog.Count)
            {
                index = mLog.Count - 1;
            }
            MemManager.Log.SnapShot diffLogSnap = new MemManager.Log.SnapShot(mLog, index, mLogSnap, false);
            PopulateFromSnapshot(mLog, diffLogSnap, "", 0, false);
            if (mGraphicalView != null)
            {
                mGraphicalView.NewSnapShot(diffLogSnap);
            }
        }

		public void CloseMemoryGraph()
		{
			mMemoryGraph = null;
		}

		public void CloseGraphicalView()
		{
			mGraphicalView = null;
		}

		public void SelectSnapShot(int index)
		{
            mLogSnap = new MemManager.Log.SnapShot(mLog, index, false);
			PopulateFromSnapshot(mLog, mLogSnap, "", 0, false);
			if (mMemoryGraph != null)
			{
				mMemoryGraph.HilightLogEntry(index);
			}
		}
	}


	//-----------------------------------------------------------------------------
    // holds a list of indices of the log entries in the current snapshot
	public class GVstruct
	{	
		public ArrayList mMemoryBlockIndex;
		
		//-------------------------------------------------------------------------
		public GVstruct()
		{
			mMemoryBlockIndex = new ArrayList(1024);
		}

		//--------------------------------------------------------------------------
		//  This functions populates the snap shot of the current allocations
		public void PopulateGVStructFromSnapshot (MemManager.Log.Log log, MemManager.Log.SnapShot snap, int catfilter)
		{
            mMemoryBlockIndex.Clear();
            for (int i = 0, e = snap.Count; i < e; i++)
            {
                int logindex = snap[i];
                MemManager.Log.LogEntry le = log[logindex];
                if (le.category == catfilter || catfilter == 0)
                {
                    mMemoryBlockIndex.Add(snap[i]);
                }
            }
            mMemoryBlockIndex.Sort();
        }

    };
}
                                                                                          