using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using Opkee;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opkee.Data
{
    public class DataManager
    {
        string _productsFolder = "";

        Dictionary<string, Dictionary<string, ProductInfo>> _productsByCodeByVendor = new Dictionary<string, Dictionary<string, ProductInfo>>();
        Dictionary<string, Dictionary<string, ProductContent>> _productContentByProductByVendor = new Dictionary<string, Dictionary<string, ProductContent>>();

        public DataManager(string productsFolder)
        {
            _productsFolder = productsFolder;
            LoadProducts();
        }

        void LoadProducts()
        {
            string[] subDirectories = Directory.GetDirectories(_productsFolder);

            string startFilter = (_productsFolder + Path.DirectorySeparatorChar + "0x").ToLower();
            JsonSerializerOptions options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

            foreach (string subdirectory in subDirectories)
            {
                if (subdirectory.ToLower().StartsWith(startFilter))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(subdirectory);
                    string vendorAddress = directoryInfo.Name.ToUpper();
                    Dictionary<string, ProductInfo> productsByCode = _productsByCodeByVendor.ContainsKey(vendorAddress) ? _productsByCodeByVendor[vendorAddress] : new Dictionary<string, ProductInfo>();
                    Dictionary<string, ProductContent> productDataByProduct = _productContentByProductByVendor.ContainsKey(vendorAddress) ? _productContentByProductByVendor[vendorAddress] : new Dictionary<string, ProductContent>();

                    foreach (string file in Directory.GetFiles(subdirectory, "*.json", SearchOption.AllDirectories))
                    {
                        try
                        {
                            string json = File.ReadAllText(file);

                            ProductInfo product = JsonSerializer.Deserialize<ProductInfo>(json, options);
                            ProductContent productData = null;

                            FileInfo fileInfo = new FileInfo(file);
                            string datafile = file.Replace(".json", ".data");
                            if (File.Exists(datafile))
                            {                                
                                if (product.IsText)
                                {
                                    string html = File.ReadAllText(datafile);
                                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(html);
                                    productData = new ProductContent(product.Code, product.ContentType, System.Convert.ToBase64String(plainTextBytes));
                                }
                                else
                                {
                                    byte[] bytes = File.ReadAllBytes(datafile);
                                    productData = new ProductContent(product.Code, product.ContentType, System.Convert.ToBase64String(bytes));
                                }
                            }
                            else
                            {
                                productData = new ProductContent(product.Code, product.ContentType, "");
                            }

                            if (product != null)
                            {
                                productsByCode.Add(product.Code.ToUpper(), product);

                                if (productData != null)
                                {
                                    productDataByProduct.Add(product.Code.ToUpper(), productData);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    _productsByCodeByVendor[vendorAddress] = productsByCode;
                    _productContentByProductByVendor[vendorAddress] = productDataByProduct;
                }
            }
        }

        public List<ProductInfo> GetProducts(string vendorAddress)
        {
            if (vendorAddress != null)
            {
                if (_productsByCodeByVendor.ContainsKey(vendorAddress.ToUpper()))
                {
                    Dictionary<string, ProductInfo> productsByCode = _productsByCodeByVendor[vendorAddress.ToUpper()];
                    return productsByCode.Values.ToList();
                }
            }

            return new List<ProductInfo>();
        }

        public ProductInfo GetProduct(string vendorAddress, string productCode)
        {
            if ((vendorAddress != null) && (productCode != null))
            {
                if (_productsByCodeByVendor.ContainsKey(vendorAddress.ToUpper()))
                {
                    Dictionary<string, ProductInfo> productsByCode = _productsByCodeByVendor[vendorAddress.ToUpper()];

                    if (productsByCode.ContainsKey(productCode.ToUpper()))
                    {
                        return productsByCode[productCode.ToUpper()];
                    }
                }
            }

            return null;
        }

        public ProductContent GetProductContent(string vendorAddress, string productCode)
        {
            if ((vendorAddress != null) && (productCode != null))
            {
                if (_productContentByProductByVendor.ContainsKey(vendorAddress.ToUpper()))
                {
                    Dictionary<string, ProductContent> _productDataByProduct = _productContentByProductByVendor[vendorAddress.ToUpper()];

                    if ((_productDataByProduct != null) & (_productDataByProduct.ContainsKey(productCode.ToUpper())))
                    {
                        return _productDataByProduct[productCode.ToUpper()];
                    }
                }
            }

            return null;
        }
    }
}