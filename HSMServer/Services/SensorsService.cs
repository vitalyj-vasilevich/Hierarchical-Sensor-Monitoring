﻿using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HSMServer.MonitoringServerCore;
using SensorsService;
using NLog;

namespace HSMServer.Services
{
    public class SensorsService : Sensors.SensorsBase
    {
        private readonly Logger _logger;
        private readonly IMonitoringCore _monitoringCore;

        public SensorsService(MonitoringCore monitoringCore)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _monitoringCore = monitoringCore;

            _logger.Info("Sensors service started");
        }

        public override Task<SensorsUpdateMessage> GetMonitoringUpdates(Empty request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            return Task.FromResult(_monitoringCore.GetSensorUpdates(httpContext.Connection.ClientCertificate));
        }

        public override Task<SensorsUpdateMessage> GetMonitoringTree(Empty request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            return Task.FromResult(_monitoringCore.GetAllAvailableSensorsUpdates(httpContext.Connection.ClientCertificate));
        }

        public override Task<SensorsUpdateMessage> GetSensorHistory(GetSensorHistoryMessage request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            return Task.FromResult(_monitoringCore.GetSensorHistory(httpContext.Connection.ClientCertificate, request));
        }

        public override Task<ProductsListMessage> GetProductsList(Empty request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            return Task.FromResult(_monitoringCore.GetProductsList(httpContext.Connection.ClientCertificate));
        }

        public override Task<AddProductResultMessage> AddNewProduct(AddProductMessage request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            return Task.FromResult(_monitoringCore.AddNewProduct(httpContext.Connection.ClientCertificate, request));
        }

        public override Task<RemoveProductResultMessage> RemoveProduct(RemoveProductMessage request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            return Task.FromResult(_monitoringCore.RemoveProduct(httpContext.Connection.ClientCertificate, request));
        }

        public override Task<SignedCertificateMessage> SignClientCertificate(CertificateSignRequestMessage request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            return Task.FromResult(
                _monitoringCore.SignClientCertificate(httpContext.Connection.ClientCertificate, request));
        }

        public override Task<GenerateServerCertificateResulMessage> GenerateServerCertificate(CertificateRequestMessage request, ServerCallContext context)
        {
            return base.GenerateServerCertificate(request, context);
        }

        public override Task<ServerAvailableMessage> CheckServerAvailable(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ServerAvailableMessage() {Time = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())});
        }
    }
}
