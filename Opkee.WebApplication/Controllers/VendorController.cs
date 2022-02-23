using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Opkee;
using Opkee.Data;
using Opkee.Token;
using Microsoft.Extensions.Configuration.Json;

namespace Opkee.WebApplication.Controllers
{
    [EnableCors]

    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : OpkeeBaseController
    {
        private readonly ILogger<VendorController> _logger;

        public VendorController(ILogger<VendorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{vendorAddress}/Products")]
        public IEnumerable<ProductInfo> Get(string vendorAddress)
        {
            return GetDataManager().GetProducts(vendorAddress);
        }

        [HttpGet]
        [Route("{vendorAddress}/Product/{productCode}/Content")]
        public ProductContent GetProductContent(string vendorAddress, string productCode)
        {
            if ((vendorAddress != null) && (productCode != null))
            {
                string signature = Request.Query.ContainsKey("sign") ? Request.Query["sign"] : Request.Headers.ContainsKey("sign") ? Request.Headers["sign"] : "";
                string chainIDText = Request.Query.ContainsKey("chainID") ? Request.Query["chainID"] : Request.Headers.ContainsKey("chainID") ? Request.Headers["chainID"] : "";

                if ((signature != null) && (signature.Length > 0) && (chainIDText != null) && (chainIDText.Length > 0))
                {
                    long chainID = long.Parse(chainIDText);

                    string senderAddress = Tool.GetSenderAddress("OpkeePrivateAccountSignature", signature);
                    OpkeeContract opkeeContract = GetContract(chainID);

                    if (opkeeContract != null)
                    {
                        if (ContractManager.HasProductAccess(GetDataManager(), chainID, opkeeContract, senderAddress, vendorAddress, productCode))
                        {
                            return GetDataManager().GetProductContent(vendorAddress, productCode);
                        }
                        else
                        {
                            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return new ProductContent();
                        }
                    }
                }
            }

            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return new ProductContent();
        }
    }
}