# BlobSimplierSDK

**BlobSimplierSDK** is a .NET library for interacting with Azure Blob Storage. It simplifies operations such as managing containers and blobs, and performing JSON-related tasks.

## Features

- **Container management:** list, create, delete
- **Blob management:** upload, download, delete, rename
- **JSON operations:** serialize, deserialize, merge
- **Search operations:** search fields, search by ID
- **Utility functions:** move blobs between containers

## Installation

1. Place the source code in your `Services` folder or any part of your project.
2. Add the following using directive to your code:

   ```csharp
   using BlobSimplierSDK;
   ```

## Usage

### Setup

Create an instance of `Blober` and set the connection string:

```csharp
var blober = new Blober
{
    ConnectionString = "your-azure-blob-storage-connection-string"
};
```

### Methods

#### Container Management

- **ListAllContainersAsync**

  ```csharp
  var containers = await blober.ListAllContainersAsync();
  ```

- **CreateContainerAsync**

  ```csharp
  var containerUri = await blober.CreateContainerAsync("my-container");
  ```

- **DeleteContainerAsync**

  ```csharp
  bool success = await blober.DeleteContainerAsync("my-container");
  ```

#### Blob Management

- **ContainerItemList**

  ```csharp
  var items = await blober.ContainerItemList("my-container");
  ```

- **GetItemJsonContent**

  ```csharp
  var content = await blober.GetItemJsonContent("my-container", "my-item.json");
  ```

- **PostData<T>**

  ```csharp
  var postedData = await blober.PostData("my-container", myDataObject);
  ```

- **UpdateData<T>**

  ```csharp
  var updatedData = await blober.UpdateData("my-container", "my-item-id", myUpdatedDataObject);
  ```

- **DeleteItemFromContainer**

  ```csharp
  var result = await blober.DeleteItemFromContainer("my-container", "my-item.json");
  ```

- **MoveBlobAsync**
# Blob-Storage-Lazier-Script
  ```csharp
  var result = await blober.MoveBlobAsync("source-container", "destination-container", "my-item.json");
  ```

- **RenameBlobAsync**

  ```csharp
  var result = await blober.RenameBlobAsync("my-container", "old-name.json", "new-name.json");
  ```

#### JSON Operations

- **MergeAllJsonItems**

  ```csharp
  var mergedJson = await blober.MergeAllJsonItems("my-container");
  ```

- **MergeAllJsonItems<T>**

  ```csharp
  var mergedJson = await blober.MergeAllJsonItems<MyClass>("my-container");
  ```

#### Search Operations

- **SearchInContainerAsync**

  ```csharp
  var value = await blober.SearchInContainerAsync("my-container", "my-item.json", "fieldName");
  ```

- **SearchIDByField**

  ```csharp
  var id = await blober.SearchIDByField("my-container", "fieldName", "searchValue");
  ```

## Contributing

Submit issues or pull requests to contribute to the project.

## License

MIT License - see the [LICENSE](LICENSE) file for details.

