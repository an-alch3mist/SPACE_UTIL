using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using SPACE_UTIL;

namespace SPACE_SYNTAX
{
	/// <summary>
	/// Comprehensive JSON prettifier and minifier with support for:
	/// - Single and double quotes
	/// - Escaped characters (including Unicode surrogate pairs)
	/// - Multi-line comments (/* */) - for JSON-with-comments variant
	/// - Proper number formatting (scientific notation, negatives)
	/// - Null character handling
	/// - Proper indentation
	/// </summary>
	public static class JSON
	{
		#region public API
		/// <summary>
		/// Formats JSON string based on prettify flag
		/// </summary>
		/// <param name="json">Input JSON string</param>
		/// <param name="pretifyMinify">True to prettify, false to minify</param>
		/// <param name="useSpaces">True to use spaces, false to use tabs (default: false/tabs)</param>
		/// <param name="indentSize">Number of spaces for indentation when useSpaces is true (default: 1)</param>
		/// <returns>Formatted JSON string</returns>
		public static string FormatJSON(this string json, bool pretifyMinify, bool useSpaces = false, int indentSize = 1)
		{
			if (string.IsNullOrEmpty(json))
				return json;

			json = json.Trim();

			return pretifyMinify ? Prettify(json, useSpaces, indentSize) : Minify(json);
		}

