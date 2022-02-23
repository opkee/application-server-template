using System;

namespace Opkee
{
    public class ProductContent
    {
        public ProductContent()
        {
        }

        public ProductContent(string code, string contentType, string data)
        {
            ProductCode = code;
            ContentType = contentType;
            Data = data;
        }

        public string ProductCode { get; }
        public string ContentType { get; }
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

        public string Data { get; }
    }
}