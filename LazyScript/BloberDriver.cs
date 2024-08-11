using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlobSimplierSDK;

public class BloberDriver
{
    public async Task<string> ListAllContainersAsync(string connString)
    {
        var containerNames = new List<string>();

        var blobServiceClient = new BlobServiceClient(connString);
        await foreach (var containerItem in blobServiceClient.GetBlobContainersAsync())
        {
            containerNames.Add(containerItem.Name);
        }

        // Serialize the list to JSON
        return JsonSerializer.Serialize(containerNames, new JsonSerializerOptions { WriteIndented = true });
    }


    public async Task<string> CreateContainerAsync(string containerName, string connString)
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
        BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
        return containerClient.Uri.ToString();
    }

    public async Task<bool> DeleteContainerAsync(string containerName, string connString)
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        try
        {
            await containerClient.DeleteAsync();
            return true;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.ContainerNotFound)
        {
            return false;
        }
    }

    public async Task<T> GetItemJsonContent<T>(string containerName, string itemName, string connString)
    {
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(itemName);

        if (await blobClient.ExistsAsync())
        {
            var downloadInfo = await blobClient.DownloadAsync();
            using (var reader = new StreamReader(downloadInfo.Value.Content))
            {
                string content = await reader.ReadToEndAsync();
                // Deserialize with JsonSerializerOptions to handle null values
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Optional: ignore case
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Handle null values
                };

                // Deserialize the JSON content into the specified type (T)
                T deserializedObject = JsonSerializer.Deserialize<T>(content, options);

                // Set the Id property based on itemName without file extension
                var propertyInfo = typeof(T).GetProperty("Id");
                if (propertyInfo != null && propertyInfo.PropertyType == typeof(string))
                {
                    string idValue = Path.GetFileNameWithoutExtension(itemName);
                    propertyInfo.SetValue(deserializedObject, idValue, null);
                }
                return deserializedObject;
            }
        }
        else
        {
            throw new FileNotFoundException($"Blob '{itemName}' not found in container '{containerName}'.");
        }
    }

    public async Task<T> PostData<T>(string containerName, T data, string connString)
    {
        var itemId = Guid.NewGuid(); // Generate a new unique identifier

        // Check if the data type has an Id property
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            // Set the Id property based on its type
            if (idProperty.PropertyType == typeof(Guid))
            {
                idProperty.SetValue(data, itemId);
            }
            else if (idProperty.PropertyType == typeof(string))
            {
                idProperty.SetValue(data, itemId.ToString());
            }
            else
            {
                throw new InvalidOperationException("The Id property must be of type Guid or string.");
            }
        }

        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient($"{itemId}.json");

        // Upload the serialized data to the blob
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data))))
        {
            await blobClient.UploadAsync(stream, true);
        }

        // Return the posted data object with the updated Id
        return data;
    }

    public async Task<T> UpdateData<T>(string containerName, string itemId, T data, string connString)
    {
        // Ensure the item ID is provided
        if (string.IsNullOrEmpty(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or empty.");
        }

        // Get the blob client for the specified item ID
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient($"{itemId}.json");

        // Check if the blob exists
        var exists = await blobClient.ExistsAsync();
        if (!exists)
        {
            throw new KeyNotFoundException("Item not found.");
        }

        // Serialize the updated data to JSON
        string jsonContent = JsonSerializer.Serialize(data);

        // Deserialize the JSON to a dynamic object to ensure Id remains unchanged
        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
        if (jsonObject.ContainsKey("Id"))
        {
            jsonObject["Id"] = itemId;
        }

        // Serialize the object back to JSON
        jsonContent = JsonSerializer.Serialize(jsonObject);

        // Upload the serialized data to the blob
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)))
        {
            await blobClient.UploadAsync(stream, true);
        }

        // Return the updated data object
        return data;
    }






    public async Task<string> ContainerItemList(string containerName, string connString)
    {
        var blobItems = new List<string>();
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            blobItems.Add(blobItem.Name);
        }
        string jsonResult = JsonSerializer.Serialize(blobItems);
        return jsonResult;
    }

    public async Task<string> GetItemJsomContent(string containerName, string itemName, string connString)
    {
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(itemName);
        if (await blobClient.ExistsAsync())
        {
            var downloadInfo = await blobClient.DownloadAsync();
            using (var reader = new StreamReader(downloadInfo.Value.Content))
            {
                string content = await reader.ReadToEndAsync();
                return content;
            }
        }
        else
        {
            return "404";
        }
    }

    public async Task<string> MergeAllJsonItems(string containerName, string connString)
    {
        var jsonItems = new List<JsonDocument>();
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            if (await blobClient.ExistsAsync())
            {
                var downloadInfo = await blobClient.DownloadAsync();
                using (var reader = new StreamReader(downloadInfo.Value.Content))
                {
                    string content = await reader.ReadToEndAsync();
                    var jsonDocument = JsonDocument.Parse(content);
                    jsonItems.Add(jsonDocument);
                }
            }
        }
        // Merge all JSON items into a single JSON array
        using (var stream = new MemoryStream())
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartArray();
            foreach (var jsonItem in jsonItems)
            {
                writer.WriteStartObject();
                foreach (var element in jsonItem.RootElement.EnumerateObject())
                {
                    element.WriteTo(writer);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.Flush();
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
    }
    public async Task<string> MergeAllJsonItems<T>(string containerName, string connString) where T : class
    {
        var itemList = new List<T>();
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            if (await blobClient.ExistsAsync())
            {
                var downloadInfo = await blobClient.DownloadAsync();
                using (var reader = new StreamReader(downloadInfo.Value.Content))
                {
                    string content = await reader.ReadToEndAsync();
                    var item = JsonSerializer.Deserialize<T>(content);
                    if (item != null)
                    {
                        // Set the Id property based on the blob item name without file extension
                        var propertyInfo = typeof(T).GetProperty("Id");
                        if (propertyInfo != null && propertyInfo.PropertyType == typeof(string))
                        {
                            string idValue = Path.GetFileNameWithoutExtension(blobItem.Name);
                            propertyInfo.SetValue(item, idValue, null);
                        }
                        itemList.Add(item);
                    }
                }
            }
        }
        string mergedJson = JsonSerializer.Serialize(itemList);
        return mergedJson;
    }


    public async Task<string> SearchInContainerAsync(string containerName, string itemName, string searchField, string connString)
    {
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(itemName);

        if (await blobClient.ExistsAsync())
        {
            var downloadInfo = await blobClient.DownloadAsync();
            using (var reader = new StreamReader(downloadInfo.Value.Content))
            {
                string content = await reader.ReadToEndAsync();
                using (JsonDocument document = JsonDocument.Parse(content))
                {
                    if (document.RootElement.TryGetProperty(searchField, out JsonElement element))
                    {
                        return element.ToString();
                    }
                    else
                    {
                        return "404";
                    }
                }
            }
        }
        else
        {
            return "404";
        }
    }

    public async Task<string> SearchIDByField(string containerName, string searchField, string searchValue, string connString)
    {
        var blobServiceClient = new BlobServiceClient(connString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            if (await blobClient.ExistsAsync())
            {
                var downloadInfo = await blobClient.DownloadAsync();
                using (var reader = new StreamReader(downloadInfo.Value.Content))
                {
                    string content = await reader.ReadToEndAsync();
                    using (JsonDocument document = JsonDocument.Parse(content))
                    {
                        if (document.RootElement.TryGetProperty(searchField, out JsonElement element) &&
                          element.GetString() == searchValue)
                        {
                            return Path.GetFileNameWithoutExtension(blobItem.Name);
                        }
                    }
                }
            }
        }
        return "404";
    }

    public async Task<string> DeleteItemFromContainer(string containerName, string itemName, string connString)
    {
        var status = await GetItemJsomContent(containerName, itemName, connString);
        if (status == "404")
        {
            return "You cannot delete something do not exist.";
        }
        else
        {
            var blobServiceClient = new BlobServiceClient(connString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(itemName);
            await blobClient.DeleteIfExistsAsync();
            return "Deleted  " + itemName;
        }
    }

    // Move a file from container A to container B
    public async Task<string> MoveBlobAsync(string connString, string fromCont, string toCont, string fileName)
    {
        try
        {
            // Create BlobServiceClient using the connection string
            var blobServiceClient = new BlobServiceClient(connString);

            // Get container clients
            var sourceContainerClient = blobServiceClient.GetBlobContainerClient(fromCont);
            var destinationContainerClient = blobServiceClient.GetBlobContainerClient(toCont);

            // Ensure the destination container exists
            await destinationContainerClient.CreateIfNotExistsAsync();

            // Get blob clients
            var sourceBlobClient = sourceContainerClient.GetBlobClient(fileName);
            var destinationBlobClient = destinationContainerClient.GetBlobClient(fileName);

            // Copy the blob to the destination container
            await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

            // Optionally, delete the blob from the source container
            await sourceBlobClient.DeleteIfExistsAsync();

            return "Blob moved successfully.";
        }
        catch (Exception ex)
        {
            // Return the error message
            return $"Error moving blob: {ex.Message}";
        }
    }
    public async Task<string> RenameBlobAsync(string connString, string containerName, string oldFileName, string newFileName)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(connString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var oldBlobClient = containerClient.GetBlobClient(oldFileName);
            var newBlobClient = containerClient.GetBlobClient(newFileName);

            // Copy the blob to the new name
            await newBlobClient.StartCopyFromUriAsync(oldBlobClient.Uri);

            // Optionally, delete the old blob
            await oldBlobClient.DeleteIfExistsAsync();

            return "Blob renamed successfully.";
        }
        catch (Exception ex)
        {
            return $"Error renaming blob: {ex.Message}";
        }
    }

    public async Task<string> OpenPost(string connstring, string containerName, string jsonValue)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(connstring);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await blobContainerClient.CreateIfNotExistsAsync();

            // Generate auto-incremented filename
            var existingBlobs = blobContainerClient.GetBlobsAsync();
            int maxIndex = 0;

            await foreach (var blobItem in existingBlobs)
            {
                var blobFileName = Path.GetFileNameWithoutExtension(blobItem.Name);
                if (int.TryParse(blobFileName, out int index))
                {
                    if (index >= maxIndex)
                    {
                        maxIndex = index + 1;
                    }
                }
            }

            string newFileName = $"{maxIndex}.json";
            var blobClient = blobContainerClient.GetBlobClient(newFileName);

            // Upload JSON text directly to the blob
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonValue)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return $"File '{newFileName}' uploaded successfully to container '{containerName}'.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> GetFileContentAsync(string containerName, string fileName, string connectionString)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (await blobClient.ExistsAsync())
        {
            var downloadInfo = await blobClient.DownloadAsync();
            using (var reader = new StreamReader(downloadInfo.Value.Content))
            {
                return await reader.ReadToEndAsync();
            }
        }
        else
        {
            throw new FileNotFoundException($"Blob '{fileName}' not found in container '{containerName}'.");
        }
    }

    public async Task<string> UploadPdfAsync(string containerName, string filePath, string connString)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(connString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var fileName = Path.GetFileName(filePath);
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload the PDF file to the blob
            using (var fileStream = File.OpenRead(filePath))
            {
                await blobClient.UploadAsync(fileStream, overwrite: true);
            }

            return $"PDF file '{fileName}' uploaded successfully to container '{containerName}'.";
        }
        catch (Exception ex)
        {
            return $"Error uploading PDF: {ex.Message}";
        }
    }

}
