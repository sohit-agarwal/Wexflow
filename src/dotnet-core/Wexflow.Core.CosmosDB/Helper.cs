using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wexflow.Core.CosmosDB
{
    public class Helper
    {

        public string EndpointUrl { get; }
        public string AuthorizationKey { get; }

        public Helper(string endpointUrl, string authorizationKey)
        {
            EndpointUrl = endpointUrl;
            AuthorizationKey = authorizationKey;
        }

        public void CreateDatabaseIfNotExists(string databaseName)
        {
            CreateDatabaseIfNotExistsAsync(databaseName).Wait();
        }

        private async Task CreateDatabaseIfNotExistsAsync(string databaseName)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });
            }
        }

        public DocumentCollection CreateDocumentCollectionIfNotExists(string databaseName, string collectionName)
        {
            var task = CreateDocumentCollectionIfNotExistsAsync(databaseName, collectionName);
            task.Wait();
            return task.Result;
        }

        private async Task<DocumentCollection> CreateDocumentCollectionIfNotExistsAsync(string databaseName, string collectionName)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                // Set throughput to the minimum value of 400 RU/s
                DocumentCollection simpleCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseName),
                new DocumentCollection { Id = collectionName },
                new RequestOptions { OfferThroughput = 400 });

                return simpleCollection;
            }
        }

        public DocumentCollection ReadDocumentCollection(string databaseName, string collectionName)
        {
            var task = ReadDocumentCollectionAsync(databaseName, collectionName);
            task.Wait();
            return task.Result;
        }

        private async Task<DocumentCollection> ReadDocumentCollectionAsync(string databaseName, string collectionName)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                DocumentCollection collection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));

                return collection;
            }
        }

        public Document[] ReadDocumentFeed(string databaseName, string collectionName)
        {
            var task = ReadDocumentFeedAsync(databaseName, collectionName);
            task.Wait();
            return task.Result;
        }

        private async Task<Document[]> ReadDocumentFeedAsync(string databaseName, string collectionName)
        {
            List<Document> docs = new List<Document>();
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                string continuationToken = null;
                do
                {
                    var feed = await client.ReadDocumentFeedAsync(
                        UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                        new FeedOptions { MaxItemCount = 10, RequestContinuation = continuationToken });
                    continuationToken = feed.ResponseContinuation;
                    foreach (Document document in feed)
                    {
                        docs.Add(document);
                    }
                } while (continuationToken != null);
            }

            return docs.ToArray();

        }

        public void DeleteAllDocuments(string databaseName, string collectionName)
        {
            DeleteAllDocumentsAsync(databaseName, collectionName).Wait();
        }

        private async Task DeleteAllDocumentsAsync(string databaseName, string collectionName)
        {
            var docs = ReadDocumentFeed(databaseName, collectionName);

            await DeleteDocumentsAsync(docs);
        }

        public void DeleteDocuments(Document[] docs)
        {
            DeleteDocumentsAsync(docs).Wait();
        }

        private async Task DeleteDocumentsAsync(Document[] docs)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                foreach (var doc in docs)
                {
                    await client.DeleteDocumentAsync(doc.SelfLink);
                }
            }
        }

        public void ReplaceDocument<T>(string databaseName, string collectionName, T o, string id)
        {
            ReplaceDocumentAsync(databaseName, collectionName, o, id).Wait();
        }

        private async Task ReplaceDocumentAsync<T>(string databaseName, string collectionName, T o, string id)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                ResourceResponse<Document> response = await client.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri(databaseName, collectionName, id), o);
            }
        }

        public void DeleteDocument(string databaseName, string collectionName, string id)
        {
            DeleteDocumentAsync(databaseName, collectionName, id).Wait();
        }

        private async Task DeleteDocumentAsync(string databaseName, string collectionName, string id)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                ResourceResponse<Document> response = await client.DeleteDocumentAsync(
                UriFactory.CreateDocumentUri(databaseName, collectionName, id));
            }
        }

        public ResourceResponse<Document> CreateDocument<T>(string databaseName, string collectionName, T o)
        {
            var task = CreateDocumentAsync(databaseName, collectionName, o);
            task.Wait();
            return task.Result;
        }

        private async Task<ResourceResponse<Document>> CreateDocumentAsync<T>(string databaseName, string collectionName, T o)
        {
            using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey))
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);

                return await client.CreateDocumentAsync(collectionUri, o);
            }

        }
    }


}

