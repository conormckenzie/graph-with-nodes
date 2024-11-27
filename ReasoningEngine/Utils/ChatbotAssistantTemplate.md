# Introduction Prompt Template for AI Chatbots

## Version:
Introduction Prompt Template for AI Chatbots, Version 2.4

## Context:
This message contains information that may be relevant to our conversation, or may not. It is not necessarily tailored specifically to this conversation, and instead is a template which is easy to copy and paste to quickly give you (the AI) hopefully-relevant information. This is an attempt to save me (the user & human conversation participant) time.
If you (the AI) are asked to produce this template with any changes or edits, please output it as markdown code so it can be copied and pasted correctly; default output formatters sometimes remove the markdown formatting which results in loss of the information about the hierarchical structure of this template.
If when producing this template you suggest any changes, please update the template version with the standard naming scheme `Version x.y` where x represents the major version and y represents the minor version. At your discretion you may choose to consider the changes as a major update or a minor version update, and update the template version accordingly. Please do not use sub-minor versions or any other deviation from this naming scheme.
If producing this template, please output the full text, so that it can be copied and pasted over the previous version. Do not use shortcuts such as "(section remains unchanged)" since this breaks the ability to copy and paste correctly over the previous version.
If producing this template, please also replace the "Goals for this Conversation" section content with "(Please ask the user for the goals for this conversation)" as a placeholder.

## Key Information:
- **Project Name:** Reasoning Engine

## Overview:
An AI system designed to exceed human-level problem-solving in general contexts by:
- Encoding knowledge in a human-readable graph.
- Performing explicit and auditable reasoning.
- Incorporating new information via data ingestion algorithms assisted by LLMs.
- Enhancing knowledge accuracy and predictive power through refinement algorithms.
The project now implements a versioning system for nodes and edges, allowing for future extensibility while maintaining backward compatibility.

## Scope:
The reasoning engine is intended to solve general problems through logical inference, with an emphasis on transparency and auditability.

## Objectives:

### **Short Term:**
- Refine the knowledge graph to incorporate new [data/concepts/relationships].
- Nodes should be one of two types: single-input multi-output [SIMO] / multi-input single-output [MISO].
- SIMO nodes' numeric values are finite probability density functions over a domain of some subset of the real numbers.
- SIMO nodes should have a parameter that tells how to interpret the domain of the node's numeric value, such as "Truth" or "Range x y".
- Edges should be one of two types: parameter [Param] (from SIMO node to MISO node) / functional [Func] (from MISO node to SIMO node).
- There are no edges from MISO nodes to other MISO nodes, nor from SIMO nodes to other SIMO nodes.
- Func edges should be function-weighted and represent performing a function on multiple inputs to get a single output.
- Param edges should have a weight of 1, transferring the value from the source node without changing it.
- Implement hierarchical edge indexing for efficient retrieval of incoming and outgoing edges.
- Update edge and node directory structures to limit entries per directory.
- Implement index.json files at each directory level for efficient traversal.
- Improve Menu Item handling so that MenuItem encapsulates more of its own logic.

### **Long Term:**
- Expand the engine's reasoning capabilities to handle more complex queries and refine its knowledge base dynamically through interaction with LLMs.
- Consider performance optimizations and integration with large language models.
- Address concurrency and consistency concerns as the system scales.
- Split the main node index into multiple files to handle large numbers of nodes efficiently.
- Implement comprehensive test coverage for both unit and integration tests.
- Develop API documentation and interactive exploration tools.

