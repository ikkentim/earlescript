Sample Grammar
--------------
```
S -> A a
A -> B D
B -> b
B -> ε
D -> d
D -> ε
```

first
-----
The FIRST(A) set contains the first terminal symbols which may occur for symbol A. If symbol A is a terminal, FIRST(A) will only contain the terminal. If non-terminal A can be resolved to nothing, ε (epsilon) can is added to FIRST(A). The FIRST(AB) consists of all FIRST(A) except ε. If FIRST(A) contains ε then FIRST(AB) also contains FIRST(B).

follow
------
FOLLOW(A) contains the terminal symbols and delimiters($ for end of file) which may be found after an occurance of A. For Q -> AB, FOLLOW(A) would contain FIRST(B).

LR0 Item
---------
An LR0 item represents a state in which a shift-reduce parser can be within a production rule when a decision is to be made whether to shift or reduce. A dot usually indicates the position of the "cursor" within the production rule.

Some examples of LR0 items:

- A -> • B D
- A -> B • D
- D -> • d

LR Closure
----------
A closure in a set of LR0 items.

LR Canonical Collection
-----------------------
A canonical collection is a {closure -> go to row} dictionary. The closure contains all possible LR0 items which can occur while the cursor of the parser is at a single position.  For the sample grammar, the first closure would be: `S -> • A a; A -> • B D; B -> • b; B -> •` where `;` separates te LR0 items. The go to row is a {symbol -> closure} dictionary which contains all possible next states the shift-reduce parser could be moved to when a certain symbol is reduced. The closure contains the next possible states of the parser and can be found as a key in the canonical collection. For the sample closure above, the go to row would contain the following (in `KEY :: value,` notation): `A :: S -> A • a, B :: A -> B • D; D ->  • d; D -> •, b :: B -> b •,`. When the cursor is at the end of the production rule, like in `B -> b •`, the rule can immediately be reduced, in this case `b` to `B`.


