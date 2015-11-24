using System;
using System.IO;
using System.Collections;

namespace MemManager
{
	namespace Log
	{

		/// <summary>
		/// Log file entry
		/// </summary>
		public class LogEntry
		{                                   //bits
			public char type;               //8
//			public bool temporary;          //8

			public byte category;           //8
            public byte allocator;          //8

			public uint address;            //32
		//	public uint reqSize;            //32
			public uint allocSize;          //32
		//	public uint alignment;          //32
			public int nameString;          //32

		//	public int stackTraceString;    //32
			public uint index;            //32
            public LogEntry() { type = 'G'; category = (byte)1; allocator = (byte)0; index = address = allocSize = 0; /*alignment = 16;*/ nameString = -1; }
		};

		/// <summary>
		/// Log file container
		/// </summary>
		/// 
		
		public class LogEntryAllocators
		{
			public string mName;
			public uint mStartAddress;
			public uint mEndAddress;
			
			public LogEntryAllocators()
			{
				mName = string.Empty;
				mStartAddress = 0;
				mEndAddress = 0;
			}
		};

		public class Log
		{
			private string BinaryHeaderVersionString = "LogBinary Format 1.1";
			private string mFormat;
            private int prevAllocator = -1;
            public bool mLogFixed = false;

			public Log()
			{
				mFormat = "";
				categoryHash[0] = 0;
				categories.Add( "N/A" );
                backupCategories.Add("N/A");
			}

			public Log(string format)
			{
				mFormat = format;
				categoryHash[0] = 0;
				categories.Add( "N/A" );
                backupCategories.Add("N/A");
			}

			// Binary save
			public void Save(string filename)
			{
				FileStream binFile = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
				//			ICSharpCode.SharpZipLib.GZip.GZipOutputStream gz = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(binFile);
				//			BinaryWriter writer = new BinaryWriter(gz);
				BinaryWriter writer = new BinaryWriter(binFile);

				writer.Write(BinaryHeaderVersionString);
				writer.Write(mFormat);

				// Write out string table
				writer.Write( stringTable.Count );
				for (int i = 0, e = stringTable.Count; i < e; i++)
				{
					writer.Write( stringTable[i] );
				}

				// Write out categories
				writer.Write( categories.Count );
				foreach (string i in categories)
				{
					writer.Write( i );
				}

				// Write out heap ranges.
				writer.Write( mAllocators.Count );
				foreach (LogEntryAllocators e in mAllocators)
				{
					writer.Write(e.mName);
					writer.Write(e.mStartAddress);
					writer.Write(e.mEndAddress);
				}

				//Write out DataFields.
				writer.Write(field.Count);
				foreach (string e in field)
				{
					writer.Write(e);
				}

				writer.Write( log.Count );
				for (int i = 0, e = log.Count; i < e; i++)
				{
					LogEntry logentry = (LogEntry)(log[i]);
					writer.Write( logentry.type );
					writer.Write( logentry.category );
                    writer.Write( logentry.allocator );
					writer.Write( logentry.address );
				//	writer.Write( logentry.reqSize );
					writer.Write( logentry.allocSize );
				//	writer.Write( logentry.alignment );
					writer.Write( logentry.nameString );
				//	writer.Write( logentry.stackTraceString );
					writer.Write( logentry.index );
				}

				writer.Close();
				binFile.Close();
			}

            public string ReadBinString(ref BinaryReader reader, int size)
            {
                char[] varc = reader.ReadChars(size); 
                return new string(varc);
            }