## Key Concepts:
- **Human-Readable Knowledge Graph:** A graph data structure designed to store knowledge in a way that is both machine-processable and easily interpretable by humans. Each node represents a concept, and each edge represents a logical or relational connection between these concepts. This structure supports explicit reasoning, where inferences and deductions can be made by traversing the graph using logical rules.
- **Chain-of-Logic Inference:** The process of making deductions by sequentially applying logical rules to known facts in the graph. This involves traversing the graph and combining information from various nodes and edges to reach a conclusion.
- **Disambiguation:** The process by which the model clarifies, resolves conflicts, and reduces uncertainty when multiple potential outcomes or interpretations arise under the same conditions, ensuring that the most appropriate or likely outcome is identified.
- **Versioned Nodes and Edges:** The system uses abstract base classes (NodeBase and EdgeBase) with version-specific implementations to allow for future extensions. The versioning system uses linear inheritance to ensure backward compatibility without branching functionalities.
- **Version Compatibility:** A VersionCompatibilityAttribute and VersionCompatibilityChecker are used to manage algorithm compatibility with different node/edge versions.
- **Hierarchical Edge Indexing:** A system designed to efficiently retrieve incoming and outgoing edges without scanning large directories, using index.json files at each directory level.
- **File Management System:** A hierarchical file-based storage system that efficiently organizes and manages nodes and edges, following principles of self-documentation, error resistance, and consistency.
- **Probability Distribution System:** A system for representing and manipulating probability distributions with measurement uncertainty:
  - Every numeric value has ±EPSILON uncertainty (EPSILON = 1e-10)
  - Uses closed ranges [a,b] with uncertain bounds
  - Minimum range width of 5*EPSILON to ensure clear separation
  - Supports three domain types:
    - Continuous: Uses ranges
    - DiscreteInteger: Uses integer points
    - Truth: Uses ranges or points in [0,1]
  - Operations include adding points/ranges, querying probabilities, checking completeness
  - Maintains invariants like total probability ≤ 1, no overlapping ranges
  - Handles edge cases consistently based on distance to range boundaries
- **RESTful API Design:** The system exposes its functionality through a RESTful API with clear, consistent patterns for endpoint naming and operations. Each operation is explicitly named in the URL for clarity (e.g., /api/nodes/{id}/update instead of just PUT /api/nodes/{id}).
- **API Response Consistency:** All API responses follow a consistent format using an ApiResponse<T> wrapper that includes success status, data payload, and error information.

### **Approaches for Disambiguation**:

1. **Approach 0: Consideration of Additional Factors**
   - **Concept**: This approach involves identifying and analyzing additional input variables, conditions, or factors that might influence which of the possible outcomes will occur. By considering these factors, you can refine the model and improve the accuracy of predictions.
   - **When to Use**: Use this approach when you suspect that there are external conditions or internal variables that haven't been fully accounted for and that these could clarify which outcome is more likely.
   - **Why It's Important**: Many ambiguities arise because not all relevant factors have been considered. By incorporating more variables into your analysis, you can often resolve ambiguities and make more accurate predictions.
   - **Example**: If you're trying to determine whether water will freeze (B1) or remain liquid (B2) at 0°C, you might consider factors like salinity, air pressure, or the presence of impurities. Recognizing that the water contains a high concentration of salt could lead you to expect B2 (the water does not freeze).

2. **Approach 1: Probabilistic Reasoning**
   - **Concept**: Instead of seeking a deterministic or absolute outcome, this approach acknowledges that the system may produce multiple outcomes, each with a certain probability. You use probabilistic reasoning to assess the likelihood of each outcome and make predictions based on these probabilities.
   - **When to Use**: This approach is useful when dealing with systems that exhibit inherent randomness or uncertainty, where it's impossible to predict a single outcome with certainty.
   - **Why It's Important**: Probabilistic reasoning allows you to handle uncertainty in a systematic way, providing a way to make informed predictions even when the system doesn't behave deterministically.
   - **Example**: If past observations show that water freezes (B1) in 70% of cases and remains liquid (B2) in 30% of cases when at 0°C, you would use these probabilities to predict future outcomes.

3. **Approach 5.1: Questioning the Definitions**
   - **Concept**: This approach involves revisiting and clarifying the definitions of key terms or concepts involved in the ambiguous situation. By ensuring that terms are clearly and consistently defined, you can resolve ambiguities that arise from different interpretations or misunderstandings.
   - **When to Use**: Use this approach when the ambiguity seems to arise from different interpretations of the same term or concept. If two outcomes seem contradictory, it may be due to differing definitions of the terms involved.
   - **Why It's Important**: Many ambiguities are semantic rather than substantive. Clarifying definitions ensures that everyone is discussing the same concepts in the same way, which is crucial for accurate reasoning and communication.
   - **Example**: If B1 states that "water freezes at 0°C" and B2 states that "water remains liquid at 0°C," the ambiguity might arise from different definitions of "water" (e.g., pure water vs. saltwater). Clarifying that "water" in B1 refers to pure water could resolve the ambiguity.

