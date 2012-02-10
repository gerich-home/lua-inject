//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Text;

namespace Designers
{
    /// <summary>
    /// Generated random color. It is used by MyRootDesigner
    /// </summary>
    public class RandomUtil
    {
        internal const int MaxRGBInt = 255;
        private static Random rand = null;

        public RandomUtil() 
        {
            if ( rand == null )
				InitializeRandoms(new Random().Next());
        }

		private void InitializeRandoms(int seed)
		{
			rand = new Random(seed);
		}

		public virtual Color GetColor()
		{
			byte rval, gval, bval;

			rval = (byte)GetRange(0, MaxRGBInt);
			gval = (byte)GetRange(0, MaxRGBInt);
			bval = (byte)GetRange(0, MaxRGBInt);

			Color c = Color.FromArgb(rval, gval, bval);

			return c;
		}
		public int GetRange(int nMin, int nMax)
		{
			// Swap max and min if min > max
			if (nMin > nMax)
			{
				int nTemp = nMin;

				nMin = nMax;
				nMax = nTemp;
			}

			if (nMax != Int32.MaxValue)
				++nMax;

			int retVal = rand.Next(nMin, nMax);

			return retVal;
		}
    }
}
