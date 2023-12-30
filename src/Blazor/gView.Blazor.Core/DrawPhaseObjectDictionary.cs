using gView.Blazor.Core.Extensions;
using gView.Framework.Core.Carto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace gView.Blazor.Core;
public class DrawPhaseObjectDictionary<T>
{
    private readonly ConcurrentDictionary<DrawPhase, T?> _dict = new();

    public T Set(DrawPhase drawPhase, T value)
    {
        if (drawPhase == DrawPhase.All)
        {
            Set(DrawPhase.Geography, value);
            Set(DrawPhase.Selection, value);
            Set(DrawPhase.Graphics, value);
        }
        else
        {
            _dict[drawPhase] = value;
        }

        return value;
    }

    public T? Get(DrawPhase drawPhase)
    {
        //if (drawPhase == DrawPhase.All)
        //{
        //    return _dict[DrawPhase.Geography]
        //        ?? _dict[DrawPhase.Selection]
        //        ?? _dict[DrawPhase.Graphics];
        //}

        if(_dict.ContainsKey(drawPhase))
            return _dict[drawPhase];

        return default(T?);
    }

    public DrawPhaseObjectDictionary<T> ForAny(DrawPhase drawPhase, Action<T> action)
    {
        List<T?> list = new List<T?>();

        if (drawPhase == DrawPhase.All)
        {
            if (_dict.ContainsKey(DrawPhase.Geography))
                list.Add(_dict[DrawPhase.Geography]);
            if (_dict.ContainsKey(DrawPhase.Selection))
                list.Add(_dict[DrawPhase.Selection]);
            if (_dict.ContainsKey(DrawPhase.Graphics))
                list.Add(_dict[DrawPhase.Graphics]);
        }
        else
        {
            if (_dict.ContainsKey(drawPhase))
                list.Add(_dict[drawPhase]);
        }

        list.Where(t => t is not null)
            .ToList()
            .ForEach(t => action(t!));

        return this;
    }

    public DrawPhaseObjectDictionary<T> Clear(DrawPhase drawPhase)
    {
        if(drawPhase == DrawPhase.All)
        {
            Clear(DrawPhase.Geography);
            Clear(DrawPhase.Selection);
            Clear(DrawPhase.Graphics);
        } 
        else
        {
            if (_dict.ContainsKey(drawPhase))
                _dict[drawPhase] = default(T);
        }

        return this;
    }
}
