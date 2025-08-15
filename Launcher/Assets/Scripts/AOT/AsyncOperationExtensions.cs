//������Unity��AsyncOperationת��Ϊ�ɵȴ���Task

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class AsyncOperationExtensions
{
    // �ṩһ��ͨ�õ���չ���������������м̳���AsyncOperation����
    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += _ => { tcs.TrySetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
}