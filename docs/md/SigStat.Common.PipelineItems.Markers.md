#### `LogMarker`

Logs the Pipeline Input. Useful for logging TimeMarker results.  <para>Default Pipeline Output: -</para>
```csharp
public class SigStat.Common.PipelineItems.Markers.LogMarker
    : PipelineBase, ITransformation, ILogger, IProgress, IPipelineIO

```

###### Methods

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| ---- | ---- | ---- | 
| `void` | Transform(Signature) |  | 


#### `TimeMarkerStart`

Starts a timer to measure completion time of following transforms.  <para>Default Pipeline Output: (`System.DateTime`) DefaultTimer</para>
```csharp
public class SigStat.Common.PipelineItems.Markers.TimeMarkerStart
    : PipelineBase, ITransformation, ILogger, IProgress, IPipelineIO

```

###### Methods

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| ---- | ---- | ---- | 
| `void` | Transform(Signature) |  | 


#### `TimeMarkerStop`

Stops a timer to measure completion time of previous transforms.  <para>Default Pipeline Output: (`System.DateTime`) DefaultTimer</para>
```csharp
public class SigStat.Common.PipelineItems.Markers.TimeMarkerStop
    : PipelineBase, ITransformation, ILogger, IProgress, IPipelineIO

```

###### Methods

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| ---- | ---- | ---- | 
| `void` | Transform(Signature) |  | 


