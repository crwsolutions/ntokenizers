#r "src/NTokenizers/bin/Debug/netstandard2.0/NTokenizers.dll"
open NTokenizers.Java

let tokens = JavaTokenizer.Create().Parse("// line comment\n/* block comment */ /** Javadoc */")
for t in tokens do
  printfn "%A: %s" t.TokenType t.Value
