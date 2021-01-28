using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tresorit.Models
{
    public class FileEntity : TableEntity
    {
        public FileEntity()
        {
        }

        public FileEntity(string fileName, string contentType)
        {
            // remove the path part from the file name since IE11 sends the full path

            int backSlashIndex = fileName.LastIndexOf('\\');
            if (backSlashIndex > -1 && fileName.Length > backSlashIndex + 1) fileName = fileName.Substring(backSlashIndex + 1);

            int forwardSlashIndex = fileName.LastIndexOf('/');
            if (forwardSlashIndex > -1 && fileName.Length > forwardSlashIndex + 1) fileName = fileName.Substring(forwardSlashIndex + 1);

            PartitionKey = "1";
            RowKey = fileName;
            ContentType = contentType;
        }

        public string ContentType { get; set; }
    }
}