			// Binary load
			public bool Load(string filename)
			{
				FileStream binFile = new FileStream(filename, FileMode.Open, FileAccess.Read);
				BinaryReader reader = new BinaryReader(binFile);

                int size = reader.ReadByte(); 
                string ver = ReadBinString(ref reader, size);
				if (ver != BinaryHeaderVersionString)
					return false;	// bail ;

                size = reader.ReadByte(); 
                mFormat = ReadBinString(ref reader, size); 
                if (mFormat != "MetricsMemoryLog")
                    return false;
				
//                 // Read in string table.
// 				stringTable.Clear();
// 				int numberStrings = reader.ReadInt32();
// 				for (int i = 0; i < numberStrings; i++)
// 				{
// 					stringTable.AddUnique( reader.ReadString() );
// 				}
                
				// Read in categories
				categories.Clear();
                categories.Add("N/A");
                backupCategories.Clear();
                backupCategories.Add("N/A");

				cachedCategoryIndex = 0;

                // Read if there are callstacks
                byte callstackCode = reader.ReadByte();
                bool haveCallstack = callstackCode == 8;

                // Read total budget
                uint totalBudget = reader.ReadUInt32();

				// Read in log entries
				log.Clear();
                UInt32 addrLow =  uint.MaxValue;
                UInt32 addrHigh = uint.MinValue;
                UInt32 logEntries = reader.ReadUInt32();
              //  if (logEntries == 0)
                    logEntries = uint.MaxValue;
                UInt32 lastFrameIdx = uint.MaxValue;
                
                uint currCount = 0;
                for (UInt32 i = 0;; i++)
				{
                    LogEntry logentry = new LogEntry();
                    try
                    {
                        logentry.type = reader.ReadChar();

                        //   logentry.category = (Byte)(1); 
                        // reading allocation address and size
                        if (logentry.type == 'A' || logentry.type == 'F')
                        {
                            logentry.address = reader.ReadUInt32();
                            logentry.allocSize = reader.ReadUInt32();
                            addrLow = Math.Min(addrLow, logentry.address);
                            addrHigh = Math.Max(addrHigh, logentry.address + logentry.allocSize);
                        }

                        // reading tag
                        if (logentry.type == 'A' || logentry.type == 'L')
                        {
                            int tagsize = reader.ReadChar();
                            if (tagsize > 0)
                            {
                                string tag = ReadBinString(ref reader, tagsize);
                                //   System.Diagnostics.Debug.Assert(false);
                                logentry.nameString = AddString(tag.Trim());
                            }
                            else
                                logentry.nameString = AddString("NoTag");
                        }

                        // reading category
                        if (logentry.type == 'A')
                        {
                            //      sbyte catIndex = (sbyte)reader.ReadByte();
                            //                         if(catIndex >= 0)
                            //                         { // so category is already in table
                            //                             logentry.category = (byte)catIndex;
                            //                         }
                            //                         else
                            //                       { // this is a new category. Add it to the table
                            int catSize = reader.ReadChar();
                            string cat = ReadBinString(ref reader, catSize);
                            logentry.category = AddCategory(cat.Trim());
                            //                      }
                        }

                        // reading frame swap. This serves as time counter
                        if (logentry.type == 'S')
                        {
                            if (i != lastFrameIdx + 1)
                            {
                                logentry.nameString = AddString("FRAME");
                                lastFrameIdx = i;
                            }
                            else
                            {
                                lastFrameIdx = i;
                                continue;
                            }
                        }

                        if (logentry.type == 'F')
                            logentry.nameString = AddString("FREE");

                        if (logentry.type != 'A' && logentry.type != 'F' && logentry.type != 'L' && logentry.type != 'S')
                            System.Diagnostics.Debug.Assert(false, "Wrong Entry Type!");

                        /*  logentry.stackTraceString = -1;
                          if ((logentry.type == 'A' || logentry.type == 'F') && haveCallstack)
                          {
                              uint stackdepth = reader.ReadByte(); 
                              string stacktrace = "";
                              for (int j = 0; j < stackdepth; j++)
                              {
                                  uint address = reader.ReadUInt32();
                                  string addstr = System.Convert.ToString(address);
                                  stacktrace += addstr;
                                  stacktrace += ",";
                              }
                              logentry.stackTraceString = AddString(stacktrace);
                          } */
                    }
                    catch(System.IO.EndOfStreamException)
                    {
                        logEntries = currCount;
                        System.Diagnostics.Debug.Print("End of File reached\n");
                        break;
                    }

                    logentry.index = currCount++;  
					log.Add(logentry);
				}
				reader.Close();
				binFile.Close();

                // add a default allocator
                LogEntryAllocators entry = new LogEntryAllocators();
                entry.mName = "DefaultAlloc";
                entry.mStartAddress = addrLow;
                entry.mEndAddress = addrHigh; // +totalBudget;
                AddAllocatorEntry(entry);

				return true;
			}

            //swap category info for heap allocator info
            public void SwapToHeap()
            {
                if (categories.Count == 2 && mAllocators.Count == 1)
                    return;
                categories.Clear();
                categories.Add("N/A");
                foreach(LogEntryAllocators allocator in mAllocators)
                {
                    categories.Add(allocator.mName);
                }

                foreach (LogEntry entry in log)
                {
                    byte temp = entry.category;
                    entry.category = (byte)((int)entry.allocator);
                    entry.allocator = temp;
                }
            }

            //swap heap info for category info
            public void SwapToCategory()
            {
                categories.Clear();
                categories.Add("N/A");
                foreach (string category in backupCategories)
                {
                    categories.Add(category);
                }

                foreach (LogEntry entry in log)
                {
                    byte temp = entry.allocator;
                    entry.allocator = (byte)((int)entry.category);
                    entry.category = temp;
                 }
            }

            //find the index for the heap in which the memory is located
            public byte GetAllocatorIndex(uint StartAddress, uint EndAddress)
            {
                ArrayList allocators = GetAllocatorList();
                if (allocators.Count == 1)
                    return 0;

                int allocIndex = 0;
                if (prevAllocator != -1)
                {
                    LogEntryAllocators alloc = (LogEntryAllocators)allocators[prevAllocator];
                    if (StartAddress >= alloc.mStartAddress && EndAddress <= alloc.mEndAddress)
                    {
                        allocIndex = prevAllocator;
                        return (byte)allocIndex;
                    }
                }
                for (int index = 0; index < allocators.Count; index++)
                {
                    LogEntryAllocators alloc = (LogEntryAllocators)allocators[index];
                    if (StartAddress >= alloc.mStartAddress && EndAddress <= alloc.mEndAddress)
                    {
                        allocIndex = index;
                        prevAllocator = index;
                        break;
                    }
                }
                return (byte)allocIndex;
            }

