// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace apa.BOL.EventHubs
{
    //--------------------------------------------------------------------------------------------------------------
    public class DredgerTelemetryData
    {
        public string DREDGE_ID { get; set; } // like an Id no., just simpler to use a Guid for our sample!
        public string DREDGE_NAME { get; set; }
        public DateTimeOffset DATE_TIME { get; set; }
        public int LOAD_NUMBER { get; set; }
        public double VESSEL_X { get; set; }
        public double VESSEL_Y { get; set; }
        public double PORT_DRAG_X { get; set; }
        public double PORT_DRAG_Y { get; set; }
        public double STBD_DRAG_X { get; set; }
        public double STBD_DRAG_Y { get; set; }
        public string HULL_STATUS { get; set; }
        public int VESSEL_COURSE { get; set; }
        public decimal VESSEL_SPEED { get; set; }
        public decimal VESSEL_HEADING { get; set; }
        public decimal TIDE { get; set; }
        public decimal DRAFT_FORE { get; set; }
        public decimal DRAFT_AFT { get; set; }
        public decimal ULLAGE_FORE { get; set; }
        public decimal ULLAGE_AFT { get; set; }
        public decimal HOPPER_VOLUME { get; set; }
        public decimal DISPLACEMENT { get; set; }
        public decimal EMPTY_DISPLACEMENT { get; set; }
        public decimal DRAGHEAD_DEPTH_PORT { get; set; }
        public decimal DRAGHEAD_DEPTH_STBD { get; set; }
        public decimal PORT_DENSITY { get; set; }
        public decimal STBD_DENSITY { get; set; }
        public decimal PORT_VELOCITY { get; set; }
        public decimal STBD_VELOCITY { get; set; }
        public decimal PUMP_RPM_PORT { get; set; }
        public decimal PUMP_RPM_STBD { get; set; }
        public bool MIN_PUMP_EFFORT_PORT { get; set; }
        public bool MIN_PUMP_EFFORT_STBD { get; set; }
        public bool PUMP_WATER_PORT { get; set; }
        public bool PUMP_WATER_STBD { get; set; }
        public bool PUMP_MATERIAL_PORT { get; set; }
        public bool PUMP_MATERIAL_STBD { get; set; }
        public bool PUMP_OUT_ON { get; set; }
        public int PIQ { get; set; }
    }
}