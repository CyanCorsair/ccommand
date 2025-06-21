using System;
using System.Collections.Generic;
using Godot;

namespace CCommandCore.CTerrainEditor.Observables;

public readonly record struct TerrainInfo(TerrainTypes terrainType);

public sealed class ActiveTerrainHandler : IObservable<TerrainInfo>
{
    private IObserver<TerrainInfo> _observer;

    internal sealed class Unsubscriber<TerrainInfo> : IDisposable
    {
        private IObserver<TerrainInfo> _observer;

        internal Unsubscriber(IObserver<TerrainInfo> observer) => _observer = observer;

        public void Dispose() => _observer = null;
    }

    public IDisposable Subscribe(IObserver<TerrainInfo> observer)
    {
        _observer = observer;

        return new Unsubscriber<TerrainInfo>(_observer);
    }
}

public class ActiveTerrainReporter : IObserver<TerrainInfo>
{
    private IDisposable unsubscriber;
    public TerrainInfo activeTerrain;

    public void OnCompleted()
    {
        GD.Print("Observable done");
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(TerrainInfo value)
    {
        activeTerrain = value;
    }

    public virtual void Subscribe(IObservable<TerrainInfo> provider)
    {
        unsubscriber = provider.Subscribe(this);
    }

    public virtual void Unsubscribe()
    {
        unsubscriber.Dispose();
    }
}
