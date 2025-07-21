using GameDataTool.Core.Logging;
using GameDataTool.Core.Configuration;

namespace GameDataTool.Parsers;

public class DataValidator
{
    public Task<ValidationResult> ValidateAsync(GameData data, Validation config)
    {
        var result = new ValidationResult();
        
        Console.WriteLine();
        Console.WriteLine("Start data validation...");
        Console.WriteLine();

        if (config.EnableTypeCheck)
        {
            ValidateTypes(data, result);
        }

        if (config.EnableRequiredFieldCheck)
        {
            ValidateRequiredFields(data, result);
        }

        // Additional validation rules
        ValidateEnumReferences(data, result);
        ValidateDataIntegrity(data, result);
        ValidateFieldNames(data, result);
        ValidateForeignKeyReferences(data, result);

        return Task.FromResult(result);
    }

    private void ValidateTypes(GameData data, ValidationResult result)
    {
        foreach (var table in data.Tables)
        {
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];
                for (int colIndex = 0; colIndex < table.Fields.Count && colIndex < row.Values.Count; colIndex++)
                {
                    var field = table.Fields[colIndex];
                    var value = row.Values[colIndex];

                    // Only validate non-empty values for type checking
                    // Empty strings are valid for string fields and should be allowed
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!ValidateFieldType(field.Type, value))
                        {
                            result.Errors.Add($"Table {table.Name} Row {rowIndex + 4} Col {colIndex + 1}: Field {field.Name} value '{value}' does not match type {field.Type}");
                        }
                    }
                }
            }
        }
    }

    private bool ValidateFieldType(FieldType type, string value)
    {
        return type switch
        {
            FieldType.Int => int.TryParse(value, out _),
            FieldType.Long => long.TryParse(value, out _),
            FieldType.Float => float.TryParse(value, out _),
            FieldType.Bool => bool.TryParse(value, out _) || value.ToLower() is "0" or "1" or "true" or "false",
            FieldType.String => true, // Any string value is valid, including empty strings
            FieldType.Enum => int.TryParse(value, out _),
            _ => true
        };
    }

    private void ValidateRequiredFields(GameData data, ValidationResult result)
    {
        foreach (var table in data.Tables)
        {
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];
                for (int colIndex = 0; colIndex < table.Fields.Count && colIndex < row.Values.Count; colIndex++)
                {
                    var field = table.Fields[colIndex];
                    var value = row.Values[colIndex];

                    if (IsRequiredField(field.Name) && string.IsNullOrEmpty(value))
                    {
                        result.Errors.Add($"Table {table.Name} Row {rowIndex + 4} Col {colIndex + 1}: Required field {field.Name} is empty");
                    }
                }
            }
        }
    }

    private bool IsRequiredField(string fieldName)
    {
        var lowerName = fieldName.ToLower();
        return lowerName.Contains("id");
    }

    private void ValidateEnumReferences(GameData data, ValidationResult result)
    {
        // Validate enum references
        foreach (var table in data.Tables)
        {
            for (int fieldIndex = 0; fieldIndex < table.Fields.Count; fieldIndex++)
            {
                var field = table.Fields[fieldIndex];
                if (field.Type == FieldType.Enum)
                {
                    // Try to find the corresponding enum
                    var enumName = field.EnumType ?? GetEnumNameFromField(field.Name);
                    var enumType = data.Enums.FirstOrDefault(e => e.Name.Equals(enumName, StringComparison.OrdinalIgnoreCase));
                    if (enumType == null)
                    {
                        result.Errors.Add($"Table {table.Name} Field {field.Name}: Enum type {enumName} is not defined");
                        continue;
                    }
                    // Validate enum value is in defined range
                    for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        var row = table.Rows[rowIndex];
                        if (fieldIndex < row.Values.Count)
                        {
                            var value = row.Values[fieldIndex];
                            if (!string.IsNullOrEmpty(value))
                            {
                                int enumValue;
                                if (int.TryParse(value, out enumValue))
                                {
                                    if (!enumType.Values.Any(v => v.Value == enumValue))
                                    {
                                        result.Errors.Add($"Table {table.Name} Row {rowIndex + 2} Col {fieldIndex + 1}: Enum value {enumValue} is not defined in {enumName}");
                                    }
                                }
                                else
                                {
                                    var found = enumType.Values.FirstOrDefault(ev => ev.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
                                    if (found == null)
                                    {
                                        result.Errors.Add($"Table {table.Name} Row {rowIndex + 2} Col {fieldIndex + 1}: Enum name '{value}' is not defined in {enumName}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private string GetEnumNameFromField(string fieldName)
    {
        // Improved enum name inference
        if (fieldName.ToLower().Contains("type"))
        {
            // If field name is just "Type", try to find a more specific enum name
            if (fieldName.ToLower() == "type")
            {
                // This will be handled by the calling context to find the appropriate enum
                return "Type";
            }
            // For fields like "CharacterType", return as is
            return fieldName;
        }
        return fieldName + "Type";
    }

    private void ValidateDataIntegrity(GameData data, ValidationResult result)
    {
        foreach (var table in data.Tables)
        {
            // Check ID uniqueness
            var idFieldIndex = -1;
            for (int i = 0; i < table.Fields.Count; i++)
            {
                if (table.Fields[i].Name.ToLower().Contains("id"))
                {
                    idFieldIndex = i;
                    break;
                }
            }

            if (idFieldIndex >= 0)
            {
                var ids = new HashSet<string>();
                for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                {
                    var row = table.Rows[rowIndex];
                    if (idFieldIndex < row.Values.Count)
                    {
                        var id = row.Values[idFieldIndex];
                        if (!string.IsNullOrEmpty(id))
                        {
                            if (ids.Contains(id))
                            {
                                result.Errors.Add($"Table {table.Name} Row {rowIndex + 4}: ID '{id}' is duplicated");
                            }
                            else
                            {
                                ids.Add(id);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ValidateFieldNames(GameData data, ValidationResult result)
    {
        foreach (var table in data.Tables)
        {
            var fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            for (int i = 0; i < table.Fields.Count; i++)
            {
                var field = table.Fields[i];
                
                // Check if field name is empty
                if (string.IsNullOrWhiteSpace(field.Name))
                {
                    result.Errors.Add($"Table {table.Name} Col {i + 1}: Field name cannot be empty");
                    continue;
                }

                // Check for duplicate field names
                if (fieldNames.Contains(field.Name))
                {
                    result.Errors.Add($"Table {table.Name} Col {i + 1}: Field name '{field.Name}' is duplicated");
                }
                else
                {
                    fieldNames.Add(field.Name);
                }

                // Check for special characters in field name
                if (field.Name.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                {
                    result.Errors.Add($"Table {table.Name} Col {i + 1}: Field name '{field.Name}' contains special characters. Only letters, digits, and underscores are recommended");
                }
            }
        }
    }

    private void ValidateForeignKeyReferences(GameData data, ValidationResult result)
    {
        foreach (var table in data.Tables)
        {
            for (int fieldIndex = 0; fieldIndex < table.Fields.Count; fieldIndex++)
            {
                var field = table.Fields[fieldIndex];
                if (!string.IsNullOrEmpty(field.ReferenceTable) && !string.IsNullOrEmpty(field.ReferenceField))
                {
                    // search for target table & field
                    var refTable = data.Tables.FirstOrDefault(t => t.Name.Equals(field.ReferenceTable, StringComparison.OrdinalIgnoreCase));
                    if (refTable == null)
                    {
                        result.Errors.Add($"Table {table.Name} Field {field.Name}: Referenced table {field.ReferenceTable} does not exist");
                        continue;
                    }
                    var refFieldIndex = refTable.Fields.FindIndex(f => f.Name.Equals(field.ReferenceField, StringComparison.OrdinalIgnoreCase));
                    if (refFieldIndex < 0)
                    {
                        result.Errors.Add($"Table {table.Name} Field {field.Name}: Referenced field {field.ReferenceField} does not exist in table {field.ReferenceTable}");
                        continue;
                    }
                    // collect all referenced fields
                    var refValues = new HashSet<string>(refTable.Rows.Select(r => refFieldIndex < r.Values.Count ? r.Values[refFieldIndex] : ""));
                    // check all fields in the table
                    for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        var row = table.Rows[rowIndex];
                        if (fieldIndex < row.Values.Count)
                        {
                            var value = row.Values[fieldIndex];
                            // Only validate non-empty values for foreign key references
                            // Empty strings are valid for string fields
                            if (!string.IsNullOrEmpty(value) && !refValues.Contains(value))
                            {
                                result.Errors.Add($"Table {table.Name} Row {rowIndex + 2} Col {fieldIndex + 1}: Field {field.Name} value '{value}' not found in {field.ReferenceTable}.{field.ReferenceField}");
                            }
                        }
                    }
                }
            }
        }
    }
}

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = new();
} 