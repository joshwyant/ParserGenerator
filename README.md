# LRGenerator
A proof-of-concept LALR parser generator with combinator input, written in C#.

The parser generator also has the ability to load and save parse tables as a caching/installation mechanism.

The parse table can be compiled to machine code on the fly or to a .NET assembly, whether the table was
loaded from a file or generated dynamically from the grammar.

Example grammar in C#:

    /* For the grammar:
       S -> L = R | R
       L -> *R | id
       R -> L
    */
    grammar = new LALRGrammar();
    
    s = grammar.DefineProduction(S);
    l = grammar.DefineProduction(L);
    r = grammar.DefineProduction(R);
    
    ProductionRule t;
    
    // S -> L = R
    t = 
      s % L / Equals / R;
    
    // S -> R
    t = 
      s % R;
    
    // L -> *R
    t = 
      l % Asterisk / R;
    
    // L -> id
    t = 
      l % Ident;
    
    // R -> L
    t = 
      r % L;
