# Act as a Senior Lead Developer and Prompt Engineer.

## I am building the "LOOP Language" project (a Unity C# Python Interpreter) based on a detailed specification. The project has generated over 15 distinct C# files (Lexer.cs, Parser.cs, AST.cs, PythonInterpreter.cs, etc.), and the codebase is growing large.

## I cannot upload all these files into a new chat session every time I want to add a feature because it will hit token limits or degrade reasoning quality.

## I need you to teach me a `Stateless Maintenance Workflow` (i heard the xml Prompt works best for Claude 4.5 in Future) so I can work with you indefinitely on this project without re-uploading everything(sincse the all `*.cs` may surpass the 200k token upload limit ? ).

## Please provide the following 3 deliverables(in a single file):

1. THE MAP TEMPLATE
Create a detailed(without leaving any) "Project Map" text file template for this specific project. It should list every file we have generated so far, a 1-sentence summary of its responsibility, and key signatures (like for example, the main `Evaluate()` method or `TokenType` enum names).
*Goal:* I want to paste this *one* text block at the start of every chat so you understand the architecture without seeing the code.

2. THE "SCOUT" PROMPT
Write a standardized prompt I should use when requesting a new feature. This prompt should:
- Present the "Project Map" to you.
- State the new feature goal (e.g., "Add a 'teleport()' game command").
- Explicitly instruct you NOT to generate code yet, but instead to analyze the map and tell me exactly which 2-3 specific files you need to see to implement the feature.

3. THE WORKFLOW EXAMPLE
Walk me through a hypothetical example:
"I want to add a new 'Modulo (%)' operator to the language."
Show me exactly how our interaction would look using the prompts you just designed (Me sending the Scout Prompt -> You asking for specific files -> Me providing them -> You generating the solution).



## provide me an example how exactly in future i should make use of it so that in future following is possible:
