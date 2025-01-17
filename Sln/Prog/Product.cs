using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 


namespace Prog
{
    public class Product
    {
        public string Code { get; set; }
        public decimal Price { get; set; }

        public static async Task<IEnumerable<Product>> ReadPricesFromFile(string path)
        {
            var fileContents = await ReadFromFile(path);

            return ConvertPricesToEnumberable(fileContents);
        }

        public static IEnumerable<Product> ConvertPricesToEnumberable(string[] values)
        {
            return values.Select(product => product.Split(','))
                .Select(product => new Product
                {
                    Code = product[0],
                    Price = Convert.ToDecimal(product[1])
                });
        }

        public static async Task<IEnumerable<Product>> ReadReceiptFromFile(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            var receipts = new List<Product>();

            foreach (var file in files)
            {
                if (file.Contains("PaxHeader")) continue;
                var fileContents = await ReadFromFile(file);

                receipts.AddRange(ConvertReceiptToEnumerable(fileContents));
            }

            return receipts;
        }

        public static IEnumerable<Product> ConvertReceiptToEnumerable(string[] values)
        {
            var usefulLines = values.Skip(1).SkipLast(1);

            var regex = new Regex(@"\d+(\.\d+)?");

            var receiptProductList = new List<Product>();

            foreach (var line in usefulLines)
            {
                if (line.Contains("VOIDED"))
                {
                    receiptProductList.RemoveAt(receiptProductList.Count - 1);
                    continue;
                }
                
                var match = regex.Match(line);
                var code = match.Value;
                var price = Convert.ToDecimal(match.NextMatch().Value);

                receiptProductList.Add(new Product
                {
                    Code = code,
                    Price = price
                });
            }

            return receiptProductList;
        }

        private static async Task<string[]> ReadFromFile(string path)
        {
            return await File.ReadAllLinesAsync(path);
        }

    }
}