using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Castle.Core.Internal;
using Constants = VersionCommander.Constants;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("VersionCommander")]
[assembly: AssemblyDescription("Memory Versioning system for property bags")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Digital Leyline")]
[assembly: AssemblyProduct("VersionCommander")]
[assembly: AssemblyCopyright("Copyright ©  2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("5467afd8-1e80-499e-a37c-c97cac8cf4e1")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

// For testing
[assembly: InternalsVisibleTo(Constants.TestingAssemblyName)]
// To keep ickey methods away from users, but expose it to dynamic proxies
[assembly: InternalsVisibleTo("Castle.Core")]
[assembly: InternalsVisibleTo("Castle.Proxies")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]