using System.Net.Http.Json;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlobSimplierSDK;

/*
  This liblary will make blob storage more simplier, can be used on future API,
*/

public class Blober
{
    public string ConnectionString { get; set; }
    BloberDriver bloberDriver = new BloberDriver();

    /*
      It will list all the containers on the blob storage
    */
    public async Task<string> ListAllContainersAsync()
    {
        return await bloberDriver.ListAllContainersAsync(ConnectionString);
    }

    /*
      It creates a new container
    */
    public async Task<string> CreateContainerAsync(string containerName)
    {
        return await bloberDriver.CreateContainerAsync(containerName, ConnectionString);
    }

    /*
      It will delete a container
    */
    public async Task<bool> DeleteContainerAsync(string containerName)
    {
        return await bloberDriver.DeleteContainerAsync(containerName, ConnectionString);
    }

    /*
      It will lists all the items in a container on the blob storage
    */
    public async Task<string> ContainerItemList(string containerName)
    {
        return await bloberDriver.ContainerItemList(containerName, ConnectionString);
    }

    // It will get the value of an specific item from the container in the blob storage
    public async Task<string> GetItemJsonContent(string containerName, string itemName)
    {
        return await bloberDriver.GetItemJsomContent(containerName, itemName, ConnectionString);
    }

    // it will get the value of an specified item from the container in the blob storage base on a class template
    public async Task<T> GetItemJsonContent<T>(string containerName, string itemName)
    {
        return await bloberDriver.GetItemJsonContent<T>(containerName, itemName, ConnectionString);
    }

    public async Task<T> PostData<T>(string containerName, T data)
    {
        return await bloberDriver.PostData<T>(containerName, data, ConnectionString);
    }

    public async Task<T> UpdateData<T>(string containerName, string itemId, T data)
    {
        return await bloberDriver.UpdateData(containerName, itemId, data, ConnectionString);
    }

    // Merge all the json item content as one base on the container
    public async Task<string> MergeAllJsonItems(string containerName)
    {
        return await bloberDriver.MergeAllJsonItems(containerName, ConnectionString);
    }

    public async Task<string> MergeAllJsonItems<T>(string containerName) where T : class
    {
        return await bloberDriver.MergeAllJsonItems<T>(containerName, ConnectionString);
    }

    // It will search field's value according to it's container name, item name, and field
    public async Task<string> SearchInContainerAsync(string containerName, string itemName, string searchField)
    {
        return await bloberDriver.SearchInContainerAsync(containerName, itemName, searchField, ConnectionString);
    }

    // Searches A Specific Value from Field on an item in a continer
    public async Task<string> SearchIDByField(string containerName, string searchField, string searchValue)
    {
        return await bloberDriver.SearchIDByField(containerName, searchField, searchValue, ConnectionString);
    }

    // Deletes an item in a specific continer
    public async Task<string> DeleteItemFromContainer(string containerName, string itemName)
    {
        return await bloberDriver.DeleteItemFromContainer(containerName, itemName, ConnectionString);
    }

    // Move a file from container A to container B
    public async Task<string> MoveBlobAsync(string fromCont, string toCont, string fileName)
    {
        return await bloberDriver.MoveBlobAsync(ConnectionString, fromCont, toCont, fileName);
    }

    public async Task<string> RenameBlobAsync(string containerName, string oldFileName, string newFileName)
    {
        return await bloberDriver.RenameBlobAsync(ConnectionString, containerName, oldFileName, newFileName);

    }

    public async Task<string> OpenPost(string containerName, string jsonValue)
    {
        return await bloberDriver.OpenPost(ConnectionString, containerName, jsonValue);
    }

    public async Task<string> GetFileContentAsync(string containerName, string fileName)
    {
        return await bloberDriver.GetFileContentAsync(containerName, fileName, ConnectionString);
    }

}
