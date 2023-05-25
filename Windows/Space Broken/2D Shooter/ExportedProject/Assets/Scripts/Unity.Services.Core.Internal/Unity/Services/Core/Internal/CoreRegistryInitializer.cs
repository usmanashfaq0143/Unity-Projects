using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Unity.Services.Core.Internal
{
	internal class CoreRegistryInitializer
	{
		[NotNull]
		private readonly CoreRegistry m_Registry;

		[NotNull]
		private readonly List<int> m_SortedPackageTypeHashes;

		public CoreRegistryInitializer([NotNull] CoreRegistry registry, [NotNull] List<int> sortedPackageTypeHashes)
		{
			m_Registry = registry;
			m_SortedPackageTypeHashes = sortedPackageTypeHashes;
		}

		public async Task InitializeRegistryAsync()
		{
			if (m_SortedPackageTypeHashes.Count <= 0)
			{
				return;
			}
			DependencyTree dependencyTree = m_Registry.PackageRegistry.Tree;
			if (dependencyTree == null)
			{
				throw new NullReferenceException("Registry requires a valid dependency tree to be initialized.");
			}
			m_Registry.ComponentRegistry.ResetProvidedComponents(dependencyTree.ComponentTypeHashToInstance);
			List<Exception> failureReasons = new List<Exception>(m_SortedPackageTypeHashes.Count);
			for (int i = 0; i < m_SortedPackageTypeHashes.Count; i++)
			{
				try
				{
					await InitializePackageAtIndexAsync(i);
				}
				catch (Exception item)
				{
					failureReasons.Add(item);
				}
			}
			if (failureReasons.Count > 0)
			{
				Fail();
			}
			void Fail()
			{
				AggregateException innerException = new AggregateException(failureReasons);
				throw new ServicesInitializationException("Some services couldn't be initialized. Look at inner exceptions to get more information.", innerException);
			}
			async Task InitializePackageAtIndexAsync(int index)
			{
				int key = m_SortedPackageTypeHashes[index];
				await dependencyTree.PackageTypeHashToInstance[key].Initialize(m_Registry);
			}
		}
	}
}
