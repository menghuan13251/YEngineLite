using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"UnityEngine.AssetBundleModule.dll",
		"UnityEngine.CoreModule.dll",
		"UnityEngine.JSONSerializeModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// System.Action<float>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<object>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,object>
	// System.Func<object>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory<object>
	// UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene,int>
	// }}

	public void RefMethods()
	{
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27>(System.Runtime.CompilerServices.TaskAwaiter&,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager.<LoadSceneAsync>d__19>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager.<LoadSceneAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,ResourceManager.<LoadSceneAsync>d__19>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,ResourceManager.<LoadSceneAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27>(System.Runtime.CompilerServices.TaskAwaiter&,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager.<LoadSceneAsync>d__19>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager.<LoadSceneAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,ResourceManager.<LoadSceneAsync>d__19>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,ResourceManager.<LoadSceneAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,ResourceManager.<LoadAssetFromABAsync>d__26<object>>(System.Runtime.CompilerServices.TaskAwaiter&,ResourceManager.<LoadAssetFromABAsync>d__26<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager.<LoadAssetByFullPathInternalAsync>d__22<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager.<LoadAssetByFullPathInternalAsync>d__22<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager.<LoadAsync>d__17<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager.<LoadAsync>d__17<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,ResourceManager.<LoadAssetBundleFromFileInternalAsync>d__29>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,ResourceManager.<LoadAssetBundleFromFileInternalAsync>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,ResourceManager.<LoadAssetFromABAsync>d__26<object>>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,ResourceManager.<LoadAssetFromABAsync>d__26<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27>(ResourceManager.<LoadAssetBundleWithDependenciesAsync>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<ResourceManager.<LoadSceneAsync>d__19>(ResourceManager.<LoadSceneAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<ResourceManager.<LoadAssetBundleFromFileInternalAsync>d__29>(ResourceManager.<LoadAssetBundleFromFileInternalAsync>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<ResourceManager.<LoadAssetByFullPathInternalAsync>d__22<object>>(ResourceManager.<LoadAssetByFullPathInternalAsync>d__22<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<ResourceManager.<LoadAssetFromABAsync>d__26<object>>(ResourceManager.<LoadAssetFromABAsync>d__26<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<ResourceManager.<LoadAsync>d__17<object>>(ResourceManager.<LoadAsync>d__17<object>&)
		// object UnityEngine.AssetBundle.LoadAsset<object>(string)
		// UnityEngine.AssetBundleRequest UnityEngine.AssetBundle.LoadAssetAsync<object>(string)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.JsonUtility.FromJson<object>(string)
		// object[] UnityEngine.Object.FindObjectsOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object[] UnityEngine.Resources.ConvertObjects<object>(UnityEngine.Object[])
	}
}