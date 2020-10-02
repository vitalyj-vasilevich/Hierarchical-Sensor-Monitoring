﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using HSMClient.Common;
using HSMClient.Configuration;
using HSMClient.Connections;
using HSMClient.Connections.gRPC;
using HSMClient.StatusHandlers;
using HSMClientWPFControls;
using HSMClientWPFControls.Objects;
using HSMClientWPFControls.UpdateObjects;
using SensorsService;

namespace HSMClient.ConnectionNode
{
    class SensorMonitoringNode : OneConnectionMonitoringNode
    {
        private string _sensorName;
        private string _machineName;
        public SensorMonitoringNode(string name, string address, SensorMonitoringInfo sensorInfo, MonitoringNodeBase parent = null) : base(sensorInfo.UpdatePeriod, name, address, parent)
        {
            Handler = new JobSensorsStatusHandler(sensorInfo);
            _sensorName = sensorInfo.Name;
            _machineName = sensorInfo.MachineName;
        }

        public override ConnectorBase InitializeClient()
        {
            return new SensorsClient(_address, _sensorName, _machineName);
        }
        public override MonitoringNodeUpdate ConvertResponse(object responseObj)
        {
            MonitoringNodeUpdate result = new MonitoringNodeUpdate();
            SensorResponse typedResponse = (SensorResponse) responseObj;
            MonitoringCounterUpdate update = new MonitoringCounterUpdate
            {
                DataObject =  typedResponse, 
                ShortValue = GetShortValue(typedResponse),
                CounterType = CounterTypes.JobSensor,
                Name = _sensorName
            };

            result.Counters = new List<MonitoringCounterUpdate> { update };
            result.Name = Parent.Name;
            result.SubNodes = new List<MonitoringNodeUpdate>();
            return result;
        }
        //public override MonitoringNodeUpdate ConvertResponse(string response)
        //{
        //    MonitoringNodeUpdate result = new MonitoringNodeUpdate();
        //    response = response.Replace("[", "").Replace("]", "");
        //    ShortSensorData data = JsonSerializer.Deserialize<ShortSensorData>(response);
        //    MonitoringCounterUpdate update = new MonitoringCounterUpdate
        //    {
        //        ShortValue =  GetShortValue(data),
        //        DataObject = data,
        //        CounterType = CounterTypes.JobSensor
        //    };
        //    update.Name = this.Name;

        //    result.Counters = new List<MonitoringCounterUpdate> {update};
        //    result.Name = Parent.Name;
        //    result.SubNodes = new List<MonitoringNodeUpdate>();
        //    return result;
        //}

        private string GetShortValue(SensorResponse data)
        {
            DateTime convertedTime = new DateTime(data.Ticks);
            if (string.IsNullOrEmpty(data.Comment))
            {
                return $"The task has been {ConvertStatus(data.Success)} at {convertedTime:s}";
            }
            return $"The task has been {ConvertStatus(data.Success)} at {convertedTime:s}   {data.Comment}";
        }

        private string ConvertStatus(bool status)
        {
            return status ? TextConstants.CompletedText : TextConstants.FailedText;
        }
    }
}
