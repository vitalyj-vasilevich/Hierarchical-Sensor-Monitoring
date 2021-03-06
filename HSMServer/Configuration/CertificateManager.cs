﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NLog;

namespace HSMServer.Configuration
{
    public class CertificateManager
    {
        private readonly Logger _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(10);
        private readonly List<CertificateDescriptor> _certificates = new List<CertificateDescriptor>();
        private readonly DateTime _lastUpdate = DateTime.MinValue;

        public CertificateManager()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Certificate manager initialized");
        }

        private IEnumerable<CertificateDescriptor> ReadUserCertificates()
        {
            string certFolderPath = Config.CertificatesFolderPath;

            if(!Directory.Exists(certFolderPath))
                yield break;

            string[] files = Directory.GetFiles(certFolderPath, "*.crt");
            foreach (var file in files)
            {
                X509Certificate2 cert = null;
                CertificateDescriptor descriptor = null;
                try
                {
                    cert = new X509Certificate2(file);
                    descriptor = new CertificateDescriptor {Certificate = cert, FileName = Path.GetFileName(file)};
                }
                catch
                {
                    continue;
                }

                yield return descriptor;
            }
        }

        private void UpdateCertificates()
        {
            if (DateTime.Now - _lastUpdate > _updateInterval)
            {
                _certificates.Clear();
                _certificates.AddRange(ReadUserCertificates());
            }
        }
        public List<CertificateDescriptor> GetUserCertificates()
        {
            UpdateCertificates();

            return _certificates;
        }

        public X509Certificate2 GetCertificateByFileName(string fileName)
        {
            UpdateCertificates();

            return _certificates.FirstOrDefault(d => d.FileName.Equals(fileName))?.Certificate;
        }

        private List<X509Certificate2> GetCertificatesFromStore()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            return store.Certificates.Cast<X509Certificate2>().ToList();
        }

        public void SaveClientCertificate(X509Certificate2 certificate, string fileName)
        {
            string certPath = Path.Combine(Config.CertificatesFolderPath, fileName);
            byte[] certBytes = certificate.Export(X509ContentType.Cert);

            FileStream fs = new FileStream(certPath, FileMode.CreateNew);
            fs.Write(certBytes, 0, certBytes.Length);
            fs.Flush();
            fs.Close();
        }

        public void InstallClientCertificate(X509Certificate2 certificate)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
        }

        public X509Certificate2 GetCrtCertificateFromPfx(X509Certificate2 pfxCert)
        {
            byte[] bytes = pfxCert.Export(X509ContentType.Cert, "");
            X509Certificate2 crtCert = new X509Certificate2(bytes);
            return crtCert;
        }
    }
}
