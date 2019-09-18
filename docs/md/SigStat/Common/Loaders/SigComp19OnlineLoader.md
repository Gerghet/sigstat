# [SigComp19OnlineLoader](./SigComp19OnlineLoader.md)

Namespace: [SigStat]() > [Common]() > [Loaders]()

Assembly: SigStat.Common.dll

Implements [IDataSetLoader](./IDataSetLoader.md), [ILoggerObject](./../ILoggerObject.md)

## Summary
`SigStat.Common.Loaders.DataSetLoader` for the SigComp19 dataset

## Constructors

| Name | Summary | 
| --- | --- | 
| SigComp19OnlineLoader ( [`String`](https://docs.microsoft.com/en-us/dotnet/api/System.String), [`Boolean`](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean) ) | Initializes a new instance of the `SigStat.Common.Loaders.SigComp19OnlineLoader` class. | 


## Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| [String](https://docs.microsoft.com/en-us/dotnet/api/System.String) | DatabasePath | Gets or sets the database path. | 
| [Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean) | StandardFeatures | Gets or sets a value indicating whether features are also loaded as `SigStat.Common.Features` | 


## Methods

| Return | Name | Summary | 
| --- | --- | --- | 
| [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1)\<[Signer](./../Signer.md)> | EnumerateSigners ( [`Predicate`](https://docs.microsoft.com/en-us/dotnet/api/System.Predicate-1)\<[`Signer`](./../Signer.md)> ) |  | 


## Static Methods

| Return | Name | Summary | 
| --- | --- | --- | 
| void | LoadSignature ( [`Signature`](./../Signature.md), [`MemoryStream`](https://docs.microsoft.com/en-us/dotnet/api/System.IO.MemoryStream), [`Boolean`](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean) ) | Loads one signature from specified stream. | 