4. **Approach 5.4: Identifying Possible Logical Errors**
   - **Concept**: This approach focuses on analyzing the reasoning process that led to the ambiguous outcomes to identify and correct any logical inconsistencies or errors. By ensuring that the logic connecting conditions (C1) and outcomes (C2) is sound, you can resolve ambiguities that stem from flawed reasoning.
   - **When to Use**: Use this approach when you suspect that the ambiguity might be due to a logical fallacy, contradiction, or inconsistency in the reasoning process.
   - **Why It's Important**: Logical errors can lead to incorrect or conflicting conclusions, so identifying and correcting these errors is crucial for maintaining the integrity of the reasoning process.
   - **Example**: If B1 and B2 seem to contradict each other, you might examine the logical steps that connect "water is below 0°C" (C1) to "water is solid" (C2) in B1, and similarly for B2. If you find that B2 was based on a faulty assumption or an overlooked step, correcting this error could resolve the ambiguity.

## Technical Details:
- The program is implemented in C#/.NET
- The program uses a GraphFileManager for handling file operations related to versioned nodes and edges.
- An IndexManager is used to keep track of all nodes, their versions, and associated metadata.
- The project includes a CommandProcessor for handling user commands and a ConsoleMenu for user interactions with the graph.
- Debug utilities (DebugWriter and DebugOptions) are implemented for easier debugging and development.
- The project follows a specific file and directory structure for storing node and edge data, including hierarchical indexing for edges.
- The file management system uses a hierarchical structure to organize nodes and edges, with specific naming conventions to ensure self-documentation and error resistance.
- Edges are stored bidirectionally (as both outgoing and incoming) to allow efficient querying in both directions.
- The system uses deterministic algorithms to generate file paths for nodes and edges based on their IDs.
- OpenAPI/Swagger integration provides interactive API documentation and testing capabilities.
- Comprehensive unit and integration tests ensure system reliability.
- Standard response formats using ApiResponse<T> wrapper ensure consistent error handling and data retrieval.

## Project Structure:
- Core: Contains fundamental classes like NodeBase, EdgeBase, and their versioned implementations.
- GraphAccess: Includes CommandProcessor and ConsoleMenu for user interaction and command processing.
- GraphFileHandling: Contains GraphFileManager and IndexManager for file operations and indexing.
- Utils: Includes debugging utilities and this AI template.
- Tests: Contains both unit and integration tests for system components.
- API Documentation: Swagger UI provides interactive exploration of available endpoints.

## Development Guidelines:
- **Contributing**: Please review the [CONTRIBUTING.md](CONTRIBUTING.md) file for detailed guidelines on the development workflow, coding standards, and testing practices before contributing to the project.
- **API Design**: Follow RESTful principles but prioritize clarity over strict REST compliance. Include operation names in endpoints for better clarity.
- **Response Format**: Always use the ApiResponse<T> wrapper for consistent error handling and success/failure indication.
- **Testing**: Write both unit and integration tests. Use appropriate test setup and teardown to ensure test isolation.
- **Environment Configuration**: Use environment variables and .env files for configuration. Always document required environment variables.
- **Documentation**: Keep API documentation up to date. Use clear descriptions and examples in Swagger documentation.
- **Error Handling**: Provide meaningful error messages and appropriate HTTP status codes.
- **Placeholders**: If certain sections are not yet fully developed, use clear and concise placeholders to maintain a professional tone.
- **Future Development**: Always include a section for future development or a roadmap to show that the project is actively evolving.
- **Formatting**: When documenting directory structures and code blocks, remember to escape special characters (e.g., backticks) and provide instructions on how to un-escape them to avoid formatting issues.
- **Debugging**: Use the DebugWriter class for debug messages. Debug messages should follow the format "#XXXXXX#" where XXXXXX is a unique 6-character string.
- When implementing new features, consider version compatibility and update the VersionCompatibilityAttribute as necessary.
- Ensure proper error handling and input validation, especially in user-facing methods.
- When updating edge or node structures, ensure that the changes are reflected in both the file system and the indexing system.
- Follow the correct file structure for nodes and edges as outlined in the file management documentation. Pay special attention to the differences between outgoing and incoming edge structures.

