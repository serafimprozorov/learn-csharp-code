# C# Language Features Roadmap
### C# 2.0 (2005)
- #pragma warning disable/restore directives;
- Partial types definition;
- Global namespace qualifier;
- Static classes and structures;
- Null coalescing operator (??);
- Nullable value types and T? definition;
- Iterators and yield operator;
- Anonymous delegates and closures;
- Generic types, delegates and methods;
- [Fixed size buffers](#fixed-size-buffers).

### C# 3.0 (2007)
- Implicit variable typing and the var keyword;
- Extension methods;
- Partial methods;
- Lambda expressions and expression trees;
- Object initializers;
- Anonymous types;
- Auto properties;
- Language Integrated Queries (LINQ).

### C# 4.0 (2010)
- Late binding and dynamic keyword;
- Named and optional parameters;
- Generic interfaces and delegates variance.

### C# 5.0 (2012)
- Asynchronous methods, async modifier and await operator;
- Caller information attributes;

### C# 6.0 (2015)
- Readonly auto properties;
- Auto property initializers;
- Expression methods and properties body;
- using static directive;
- Null propagation operator (?.);
- String interpolation;
- Exception filters;
- nameof operator;
- Associative collections initializers;
- Collection initialization extensions;

### C# 7.0 (2017)
- Out variables declaration;
- Pattern matching;
- Tuples;
- Deconstruction;
- Local functions;
- Reference variables;
- Awaitable types;
- Expression members bodies;
- Throw expressions;

### C# 7.1 (2017)
- Asynchronous Main method;
- Default literals;
- Tuple names comprehesion.

### C# 7.2 (2017)
- Reference value types;
- Span allocation support.

### C# 7.3 planned features (2018)
- Delegates as generic types constraints;
- Ranges and .. operator;
- Tuple equality operators;
- Auto-Implemented Property Field-Targeted Attributes
- Stackalloc array initializers
- Custom fixed size buffers.

### C# 8.0 rumors (2018?)
- Nullable reference types;
- Record classes;
- Default interface implementation;
- IAsyncEnumerable interface;
- IAsyncDisposable interface;
- Target-typed new operator;
- Generic custom attributes.

#### Fixed size buffers
```
public unsafe struct Sha256
{
    private const int Length = 32;

    // This is the fixed size array in C-style
    // It is allocated inside the structure
    // So our structure has size 32 bytes.
    // We have no overhead for utilitary internal 
    // runtime fields and memory allocation.
    // For example, if we need 32 bytes array, we alocate:
    // - 32 bytes in heap for an array values;
    // - 8 bytes for an array pointer;
    // - 16 bytes for lock object and type pointer;
    // Suddenly... we have 32 bytes of payload and 24(!) meaningless data.
    // If we use this struct we only use 32 bytes.
    private fixed byte _bytes[Length];
    
    public Sha256(Sha256 other)
    {
        for (var i = 0; i < Length; ++i)
        {
            this[i] = other[i];
        }
    }
    
    public byte this[int i]
    {
        get
        {
            if (i < 0 || i >= Length)
                throw new ArgumentOutOfRangeException(nameof(i));
            return _bytes[i];
        }
        set
        {
            if (i < 0 || i >= Length)
                throw new ArgumentOutOfRangeException(nameof(i));
            _bytes[i] = value;
        }
    }
}
```
