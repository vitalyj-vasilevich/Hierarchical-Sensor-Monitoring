﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HSMCommon.Keys;
using HSMServer.DataLayer;
using HSMServer.DataLayer.Model;
using NLog;
using Product = HSMServer.DataLayer.Model.Product;

namespace HSMServer.Products
{
    public class ProductManager
    {
        private readonly Logger _logger;
        private readonly List<Product> _products;
        private readonly Dictionary<string, List<string>> _productSensorsDictionary = new Dictionary<string, List<string>>();
        private readonly object _productsLock = new object();
        private readonly object _dictionaryLock = new object();
        public ProductManager()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _products = new List<Product>();
            //AddProduct("TEST");
            //DatabaseClass.Instance.ClearProductsList();
            InitializeProducts();
        }

        private void InitializeProducts()
        {
            List<string> productNames = DatabaseClass.Instance.GetProductsList();
            foreach (var productName in productNames)
            {
                var info = DatabaseClass.Instance.GetProductInfo(productName);
                var sensors = DatabaseClass.Instance.GetSensorsList(productName);
                if (info != null)
                {
                    lock (_productsLock)
                    {
                        _products.Add(info);
                    }

                    lock (_dictionaryLock)
                    {
                        _productSensorsDictionary[productName] = new List<string>();
                        _productSensorsDictionary[productName].AddRange(productNames);
                    }
                }
            }

            lock (_productsLock)
            {
                _logger.Info($"{_products.Count} products read, ProductManager initialized");
            }
            
        }

        public void RemoveProduct(string name)
        {
            try
            {
                DatabaseClass.Instance.RemoveProductFromList(name);
                DatabaseClass.Instance.RemoveProductInfo(name);
                var product = GetProductByName(name);
                if (product != null)
                {
                    lock (_productsLock)
                    {
                        _products.Remove(product);
                    }

                    lock (_dictionaryLock)
                    {
                        _productSensorsDictionary[product.Name]?.Clear();
                        _productSensorsDictionary.Remove(product.Name);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove product, name = {name}");
            }
        }
        public void AddProduct(string name)
        {
            string key = KeyGenerator.GenerateProductKey(name);
            _logger.Info($"Created product key = '{key}' for product = '{name}'");
            Product product = new Product {Key = key, Name = name, DateAdded = DateTime.Now};
            try
            {
                DatabaseClass.Instance.AddProductToList(name);
                DatabaseClass.Instance.PutProductInfo(product);
                lock (_productsLock)
                {
                    _products.Add(product);
                }

                lock (_dictionaryLock)
                {
                    _productSensorsDictionary[product.Name] = new List<string>();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to add new product, name = {name}");
            }
        }

        public bool IsSensorRegistered(string productName, string sensorName)
        {
            lock (_dictionaryLock)
            {
                if (!_productSensorsDictionary.ContainsKey(productName))
                    return false;

                return _productSensorsDictionary[productName].Contains(sensorName);
            }
        }

        public void AddSensor(SensorInfo sensorInfo)
        {
            lock (_dictionaryLock)
            {
                if (!_productSensorsDictionary.ContainsKey(sensorInfo.ProductName))
                {
                    _productSensorsDictionary[sensorInfo.ProductName] = new List<string>();
                }
                _productSensorsDictionary[sensorInfo.ProductName].Add(sensorInfo.SensorName);
            }

            ThreadPool.QueueUserWorkItem(_ => DatabaseClass.Instance.AddSensor(sensorInfo));
            ThreadPool.QueueUserWorkItem(_ =>
                DatabaseClass.Instance.AddNewSensorToList(sensorInfo.ProductName, sensorInfo.SensorName));
        }
        public string GetProductKeyByName(string name)
        {
            Product product = null;
            lock (_productsLock)
            {
                product = _products.FirstOrDefault(p => p.Name.Equals(name));
            }
            return product?.Key;
        }

        public string GetProductNameByKey(string key)
        {
            Product product = null;
            lock (_productsLock)
            {
                product = _products.FirstOrDefault(p => p.Key.Equals(key));
            }
            return product?.Name;
        }

        public Product GetProductByName(string name)
        {
            Product product = null;
            lock (_productsLock)
            {
                product = _products.FirstOrDefault(p => p.Name.Equals(name));
            }

            return product;
        }
    }
}
