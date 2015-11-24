using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace MemManager
{
	namespace Log
	{
		public class SnapShot
		{
			static System.Threading.Mutex mAllocListsMutex = new System.Threading.Mutex();
			static Utility.AllocLists mAllocLists;
			static int mAllocListsLastIndex;

			ArrayList mArray;		
			int mIndex;
            public int mMaxLogIndex;
			bool mSorted = false;

            public SnapShot(Log log, SnapShot snap, string filter, int size, bool dir)
            {
                Utility.AllocLists li = new Utility.AllocLists();
                Regex r = new Regex(filter, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                for (int i = 0, e = snap.Count; i < e; i++)
                {
                    int logindex = snap[i];
                    MemManager.Log.LogEntry le = log[logindex];
                    
                    string name = log.GetString(le.nameString);
                    if ( r.IsMatch(log.GetString(le.nameString)) )
                    {
                        if(size == 0)
                            li.Allocate(log[logindex].address, logindex);
                        else if(dir==false && le.allocSize >= size)
                            li.Allocate(log[logindex].address, logindex);
                        else if(dir==true && le.allocSize <= size)
                            li.Allocate(log[logindex].address, logindex);
                    }
                }

				mArray = li.GetArray(ref mMaxLogIndex);
                mArray.Sort();
            }

            // fixes up unassigned field for each "Free" log entry.
            public void FixupLog(Log log)
            {
                Utility.AllocLists li = new Utility.AllocLists();
                int index = log.Count;
                try
                {
                    for (int i = 0; i < index; i++)
                    {
                        LogEntry logentry = log[i];
                        if (logentry.type == 'A')
                        {
                            if (i != logentry.index)
                                System.Diagnostics.Debug.Print("indices don't match: {0} != {1}\n", i, logentry.index);
                            li.Allocate(logentry.address, (int)logentry.index);
                        }
                        else if (logentry.type == 'F')
                        {
                            int idx = li.Free(logentry.address);
                            if (idx > 0)
                            {
                                //  ((LogEntry)log[idx]).allocSize = logentry.allocSize;
                                logentry.category = ((LogEntry)log[idx]).category;
                                logentry.allocator = ((LogEntry)log[idx]).allocator;
                              //  System.Diagnostics.Debug.Assert(((LogEntry)log[idx]).allocSize == logentry.allocSize, "ERROR! Allocated and deleted sizes don't match");
                            }
                            else
                            {
                                // System.Diagnostics.Debug.Print("Missing alloc! For now 'disable' this Free by setting its size and address to 0");
                                ((LogEntry)log[i]).allocSize = ((LogEntry)log[i]).address = 0;
                                //   removeFrees.Add(logentry.index);
                            }
                        }
                    }

                    log.mLogFixed = true;
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    System.Diagnostics.Debug.Print("!!System.ArgumentOutOfRangeException");
                }
            }

			public SnapShot(Log log, int index, bool addressSorted)
			{
                if (!log.mLogFixed)
                    FixupLog(log);

				mIndex = index;
				Utility.AllocLists li = new Utility.AllocLists();
				// Play through allocations till index reached.
                for (int i = 0; i < index; i++)
                {
                    LogEntry logentry = log[i];
                    if (logentry.type == 'A')
                    {
                        li.Allocate(logentry.address, (int)logentry.index);
                    }
                    else if (logentry.type == 'F')
                    {
                        int idx = li.Free(logentry.address);
                    }
                }
                
				// Now store as an array
                if (addressSorted)
                    mArray = li.GetArrayAddressSorted(ref mMaxLogIndex);
                else
                {
                    mArray = li.GetArray(ref mMaxLogIndex);
                    mArray.Sort(new CompareIndices());
                }

				mSorted = false;
				mAllocListsMutex.WaitOne();
				mAllocListsLastIndex = index;
				mAllocLists = li;
				mAllocListsMutex.ReleaseMutex();
			}

			public SnapShot(Log log, int index, SnapShot prev, bool diff)
			{
                if (prev.mIndex > index)
                {
                    System.Diagnostics.Debug.Print("Second selection must be on the right hand side of the first one to compare");
                    //throw new Exception("Snapshot passed needs to be behind current one");
                    return;
                }

				mIndex = index;
				Utility.AllocLists li;
                if (!diff)
                {
                    mAllocListsMutex.WaitOne();
                    if (mAllocListsLastIndex == prev.mIndex)
                    {
                        li = mAllocLists;
                        mAllocListsMutex.ReleaseMutex();
                    }
                    else
                    {
                        mAllocListsMutex.ReleaseMutex();

                        // Populate snapshot with previous snapshot
                        li = new Utility.AllocLists();
                        for (int i = 0; i < prev.mArray.Count; i++)
                        {
                            int idx = (int)(prev.mArray[i]);
                            uint address = log[idx].address;
                            li.Allocate(address, idx);
                        }
                    }
                }
                else
                    li = new MemManager.Utility.AllocLists();

				// Play through allocations till index reached.
				for (int i = prev.mIndex; i < index; i++)
				{
					LogEntry logentry = log[i];
					if (logentry.type == 'A')
						li.Allocate(logentry.address, i);
					else if (logentry.type == 'F')
						li.Free(logentry.address);
				}

				// Now store as an array
				mArray = li.GetArray(ref mMaxLogIndex);
				mSorted = false;

				mAllocListsMutex.WaitOne();
				mAllocListsLastIndex = index;
				mAllocLists = li;
				mAllocListsMutex.ReleaseMutex();
			}

            class CompareIndices : IComparer
            {
                int System.Collections.IComparer.Compare(object x, object y)
                {
                    int a = (int)x;
                    int b = (int)y;
                    return (a - b);
                }
            };

			public int this[int index]
			{
				get { return (int)mArray[index]; }
			}

			public int Count
			{
				get { return mArray.Count; }
			}
		}
	}
}

