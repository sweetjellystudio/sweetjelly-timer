# SWeetJelly Timer Service

- TimerInitModule
- TimerManager
- ITimer
- BaseTimer
- Timers:
  - CountdownTimer
  - FrequencyTimer
  - StopWatchTimer

## ITimer

### Property

```csharp
public bool UseUnscaledTime {get;}
```

```csharp
public float TickSpeed {get;}
```

```csharp
public float CurrentTime {get;}
```

```csharp
public bool IsRunning {get;}
```

```csharp
public float Progress {get;}
```

```csharp
public abstract bool IsFinished {get;}
```

### Event

```csharp
public event EventHandler Started;
```

```csharp
public event EventHandler Paused;
```

```csharp
public event EventHandler Stopped;
```

```csharp
public event EventHandler ResetCompleted;
```

### Method

```csharp
public void Start()
```

```csharp
public void Stop()
```

```csharp
public abstract void Tick(float deltaTime);
```

```csharp
public void Resume();
```

```csharp
public void Pause();
```

```csharp
public virtual void Reset();
```

```csharp
public virtual void Reset(float time);
```

```csharp
ITimer timer = new CountdownTimer(30f);
timer.Reset(60f);
```