			public string GetFormat()
			{
				return mFormat;
			}

			// Add/Get category index from passed string, this will terminate at either
			// the end of the string or the first ":" found.
			//
			// If it does not exist as a category a new one is added.
			// You can have upto 255 categories.
			public byte AddCategory(string s)
			{
				uint hash = 0;
				int len = 0;
				HashCategory(s, ref hash, ref len);

				// Check cached hash
				if (categoryHash[ cachedCategoryIndex ] == hash)
				{
					return cachedCategoryIndex;
				}

				// Search existing categories
				for (int i = 0; i < categories.Count; i++)
				{
					if (categoryHash[i] == hash)
					{
						cachedCategoryIndex = (byte)i;
                        return cachedCategoryIndex;
					}
				}

				if (categories.Count >= 128)
					throw new Exception("You can only have upto a max of 256 categories");

				// New entry..
				string catname = s.Substring(0, len);
				categoryHash[ categories.Count ] = hash; 
				cachedCategoryIndex = (byte)categories.Count;
				categories.Add( catname );
                backupCategories.Add(catname);
				return cachedCategoryIndex;
			}

			// Property Count
			public int Count
			{
				get { return log.Count; }
			}

			// Get number of categories
			public int GetNumberCategories()
			{
                return categories.Count;
			}

			// Get category string
			public string GetCategory(byte index)
			{
				return (string)categories[index];
			}

            public ArrayList GetCategories()
            {
                return categories;
            }

            public string GetAllocator(byte index)
            {
                return (string)((LogEntryAllocators)mAllocators[index]).mName;
            }

			//Get index in of the input category
			public int GetIndexForCategory(string category)
			{
				for(int i =0; i <categories.Count; i++)
				{
					if (category == (string)categories[i])
                        return i;
				}
				return 0;
			}

			// Get string
			public string GetString(int index)
			{
                if (index < 0)
                    return "";
				return stringTable[index];
			}

			// Add a string to the string table
			public int AddString(string s)
			{
				return stringTable.Add(s);
			}

			// Add a log entry
			public void Add(LogEntry l)
			{
				log.Add(l);
			}

			// Get a log entry
			public LogEntry this[int index]
			{
				get 
				{
					return (LogEntry)log[index];
				}
			}

			// Get list of labels, caching them after the first call.
			public ArrayList GetLabels()
			{
				if (labels.Count == 0)
				{
					// Assume not done before. so Scan log
					for (int i = 0; i < log.Count; i++)
					{
						LogEntry le = (LogEntry)log[i];
						if (le.type == 'L')
						{
							le.index = (uint)i;
							labels.Add(le);
						}
					}
				}
				return labels;
			}
			
			// Find the index of the HW for category 'index'
			public int FindHWIndexForCategory(int index)
			{
				byte cat = (byte)index;
				uint amount = 0;
				uint amountHW = 0;
				int amountHWIndex = log.Count;

				for (int i = 0; i < log.Count; i++)
				{
					LogEntry le = (LogEntry)log[i];
					if (cat == 0 || le.category == cat)
					{
						if (le.type == 'A')
						{
							amount += le.allocSize;
							if (amount > amountHW)
							{
								amountHWIndex = i;
								amountHW = amount;
							}
						}
						else if (le.type == 'F')
						{
							amount = amount >= le.allocSize ? amount - le.allocSize : 0;
						}
					}
				}
				return amountHWIndex + 1;
			}
			
			//All the fields that are going to be parsed in the file
			public void AddFieldName(string name)
			{
				field.Add(name);
				return;
			}

			public bool FindFieldName(string name)
			{
				bool result = false;
				result = field.Contains(name);
				return result;
			}

			// Hash category
			private void HashCategory(string s, ref uint hash_, ref int len_)
			{
				uint hash = 5381;
				uint c;
				int i, e;

				for (i = 0, e = s.Length; i < e; i++)
				{
					c = s[i];
					if (c == ':')
					{
						break;
					}
					hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
				}
				hash_ = hash;
				len_ = i;
			}

            public void RemoveDeletes(ArrayList delIndices)
            {
                int delCount = delIndices.Count-1;
                for (int i = log.Count - 1; i >= 0 && delCount >= 0; i--)
                {
                    LogEntry logentry = (LogEntry)log[i];
                    if (logentry.index == (uint)delIndices[delCount])
                    {
                        log.RemoveAt(i);
                        delCount--;
                    }
                }   
            }

			public void AddAllocatorEntry(LogEntryAllocators entry)
			{
				mAllocators.Add(entry);
			}

			public ArrayList GetAllocatorList()
			{
				return mAllocators;
			}

			private byte cachedCategoryIndex = 0;
			private uint[] categoryHash = new uint [ 256 ];
			private ArrayList categories = new ArrayList();
            private ArrayList backupCategories = new ArrayList();
			private ArrayList labels = new ArrayList();
			private Utility.StringTable stringTable = new Utility.StringTable();
			private ArrayList log = new ArrayList(8192);
			private ArrayList field = new ArrayList(); 
			private ArrayList mAllocators = new ArrayList();
		};
		
	}
}
