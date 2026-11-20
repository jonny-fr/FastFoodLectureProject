using System.Globalization;
using BurgerShopConsoleApp.Application.Cart;
using BurgerShopConsoleApp.Application.Menu;
using BurgerShopConsoleApp.Application.Parsing;
using BurgerShopConsoleApp.Application.Receipts;
using BurgerShopConsoleApp.Application.Time;

namespace BurgerShopConsoleApp.Cli;

/// <summary>
///     Represents the presentation and infrastructure boundary of the application,
///     implementing a Read-Eval-Print Loop (REPL) to facilitate user interaction.
/// </summary>
/// <remarks>
///     The class is declared as <c>sealed</c> to preclude inheritance, ensuring that
///     the interaction lifecycle remains strictly controlled. The utilization of a
///     primary constructor encapsulates all dependencies, enforcing mandatory injection
///     and reducing boilerplate.
///     Didactically, the abstraction of input and output streams via <see cref="TextReader" />
///     and <see cref="TextWriter" /> is a critical architectural decision. By decoupling
///     the shell from the concrete <see cref="Console" /> class, the application enables
///     deterministic automated testing. Test harnesses can inject <see cref="StringReader" />
///     and <see cref="StringWriter" /> instances to simulate user input and capture output
///     without requiring actual console I/O or operating system-level process manipulation.
/// </remarks>
public sealed class InteractiveShell(
    CartService cartService,
    IMenuCatalog menuCatalog,
    ISettableShopClock clock,
    TextReader input,
    TextWriter output)
{
    /// <summary>
    ///     Initiates the main execution loop of the command-line interface.
    /// </summary>
    /// <remarks>
    ///     This method implements the classic REPL (Read-Eval-Print Loop) pattern.
    ///     It continuously reads user input, evaluates the corresponding command,
    ///     and prints the result until a termination condition is met.
    /// </remarks>
    public void Run()
    {
        // The initial invocation of the help command provides the user with 
        // immediate context regarding available operations upon shell startup.
        WriteHelp();

        while (true)
        {
            // The prompt is written to the output stream to indicate readiness 
            // for user input.
            output.Write("> ");

            // The ReadLine method blocks until a line of characters is entered 
            // or the end of the stream is reached.
            var line = input.ReadLine();

            // A null return value from ReadLine signifies the End-Of-File (EOF) 
            // condition. In a console environment, this typically occurs when the 
            // input stream is closed. Handling this gracefully ensures the application 
            // terminates cleanly without throwing a NullReferenceException.
            if (line is null) return;

            // Empty or whitespace-only inputs are ignored, preventing unnecessary 
            // command parsing and error generation for accidental keystrokes.
            if (string.IsNullOrWhiteSpace(line)) continue;

            // The input line is tokenized using the space character as a delimiter. 
            // The RemoveEmptyEntries option ensures that multiple consecutive spaces 
            // do not result in empty string tokens, enhancing the parser's resilience 
            // to irregular user typing.
            var tokens = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // The first token is normalized to lowercase to ensure case-insensitive 
            // command routing, improving the user experience by accepting commands 
            // in any casing variation.
            var command = tokens[0].ToLowerInvariant();

            try
            {
                // The command is dispatched to the execution handler. The boolean 
                // return value dictates the continuation of the REPL loop; a false 
                // value signals a termination request.
                if (!ExecuteCommand(command, tokens)) return;
            }
            catch (CartOperationException exception)
            {
                // This catch block represents a crucial boundary translation mechanism. 
                // While the application and domain layers utilize exceptions to signal 
                // business rule violations, the presentation layer must translate these 
                // into user-friendly feedback. By catching the specific CartOperationException, 
                // the shell prevents the application from crashing due to expected user errors, 
                // thereby maintaining the continuity of the interactive session. Unexpected 
                // infrastructure exceptions will bypass this block and propagate, adhering 
                // to the fail-fast principle.
                output.WriteLine($"Error: {exception.Message}");
            }
        }
    }

    /// <summary>
    ///     Routes the parsed command to the appropriate handler method.
    /// </summary>
    /// <param name="command">The normalized command identifier.</param>
    /// <param name="tokens">The complete array of parsed input tokens.</param>
    /// <returns>
    ///     <c>true</c> if the REPL loop should continue; <c>false</c> if the user
    ///     has requested to terminate the session.
    /// </returns>
    private bool ExecuteCommand(string command, string[] tokens)
    {
        // The switch statement provides a centralized routing mechanism. 
        // This structure ensures that all command handlers are co-located, 
        // facilitating maintainability and providing a clear overview of the 
        // shell's capabilities.
        switch (command)
        {
            case "exit":
            case "quit":
                // Returning false signals the Run method to break the REPL loop 
                // and terminate the application gracefully.
                return false;

            case "help":
                WriteHelp();
                break;

            case "menu":
                WriteMenu();
                break;

            case "status":
                WriteStatus();
                break;

            case "add":
                HandleAdd(tokens);
                break;

            case "remove":
                HandleRemove(tokens);
                break;

            case "note":
                HandleNote(tokens);
                break;

            case "clear":
                cartService.Clear();
                output.WriteLine("Cart cleared.");
                break;

            case "checkout":
                HandleCheckout();
                break;

            case "at":
                HandleAt(tokens);
                break;

            case "now":
                // This command resets the temporal abstraction to the physical system clock. 
                // It is primarily utilized for debugging or reverting from a simulated time state.
                clock.UseSystemTime();
                output.WriteLine("Shop time set to system time.");
                break;

            default:
                // The default branch handles unrecognized commands, providing immediate 
                // feedback to the user and guiding them toward the help command.
                output.WriteLine($"Unknown command '{command}'. Type 'help'.");
                break;
        }

        // Returning true ensures the REPL loop continues to the next iteration.
        return true;
    }

    private void WriteHelp()
    {
        // Raw string literals (denoted by """) are utilized to preserve the exact 
        // whitespace and formatting of the help text, enhancing readability within 
        // the source code while producing a cleanly aligned output in the console.
        output.WriteLine("""
                         Commands:
                           menu                       show menu
                           add <product> <quantity>   add product
                           remove <product>           remove product
                           note [text]                set or remove order note
                           clear                      clear cart
                           status                     show cart and shop time
                           checkout                   checkout
                           at <yyyy-MM-ddTHH:mm>      set shop time
                           now                        use system time
                           exit                       leave
                         """);
    }

    private void WriteMenu()
    {
        output.WriteLine("Menu:");

        // The LINQ query enforces a deterministic ordering of the menu items. 
        // Sorting first by category and then by name (using ordinal comparison) 
        // guarantees that the output is consistent across multiple executions. 
        // This predictability is a fundamental requirement for both a stable user 
        // experience and reliable automated testing, where exact string matching 
        // is often employed to verify output.
        foreach (var product in menuCatalog.Products
                     .OrderBy(product => product.Category)
                     .ThenBy(product => product.Name, StringComparer.Ordinal))
            // The string.Create method is utilized for high-performance string 
            // interpolation. By specifying CultureInfo.InvariantCulture, the 
            // formatting of numerical values (such as price and preparation time) 
            // is strictly decoupled from the host machine's regional settings, 
            // ensuring uniform output regardless of the deployment environment.
            output.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"{product.Name,-8} {product.Price,6:F2} EUR, {product.PreparationMinutes,2} min, {product.Description}"));
    }

    private void WriteStatus()
    {
        // The temporal state is retrieved from the injected clock abstraction. 
        // The formatting strictly adheres to the ISO 8601 standard for the date 
        // and a 24-hour clock for the time, utilizing the invariant culture to 
        // prevent locale-specific variations.
        output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Shop time: {clock.LocalNow:yyyy-MM-dd HH:mm}"));

        // The total quantity is computed on demand by aggregating the quantities 
        // of all individual cart lines. This derived metric provides the user with 
        // a concise summary of the cart's physical volume.
        output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Cart items: {cartService.Lines.Sum(line => line.Quantity)}"));

        output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Cart total: {cartService.Subtotal:F2} EUR"));

        // The note is conditionally rendered only if it contains non-whitespace 
        // characters, preventing the display of empty or meaningless metadata.
        if (!string.IsNullOrWhiteSpace(cartService.Note)) output.WriteLine($"Note: {cartService.Note}");
    }

    private void HandleAdd(string[] tokens)
    {
        // A structural validation ensures that the command contains at least 
        // a product name and a quantity. This fail-fast mechanism prevents 
        // index-out-of-range exceptions during subsequent token extraction.
        if (tokens.Length < 3)
        {
            output.WriteLine("Usage: add <product> <quantity>");
            return;
        }

        // The parsing strategy assumes that the final token represents the 
        // numerical quantity, while all preceding tokens constitute the product name. 
        // This approach elegantly supports multi-word product names without requiring 
        // complex quoting mechanisms in the CLI interface.
        var productName = string.Join(' ', tokens[1..^1]);
        var quantityText = tokens[^1];

        // The application service's TryAddProduct method is invoked, utilizing 
        // the non-exception-based "Try" pattern. At the presentation boundary, 
        // invalid user input is an expected occurrence; therefore, handling it 
        // via boolean returns and error messages is more efficient and semantically 
        // appropriate than relying on exception-driven control flow.
        if (cartService.TryAddProduct(productName, quantityText, out var errorMessage))
            output.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"Added {productName} x {quantityText}."));
        else
            output.WriteLine(errorMessage);
    }

    private void HandleRemove(string[] tokens)
    {
        if (tokens.Length < 2)
        {
            output.WriteLine("Usage: remove <product>");
            return;
        }

        // Similar to the add command, all tokens following the command identifier 
        // are joined to reconstruct the full product name, accommodating multi-word items.
        var productName = string.Join(' ', tokens[1..]);

        // Unlike the add operation, the remove operation delegates to a method that 
        // throws a CartOperationException if the product is not found. This design 
        // choice reflects the semantic difference: adding an invalid item is a common 
        // user error (expected), whereas attempting to remove a non-existent item 
        // indicates a logical inconsistency in the user's mental model of the cart state, 
        // which is treated as a distinct operational failure.
        cartService.RemoveProduct(productName);

        output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Removed {productName}."));
    }

    private void HandleNote(string[] tokens)
    {
        // The command supports both setting and clearing the note. If no additional 
        // tokens are provided, the note is set to an empty string, effectively clearing it. 
        // Otherwise, the remaining tokens are joined to form the note's text.
        var note = tokens.Length == 1
            ? string.Empty
            : string.Join(' ', tokens[1..]);

        cartService.Note = note;

        // The feedback provided to the user is dynamically determined based on 
        // the resulting state of the note, ensuring clear communication of the 
        // operation's outcome.
        output.WriteLine(note.Length == 0
            ? "Note removed."
            : "Note stored.");
    }

    private void HandleCheckout()
    {
        // The checkout process utilizes the TryCheckout method to handle expected 
        // business rule violations (such as attempting to checkout an empty cart) 
        // gracefully. If the operation succeeds, the resulting immutable Receipt 
        // is passed to the ReceiptFormatter for textual serialization. If it fails, 
        // the extracted error message is displayed to the user. This pattern ensures 
        // that the interactive session remains uninterrupted by anticipated user errors.
        output.WriteLine(cartService.TryCheckout(out var receipt, out var errorMessage)
            ? ReceiptFormatter.Format(receipt!)
            : errorMessage);
    }

    private void HandleAt(string[] tokens)
    {
        if (tokens.Length != 2)
        {
            output.WriteLine("Usage: at <yyyy-MM-ddTHH:mm>");
            return;
        }

        // This command demonstrates the practical application of the temporal 
        // abstraction. By parsing the user-provided string and injecting it into 
        // the ISettableShopClock, the application's perception of "now" is altered. 
        // This capability is indispensable for testing time-dependent business rules 
        // (e.g., opening hours, preparation deadlines) without modifying the physical 
        // system clock or waiting for real-time progression.
        if (InputParser.TryParseLocalDateTime(tokens[1], out var localNow))
        {
            clock.Set(localNow);

            output.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"Shop time set to {localNow:yyyy-MM-dd HH:mm}."));
        }
        else
        {
            output.WriteLine("Invalid date/time. Use yyyy-MM-ddTHH:mm.");
        }
    }
}