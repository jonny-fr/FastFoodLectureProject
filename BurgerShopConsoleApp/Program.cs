// The following block constitutes the Composition Root of the application. 
// In accordance with the Dependency Inversion Principle, this is the singular 
// architectural boundary where concrete infrastructure implementations are 
// instantiated and wired to their respective abstractions. By restricting 
// the knowledge of concrete types to this specific location, the remainder 
// of the application remains entirely decoupled from infrastructure concerns, 
// thereby maximizing modularity, testability, and maintainability.

// The temporal abstraction is materialized by wrapping the physical system 
// time provider within a SessionClock. This concrete implementation satisfies 
// both the read-only IShopClock contract required by the application services 
// and the mutable ISettableShopClock contract required by the presentation 
// boundary for temporal simulation and deterministic testing purposes.

using BurgerShopConsoleApp.Application.Cart;
using BurgerShopConsoleApp.Application.Menu;
using BurgerShopConsoleApp.Application.Time;
using BurgerShopConsoleApp.Cli;

var clock = new SessionClock(TimeProvider.System);

// The domain catalog is populated utilizing a dedicated factory method. 
// This encapsulates the seed data ingestion logic, ensuring that the 
// application service receives a fully validated and immutable MenuCatalog 
// instance without needing to understand the underlying parsing mechanics.
var catalog = MenuCatalogFactory.CreateStandard();

// The application service is instantiated with its required dependencies. 
// The CartService operates strictly against the IShopClock and IMenuCatalog 
// abstractions, remaining entirely agnostic to the concrete SessionClock 
// or the specific data source of the catalog.
var cartService = new CartService(clock, catalog);

// The presentation boundary (InteractiveShell) is composed by injecting 
// the application service, the catalog abstraction, the settable clock, 
// and the standard input/output streams. The explicit injection of 
// Console.In and Console.Out as TextReader and TextWriter, respectively, 
// decouples the shell's logic from the physical operating system console. 
// This architectural decision facilitates deterministic automated testing 
// by allowing the injection of in-memory stream equivalents during test execution.
var shell = new InteractiveShell(
    cartService,
    catalog,
    clock,
    Console.In,
    Console.Out);

// The execution lifecycle is initiated, transferring control to the 
// presentation boundary's Read-Eval-Print Loop (REPL).
shell.Run();