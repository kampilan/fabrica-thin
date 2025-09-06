﻿using System.Text.Json.Serialization;
using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public abstract class BasePipelineContext
{
    
    public bool Success { get; set; } = true;

    [JsonConverter( typeof(JsonStringEnumConverter<PipelinePhase>))]    
    public PipelinePhase Phase { get; set; }

    public string FailedStep { get; set; } = string.Empty;
    
    [JsonIgnore]
    public Exception? Cause { get; set; }

    public string ExceptionType => Cause?.GetType().Name ?? string.Empty;    
    public string ExceptionMessage => Cause?.Message ?? string.Empty;
    
}