		/// <summary>
		/// Validates if a string is valid JSON (ignoring comments)
		/// </summary>
		public static bool IsValidJson(this string json)
		{
			if (string.IsNullOrEmpty(json))
				return false;

			try
			{
				// Remove comments for validation
				json = RemoveCommentsForValidation(json).Trim();

				int depth = 0;
				bool inString = false;
				char stringDelimiter = '"';
				bool escape = false;
				bool expectingValue = false;
				bool expectingKey = false;
				char lastStructure = '\0';

				for (int i = 0; i < json.Length; i++)
				{
					char c = json[i];

					if (escape)
					{
						escape = false;
						// Validate escape sequences
						if (c == 'u')
						{
							if (i + 4 >= json.Length)
								return false;
							// Validate next 4 characters are hex
							for (int j = 1; j <= 4; j++)
							{
								if (!IsHexDigit(json[i + j]))
									return false;
							}
							i += 4;
						}
						else if (c != '"' && c != '\\' && c != '/' && c != 'b' &&
								 c != 'f' && c != 'n' && c != 'r' && c != 't')
						{
							return false; // Invalid escape sequence
						}
						continue;
					}

					if (c == '\\')
					{
						if (!inString)
							return false;
						escape = true;
						continue;
					}

					// Only double quotes are valid for standard JSON
					if (c == '"' && !escape)
					{
						if (!inString)
						{
							inString = true;
							stringDelimiter = c;
							expectingValue = false;
							expectingKey = false;
						}
						else if (c == stringDelimiter)
						{
							inString = false;
						}
						continue;
					}

					// Single quotes not allowed in standard JSON
					if (c == '\'' && !inString)
						return false;

					if (inString)
					{
						// Check for unescaped control characters
						if (char.IsControl(c) && c != '\t')
							return false;
						continue;
					}

					// Skip whitespace
					if (char.IsWhiteSpace(c))
						continue;

					if (c == '{')
					{
						depth++;
						lastStructure = '{';
						expectingKey = true;
						expectingValue = false;
					}
					else if (c == '[')
					{
						depth++;
						lastStructure = '[';
						expectingValue = true;
						expectingKey = false;
					}
					else if (c == '}')
					{
						// Check for trailing comma before closing brace
						if (expectingKey || expectingValue)
						{
							// Look back for comma (trailing comma check)
							for (int j = i - 1; j >= 0; j--)
							{
								if (char.IsWhiteSpace(json[j]))
									continue;
								if (json[j] == ',')
									return false; // Trailing comma
								break;
							}
						}

						if (lastStructure != '{' && lastStructure != '\0')
						{
							// Track stack properly
							int tempDepth = 0;
							char expectedClose = '}';
							for (int j = i - 1; j >= 0; j--)
							{
								if (json[j] == '}') tempDepth++;
								else if (json[j] == ']') tempDepth++;
								else if (json[j] == '{')
								{
									if (tempDepth == 0)
									{
										expectedClose = '}';
										break;
									}
									tempDepth--;
								}
								else if (json[j] == '[')
								{
									if (tempDepth == 0)
									{
										expectedClose = ']';
										break;
									}
									tempDepth--;
								}
							}
							if (expectedClose != '}')
								return false;
						}

						depth--;
						expectingKey = false;
						expectingValue = false;
					}
					else if (c == ']')
					{
						// Check for trailing comma before closing bracket
						if (expectingValue)
						{
							// Look back for comma
							for (int j = i - 1; j >= 0; j--)
							{
								if (char.IsWhiteSpace(json[j]))
									continue;
								if (json[j] == ',')
									return false; // Trailing comma
								break;
							}
						}

						depth--;
						expectingKey = false;
						expectingValue = false;
					}
					else if (c == ':')
					{
						if (lastStructure != '{')
							return false;
						expectingValue = true;
						expectingKey = false;
					}
					else if (c == ',')
					{
						if (lastStructure == '{')
							expectingKey = true;
						else
							expectingValue = true;
					}
					else if (IsNumberStart(c, json, i))
					{
						// Validate number format
						if (!ValidateNumber(json, ref i))
							return false;
						expectingValue = false;
						expectingKey = false;
					}
					else if (c == 't' || c == 'f' || c == 'n')
					{
						// Validate true/false/null
						if (!ValidateLiteral(json, i))
							return false;
						expectingValue = false;
						expectingKey = false;
					}

					if (depth < 0)
						return false;
				}

				return depth == 0 && !inString && !expectingValue && !expectingKey;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#region private API

		private static string Prettify(string json, bool useSpaces, int indentSize)
		{
			StringBuilder sb = new StringBuilder();
			int indent = 0;
			bool inString = false;
			char stringDelimiter = '"';
			bool escape = false;
			int commentDepth = 0;

			for (int i = 0; i < json.Length; i++)
			{
				char c = json[i];

				// Handle escape sequences
				if (escape)
				{
					sb.Append(c);
					escape = false;

					// Handle Unicode escape sequences including surrogate pairs
					if (c == 'u' && i + 4 < json.Length)
					{
						// Validate and append the 4 hex digits
						bool validHex = true;
						for (int j = 0; j < 4; j++)
						{
							if (!IsHexDigit(json[i + j + 1]))
							{
								validHex = false;
								break;
							}
						}

						if (validHex)
						{
							for (int j = 0; j < 4; j++)
							{
								i++;
								sb.Append(json[i]);
							}
						}
					}
					continue;
				}

				if (c == '\\')
				{
					sb.Append(c);
					escape = true;
					continue;
				}

				// Handle string boundaries
				if ((c == '"' || c == '\'') && !escape)
				{
					if (!inString)
					{
						inString = true;
						stringDelimiter = c;
						sb.Append(c);
					}
					else if (c == stringDelimiter)
					{
						inString = false;
						sb.Append(c);
					}
					else
					{
						sb.Append(c);
					}
					continue;
				}

				// If we're inside a string, just append (including null chars)
				if (inString)
				{
					sb.Append(c);
					continue;
				}

				// Handle multi-line comments /* */ with proper nesting
				if (c == '/' && i + 1 < json.Length && json[i + 1] == '*')
				{
					commentDepth = 1;
					sb.Append("/*");
					i += 2;

					while (i < json.Length && commentDepth > 0)
					{
						if (i + 1 < json.Length)
						{
							if (json[i] == '/' && json[i + 1] == '*')
							{
								commentDepth++;
								sb.Append("/*");
								i += 2;
								continue;
							}
							else if (json[i] == '*' && json[i + 1] == '/')
							{
								commentDepth--;
								sb.Append("*/");
								i += 2;
								continue;
							}
						}
						sb.Append(json[i]);
						i++;
					}
					i--;
					continue;
				}

				// Handle numbers and negative signs - keep them together
				// Handle numbers and negative signs - keep them together
				if (IsNumberStart(c, json, i))
				{
					int startPos = i;
					sb.Append(c);
					i++;
					// modify for: Fix overly permissive number character checking
					// Continue appending number characters with proper validation
					while (i < json.Length && IsNumberChar(json[i]))
					{
						char numChar = json[i];

						// +/- only allowed immediately after 'e' or 'E'
						if ((numChar == '+' || numChar == '-'))
						{
							if (i == 0 || (json[i - 1] != 'e' && json[i - 1] != 'E'))
								break; // Stop if +/- appears in wrong position
						}

						sb.Append(json[i]);
						i++;
					}
					i--; // Back up one since loop will increment
					continue;
				}

				// Handle formatting outside strings
				switch (c)
				{
					case '{':
					case '[':
						sb.Append(c);
						sb.Append('\n');
						indent++;
						sb.Append(GetIndent(indent, useSpaces, indentSize));
						break;

					case '}':
					case ']':
						sb.Append('\n');
						indent--;
						sb.Append(GetIndent(indent, useSpaces, indentSize));
						sb.Append(c);
						break;

					case ',':
						sb.Append(c);
						sb.Append('\n');
						sb.Append(GetIndent(indent, useSpaces, indentSize));
						break;

					case ':':
						sb.Append(c);
						sb.Append(' ');
						break;

					case ' ':
					case '\t':
					case '\n':
					case '\r':
						// Skip whitespace outside strings and comments
						break;

					default:
						sb.Append(c);
						break;
				}
			}

			return sb.ToString();
		}

		private static string Minify(string json)
		{
			StringBuilder sb = new StringBuilder();
			bool inString = false;
			char stringDelimiter = '"';
			bool escape = false;
			int commentDepth = 0;

			for (int i = 0; i < json.Length; i++)
			{
				char c = json[i];

				// Handle escape sequences
				if (escape)
				{
					sb.Append(c);
					escape = false;

					// Handle Unicode escape sequences including surrogate pairs
					if (c == 'u' && i + 4 < json.Length)
					{
						// Validate and append the 4 hex digits
						bool validHex = true;
						for (int j = 0; j < 4; j++)
						{
							if (!IsHexDigit(json[i + j + 1]))
							{
								validHex = false;
								break;
							}
						}

						if (validHex)
						{
							for (int j = 0; j < 4; j++)
							{
								i++;
								sb.Append(json[i]);
							}
						}
					}
					continue;
				}

				if (c == '\\')
				{
					sb.Append(c);
					escape = true;
					continue;
				}

				// Handle string boundaries
				if ((c == '"' || c == '\'') && !escape)
				{
					if (!inString)
					{
						inString = true;
						stringDelimiter = c;
						sb.Append(c);
					}
					else if (c == stringDelimiter)
					{
						inString = false;
						sb.Append(c);
					}
					else
					{
						sb.Append(c);
					}
					continue;
				}

				// If we're inside a string, append everything (including null chars)
				if (inString)
				{
					sb.Append(c);
					continue;
				}

				// Handle multi-line comments /* */ with nesting - preserve with single space
				if (c == '/' && i + 1 < json.Length && json[i + 1] == '*')
				{
					commentDepth = 1;
					StringBuilder comment = new StringBuilder();
					i += 2;

					while (i < json.Length && commentDepth > 0)
					{
						if (i + 1 < json.Length)
						{
							if (json[i] == '/' && json[i + 1] == '*')
							{
								commentDepth++;
								comment.Append("/*");
								i += 2;
								continue;
							}
							else if (json[i] == '*' && json[i + 1] == '/')
							{
								commentDepth--;
								if (commentDepth > 0)
									comment.Append("*/");
								i += 2;
								continue;
							}
						}
						comment.Append(json[i]);
						i++;
					}

					// Collapse whitespace in comment
					string commentStr = System.Text.RegularExpressions.Regex.Replace(comment.ToString().Trim(), @"\s+", " ");
					sb.Append("/* ");
					sb.Append(commentStr);
					sb.Append(" */");
					i--;
					continue;
				}

				// Handle numbers - don't split them
				// Handle numbers - don't split them
				if (IsNumberStart(c, json, i))
				{
					sb.Append(c);
					i++;
					// modify for: Fix overly permissive number character checking
					// Continue appending number characters without spaces
					while (i < json.Length && IsNumberChar(json[i]))
					{
						char numChar = json[i];

						// +/- only allowed immediately after 'e' or 'E'
						if ((numChar == '+' || numChar == '-'))
						{
							if (i == 0 || (json[i - 1] != 'e' && json[i - 1] != 'E'))
								break; // Stop if +/- appears in wrong position
						}

						sb.Append(json[i]);
						i++;
					}
					i--; // Back up one since loop will increment
					continue;
				}

				// Outside strings, skip whitespace
				if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
					continue;

				sb.Append(c);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Check if character starts a number (including negative)
		/// </summary>
		private static bool IsNumberStart(char c, string json, int index)
		{
			// Digit starts a number
			if (char.IsDigit(c))
				return true;

			// Negative sign followed by digit
			if (c == '-' && index + 1 < json.Length && char.IsDigit(json[index + 1]))
				return true;

			return false;
		}

		/// <summary>
		/// Check if character is part of a number
		/// Handles: digits, decimal point, scientific notation (e/E), +/- signs
		/// </summary>
		private static bool IsNumberChar(char c)
		{
			return char.IsDigit(c) ||
				   c == '.' ||
				   c == 'e' ||
				   c == 'E' ||
				   c == '+' ||
				   c == '-';
		}

		/// <summary>
		/// Validates a complete number in JSON format
		/// </summary>
		private static bool ValidateNumber(string json, ref int index)
		{
			int start = index;
			bool hasDecimal = false;
			bool hasExponent = false;
			bool hasDigits = false;

			// Optional negative sign
			if (json[index] == '-')
				index++;

			if (index >= json.Length)
				return false;

			// Leading zero check - if starts with 0, must be followed by . or e/E or end
			if (json[index] == '0')
			{
				hasDigits = true;
				index++;
				if (index < json.Length && char.IsDigit(json[index]))
					return false; // Leading zeros not allowed (007)
			}
			else
			{
				// Read integer part
				while (index < json.Length && char.IsDigit(json[index]))
				{
					hasDigits = true;
					index++;
				}
			}

			if (!hasDigits)
				return false;

			// Optional decimal part
			if (index < json.Length && json[index] == '.')
			{
				if (hasDecimal)
					return false; // Multiple decimals
				hasDecimal = true;
				index++;

				// Must have at least one digit after decimal
				if (index >= json.Length || !char.IsDigit(json[index]))
					return false;

				while (index < json.Length && char.IsDigit(json[index]))
					index++;
			}

			// Optional exponent
			if (index < json.Length && (json[index] == 'e' || json[index] == 'E'))
			{
				if (hasExponent)
					return false;
				hasExponent = true;
				index++;

				// Optional +/- sign
				if (index < json.Length && (json[index] == '+' || json[index] == '-'))
					index++;

				// Must have at least one digit
				if (index >= json.Length || !char.IsDigit(json[index]))
					return false;

				while (index < json.Length && char.IsDigit(json[index]))
					index++;
			}

			index--; // Back up for the outer loop
			return true;
		}

		/// <summary>
		/// Validates true/false/null literals
		/// </summary>
		private static bool ValidateLiteral(string json, int index)
		{
			if (index + 4 <= json.Length && json.Substring(index, 4) == "true")
				return true;
			if (index + 5 <= json.Length && json.Substring(index, 5) == "false")
				return true;
			if (index + 4 <= json.Length && json.Substring(index, 4) == "null")
				return true;
			return false;
		}

		/// <summary>
		/// Check if character is a valid hexadecimal digit
		/// </summary>
		private static bool IsHexDigit(char c)
		{
			return (c >= '0' && c <= '9') ||
				   (c >= 'a' && c <= 'f') ||
				   (c >= 'A' && c <= 'F');
		}

		private static string GetIndent(int level, bool useSpaces, int size)
		{
			if (useSpaces)
				return new string(' ', level * size);
			else
				return new string('\t', level);
		}

		private static string RemoveCommentsForValidation(string json)
		{
			StringBuilder sb = new StringBuilder();
			bool inString = false;
			char stringDelimiter = '"';
			bool escape = false;
			int commentDepth = 0;

			for (int i = 0; i < json.Length; i++)
			{
				char c = json[i];

				// Handle escape sequences
				if (escape)
				{
					sb.Append(c);
					escape = false;

					// Handle Unicode escape sequences
					if (c == 'u' && i + 4 < json.Length)
					{
						bool validHex = true;
						for (int j = 0; j < 4; j++)
						{
							if (!IsHexDigit(json[i + j + 1]))
							{
								validHex = false;
								break;
							}
						}

						if (validHex)
						{
							for (int j = 0; j < 4; j++)
							{
								i++;
								sb.Append(json[i]);
							}
						}
					}
					continue;
				}

				if (c == '\\')
				{
					sb.Append(c);
					escape = true;
					continue;
				}

				// Track string boundaries (only double quotes for standard JSON)
				if (c == '"' && !escape)
				{
					if (!inString)
					{
						inString = true;
						stringDelimiter = c;
					}
					else if (c == stringDelimiter)
					{
						inString = false;
					}
					sb.Append(c);
					continue;
				}

				// If in string, just append (including null chars)
				if (inString)
				{
					sb.Append(c);
					continue;
				}

				// Check for multi-line comments /* */ outside strings with nesting support
				if (c == '/' && i + 1 < json.Length && json[i + 1] == '*')
				{
					commentDepth = 1;
					i += 2;

					while (i < json.Length && commentDepth > 0)
					{
						if (i + 1 < json.Length)
						{
							if (json[i] == '/' && json[i + 1] == '*')
							{
								commentDepth++;
								i += 2;
								continue;
							}
							else if (json[i] == '*' && json[i + 1] == '/')
							{
								commentDepth--;
								i += 2;
								continue;
							}
						}
						i++;
					}
					i--;
					continue;
				}

				sb.Append(c);
			}

			return sb.ToString();
		}

		#endregion
	}
}