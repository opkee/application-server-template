using System;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.JsonRpc.Client;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;
using System.IO;
using System.Text.Json;

namespace Opkee.Token
{
    public class OpkeeContract
    {
        const string _ABIFile = @"OpkeeABI.json";

        protected Contract _contract;
        protected string _abi;
        protected string _resourcesFolder;

        public string Address { get; }
        public string ABI { get { return _abi; } }
        public Web3 Web3 { get; }
        public Contract Contract { get { return _contract; } }

        Function _name;
        Function _symbol;
        Function _getTransfers;

        [FunctionOutput]
        public class TrackedTransfer : IFunctionOutputDTO
        {
            [Parameter("uint112", "time", 1)]
            public virtual BigInteger Time { get; set; }

            [Parameter("uint112", "amount", 2)]
            public virtual BigInteger Amount { get; set; }

            [Parameter("string", "transferData", 3)]
            public virtual string TransferData { get; set; }
        }

        [FunctionOutput]
        public class TrackedTransferTuple : IFunctionOutputDTO
        {
            [Parameter("tuple[]", "", 1)]
            public virtual List<TrackedTransfer> Values { get; set; }
        }

        public OpkeeContract(string rpcEndpoint, string contractAddress, string resourcesFolder)
        {
            _resourcesFolder = resourcesFolder;
            Address = contractAddress;
            var rpcClient = new RpcClient(new Uri(rpcEndpoint));
            Web3 = new Nethereum.Web3.Web3(rpcClient);
            _abi = GetABIFromJsonFile();
        }

        protected string GetABIFromJsonFile()
        {
            string fullFileName = Path.Combine(_resourcesFolder, _ABIFile);
            string abiText = File.ReadAllText(fullFileName);
            using JsonDocument doc = JsonDocument.Parse(abiText);
            JsonElement root = doc.RootElement;
            var abiElement = root.GetProperty("abi");
            return abiElement.ToString();
        }

        public void BuildContract()
        {
            _contract = Web3.Eth.GetContract(ABI, Address);
        }

        public string GetName()
        {
            if (_name == null)
            {
                _name = _contract.GetFunction("name");
            }

            var callResult = _name.CallAsync<string>();
            callResult.Wait();
            return callResult.Result;
        }

        public string GetSymbol()
        {
            if (_symbol == null)
            {
                _symbol = _contract.GetFunction("symbol");
            }

            var callResult = _symbol.CallAsync<string>();
            callResult.Wait();
            return callResult.Result;
        }

        public List<TrackedTransfer> GetTransfers(string senderAddress, string vendorWallet)
        {
            if ((senderAddress != null) && (vendorWallet != null))
            {
                if (_getTransfers == null)
                {
                    _getTransfers = _contract.GetFunction("getTransfers");
                }

                object[] callParams = new object[2];
                callParams[0] = senderAddress;
                callParams[1] = vendorWallet;

                var callResult = _getTransfers.CallDeserializingToObjectAsync<TrackedTransferTuple>(callParams);
                callResult.Wait();

                return callResult.Result.Values;
            }

            return new List<TrackedTransfer>();
        }
    }
}