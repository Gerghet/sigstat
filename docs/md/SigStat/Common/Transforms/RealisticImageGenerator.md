# [RealisticImageGenerator](./RealisticImageGenerator.md)

Namespace: [SigStat]() > [Common]() > [Transforms]()

Assembly: SigStat.Common.dll

Implements [ILoggerObject](./../ILoggerObject.md), [IProgress](./../Helpers/IProgress.md), [IPipelineIO](./../Pipeline/IPipelineIO.md), [ITransformation](./../ITransformation.md)

## Summary
Generates a realistic looking image of the Signature based on standard features. Uses blue ink and white paper. It does NOT save the image to file.  <para>Default Pipeline Input: X, Y, Button, Pressure, Azimuth, Altitude `SigStat.Common.Features`</para><para>Default Pipeline Output: `SigStat.Common.Features.Image`</para>

## Constructors

| Name | Summary | 
| --- | --- | 
| RealisticImageGenerator ( [`Int32`](https://docs.microsoft.com/en-us/dotnet/api/System.Int32), [`Int32`](https://docs.microsoft.com/en-us/dotnet/api/System.Int32) ) |  | 


## Methods

| Return | Name | Summary | 
| --- | --- | --- | 
| void | Transform ( [`Signature`](./../Signature.md) ) |  | 