<!-- Note: Potential inconsistency - Development Guidelines could include specific guidance for implementing probability distributions -->

3. **Approach 5.1: Questioning the Definitions**
   - **Concept**: This approach involves revisiting and clarifying the definitions of key terms or concepts involved in the ambiguous situation. By ensuring that terms are clearly and consistently defined, you can resolve ambiguities that arise from different interpretations or misunderstandings.
   - **When to Use**: Use this approach when the ambiguity seems to arise from different interpretations of the same term or concept. If two outcomes seem contradictory, it may be due to differing definitions of the terms involved.
   - **Why It's Important**: Many ambiguities are semantic rather than substantive. Clarifying definitions ensures that everyone is discussing the same concepts in the same way, which is crucial for accurate reasoning and communication.
   - **Example**: If B1 states that "water freezes at 0°C" and B2 states that "water remains liquid at 0°C," the ambiguity might arise from different definitions of "water" (e.g., pure water vs. saltwater). Clarifying that "water" in B1 refers to pure water could resolve the ambiguity.

4. **Approach 5.4: Identifying Possible Logical Errors**
   - **Concept**: This approach focuses on analyzing the reasoning process that led to the ambiguous outcomes to identify and correct any logical inconsistencies or errors. By ensuring that the logic connecting conditions (C1) and outcomes (C2) is sound, you can resolve ambiguities that stem from flawed reasoning.
   - **When to Use**: Use this approach when you suspect that the ambiguity might be due to a logical fallacy, contradiction, or inconsistency in the reasoning process.
   - **Why It's Important**: Logical errors can lead to incorrect or conflicting conclusions, so identifying and correcting these errors is crucial for maintaining the integrity of the reasoning process.
   - **Example**: If B1 and B2 seem to contradict each other, you might examine the logical steps that connect "water is below 0°C" (C1) to "water is solid" (C2) in B1, and similarly for B2. If you find that B2 was based on a faulty assumption or an overlooked step, correcting this error could resolve the ambiguity.

## Technical Details:
- The program is implemented in C#/.NET
- The program uses a GraphFileManager for handling file operations related to versioned nodes and edges.
- An IndexManager is used to keep track of all nodes, their versions, and associated metadata.
- The project includes a CommandProcessor for handling user commands and a ConsoleMenu for user interactions with the graph.
- Debug utilities (DebugWriter and DebugOptions) are implemented for easier debugging and development.
- The project follows a specific file and directory structure for storing node and edge data, including hierarchical indexing for edges.
- The file management system uses a hierarchical structure to organize nodes and edges, with specific naming conventions to ensure self-documentation and error resistance.
- Edges are stored bidirectionally (as both outgoing and incoming) to allow efficient querying in both directions.
- The system uses deterministic algorithms to generate file paths for nodes and edges based on their IDs.

## Project Structure:
- Core: Contains fundamental classes like NodeBase, EdgeBase, and their versioned implementations.
- GraphAccess: Includes CommandProcessor and ConsoleMenu for user interaction and command processing.
- GraphFileHandling: Contains GraphFileManager and IndexManager for file operations and indexing.
- Utils: Includes debugging utilities and this AI template.

