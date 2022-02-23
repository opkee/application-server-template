using System;
using System.Text.Json.Serialization;

namespace Opkee
{
    public class ProductInfo
    {
        public enum DurationUnit
        {
            Unlimited,
            Minute,
            Hour,
            Day,
            Year
        }

        [JsonConstructor]
        public ProductInfo()
        {

        }

        public ProductInfo(string code, string name, string description, string contentType, string fileName, double price, DurationUnit durationUnit, long validityPeriodValue)
        {
            Code = code;
            Name = name;
            Description = description;
            ContentType = contentType;
            FileName = fileName;
            Price = price;
            ValidityPeriodUnit = durationUnit;
            ValidityPeriodValue = validityPeriodValue;
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public bool IsText
        {
            get
            {
                if ((ContentType != null) && (ContentType.ToLower().StartsWith("text/")))
                {
                    return true;
                }
                return false;
            }
        }
        public string FileName { get; set; }
        public double Price { get; set; }
        public DurationUnit ValidityPeriodUnit { get; set; }
        public long ValidityPeriodValue { get; set; }
    }
}