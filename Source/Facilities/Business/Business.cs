﻿using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Business : Barracks
    {
        //[CFGSetting]
        //public int StaffMax;
        [CFGSetting]
        public float ProductionRateMax;

        //[CareerSetting]
        //public float StaffCurrent = 0;
        //[CareerSetting]
        //public float ProductionRateCurrent =0f;
        [CareerSetting]
        public float LastCheck = 0f;

        [CFGSetting]
        public float FundsOMax;
        [CareerSetting]
        public float FundsOCurrent;



    }
}
