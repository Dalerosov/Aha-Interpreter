# Aha — A Bit-Level Esoteric Interpreter

Aha is a small interpreted language I built from scratch in C#. It started as an experiment in writing a simple binary adder, then grew into a full interpreter with functions, scoped variables, an include/linking system, and optional result caching.

The language is intentionally low-level: the only primitive type is a bit array, and the only native operations are `AND` and `NOT`. Everything else — arithmetic, comparisons, control flow — has to be composed from those primitives.

---

## How it works

The interpreter runs in a few distinct phases:

**1. Linking** — before execution begins, the source file is scanned for `INCLUDE` directives. Included files are appended to the in-memory code list and their `setup` functions are called. Double-inclusion is prevented by tracking sector names; if a sector is already registered, the file is skipped entirely.

**2. Sector resolution** — each file (and the main program) defines a named sector via `SECSTART`/`SECEND`. Sectors serve as namespaces to prevent function name collisions across included files.

**3. Execution** — the interpreter evaluates lines sequentially. Control flow is handled via `SKIP`/`NSKIP` (conditional line skipping) rather than labels or jumps. Function calls push a frame onto a `List<Function>` stack; `END` pops it and restores the previous line counter.

**4. Caching** — functions are optionally memoized. A cache key is built from the function name and the binary values of its arguments, so the same logical call with the same inputs skips re-execution.

---

## Language overview

### Variables

All variables are bit arrays. There is no integer, string, or float type.

```
VAR name[size]           // local to current function
VAR name[size] = 1010    // local, initialized (binary value)
GLOBAL name[size]        // global scope
```

Variables are freed when their enclosing function returns.

### Operations

```
OPERATION result IS a AND b
OPERATION result IS NOT a
```

Operations work on single bits (index 0 of the variable). To work with multi-bit values, copy bits explicitly using `SET`:

```
SET target target_start source source_start length
```

### Control flow

```
SKIP 3 IF flag 0          // skip 3 lines if bit 0 of flag is 1
NSKIP 2 NOT flag 0        // skip 2 lines if bit 0 of flag is NOT 0
```

Multiple `IF`/`NOT` clauses are ANDed together.

### Functions

```
FUNCTION main
  // code
END

FUNCTION add(a,b)
  // code
END a_plus_b
```

Call with return value:
```
USE result IS add(x,y)
```

Call without return value:
```
INVOKE add(x,y)
```

Parameters are currently passed by reference. A copy utility function is the recommended workaround for value semantics.

### Output

```
PRINT varname            // print raw binary
PRINT varname DECIMAL    // print as signed decimal
PRINT varname UDECIMAL   // print as unsigned decimal
PRINT varname TRIM       // strip leading zeros
```

### Includes

```
INCLUDE math.aha
```

Included files must define a `setup` function (can be empty) and wrap their contents in `SECSTART name` / `SECEND name`. Recursive and duplicate includes are handled automatically.

---

## Debugging

```
VOMIT CODE       // print all linked code with line numbers
VOMIT SECTORS    // print sector names and their line ranges
VOMIT STEPS      // print current execution step count
```

To enable step-by-step tracing, set `setting_dps` early in `main`:
```
ASSIGN setting_dps 1    // print line number on each step
```
Values `01`, `10`, `11`, `001`, `101` give progressively more detail (sector name, token, full line).

---

## Example

A program that computes NOT of a bit and prints the result:

```
FUNCTION main
  VAR a[1] = 1
  VAR b[1]
  OPERATION b IS NOT a
  PRINT b UDECIMAL
END
```

Output: `0`

---

## Known limitations and design notes

- **Hardcoded file path** — the interpreter currently reads from `C:/aha/prog.aha`. Making this a CLI argument is a planned improvement.
- **Bit-level storage** — values are stored as `char[]` of `'0'` and `'1'`. This is intentional for simplicity; the focus was on interpreter design, not runtime efficiency.
- **Reference semantics for parameters** — function arguments currently refer to the caller's variable directly. This is a known limitation; copying before passing is the current workaround.
- **Caching** — result caching is opt-in and trades memory for speed on repeated calls with identical arguments. It can be enabled by setting `setting_caching_out`.

---

## What I learned

Building this pushed me to think carefully about things I'd otherwise take for granted: how a call stack works at the data level, how to resolve includes without cycles, how scoping interacts with variable lookup, and how to compose complex behaviour from a minimal instruction set. It also made me appreciate how much work goes into even a "simple" language runtime.
