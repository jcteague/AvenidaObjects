using System;
using System.Collections.Generic;
using System.Reflection;

namespace AvenidaSoftware.Objects {
	public class AssemblyScanner {

		public List<Type> GetAllInheritorsOf( Type type ) {
			var return_types = new List<Type>();
			var generic_type_definition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach( var assembly in assemblies ) {
				// This try-catch is required because it can't get all types located in some assemblies (eg: mscorlib)
				try {
					var types = assembly.GetTypes();

					foreach( var item in types ) {
						if( !item.IsClass || item.IsAbstract ) continue;
						if( type.IsAssignableFrom( item ) || (generic_type_definition != null && generic_type_definition.IsAssignableFrom( item )) ) {
							return_types.Add( item );
						}
					}
				} catch( Exception ) {
					// No exception is thrown because if it cant load all types withing the assembly it might not matter anyways.
				}
			}

			return return_types;
		}

		public List<Assembly> GetAllAssembliesThatContains( Type type ) {
			var assembly_list = new List<Assembly>();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach( var assembly in assemblies ) {
				var types = assembly.GetTypes();
				foreach( var item in types ) {
					if( !type.IsAssignableFrom( item ) ) continue;
					assembly_list.Add( assembly );
					break;
				}
			}

			return assembly_list;
		}

	}
}