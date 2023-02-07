using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeOCoreTest;

internal class HookModelMock : IHookModel
{
    public void AddHook(string hook, Action<object> action) { }
    public void AddHook<T>(Action<T> action) where T : Hook, new() { }
    public void ClearHooks() { }
    public void Init(IResourceModel resourceModel) { }
    public void RemoveHook(string hook) { }
    public void RemoveHook(string hook, Action<object> action) { }
    public void RemoveHook<T>() where T : Hook, new() { }
    public void RemoveHook<T>(Action<T> action) where T : Hook, new() { }
    public void Shoot(string hook, object param) { }
    public void Shoot<T>(T hook) where T : Hook, new() { }
}

internal class ResourceModelMock : IResourceModel
{
    public void Add(List<object> values) { }
    public void Add(object value) { }
    public void Add(string key, object value) { }
    public void Add(List<Tuple<string, object>> keyValues) { }
    public T Get<T>(string key) where T : class { return null; }
    public T Get<T>() where T : class { return null; }
    public void Init(IResourceModel resourceModel) { }
    public void Remove(string key) { }
}