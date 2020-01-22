using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys
{
    public class Range
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        
        public Range()
        {
        }

        public Range(double x1, double x2)
        {
            this.X1 = x1;
            this.X2 = x2;
        }

        public Range(Range range)
        {
            this.X1 = range.X1;
            this.X2 = range.X2;
        }

        public double Width
        {
            get { return this.X2 - this.X1; }
        }

        public bool Overlapped(Range range)
        {
            return Overlapped(this, range);
        }

        private static bool Overlapped(Range r1, Range r2)
        {
            if (r1.X2 < r2.X1)
                return false;

            if (r1.X1 > r2.X2)
                return false;

            return true;
        }

        public static Range operator +(Range range, double delta)
        {
            Range r = new Range(range.X1, range.X2);
            r.X1 += delta;
            r.X2 += delta;
            return r;
        }

        public static Range operator -(Range range, double delta)
        {
            Range r = new Range(range.X1, range.X2);
            r.X1 -= delta;
            r.X2 -= delta;
            return r;
        }

        public static Range operator *(Range range1, Range range2)
        {

            Range range = new Range();

            range.X1 = Math.Max(range1.X1, range2.X1);
            range.X2 = Math.Min(range1.X2, range2.X2);

            return range;
        }


        public override string ToString()
        {
            return string.Format("({0}, {1})", this.X1, this.X2);
        }
    }
}
