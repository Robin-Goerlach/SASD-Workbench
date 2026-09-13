using System.Runtime.CompilerServices;

// The dedicated host test project verifies non-visual startup/configuration behavior only. Keeping
// WorkbenchStartupOptions internal avoids turning desktop command-line details into a public Core API.
[assembly: InternalsVisibleTo("SASD.Workbench.WinForms.Tests")]
