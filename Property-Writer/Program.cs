using System.Text.RegularExpressions;
using FF7R2;
using FF7R2.DataObject;

namespace Property_Writer;

public static class Program {
    public static void Main() {
        var files = PathHelper.GetFileList();

        foreach (var file in files) {
            IoStoreAsset data;
            try {
                data = IoStoreAsset.Load(file);
            } catch (Exception e) {
                Console.WriteLine($"Ignoring file doe to error loading it: {file}");
                Console.WriteLine($"Error: {e.Message}");
                continue;
            }

            var props = from property in data.innerAsset.frozenObject.properties.data
                        orderby property.Name.Text
                        select property.Name.Text;

            CreateConstantsFile(props, Path.GetFileNameWithoutExtension(file) + "Properties");

            var rows = (from key in data.innerAsset.frozenObject.DataTable.Keys
                        let name = key.name.Text
                        where name != "." && name != "None"
                        orderby name
                        select name).Distinct();
            CreateConstantsFile(rows, Path.GetFileNameWithoutExtension(file) + "Rows");
        }
    }

    private static void CreateConstantsFile(IEnumerable<string> props, string className) {
        Directory.CreateDirectory(PathHelper.CONSTANTS_WRITE_PATH);
        using var writer = new StreamWriter(File.Create($@"{PathHelper.CONSTANTS_WRITE_PATH}\{className}.cs"));
        writer.WriteLine("// ReSharper disable All");
        writer.WriteLine("namespace FF7R2.Constants;");
        writer.WriteLine("");
        writer.WriteLine($"public static class {className} {{");

        var regex = new Regex(@"^\d");

        foreach (var name in props) {
            var constName = name.Replace("'", "")
                                .Replace("\"", "")
                                .Replace(",", "")
                                .Replace(".", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Replace("/", "_")
                                .Replace("&", "AND")
                                .Replace("+", "_PLUS")
                                .Replace("%", "_PERCENT")
                                .Replace('-', '_')
                                .Replace(' ', '_')
                                .Replace(':', '_')
                                .Replace('{', '_')
                                .Replace('}', '_')
                                .Replace('[', '_')
                                .Replace(']', '_')
                                .Replace('#', '_');

            if (regex.Match(constName).Success) constName = $"_{constName}";

            writer.WriteLine($"    public const string {constName} = \"{name}\";");
        }
        writer.WriteLine("}");
    }
}