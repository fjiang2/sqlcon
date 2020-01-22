using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys
{
    public class RangeSet : List<Range>
    {
        public RangeSet()
        {
        }

        public bool Overlapped(Range range)
        {
            foreach (var r in this)
            {
                if (r.Overlapped(range))
                    return true;
            }

            return false;
        }

        public bool Overlapped(RangeSet R)
        {
            foreach (var r in R)
            {
                if (this.Overlapped(r))
                    return true;
            }

            return false;
        }

        public Range CommonRange(Range range)
        {
            Range r = range;
            foreach (var x in this)
            {
                r = r * x;
            }

            return r;

        }

        public static RangeSet operator +(RangeSet R, double delta)
        {
            RangeSet s = new RangeSet();
            foreach (var r in R)
                s.Add(r + delta);

            return s;
        }


        public static RangeSet operator -(RangeSet R, double delta)
        {
            RangeSet s = new RangeSet();
            foreach (var r in R)
                s.Add(r - delta);

            return s;
        }



        public static RangeSet operator *(RangeSet R, Range range)
        {
            RangeSet list = new RangeSet();
            foreach (var r in R)
            {
                if (r.Overlapped(range))
                    list.Add(r * range);
            }

            return list;
        }

        public static RangeSet operator *(RangeSet R1, RangeSet R2)
        {
            RangeSet R = new RangeSet();
            foreach (var r2 in R2)
            {
                RangeSet S = R1 * r2;
                foreach (var s in S)
                    R.Add(s);
            }

            return R;
        }

        public static RangeSet operator *(RangeSet R1, IEnumerable<RangeSet> L)
        {
            RangeSet R = R1;
            foreach (var R2 in L)
            {
                R *= R2;
            }

            return R;
        }

        public double MinX1
        {
            get { return this.Min(row => row.X1); }
        }

        public double MaxX2
        {
            get { return this.Max(row => row.X2); }
        }

        public double Width
        {
            get { return this.Sum(row => row.Width); }
        }

        public void Merge()
        {
            int i = 0;
            while(i < this.Count -1)
            {
                Range r1 = this[i];
                Range r2 = this[i+1];

                if (r1.X2 == r2.X1)
                {
                    Range r = new Range(r1.X1, r2.X2);
                    int index = this.IndexOf(r1);
                    this.Remove(r1);
                    this.Remove(r2);
                    this.Insert(index, r);
                }
                else
                    i++;
            }
        }
    }
}
