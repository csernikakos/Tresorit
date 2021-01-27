using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tresorit.Models;

namespace Tresorit.Data
{
    public class DataTableQueries
    {
        string tableName = "TresoritTable";
        string connString = "DefaultEndpointsProtocol=https;AccountName=tresoritstrorageacc;AccountKey=tq+qxPI6OO2QYMlCCSzveMTSHz+UJJVSUt/8/yvX9WVjE0umZJgaxbAVEZMeqlWReDOP20YlFXxteHd/GTD+FA==;EndpointSuffix=core.windows.net";
        CloudTable table;

        public DataTableQueries()
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            table = tableClient.GetTableReference(tableName);
        }

        public List<Product> GetUniquePartitionKeys()
        {
            TableQuery<Product> queryResult = new TableQuery<Product>();
            var itemlist = table.ExecuteQuery(queryResult);

            List<Product> distinct = itemlist.GroupBy(x => x.PartitionKey).Select(g => g.First()).ToList();
            return distinct;
        }

        public List<Product> GetPartitionKeyItems(string partitionkey)
        {
            TableQuery<Product> queryResult = new TableQuery<Product>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey)); 
            var itemlist = table.ExecuteQuery(queryResult);
            List<Product> distinct = itemlist.GroupBy(x => x.RowKey).Select(g => g.First()).OrderByDescending(t=>t.Timestamp).ToList();
            return distinct;
        }

        public decimal AverageRating(string partitionkey)
        {
            decimal result = 0;
            decimal sum = 0;
            TableQuery<Product> queryResult = new TableQuery<Product>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey));
            var itemlist = table.ExecuteQuery(queryResult);
            var itemCount = itemlist.Count();

            foreach (var item in itemlist)
            {
                if (item.Rating != null)
                {
                    sum += (int)item.Rating;
                }
            }
            if (itemlist.Count()>1)
            {
                itemCount = (itemCount - 1);
            }
            result = sum / itemCount;
            result = Math.Round(result, 1);

            return result;
        }

        public int RatingQuantity(string partitionkey)
        {
            int sum = 0;
            TableQuery<Product> queryResult = new TableQuery<Product>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey));
            var itemlist = table.ExecuteQuery(queryResult);
            foreach (var item in itemlist)
            {
                if (item.Review!=null)
                {
                    sum++;
                }
            }
            return sum;
        }

        public async Task<Product> InsertOrMergeProduct(Product product)
        {
            if (product==null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(product);
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                Product insertedProduct = result.Result as Product;

                return insertedProduct;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

    }
}
