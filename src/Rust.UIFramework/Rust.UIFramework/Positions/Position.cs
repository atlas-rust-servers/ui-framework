﻿using System.Text;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Pooling;

namespace Oxide.Ext.UiFramework.Positions
{
    public struct Position
    {
        public readonly string Min;
        public readonly string Max;
        public readonly bool IsDefaultMin;
        public readonly bool IsDefaultMax;
        
        private const string PosFormat = "0.####";
        private const char Space = ' ';

        public Position(float xMin, float yMin, float xMax, float yMax)
        {
            Min = null;
            if (xMin == 0 && yMin == 0)
            {
                IsDefaultMin = true;
            }
            else
            {
                IsDefaultMin = false;
                Min = Build(xMin, yMin);
            }
            
            Max = null;
            if (xMax == 0 && yMax == 0)
            {
                IsDefaultMax = true;
            }
            else
            {
                IsDefaultMax = false;
                Max = Build(xMax, yMax);
            }
        }

        private static string Build(float min, float max)
        {
            StringBuilder sb = UiFrameworkPool.GetStringBuilder();
            sb.Append(min.ToString(PosFormat));
            sb.Append(Space);
            sb.Append(max.ToString(PosFormat));
            string result = sb.ToString();
            UiFrameworkPool.FreeStringBuilder(ref sb);
            return result;
        }

        public override string ToString()
        {
            return string.Concat(Min, " ", Max);
        }
    }
}