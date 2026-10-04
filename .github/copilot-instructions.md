# Copilot Instructions

## Project Guidelines
- For CommonMark compliance tests in this repository, expected output may intentionally deviate from the CommonMark spec as documented in AGENTS.md; consult that guidance before evaluating or changing assertions.

## Tokenizer Guidelines
- In NTokenizers, preserve blank lines rather than consuming them; swallowing whitespace causes downstream rendering fidelity bugs.
