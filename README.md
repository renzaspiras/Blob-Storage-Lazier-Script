# BlobSimplierSDK

**BlobSimplierSDK** is a .NET library for interacting with Azure Blob Storage, offering various methods for container and blob management, JSON operations, and more.

## Installation

1. Place the source code in your project’s `Services` folder or any preferred location.
2. Add the following using directive to your code:

   ```csharp
   using BlobSimplierSDK;
   ```

## Usage

### Setup

Create an instance of `BloberDriver` and set the connection string:

```csharp
var blober = new BloberDriver();
```

### Methods

#### Container Management

- **ListAllContainersAsync**

  Lists all containers in the Azure Blob Storage account.

  ```csharp
  var containers = await blober.ListAllContainersAsync("your-connection-string");
  ```

- **CreateContainerAsync**

  Creates a new container and returns its URI.

  ```csharp
  var containerUri = await blober.CreateContainerAsync("my-container", "your-connection-string");
  ```

- **DeleteContainerAsync**

  Deletes a specified container. Returns `true` if successful, `false` if the container was not found.

  ```csharp
  bool success = await blober.DeleteContainerAsync("my-container", "your-connection-string");
  ```

#### Blob Management

- **ContainerItemList**

  Lists all blob items in a specified container.

  ```csharp
  var items = await blober.ContainerItemList("my-container", "your-connection-string");
  ```

- **GetItemJsonContent**

  Retrieves the JSON content of a specific blob and deserializes it to the specified type.

  ```csharp
  var content = await blober.GetItemJsonContent<MyType>("my-container", "my-item.json", "your-connection-string");
  ```

- **PostData<T>**

  Uploads data to a blob in JSON format. Generates a new unique identifier for the data.

  ```csharp
  var postedData = await blober.PostData("my-container", myDataObject, "your-connection-string");
  ```

- **UpdateData<T>**

  Updates an existing blob with new data. Ensures the `Id` remains unchanged.

  ```csharp
  var updatedData = await blober.UpdateData("my-container", "my-item-id", myUpdatedDataObject, "your-connection-string");
  ```

- **DeleteItemFromContainer**

  Deletes a specified blob item from a container. Returns a message indicating the result.

  ```csharp
  var result = await blober.DeleteItemFromContainer("my-container", "my-item.json", "your-connection-string");
  ```

- **MoveBlobAsync**

  Moves a blob from one container to another. Optionally deletes the blob from the source container.

  ```csharp
  var result = await blober.MoveBlobAsync("your-connection-string", "source-container", "destination-container", "my-item.json");
  ```

- **RenameBlobAsync**

  Renames a blob in a container. Optionally deletes the old blob.

  ```csharp
  var result = await blober.RenameBlobAsync("your-connection-string", "my-container", "old-name.json", "new-name.json");
  ```

#### JSON Operations

- **GetItemJsomContent**

  Retrieves the raw JSON content of a specified blob.

  ```csharp
  var content = await blober.GetItemJsomContent("my-container", "my-item.json", "your-connection-string");
  ```

- **MergeAllJsonItems**

  Merges all JSON blobs in a container into a single JSON array. Optionally deserialize to a specific type.

  ```csharp
  var mergedJson = await blober.MergeAllJsonItems("my-container", "your-connection-string");
  ```

  ```csharp
  var mergedJson = await blober.MergeAllJsonItems<MyType>("my-container", "your-connection-string");
  ```

- **SearchInContainerAsync**

  Searches for a specific field in a JSON blob and returns its value.

  ```csharp
  var value = await blober.SearchInContainerAsync("my-container", "my-item.json", "searchField", "your-connection-string");
  ```

- **SearchIDByField**

  Searches for a blob based on a field and value, returning the ID (filename without extension).

  ```csharp
  var id = await blober.SearchIDByField("my-container", "searchField", "searchValue", "your-connection-string");
  ```

#### Utility Functions

- **OpenPost**

  Uploads JSON data to a container with an auto-incremented filename.

  ```csharp
  var result = await blober.OpenPost("your-connection-string", "my-container", "{\"key\":\"value\"}");
  ```

- **GetFileContentAsync**

  Retrieves the content of a specified file as a string.

  ```csharp
  var content = await blober.GetFileContentAsync("my-container", "my-file.json", "your-connection-string");
  ```

- **UploadPdfAsync**

  Uploads a PDF file to a specified container.

  ```csharp
  var result = await blober.UploadPdfAsync("my-container", "path/to/file.pdf", "your-connection-string");
  ```

---

This comprehensive guide covers all the methods available in `BlobSimplierSDK`, making it easier to perform various operations with Azure Blob Storage.
