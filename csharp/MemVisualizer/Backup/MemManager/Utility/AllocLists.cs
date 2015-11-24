using System;
using System.Collections;

namespace MemManager
{
	namespace Utility
	{
		/// <summary>
		/// Fast allocation lookup table
		/// </summary>
		class AllocLists
		{
			static int kSize = 17389;
			static uint kDivider = 64;
			ArrayList activeAlloc = new ArrayList(kSize);
	
			struct AddressIndex
			{
				public uint address;
				public int index;
			};

			public AllocLists()
			{
				for (int i = 0; i < kSize; i++)
					activeAlloc.Add( new ArrayList(16) );
			}

			public void Allocate(uint address, int index)
			{
				uint hash = (address / kDivider) % (uint)(kSize);
				ArrayList lst = (ArrayList)(activeAlloc[(int)(hash)]);

				AddressIndex ai;
				ai.address = address;
				ai.index = index;

				lst.Add(ai);
			}

			public int Free(uint address)
			{
				uint hash = (address / kDivider) % (uint)(kSize);
				ArrayList lst = (ArrayList)(activeAlloc[(int)(hash)]);

				for (int j = 0; j < lst.Count; j++)
				{
					AddressIndex logentry = (AddressIndex)(lst[j]);
					if (logentry.address == address)
					{
						lst.RemoveAt(j);
						return logentry.index;
					}
				}
          //      System.Diagnostics.Debug.Print("!!Warning! Unfound addresss in snapshot: {0}\n", address);
//discard, as can be due to incomplete log				throw new Exception("Free of item that is not in allocation list!");
				return -1;
			}

			public ArrayList GetArray(ref int maxlogindex)
			{
				ArrayList l = new ArrayList(kSize);
                maxlogindex = -1;
				for (int i = 0; i < kSize; i++)
				{
					ArrayList lst = (ArrayList)(activeAlloc[i]);
					if (lst.Count > 0)
					{
						for (int j = 0; j < lst.Count; j++)
						{
							AddressIndex logentry = (AddressIndex)(lst[j]);
							l.Add( logentry.index );
                            maxlogindex = maxlogindex >= logentry.index ? maxlogindex : logentry.index;
						}
					}
				}
				l.TrimToSize();
				return l;
			}

            public ArrayList GetArrayAddressSorted(ref int maxlogindex)
            {
                ArrayList l = new ArrayList(kSize);
                maxlogindex = -1;
                for (int i = 0; i < kSize; i++)
                {
                    ArrayList lst = (ArrayList)(activeAlloc[i]);
                    if (lst.Count > 0)
                    {
                        for (int j = 0; j < lst.Count; j++)
                        {
                            AddressIndex logentry = (AddressIndex)(lst[j]);
                            l.Add(logentry);
                            maxlogindex = maxlogindex >= logentry.index ? maxlogindex : logentry.index;
                        }
                    }
                }

                l.TrimToSize();
                l.Sort(new CompareAddresses());

                ArrayList ll = new ArrayList(l.Count);
                for (int i = 0; i < l.Count; i++)
                {
                    AddressIndex logentry = (AddressIndex)(l[i]);
                    ll.Add(logentry.index);
                }

                return ll;

            }

            class CompareAddresses : IComparer
            {
                int System.Collections.IComparer.Compare(object x, object y)
                {
                    AddressIndex a = (AddressIndex)x;
                    AddressIndex b = (AddressIndex)y;
                    return ((int)b.address - (int)a.address);
                }
            };

        }
	}
}
