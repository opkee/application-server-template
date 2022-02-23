using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Caching;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Opkee;
using Opkee.Data;
using Opkee.Token;
using Microsoft.Extensions.Configuration.Json;

namespace Opkee.WebApplication.Controllers
{
    public class OpkeeBaseController : ControllerBase
    {
        static protected DataManager GetDataManager()
        {
            ObjectCache cache = MemoryCache.Default;
            DataManager dataManager = cache["DataManager"] as DataManager;

            if (dataManager == null)
            {
                string productsFolder = Startup.Configuration["ProductsFolder"];
                dataManager = new DataManager(productsFolder);
                CacheItemPolicy policy = new CacheItemPolicy();
                cache.Set("DataManager", dataManager, policy);
            }

            return dataManager;
        }

        static protected OpkeeContract GetContract(long chainID)
        {
            OpkeeContract opkeeContract = null;

            if (chainID > 0)
            {
                ObjectCache cache = MemoryCache.Default;
                Dictionary<long, OpkeeContract> opkeeContractByChainID = cache["OpkeeContractByChainID"] as Dictionary<long, OpkeeContract>;

                if (opkeeContractByChainID == null)
                {
                    opkeeContractByChainID = new Dictionary<long, OpkeeContract>();
                }

                if (opkeeContractByChainID.ContainsKey(chainID))
                {
                    opkeeContract = opkeeContractByChainID[chainID];
                }
                else
                {
                    string rpcEndpoint = Startup.Configuration["Blockchain" + chainID + ":RPCEndpoint"];
                    string contractAddress = Startup.Configuration["Blockchain" + chainID + ":ContractAddress"];
                    string resourcesFolder = Startup.Configuration["ResourcesFolder"];

                    if ((rpcEndpoint != null) && (contractAddress != null) && (resourcesFolder != null))
                    {
                        opkeeContract = new OpkeeContract(rpcEndpoint, contractAddress, resourcesFolder);
                        opkeeContract.BuildContract();
                        opkeeContractByChainID.Add(chainID, opkeeContract);
                        CacheItemPolicy policy = new CacheItemPolicy();
                        cache.Set("OpkeeContractByChainID", opkeeContractByChainID, policy);
                    }
                }
            }

            return opkeeContract;
        }
    }
}