## Development Guidelines:
- **Contributing**: Please review the [CONTRIBUTING.md](CONTRIBUTING.md) file for detailed guidelines on the development workflow, coding standards, and testing practices before contributing to the project.
- **Placeholders**: If certain sections are not yet fully developed, use clear and concise placeholders to maintain a professional tone.
- **Future Development**: Always include a section for future development or a roadmap to show that the project is actively evolving.
- **Formatting**: When documenting directory structures and code blocks, remember to escape special characters (e.g., backticks) and provide instructions on how to un-escape them to avoid formatting issues.
- **Debugging**: Use the DebugWriter class for debug messages. Debug messages should follow the format "#XXXXXX#" where XXXXXX is a unique 6-character string.
- When implementing new features, consider version compatibility and update the VersionCompatibilityAttribute as necessary.
- Ensure proper error handling and input validation, especially in user-facing methods.
- When updating edge or node structures, ensure that the changes are reflected in both the file system and the indexing system.
- Follow the correct file structure for nodes and edges as outlined in the file management documentation. Pay special attention to the differences between outgoing and incoming edge structures.

## Incomplete Tasks:
- **Usage Scenarios**: Example workflows and usage scenarios will be added to the readme as the project evolves.
- **Future Development**: Detailed features in the readme and a roadmap will be included in future updates.
- Integrate ConsoleMenu with CommandProcessor while maintaining separation of concerns.
- Optimize file I/O operations for better performance with large graphs.
- Implement more robust error handling and input validation across all classes.
- Refine the implementation of hierarchical edge indexing and ensure it's properly integrated with existing systems.
- Develop a strategy for splitting the main node index into multiple files to handle large numbers of nodes.
- Add API rate limiting and authentication.
- Implement response caching for frequently accessed nodes and edges.

## General Tips for you (the AI):
- Highlight potential errors or assumptions in my reasoning, even if they seem minor.
- Standardize the use of debugging functions like `DebugWriteLine` and `DebugWrite` across the project. Debug messages should be visually distinct and should not interfere with regular output.
- When making changes, consider the implications on the entire system, including file structure, indexing, and user interaction.
- When outputting one or more files (e.g. code files), for each file output the entire file contents even if only a small part of the file has been changed, unless the file is so large that doing so would be infeasible.
- Be aware of the hierarchical indexing system when discussing or implementing changes related to edge storage or retrieval.
- Ensure that any proposed changes to the file structure or indexing system align with the design principles of self-documentation, error resistance, and consistency.
- Consider API response format consistency and proper error handling in all endpoints.
- When designing new endpoints, follow the established pattern of including explicit operation names in URLs.
- Remember to update both implementation and tests when making changes to the API.

## Goals for this Conversation:
(Please ask the user for the goals for this conversation)

## Important Details from Recent Discussions (needs to be integrated into the rest of the document):
- The project uses a bidirectional edge storage system, with separate structures for incoming and outgoing edges to improve efficiency in graph operations.
- The file structure for edges includes separate "incoming" and "outgoing" subfolders under the "edges" directory to allow fast access to a node's incoming and outgoing connections.
- The `GetEdgeFilePath` method in `GraphFileManager` should take into account whether an edge is incoming or outgoing when constructing the file path.
- Edge operations (save, delete, etc.) should be performed for both the outgoing and incoming representations of each edge.
- The `UpdateNodeEdgeCount` method takes two parameters: the node ID and a boolean indicating whether it's updating outgoing or incoming edge counts.
- Consistency in implementation and adherence to the agreed-upon design are crucial for the project's integrity and functionality.
- The main node index file is planned to be split into multiple files in future development to handle large numbers of nodes more efficiently.
- The API follows a consistent response format using ApiResponse<T> wrapper for all endpoints.
- Environment variables can be set via .env file for easier configuration.
- OpenAPI/Swagger documentation is used to provide interactive API exploration.
- Endpoint naming includes explicit operation names for clarity (e.g., /api/nodes/{id}/update).
- The probability distribution system uses closed ranges with explicit handling of measurement uncertainty
- Points near range boundaries are assigned to the range with the closest boundary
- Adjacent ranges must maintain small gaps to prevent ambiguous point assignment
- All numeric comparisons must account for ±EPSILON uncertainty in values

<!-- Note: Potential inconsistency - Some tasks in Incomplete Tasks section may need updating to reflect probability distribution system implementation requirements -->
<!-- Note: Potential inconsistency - The relationship between SIMO/MISO nodes and the probability distribution system could be better explained -->