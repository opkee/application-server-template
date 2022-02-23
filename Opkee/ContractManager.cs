using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Signer;
using Opkee.Data;

namespace Opkee.Token
{
    public class ContractManager
    {
        static Dictionary<long, Dictionary<string, Dictionary<string, List<OpkeeContract.TrackedTransfer>>>> _trackedTransfersByVendorBySenderByChainCache = new Dictionary<long, Dictionary<string, Dictionary<string, List<OpkeeContract.TrackedTransfer>>>>();


        static bool IsProductAccessValid(OpkeeContract.TrackedTransfer trackedTransfer, ProductInfo productInfo)
        {
            DateTime transferDate = Tool.UnixTimeStampToDateTime(((long)trackedTransfer.Time));
            if (productInfo.ValidityPeriodUnit == ProductInfo.DurationUnit.Unlimited)
            {
                return true;
            }
            else
            {
                DateTime expirationDate = transferDate;
                switch (productInfo.ValidityPeriodUnit)
                {
                    case ProductInfo.DurationUnit.Minute:
                        expirationDate = expirationDate.AddMinutes(productInfo.ValidityPeriodValue);
                        break;
                    case ProductInfo.DurationUnit.Hour:
                        expirationDate = expirationDate.AddHours(productInfo.ValidityPeriodValue);
                        break;
                    case ProductInfo.DurationUnit.Day:
                        expirationDate = expirationDate.AddDays(productInfo.ValidityPeriodValue);
                        break;
                    case ProductInfo.DurationUnit.Year:
                        expirationDate = expirationDate.AddYears((int)productInfo.ValidityPeriodValue);
                        break;
                    default:
                        return true;
                }

                return expirationDate > DateTime.Now.ToUniversalTime();
            }
        }

        public static bool HasProductAccessInCache(long chainID, ProductInfo productInfo, string senderAddress, string vendorAddress, string productCode)
        {
            if (_trackedTransfersByVendorBySenderByChainCache.ContainsKey(chainID))
            {
                Dictionary<string, Dictionary<string, List<OpkeeContract.TrackedTransfer>>> trackedTransfersByVendorBySenderCache = _trackedTransfersByVendorBySenderByChainCache[chainID];
                if (trackedTransfersByVendorBySenderCache.ContainsKey(vendorAddress))
                {
                    Dictionary<string, List<OpkeeContract.TrackedTransfer>> trackedTransfersBySenderCache = trackedTransfersByVendorBySenderCache[vendorAddress];

                    if (trackedTransfersBySenderCache.ContainsKey(senderAddress))
                    {
                        List<OpkeeContract.TrackedTransfer> trackedTransfers = trackedTransfersBySenderCache[senderAddress];

                        if ((trackedTransfers != null) && (trackedTransfers.Count > 0))
                        {
                            foreach (var trackedTransfer in trackedTransfers)
                            {
                                if (trackedTransfer.TransferData != null)
                                {
                                    if (productCode.ToUpper() == trackedTransfer.TransferData.ToUpper())
                                    {
                                        if (IsProductAccessValid(trackedTransfer, productInfo))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool HasProductAccess(DataManager dataManager, long chainID, OpkeeContract opkeeContract, string senderAddress, string vendorAddress, string productCode)
        {
            if ((opkeeContract != null) && (dataManager != null) && (chainID > 0))
            {
                ProductInfo productInfo = dataManager.GetProduct(vendorAddress, productCode);

                if (productInfo != null)
                {
                    if (!HasProductAccessInCache(chainID, productInfo, senderAddress, vendorAddress, productCode))
                    {
                        List<OpkeeContract.TrackedTransfer> trackedTransfers = opkeeContract.GetTransfers(senderAddress, vendorAddress);

                        if ((trackedTransfers != null) && (trackedTransfers.Count > 0))
                        {
                            Dictionary<string, Dictionary<string, List<OpkeeContract.TrackedTransfer>>> trackedTransfersByVendorBySenderCache;

                            if (_trackedTransfersByVendorBySenderByChainCache.ContainsKey(chainID))
                            {
                                trackedTransfersByVendorBySenderCache = _trackedTransfersByVendorBySenderByChainCache[chainID];
                            }
                            else
                            {
                                trackedTransfersByVendorBySenderCache = new Dictionary<string, Dictionary<string, List<OpkeeContract.TrackedTransfer>>>();
                            }

                            Dictionary<string, List<OpkeeContract.TrackedTransfer>> trackedTransfersBySenderCache = trackedTransfersByVendorBySenderCache.ContainsKey(vendorAddress) ? trackedTransfersByVendorBySenderCache[vendorAddress] : new Dictionary<string, List<OpkeeContract.TrackedTransfer>>();
                            trackedTransfersBySenderCache[senderAddress] = trackedTransfers;
                            trackedTransfersByVendorBySenderCache[vendorAddress] = trackedTransfersBySenderCache;

                            foreach (var trackedTransfer in trackedTransfers)
                            {
                                if (trackedTransfer.TransferData != null)
                                {
                                    if (productCode.ToUpper() == trackedTransfer.TransferData.ToUpper())
                                    {
                                        if (IsProductAccessValid(trackedTransfer, productInfo))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            _trackedTransfersByVendorBySenderByChainCache[chainID] = trackedTransfersByVendorBySenderCache;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}