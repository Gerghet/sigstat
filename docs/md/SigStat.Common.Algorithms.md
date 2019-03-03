#### `Dtw`

Dynamic Time Warping algorithm - test
```csharp
public class SigStat.Common.Algorithms.Dtw

```

###### Properties

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| --- | --- | --- | 
| `<sub>List<ValueTuple<Int32, Int32>></sub>` | <sub>ForwardPath</sub> | Gets the list of points representing the shortest path. | 


###### Methods

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| --- | --- | --- | 
| `<sub>Double</sub>` | <sub>Compute(Double[][], Double[][])</sub> | Generate shortest path between the two sequences. | 


#### `DtwPy`

A simple implementation of the DTW algorithm.
```csharp
public static class SigStat.Common.Algorithms.DtwPy

```

###### Static Methods

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| --- | --- | --- | 
| `<sub>Double</sub>` | <sub>Dtw(IEnumerable<P>, IEnumerable<P>, Func<P, P, Double>)</sub> | Calculates the distance between two time sequences | 
| `<sub>Double</sub>` | <sub>EuclideanDistance(Double[], Double[])</sub> | Calculates the euclidean distance of two vectors | 


#### `HSCPThinningStep`

HSCP thinning algorithm  http://www.ppgia.pucpr.br/~facon/Afinamento/1987holt.pdf
```csharp
public class SigStat.Common.Algorithms.HSCPThinningStep

```

###### Properties

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| --- | --- | --- | 
| `<sub>Nullable<Boolean></sub>` | <sub>ResultChanged</sub> | Gets whether the last `SigStat.Common.Algorithms.HSCPThinningStep.Scan(System.Boolean[0:,0:])` call was effective. | 


###### Methods

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| --- | --- | --- | 
| `<sub>Boolean[,]</sub>` | <sub>Scan(Boolean[,])</sub> | Does one step of the thinning. Call it iteratively while ResultChanged. | 


#### `PatternMatching3x3`

Binary 3x3 pattern matcher with rotating option.
```csharp
public class SigStat.Common.Algorithms.PatternMatching3x3

```

###### Methods

| <sub>Type</sub> | <sub>Name</sub> | <sub>Summary</sub> | 
| --- | --- | --- | 
| `<sub>Boolean</sub>` | <sub>Match(Boolean[,])</sub> | Match the 3x3 input with the 3x3 pattern. | 
| `<sub>Boolean</sub>` | <sub>RotMatch(Boolean[,])</sub> | Match the 3x3 input with the 3x3 pattern from all 4 directions. | 


