namespace ApplicationCore.Interfaces;

public record CacheBox<T>(T? Value) where T : class;
