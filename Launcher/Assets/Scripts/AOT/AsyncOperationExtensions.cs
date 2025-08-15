//将所有Unity的AsyncOperation转换为可等待的Task

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class AsyncOperationExtensions
{
    // 提供一个通用的扩展方法，适用于所有继承自AsyncOperation的类
    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += _ => { tcs.TrySetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
}