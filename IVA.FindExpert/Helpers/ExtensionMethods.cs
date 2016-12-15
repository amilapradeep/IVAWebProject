﻿using IVA.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace IVA.FindExpert.Helpers
{
    public static class ExtensionMethods
    {
        public static DateTime GetAdjustedTime(this DateTime Time)
        {
            double offset = Convert.ToDouble(ConfigurationManager.AppSettings[Constant.ConfigurationKeys.UTC_Offset].ToString());
            DateTime adjustedTime = Time.AddHours(offset);
            return adjustedTime;
        }
    }
}